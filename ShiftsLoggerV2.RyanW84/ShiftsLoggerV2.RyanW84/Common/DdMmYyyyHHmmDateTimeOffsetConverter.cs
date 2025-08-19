using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShiftsLoggerV2.RyanW84.Common
{
    // Custom DateTimeOffset converter for dd-MM-yyyy HH:mm
    public class DdMmYyyyHHmmDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        private const string Format = "dd-MM-yyyy HH:mm";
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (DateTimeOffset.TryParseExact(str, Format, null, System.Globalization.DateTimeStyles.None, out var dto))
                return dto;
            throw new JsonException($"Invalid date format. Expected {Format}.");
        }
        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }
}
