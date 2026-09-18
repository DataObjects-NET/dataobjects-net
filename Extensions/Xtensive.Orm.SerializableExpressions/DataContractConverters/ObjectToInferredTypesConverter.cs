using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xtensive.Reflection;

namespace Xtensive.Orm.SerializableExpressions.DataContractConverters
{
  internal sealed class ObjectToInferredTypesConverter
  {
    public static readonly Lazy<ObjectToInferredTypesConverter> Default = new(() => new ObjectToInferredTypesConverter());

    private readonly Dictionary<Type, Type> TypeToSerializableTypeMap = new();

    private readonly Dictionary<Type, Func<object, object>> ValueWriters = new();
    private readonly Dictionary<Type, Func<object, object>> ValueReaders = new();
    private readonly Func<object, Type, object> EnumReader;
    private readonly Func<object, Type, object> PrimitiveTypesReader;

    public object ToSerializable(object value, Type type)
    {
      if (type.IsValueType) {
        if (!IsNullable(type, out var underlyingType)) {
          return ValueWriters.TryGetValue(type, out var converter) ? converter(value) : value;
        }
        if (value is null)
          return null;
        else {
          return ToSerializable(value, underlyingType);
        }
      }
      else if (type.IsArray) {
        var elementType = type.GetElementType();

        if (!TypeToSerializableTypeMap.TryGetValue(elementType, out var newType)) {
          return value;
        }

        var array = value as Array;
        var newArray = Array.CreateInstance(newType, array.Length);
        for (var i = 0; i < array.Length; i++) {
          var origItem = array.GetValue(i);
          if (origItem != null) {
            var newItem = ToSerializable(origItem, elementType);
            newArray.SetValue(newItem, i);
          }
          else {
            newArray.SetValue(null, i);
          }
        }

        return newArray;
      }
      else {
        return value;
      }
    }

    public object FromSerializable(object value, Type type)
    {
      if (IsNullable(type, out var underlyingType)) {
        if (value is null) {
          return null;
        }
        else {
          var deserialized = FromSerializable(value, underlyingType);
          var chagedType = Convert.ChangeType(deserialized, underlyingType);

          return Activator.CreateInstance(type, chagedType);
        }
      }

      if (type.IsArray) {
        var array = (Array) value;
        var elementType = type.GetElementType();
        var correctArray = Array.CreateInstance(elementType, array.Length);
        for (int i = 0; i < array.Length; i++) {
          var origValue = array.GetValue(i);
          if (origValue is null)
            correctArray.SetValue(null, i);
          else {
            correctArray.SetValue(FromSerializable(origValue, elementType), i);
          }
        }
        return correctArray;
      }
      if(type.IsEnum)
        return EnumReader(value, type);
      if(type.IsPrimitive)
        return PrimitiveTypesReader(value, type);
      if (ValueReaders.TryGetValue(type, out var converter)) {
        return converter(value);
      }
      return value;
    }

    private static bool IsNullable(Type type, out Type underlyingType)
    {
      if (type.IsNullable()) {
        underlyingType = Nullable.GetUnderlyingType(type);
        return true;
      }

      underlyingType = null;
      return false;
    }

