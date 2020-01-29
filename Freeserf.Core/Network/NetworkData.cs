﻿/*
 * NetworkData.cs - Basic network data interfaces
 *
 * Copyright (C) 2019  Robert Schneckenhaus <robert.schneckenhaus@web.de>
 *
 * This file is part of freeserf.net. freeserf.net is based on freeserf.
 *
 * freeserf.net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * freeserf.net is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with freeserf.net. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Freeserf.Network
{
    public interface IRemote
    {
        IPAddress GetIP();
        void Send(byte[] rawData);
    }

    public interface IResponse
    {
        IRemote GetSource();
        INetworkData GetData();
    }

    public enum NetworkDataType : UInt16
    {
        Request,
        Heartbeat,
        LobbyData,
        PlayerData,
        MapData,
        GameData,
        UserActionData, // setting, building, demolishing, etc
        // TODO ...
    }

    public interface INetworkData
    {
        NetworkDataType Type { get; }
        int GetSize();
        void Send(IRemote destination);
        INetworkData Parse(byte[] rawData);
    }

    public static class NetworkDataParser
    {
        public static INetworkData Parse(byte[] rawData)
        {
            if (rawData.Length < 2)
                throw new ExceptionFreeserf("Unknown network data.");

            NetworkDataType type = (NetworkDataType)BitConverter.ToUInt16(rawData, 0);

            switch (type)
            {
                case NetworkDataType.Request:
                    return new RequestData().Parse(rawData);
                case NetworkDataType.Heartbeat:
                    return new Heartbeat().Parse(rawData);
                case NetworkDataType.LobbyData:
                    return new LobbyData().Parse(rawData);
                // TODO ...
                default:
                    throw new ExceptionFreeserf("Unknown network data.");
            }
        }
    }

    public static class NetworkDataConverter<T> where T : struct
    {
        public static byte[] ToBytes(T data)
        {
            int size = Marshal.SizeOf(data);
            byte[] buffer = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, ptr, true);
            Marshal.Copy(ptr, buffer, 0, size);
            Marshal.FreeHGlobal(ptr);

            return buffer;
        }

        public static void ToBytes(T data, byte[] buffer, ref int offset)
        {
            int size = Marshal.SizeOf(data);

            if (offset + size > buffer.Length)
                throw new ExceptionFreeserf("Buffer is too small for network data.");

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, ptr, true);
            Marshal.Copy(ptr, buffer, offset, size);
            Marshal.FreeHGlobal(ptr);

            offset += size;
        }

        public static T FromBytes(byte[] data, ref int offset)
        {
            T obj = new T();

            int size = Marshal.SizeOf(obj);

            if (offset + size > data.Length)
                throw new ExceptionFreeserf("Network data is too short or invalid.");

            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(data, offset, ptr, size);

            obj = (T)Marshal.PtrToStructure(ptr, obj.GetType());

            Marshal.FreeHGlobal(ptr);

            offset += size;

            return obj;
        }
    }
}
