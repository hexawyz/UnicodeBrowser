using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace UnicodeBrowser.Json
{
	public class FlagsEnumConverter : JsonConverter
    {
        private struct EnumValue
        {
            public ulong IntegralValue { get; }
            public string Name { get; }

            public EnumValue(ulong integralValue, string name)
            {
                IntegralValue = integralValue;
                Name = name;
            }
        }

        private readonly ConcurrentDictionary<Type, EnumValue[]> _enumValues = new ConcurrentDictionary<Type, EnumValue[]>();

        private EnumValue[] GetEnumValues(Type enumType)
        {
            return _enumValues.GetOrAdd(enumType, GetEnumValuesCore);
        }

        private static ulong ConvertValueToUInt64(object value)
        {
            // This is quite stupid but required because Convert.ToUInt64 just can't do the work all by itself…
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return unchecked((ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture));
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Boolean:
                case TypeCode.Char:
                    return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
                default:
                    throw new InvalidOperationException();
            }
        }

        private static EnumValue[] GetEnumValuesCore(Type enumType)
        {
            var sourceValues = Enum.GetValues(enumType);
            var processedValues = new EnumValue[sourceValues.Length];

            for (int i = 0; i < sourceValues.Length; i++)
            {
                var value = sourceValues.GetValue(i);

                processedValues[i] = new EnumValue(ConvertValueToUInt64(value), ((IFormattable)value).ToString("G", CultureInfo.InvariantCulture));
            }

            Array.Sort(processedValues, (x, y) => x.IntegralValue != y.IntegralValue ? x.IntegralValue > y.IntegralValue ? 1 : -1 : 0);

            return processedValues;
        }

        public override bool CanConvert(Type objectType)
        {
            var typeInfo = (Nullable.GetUnderlyingType(objectType) ?? objectType).GetTypeInfo();

            return typeInfo.IsEnum && typeInfo.IsDefined(typeof(FlagsAttribute), false);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (Nullable.GetUnderlyingType(objectType) == null)
                {
                    throw new JsonSerializationException("Cannot convert null value to " + objectType.ToString() + ".");
                }

                return null;
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                reader.Read();

                while (true)
                {
                    if (reader.TokenType == JsonToken.String)
                    {
                    }
                    else if (reader.TokenType == JsonToken.Integer)
                    {
                    }
                    else if (reader.TokenType == JsonToken.EndArray)
                    {
                        return null;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            throw new JsonSerializationException("Unexpected token " + reader.TokenType.ToString("G") + " when parsing enum.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var enumValues = GetEnumValues(value.GetType());

                ulong integralValue = ConvertValueToUInt64(value);
                ulong integralValueRemainder = integralValue;

                writer.WriteStartArray();

                if (integralValue != 0)
                {
                    for (int i = 0; i < enumValues.Length; i++)
                    {
                        var enumValue = enumValues[i];

                        if (enumValue.IntegralValue == 0) continue;

                        if ((integralValue & enumValue.IntegralValue) == enumValue.IntegralValue)
                        {
                            integralValueRemainder = integralValueRemainder & ~enumValue.IntegralValue;
                            writer.WriteValue(enumValue.Name);
                        }
                    }

                    if (integralValueRemainder != 0)
                    {
                        if (integralValueRemainder > int.MaxValue)
                        {
                            serializer.Serialize(writer, integralValueRemainder);
                        }
                        else
                        {
                            writer.WriteValue(unchecked((int)integralValueRemainder));
                        }
                    }
                }
                else
                {
                    if (enumValues[0].IntegralValue == 0)
                    {
                        writer.WriteValue(enumValues[0].Name);
                    }
                    else
                    {
                        writer.WriteValue(0);
                    }
                }

                writer.WriteEndArray();
            }
        }
    }
}
