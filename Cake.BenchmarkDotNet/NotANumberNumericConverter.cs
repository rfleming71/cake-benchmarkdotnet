using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cake.BenchmarkDotNet
{
    internal class NotANumberTypeDetails
    {
        public object NaNValue { get; }
        public Func<string, object> ParseFunc { get; }

        public NotANumberTypeDetails(
            object naNValue,
            Func<string, object> parseFunc)
        {
            NaNValue = naNValue;
            ParseFunc = parseFunc;
        }
    }

    internal class NotANumberNumericConverter : JsonConverter
    {
        internal static NotANumberNumericConverter Instance = new NotANumberNumericConverter();

        private static readonly Dictionary<Type, NotANumberTypeDetails> _notANumberTypes = new Dictionary<Type, NotANumberTypeDetails>
        {
            { typeof(float), new NotANumberTypeDetails(float.NaN, valueAsString => float.Parse(valueAsString)) },
            { typeof(double), new NotANumberTypeDetails(double.NaN, valueAsString => double.Parse(valueAsString)) },
        };

        private NotANumberNumericConverter()
        {
            // Singleton pattern.
        }

        public override bool CanConvert(Type objectType) =>
            _notANumberTypes.ContainsKey(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var notANumberTypeDetails = _notANumberTypes[objectType];

            var readerValueAsString = reader.Value as string;
            if (readerValueAsString != null)
            {
                if (string.IsNullOrWhiteSpace(readerValueAsString) ||
                    readerValueAsString.ToLower() == "nan")
                {
                    return notANumberTypeDetails.NaNValue;
                }

                return notANumberTypeDetails.ParseFunc(readerValueAsString);
            }

            return reader.Value.GetType() != objectType
                ? Convert.ChangeType(reader.Value, objectType)
                : reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotImplementedException("Serialization not supported.");
    }
}
