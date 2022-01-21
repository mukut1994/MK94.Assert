using System;
using System.Text.Json;

namespace MK94.Assert
{
    public static class JsonSerializerHelper
    {
        public static ImplementedJsonSerializer ImplementedJsonSerializer { get; set; } = ImplementedJsonSerializer.SystemTextJson;
        
        public static string Serialize<TValue>(TValue value, JsonSerializerOptions options = default) 
        {
            switch (ImplementedJsonSerializer)
            {
                case ImplementedJsonSerializer.SystemTextJson:
                    return JsonSerializer.Serialize(value, options);
                default:
                    throw new NotImplementedException("The specified JSON Serializer is not implemented yet");
            }
        }

        public static T Deserialize<T>(string value, JsonSerializerOptions options = default)
        {
            switch (ImplementedJsonSerializer)
            {
                case ImplementedJsonSerializer.SystemTextJson:
                    return JsonSerializer.Deserialize<T>(value, options);
                default:
                    throw new NotImplementedException("The specified JSON Serializer is not implemented yet");
            }
        }
    }

    public enum ImplementedJsonSerializer
    {
        SystemTextJson = 0,
    }
}