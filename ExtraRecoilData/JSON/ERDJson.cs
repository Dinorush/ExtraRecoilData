using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using GTFO.API.JSON.Converters;
using ExtraRecoilData.Utils;

namespace ExtraRecoilData.JSON
{
    public static class ERDJson
    {
        private static readonly JsonSerializerOptions _setting = new()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            IncludeFields = false,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            IgnoreReadOnlyProperties = true,
        };

        static ERDJson()
        {
            _setting.Converters.Add(new JsonStringEnumConverter());
            _setting.Converters.Add(new MinMaxConverter());
            if (MTFOPartialDataUtil.IsLoaded)
            {
                _setting.Converters.Add(MTFOPartialDataUtil.PersistentIDConverter);
                _setting.Converters.Add(MTFOPartialDataUtil.LocalizedTextConverter);
                ERDLogger.Log("PartialData support found!");
            }

            else
            {
                _setting.Converters.Add(new LocalizedTextConverter());
            }
        }

        public static T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _setting);
        }

        public static object? Deserialize(Type type, string json)
        {
            return JsonSerializer.Deserialize(json, type, _setting);
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, _setting);
        }
    }
}
