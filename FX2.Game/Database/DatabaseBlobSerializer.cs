// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FX2.Game.Beatmap;
using SQLite.Net;

namespace FX2.Game.Database
{
    public class DatabaseBlobSerializer : IBlobSerializer
    {
        private static readonly IFormatter Formatter = new BinaryFormatter();
        private readonly HashSet<Type> registeredTypes = new HashSet<Type>();

        public DatabaseBlobSerializer()
        {
            Add<BeatmapMetadata>();
        }

        public bool CanDeserialize(Type type)
        {
            return registeredTypes.Contains(type);
        }

        private void Add<T>() where T : new()
        {
            registeredTypes.Add(typeof(T));
        }
        
        public byte[] Serialize<T>(T obj)
        {
            using(MemoryStream stream = new MemoryStream())
            {
                Formatter.Serialize(stream, obj);
                return stream.GetBuffer();
            }
        }

        public object Deserialize(byte[] data, Type type)
        {
            using(MemoryStream stream = new MemoryStream(data))
            {
                return Formatter.Deserialize(stream);
            }
        }
    }
}