/*
 * DirtyMap.cs - Map implementation which tracks the dirty state
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
using System.Collections;
using System.Collections.Generic;

namespace Freeserf.Serialize
{
    internal interface IDirtyMap : IEnumerable
    {
        bool Dirty { get; }
        int Count { get; }
        event EventHandler GotDirty;
        void Initialize(Array values);
    }

    public class DirtyMap<TKey, TValue> : IDirtyMap, IEnumerable<KeyValuePair<TKey, TValue>> where TKey : IComparable where TValue : IComparable
    {
        Dictionary<TKey, TValue> map = null;

        public DirtyMap()
        {
            map = new Dictionary<TKey, TValue>();
        }

        public DirtyMap(int capacity)
        {
            map = new Dictionary<TKey, TValue>(capacity);
        }

        public TValue this[TKey key]
        {
            get => map[key];
            set
            {
                if ((map[key] == null && value != null) || value is State || map[key].CompareTo(value) != 0)
                {
                    map[key] = value;

                    if (!Dirty)
                    {
                        Dirty = true;
                        GotDirty?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public bool Dirty
        {
            get;
            set;
        } = false;

        public int Count => map.Count;

        public event EventHandler GotDirty;

        public void Add(TKey key, TValue value)
        {
            map.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return map.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return map.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return map.TryGetValue(key, out value);
        }

        public void Initialize(Array values)
        {
            map.Clear();

            foreach (var value in values)
            {
                var entry = (KeyValuePair<object, object>)value;
                map.Add((TKey)entry.Key, (TValue)entry.Value);
            }
        }

        public static implicit operator Dictionary<TKey, TValue>(DirtyMap<TKey, TValue> map)
        {
            // TODO make this readonly/a copy?
            return map.map;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}