    public ObjectToInferredTypesConverter()
    {
      var stringType = typeof(string);
      var int64Type = typeof(long);
      var nullableInt64Type = typeof(long?);

      var dateTimeOffsetType = typeof(DateTimeOffset);
      var dateTimeType = typeof(DateTime);
      var dateOnlyType = typeof(DateOnly);
      var timeOnlyType = typeof(TimeOnly);
      var timeSpanType = typeof(TimeSpan);
      var guidType = typeof(Guid);

      var nullableDateTimeOffsetType = typeof(DateTime?);
      var nullableDateTimeType = typeof(DateTime?);
      var nullableDateOnlyType = typeof(DateOnly?);
      var nullableTimeOnlyType = typeof(TimeOnly?);
      var nullableTimeSpanType = typeof(TimeSpan?);
      var nullableGuidType = typeof(Guid?);


      // Convertsion map
      TypeToSerializableTypeMap[dateOnlyType] = stringType;
      TypeToSerializableTypeMap[dateTimeType] = stringType;
      TypeToSerializableTypeMap[dateTimeOffsetType] = stringType;
      TypeToSerializableTypeMap[nullableDateOnlyType] = stringType;
      TypeToSerializableTypeMap[nullableDateTimeType] = stringType;
      TypeToSerializableTypeMap[nullableDateTimeOffsetType] = stringType;
      TypeToSerializableTypeMap[typeof(Int128)] = stringType;
      TypeToSerializableTypeMap[typeof(UInt128)] = stringType;
      TypeToSerializableTypeMap[typeof(Int128?)] = stringType;
      TypeToSerializableTypeMap[typeof(UInt128?)] = stringType;
      TypeToSerializableTypeMap[guidType] = stringType;
      TypeToSerializableTypeMap[nullableGuidType] = stringType;

      TypeToSerializableTypeMap[timeOnlyType] = int64Type;
      TypeToSerializableTypeMap[timeSpanType] = int64Type;

      TypeToSerializableTypeMap[nullableTimeOnlyType] = nullableInt64Type;
      TypeToSerializableTypeMap[nullableTimeSpanType] = nullableInt64Type;


      // Writers
      ValueWriters[dateOnlyType] = static (tFromValue) => {
        return tFromValue is DateOnly d
          ? (object) d.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.Zero)).ToString("O")
          : throw new InvalidCastException();
      };

      ValueWriters[timeOnlyType] = static (fromValue) => fromValue is TimeOnly t ? (object) t.Ticks : throw new InvalidCastException();
      ValueWriters[timeSpanType] = static (fromValue) => fromValue is TimeSpan ts ? (object) ts.Ticks : throw new InvalidCastException();

      ValueWriters[dateTimeType] = static (fromValue) => fromValue is DateTime dt ? (object) dt.ToString("O") : throw new InvalidCastException();
      ValueWriters[dateTimeOffsetType] = static (fromValue) => fromValue is DateTimeOffset dto ? (object) dto.ToString("O") : throw new InvalidCastException();

      Func<object, object> toStringConverter = static (fromValue) => fromValue.ToString();
      ValueWriters[typeof(Int128)] = toStringConverter;
      ValueWriters[typeof(UInt128)] = toStringConverter;
      ValueWriters[guidType] = toStringConverter;


      // Readers
      ValueReaders[dateOnlyType] = static (fromValue) => DateOnly.FromDateTime(DateTime.Parse((string) fromValue));
      ValueReaders[timeOnlyType] = static (fromValue) => TimeOnly.FromTimeSpan(TimeSpan.FromTicks((long) fromValue));
      ValueReaders[timeSpanType] = static (fromValue) => TimeSpan.FromTicks((long) fromValue);
      ValueReaders[dateTimeType] = static (fromValue) => {
        var valueString = (string) fromValue;
        var parsed = DateTime.ParseExact(valueString, "O", CultureInfo.InvariantCulture);
        if (valueString.EndsWith("Z", StringComparison.OrdinalIgnoreCase)) {
          // parsing does not take into account "Z" marker (marks utc kind when ToString("O")),
          parsed = parsed.ToUniversalTime();
        }
        return parsed;
      };

      ValueReaders[dateTimeOffsetType] = static (fromValue) => DateTimeOffset.ParseExact((string) fromValue, "O", CultureInfo.InvariantCulture);
      ValueReaders[typeof(Int128)] = static (fromValue) => Int128.Parse((string) fromValue);
      ValueReaders[typeof(UInt128)] = static (fromValue) => UInt128.Parse((string) fromValue);
      ValueReaders[guidType] = static (fromValue) => Guid.Parse((string) fromValue);

      EnumReader = static (fromValue, enumType) => Enum.ToObject(enumType, Convert.ChangeType(fromValue, Enum.GetUnderlyingType(enumType)));
      PrimitiveTypesReader = static (fromValue, primitiveType) => Convert.ChangeType(fromValue, primitiveType);
    }
  }
}
