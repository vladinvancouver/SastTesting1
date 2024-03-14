using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SecurityTesting1.Common.Helpers
{
    /*
     * Source: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-6-0#deserialize-inferred-types-to-object-properties
     * 
     * When deserializing to a property of type object, a JsonElement object is created. The reason is that the deserializer doesn't know what CLR type to create,
     * and it doesn't try to guess. For example, if a JSON property has "true", the deserializer doesn't infer that the value is a Boolean, and if an element
     * has "01/01/2019", the deserializer doesn't infer that it's a DateTime.
     * 
     * Type inference can be inaccurate. If the deserializer parses a JSON number that has no decimal point as a long, that might result in out-of-range issues if
     * the value was originally serialized as a ulong or BigInteger. Parsing a number that has a decimal point as a double might lose precision if the number was
     * originally serialized as a decimal.
     *
     * For scenarios that require type inference, the following code shows a custom converter for object properties. The code converts:
     * true and false to Boolean
     * Numbers without a decimal to long
     * Numbers with a decimal to double
     * Dates to DateTime
     * Strings to string
     * Everything else to JsonElement
     */
    public class ObjectToInferredTypesConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.Number when reader.TryGetInt64(out long l) => l,
                JsonTokenType.Number => reader.GetDouble(),
                JsonTokenType.String when reader.TryGetDateTime(out DateTime datetime) => datetime,
                JsonTokenType.String => reader.GetString()!,
                _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
            };

        public override void Write(
            Utf8JsonWriter writer,
            object objectToWrite,
            JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType(), options);
    }
}
