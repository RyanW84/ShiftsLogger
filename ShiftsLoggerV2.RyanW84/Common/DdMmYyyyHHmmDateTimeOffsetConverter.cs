using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShiftsLoggerV2.RyanW84.Common;

public class DdMmYyyyHHmmDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
	private const string Format = "dd/MM/yyyy HH:mm";

	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		if (string.IsNullOrWhiteSpace(value))
			throw new JsonException("Date string is null or empty.");
		if (!DateTimeOffset.TryParseExact(value, Format, null, System.Globalization.DateTimeStyles.None, out var result))
			throw new JsonException($"Invalid date format. Expected {Format}.");
		return result;
	}

	public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString(Format));
	}
}
