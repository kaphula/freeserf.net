﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Freeserf
{
    using Endianess = Endian.Endianess;

    internal static class TypeSize<T>
    {
        public readonly static uint Size;

        static TypeSize()
        {
            var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
            ILGenerator il = dm.GetILGenerator();
            il.Emit(OpCodes.Sizeof, typeof(T));
            il.Emit(OpCodes.Ret);
            Size = (uint)(int)dm.Invoke(null, null);
        }
    }

    unsafe public class BufferStream
    {
        byte[] data;
        byte* start;
        uint size;

        public static BufferStream CreateFromValues<T>(T value, uint count)
        {
            var size = TypeSize<T>.Size * count;

            var stream = new BufferStream(size);
            dynamic v = value;

            fixed (byte* pointer = stream.data)
            {
                switch (TypeSize<T>.Size)
                {
                    case 0u:
                        break;
                    case 1u:
                        Misc.CopyByte(pointer, (byte)v, count);
                        break;
                    case 2u:
                        Misc.CopyWord(pointer, (ushort)v, count);
                        break;
                    case 4u:
                        Misc.CopyDWord(pointer, (uint)v, count);
                        break;
                    case 8u:
                        Misc.CopyQWord(pointer, (ulong)v, count);
                        break;
                }
            }

            return stream;
        }

        public BufferStream(uint capacity)
        {
            data = new byte[capacity];

            fixed (byte* pointer = data)
            {
                start = pointer;
            }
        }

        public BufferStream(byte* data, uint size)
        {
            start = data;
            this.size = size;
        }

        public BufferStream(byte[] data)
            : this(data, 0u, (uint)data.Length)
        {

        }

        public BufferStream(ushort[] data)
        {
            uint size = (uint)data.Length << 1;

            this.data = new byte[size];

            fixed (ushort* dataPointer = data)
            fixed (byte* pointer = this.data)
            {
                byte* src = (byte*)dataPointer;
                byte* end = src + size;
                byte* dst = pointer;

                while (src < end)
                    *dst++ = *src++;

                start = pointer;
            }

            this.size = size;
        }

        public BufferStream(byte[] data, uint offset, uint size)
        {
            if (offset + size > data.Length)
                throw new IndexOutOfRangeException("The combination of offset and size exceeds base stream size.");

            this.data = new byte[size];

            System.Buffer.BlockCopy(data, (int)offset, this.data, 0, (int)size);

            fixed (byte* pointer = this.data)
            {
                start = pointer;
            }

            this.size = size;
        }

        public BufferStream(BufferStream baseStream, uint offset, uint size)
        {
            if (offset + size > baseStream.Size)
                throw new IndexOutOfRangeException("The combination of offset and size exceeds base stream size.");

            start = baseStream.start + offset;
            this.size = size;
        }

        public void Realloc(uint newSize)
        {
            if (data == null)
            {
                size = newSize;
                return;
            }

            var temp = new byte[size];
            System.Buffer.BlockCopy(data, 0, temp, 0, temp.Length);

            data = new byte[newSize];

            System.Buffer.BlockCopy(temp, 0, data, 0, Math.Min(temp.Length, data.Length));

            temp = null;
        }

        public uint Size => size;

        unsafe public byte* GetPointer()
        {
            return start;
        }

        public void CopyTo(BufferStream stream)
        {
            if (stream.data == null)
            {
                // TODO: should we add data from base stream before the copy action? is there a use case?

                stream.data = new byte[size];
                stream.size = size;

                fixed (byte* pointer = stream.data)
                {
                    stream.start = pointer;
                }
            }
            else
                stream.size += size;

            // Note: If stream.data already exists we might want to check for size exceeding here.
            // But as the Buffer class handles this, we omit it here.

            if (data != null)
                System.Buffer.BlockCopy(data, 0, stream.data, 0, (int)size);
            else
            {
                byte* current = start;
                byte* end = start + size;
                byte* streamCurrent = stream.start;

                while (current != end)
                    *streamCurrent++ = *current++;
            }
        }

        public void CopyTo(Stream stream)
        {
            byte[] buffer = new byte[size];

            if (data != null)
                System.Buffer.BlockCopy(data, 0, buffer, 0, (int)size);
            else
            {
                byte* current = start;
                byte* end = start + size;

                fixed (byte* bufPointer = buffer)
                {
                    byte* dest = bufPointer;

                    while (current != end)
                        *dest++ = *current++;
                }
            }

            stream.Write(buffer, 0, buffer.Length);
        }
    }

    unsafe public class Buffer : IDisposable
    {
        protected BufferStream data = null;
        protected bool owned;
        protected Buffer parent;
        protected byte* read;
        protected Endianess endianess;

        static Endianess ChooseEndianess(Endianess endianess)
        {
            if (endianess == Endianess.Default)
                return Endian.HostEndianess;

            return endianess;
        }

        public static Buffer CreateFromValues<T>(T value, uint count, Endianess endianess = Endianess.Default)
        {
            var buffer = new Buffer(TypeSize<T>.Size * count, endianess);

            buffer.data = BufferStream.CreateFromValues(value, count);

            return buffer;
        }

        protected Buffer(uint capacity, Endianess endianess = Endianess.Default)
        {
            if (capacity > 0)
            {
                data = new BufferStream(capacity);
                read = data.GetPointer();
            }

            owned = true;
            this.endianess = ChooseEndianess(endianess);            
        }

        public Buffer(Endianess endianess = Endianess.Default)
        {
            owned = true;
            this.endianess = ChooseEndianess(endianess);
        }

        public Buffer(byte* data, uint size, Endianess endianess = Endianess.Default)
        {
            this.data = new BufferStream(data, size);
            owned = false;
            this.endianess = ChooseEndianess(endianess);
            read = this.data.GetPointer();
        }

        public Buffer(byte[] data, Endianess endianess = Endianess.Default)
        {
            this.data = new BufferStream(data);
            owned = true;
            this.endianess = ChooseEndianess(endianess);
            read = this.data.GetPointer();
        }

        public Buffer(ushort[] data, Endianess endianess = Endianess.Default)
        {
            this.data = new BufferStream(data);
            owned = true;
            this.endianess = ChooseEndianess(endianess);
            read = this.data.GetPointer();
        }

        public Buffer(Buffer parent, uint start, uint length)
        {
            data = new BufferStream(parent.data, start, length);
            owned = false;
            this.endianess = ChooseEndianess(parent.endianess);
            read = this.data.GetPointer();
        }

        public Buffer(Buffer parent, uint start, uint length, Endianess endianess)
        {
            data = new BufferStream(parent.data, start, length);
            owned = false;
            this.endianess = ChooseEndianess(endianess);
            read = this.data.GetPointer();
        }

        public Buffer(string path, Endianess endianess = Endianess.Default)
        {
            owned = true;
            this.endianess = ChooseEndianess(endianess);

            Stream fileStream;

            try
            {
                fileStream = File.OpenRead(path);
            }
            catch
            {
                throw new ExceptionFreeserf($"Failed to open file '{path}'");
            }

            byte[] data = new byte[fileStream.Length];

            fileStream.Read(data, 0, data.Length);

            this.data = new BufferStream(data);

            fileStream.Close();

            read = this.data.GetPointer();
        }

        public uint Size => (data == null) ? 0 : data.Size;
        public byte* Data => (data == null) ? null : data.GetPointer();

        public virtual byte* Unfix()
        {
            byte* result = data.GetPointer();

            data = null;

            return result;
        }

        public void SetEndianess(Endianess endianess)
        {
            this.endianess = endianess;
        }

        public bool Readable()
        {
            return (read - Data) != Size;
        }

        public Buffer Pop(uint size)
        {
            uint offset = (uint)(read - Data);

            if (offset + size > Size)
                throw new IndexOutOfRangeException("Given size exceeds buffer size.");

            var subBuffer = new Buffer(this, offset, size, endianess);

            read += size;

            return subBuffer;
        } 

        public Buffer PopTail()
        {
            uint offset = (uint)(read - Data);

            return Pop(Size - offset);
        }

        object PopInternal(int size)
        {
            uint offset = (uint)(read - Data);

            if (offset + size > Size)
                throw new IndexOutOfRangeException("Read beyond buffer size.");

            object result = null;

            switch (size)
            {
                case 1:
                    result = *read;
                    break;
                case 2:
                    result = *(ushort*)read;
                    break;
                case 4:
                    result = *(uint*)read;
                    break;
                case 8:
                    result = *(ulong*)read;
                    break;
                default:
                    throw new NotSupportedException("Invalid type size.");
            }

            read += size;

            return result;
        }

        public T Pop<T>() where T : struct
        {
            T value = (T)Convert.ChangeType(PopInternal(TypeSize<T>.Size), typeof(T));

            return (endianess == Endianess.Big) ? Endian.Betoh(value) : Endian.Letoh(value);
        }

        public Buffer GetSubBuffer(uint offset, uint size)
        {
            return new Buffer(this, offset, size, endianess);
        }

        public Buffer GetTail(uint offset)
        {
            return GetSubBuffer(offset, Size - offset);
        }

        protected void CopyTo(Buffer other)
        {
            if (data == null)
                throw new ExceptionFreeserf("Tried to copy an empty buffer.");

            if (other.data == null)
                other.data = new BufferStream(Size);

            data.CopyTo(other.data);
        }

        protected void CopyFrom(Buffer other)
        {
            other.CopyTo(this);
        }

        public bool Write(string path)
        {
            Stream fileStream;

            try
            {
                fileStream = File.Create(path);
            }
            catch
            {
                return false;
            }

            try
            {
                if (!fileStream.CanWrite)
                    throw new ExceptionFreeserf($"Unablte to write to readonly file '{path}'");

                data.CopyTo(fileStream);
            }
            finally
            {
                fileStream.Close();
            }

            return true;
        }

        protected byte* Offset(int offset)
        {
            return Data + offset;
        }


        #region IDisposable Support

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    data = null;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }

    unsafe public class MutableBuffer : Buffer
    {
        protected uint reserved;
        protected uint growth = 1000u;

        public MutableBuffer(Endianess endianess = Endianess.Default)
            : base(endianess)
        {

        }

        public MutableBuffer(uint reserved, Endianess endianess = Endianess.Default)
            : base(reserved, endianess)
        {
            this.reserved = reserved;
        }

        public override unsafe byte* Unfix()
        {
            reserved = 0;

            return base.Unfix();
        }

        public void Push(Buffer buffer)
        {
            uint size = buffer.Size;

            CheckSize(Size + size);

            CopyFrom(buffer);
        }

        public void Push(byte* data, uint size)
        {
            Push(new Buffer(data, size, endianess));
        }

        public void Push(string str)
        {
            var bytes = Settings.Encoding.GetBytes(str);

            Push(new Buffer(bytes, endianess));
        }

        public void Push(char* str)
        {
            Push(new string(str));
        }

        public void Push<T>(T value, uint count = 1)
        {
            CheckSize(Size + (TypeSize<T>.Size * count));

            Push(CreateFromValues(value, count, endianess));
        }

        protected void CheckSize(uint size)
        {
            if (this.reserved >= size)
            {
                return;
            }

            uint reserved = Math.Max(Size + growth, size);

            if (data == null)
            {
                data = new BufferStream(reserved);
                owned = true;
            }
            else
            {
                data.Realloc(reserved);
            }

            this.reserved = reserved;
        }
    }
}