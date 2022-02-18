using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace MK94.Assert
{
    public interface ISerializer
    {
        string Serialize<T>(T obj);

        T Deserialize<T>(Stream stream);
    }

    public class SerializeOnlyFunc : ISerializer
    {
        private readonly Func<object, string> serialize;

        public SerializeOnlyFunc(Func<object, string> serialize)
        {
            this.serialize = serialize;
        }

        public string Serialize<T>(T obj)
        {
            return serialize(obj);
        }

        public T Deserialize<T>(Stream stream)
        {
            throw new NotSupportedException($"Set a different serializer on your {nameof(DiskAsserter)} which implements {nameof(ISerializer)}");
        }
    }

    public class SystemTextJsonSerializer : ISerializer
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        public T Deserialize<T>(Stream stream)
        {
            return JsonSerializer.DeserializeAsync<T>(stream).Result;
        }

        public string Serialize<T>(T obj)
        {
            // TODO Replace(string, string) is a hacky fix; 
            // On windows Serialize(T) generates \r\n but on unix it's \n
            // This causes a hash mismatch
            // We should probably just ignore \r in the hash algo
            return JsonSerializer.Serialize(obj, jsonSerializerOptions).Replace("\r", string.Empty);
        }
    }
}
