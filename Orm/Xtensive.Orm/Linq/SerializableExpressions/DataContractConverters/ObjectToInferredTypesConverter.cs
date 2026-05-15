using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xtensive.Reflection;

namespace Xtensive.Linq.SerializableExpressions.DataContractConverters
{
  internal class ObjectToInferredTypesConverter
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
      TypeToSerializableTypeMap[WellKnownTypes.DateOnly] = WellKnownTypes.String;
      TypeToSerializableTypeMap[WellKnownTypes.DateTime] = WellKnownTypes.String;
      TypeToSerializableTypeMap[WellKnownTypes.DateTimeOffset] = WellKnownTypes.String;
      TypeToSerializableTypeMap[WellKnownTypes.NullableDateOnly] = WellKnownTypes.String;
      TypeToSerializableTypeMap[WellKnownTypes.NullableDateTime] = WellKnownTypes.String;
      TypeToSerializableTypeMap[WellKnownTypes.NullableDateTimeOffset] = WellKnownTypes.String;
      TypeToSerializableTypeMap[typeof(Int128)] = WellKnownTypes.String;
      TypeToSerializableTypeMap[typeof(UInt128)] = WellKnownTypes.String;
      TypeToSerializableTypeMap[typeof(Int128?)] = WellKnownTypes.String;
      TypeToSerializableTypeMap[typeof(UInt128?)] = WellKnownTypes.String;
      TypeToSerializableTypeMap[WellKnownTypes.Guid] = WellKnownTypes.String;
      TypeToSerializableTypeMap[WellKnownTypes.NullableGuid] = WellKnownTypes.String;

      TypeToSerializableTypeMap[WellKnownTypes.TimeOnly] = WellKnownTypes.Int64;
      TypeToSerializableTypeMap[WellKnownTypes.TimeSpan] = WellKnownTypes.Int64;

      TypeToSerializableTypeMap[WellKnownTypes.NullableTimeOnly] = WellKnownTypes.NullableInt64;
      TypeToSerializableTypeMap[WellKnownTypes.NullableTimeSpan] = WellKnownTypes.NullableInt64;



      ValueWriters[WellKnownTypes.DateOnly] = static (tFromValue) => {
        return tFromValue is DateOnly d
          ? (object) d.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.Zero)).ToString("O")
          : throw new InvalidCastException();
      };

      ValueWriters[WellKnownTypes.TimeOnly] = static (fromValue) => fromValue is TimeOnly t ? (object) t.Ticks : throw new InvalidCastException();
      ValueWriters[WellKnownTypes.TimeSpan] = static (fromValue) => fromValue is TimeSpan ts ? (object) ts.Ticks : throw new InvalidCastException();

      ValueWriters[WellKnownTypes.DateTime] = static (fromValue) => fromValue is DateTime dt ? (object) dt.ToString("O") : throw new InvalidCastException();
      ValueWriters[WellKnownTypes.DateTimeOffset] = static (fromValue) => fromValue is DateTimeOffset dto ? (object) dto.ToString("O") : throw new InvalidCastException();

      Func<object, object> toStringConverter = static (fromValue) => fromValue.ToString();
      ValueWriters[typeof(Int128)] = toStringConverter;
      ValueWriters[typeof(UInt128)] = toStringConverter;
      ValueWriters[WellKnownTypes.Guid] = toStringConverter;


      ValueReaders[WellKnownTypes.DateOnly] = static (fromValue) => DateOnly.FromDateTime(DateTime.Parse((string) fromValue));
      ValueReaders[WellKnownTypes.TimeOnly] = static (fromValue) => TimeOnly.FromTimeSpan(TimeSpan.FromTicks((long) fromValue));
      ValueReaders[WellKnownTypes.TimeSpan] = static (fromValue) => TimeSpan.FromTicks((long) fromValue);
      ValueReaders[WellKnownTypes.DateTime] = static (fromValue) => {
        var valueString = (string) fromValue;
        var parsed = DateTime.ParseExact(valueString, "O", CultureInfo.InvariantCulture);
        if (valueString.EndsWith("Z", StringComparison.OrdinalIgnoreCase)) {
          // parsing does not take into account "Z" marker (marks utc kind when ToString("O")),
          parsed = parsed.ToUniversalTime();
        }
        return parsed;
      };

      ValueReaders[WellKnownTypes.DateTimeOffset] = static (fromValue) => DateTimeOffset.ParseExact((string) fromValue, "O", CultureInfo.InvariantCulture);
      ValueReaders[typeof(Int128)] = static (fromValue) => Int128.Parse((string) fromValue);
      ValueReaders[typeof(UInt128)] = static (fromValue) => UInt128.Parse((string) fromValue);
      ValueReaders[WellKnownTypes.Guid] = static (fromValue) => Guid.Parse((string) fromValue);

      EnumReader = static (fromValue, enumType) => Enum.ToObject(enumType, Convert.ChangeType(fromValue, Enum.GetUnderlyingType(enumType)));
      PrimitiveTypesReader = static (fromValue, primitiveType) => Convert.ChangeType(fromValue, primitiveType);

    }
  }
}
