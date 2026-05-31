// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xtensive.Orm.Internals;
using Xtensive.Reflection;
using Xtensive.Tuples;

namespace Xtensive.Serialization.Json.Model
{
  /// <summary>
  /// Json converter factory for some of <see cref="Xtensive.Tuples.Tuple"/>s.
  /// </summary>
  public class TupleConverterFactory : JsonConverterFactory
  {
    #region Nested Types - Converters & containers

    [Serializable]
    private sealed class TypedTupleReference
    {
      [JsonInclude]
      public string TypeName { get; set; }

      [JsonInclude]
      public Tuples.Tuple Tuple { get; set; }
    }

    private sealed class PackedTupleConverter : JsonConverter<Tuples.Packed.PackedTuple>
    {
      /// <inheritdoc/>
      public override Tuples.Packed.PackedTuple Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        reader.EnsureStartObject();

        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();
        var propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(nameof(Tuples.Packed.PackedTuple.Descriptor)))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(Tuples.Packed.PackedTuple.Descriptor)), propertyName);
        _ = reader.Read();
        var descriptor = JsonSerializer.Deserialize<TupleDescriptor>(ref reader, options);

        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();
        propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(nameof(Tuples.Packed.PackedTuple.Values)))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(Tuples.Packed.PackedTuple.Values)), propertyName);
        _ = reader.Read();
        var values = JsonSerializer.Deserialize<long[]>(ref reader, options);

        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();
        propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(nameof(Tuples.Packed.PackedTuple.Objects)))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(Tuples.Packed.PackedTuple.Objects)), propertyName);
        _ = reader.Read();
        var objects = JsonSerializer.Deserialize<object[]>(ref reader, options);

        _ = reader.Read();
        reader.EnsureEndObject();

        var ctor = PackedTupleType.GetConstructor(new[] { typeof(TupleDescriptor).MakeByRefType() });
        var tuple = (Tuples.Packed.PackedTuple) ctor.Invoke(new object[] { descriptor });
        ValuesAccessor(tuple, values);
        ObjectsAccessor(tuple, objects);
        return tuple;
      }

      /// <inheritdoc/>
      public override void Write(Utf8JsonWriter writer, Tuples.Packed.PackedTuple value, JsonSerializerOptions options)
      {

        writer.WriteStartObject();

        writer.WritePropertyName(options.ApplyNamingPolicy(nameof(Tuples.Packed.PackedTuple.Descriptor)));
        JsonSerializer.Serialize<TupleDescriptor>(writer, value.Descriptor, options);

        writer.WritePropertyName(options.ApplyNamingPolicy(nameof(Tuples.Packed.PackedTuple.Values)));
        JsonSerializer.Serialize(writer, value.Values, typeof(long[]), options);

        writer.WritePropertyName(options.ApplyNamingPolicy(nameof(Tuples.Packed.PackedTuple.Objects)));
        JsonSerializer.Serialize(writer, value.Objects, typeof(object[]), options);

        writer.WriteEndObject();
      }

      #region Internal members accessors

      private static void ValuesAccessor(Tuples.Packed.PackedTuple tuple, long[] values)
      {
        var valuesField = PackedTupleType.GetField(nameof(Tuples.Packed.PackedTuple.Values), BindingFlags.Instance | BindingFlags.Public);
        valuesField.SetValue(tuple, values);
      }

      private static void ObjectsAccessor(Tuples.Packed.PackedTuple tuple, object[] objects)
      {
        var objectsField = PackedTupleType.GetField(nameof(Tuples.Packed.PackedTuple.Objects), BindingFlags.Instance | BindingFlags.Public);
        objectsField.SetValue(tuple, objects);
      }
      #endregion

    }

    private sealed class DifferentialTupleConverter : JsonConverter<Tuples.DifferentialTuple>
    {
      private const string TupleTypePropSuffix = "TupleType";

      /// <inheritdoc/>
      public override DifferentialTuple Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        reader.EnsureStartObject();

        // read origin true type
        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();
        var propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(nameof(DifferentialTuple.Origin) + TupleTypePropSuffix))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(DifferentialTuple.Origin) + TupleTypePropSuffix), propertyName);
        _ = reader.Read();
        var originTypeName = reader.GetString();
        var originType = Type.GetType(originTypeName);
        if (originType is null)
          throw JsonExceptions.NoTypeForName(originTypeName);

        // read tuple itself
        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();
        propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(nameof(DifferentialTuple.Origin)))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(DifferentialTuple.Origin)), propertyName);
        _ = reader.Read();
        var origin = (Tuples.Tuple) JsonSerializer.Deserialize(ref reader, originType, options);

        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();
        propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(nameof(DifferentialTuple.Difference) + TupleTypePropSuffix))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(DifferentialTuple.Difference) + TupleTypePropSuffix), propertyName);
        _ = reader.Read();

        Tuples.Tuple difference = null;
        if (reader.TokenType is not JsonTokenType.Null) {
          var differenceTypeName = reader.GetString();
          var differenceType = Type.GetType(differenceTypeName);
          if (differenceType is null)
            throw JsonExceptions.NoTypeForName(differenceTypeName);

          _ = reader.Read();
          propertyName = reader.GetString();
          if (propertyName != options.ApplyNamingPolicy(nameof(DifferentialTuple.Difference)))
            throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(DifferentialTuple.Difference)), propertyName);
          _ = reader.Read();
          difference = (Tuples.Tuple) JsonSerializer.Deserialize(ref reader, differenceType, options);
        }

        // no reading of backup

        _ = reader.Read();
        reader.EnsureEndObject();

        return (difference is null)
          ? new DifferentialTuple(origin)
          : new DifferentialTuple(origin, difference);
      }

      /// <inheritdoc/>
      public override void Write(Utf8JsonWriter writer, DifferentialTuple value, JsonSerializerOptions options)
      {
        writer.WriteStartObject();

        // don't store backup, at least for now, but we could
        var originType = value.Origin.GetType();
        writer.WriteString(options.ApplyNamingPolicy(nameof(DifferentialTuple.Origin) + TupleTypePropSuffix), originType.AssemblyQualifiedName);
        writer.WritePropertyName(options.ApplyNamingPolicy(nameof(DifferentialTuple.Origin)));
        JsonSerializer.Serialize(writer, value.Origin, originType, options);

        if (value.Difference != null) {
          var differenceType = value.Difference.GetType();
          writer.WriteString(options.ApplyNamingPolicy(nameof(DifferentialTuple.Difference) + TupleTypePropSuffix), differenceType.AssemblyQualifiedName);
          writer.WritePropertyName(options.ApplyNamingPolicy(nameof(DifferentialTuple.Difference)));
          JsonSerializer.Serialize(writer, value.Difference, differenceType, options);
        }
        else {
          writer.WritePropertyName(options.ApplyNamingPolicy(nameof(DifferentialTuple.Difference) + TupleTypePropSuffix));
          writer.WriteNullValue();
        }

        writer.WriteEndObject();
      }
    }

    private sealed class FastReadOnlyTupleConverter : JsonConverter<Tuples.FastReadOnlyTuple>
    {
      private const string IntertalStatesPropertyName = "_states";
      private const string InternalValuesPropertyName = "_values";

      /// <inheritdoc/>
      public override FastReadOnlyTuple Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        reader.EnsureStartObject();

        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();
        var propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(nameof(FastReadOnlyTuple.Descriptor)))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(FastReadOnlyTuple.Descriptor)), propertyName);
        _ = reader.Read();
        var descriptor = JsonSerializer.Deserialize<TupleDescriptor>(ref reader, options);

        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();
        propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(InternalValuesPropertyName))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(InternalValuesPropertyName), propertyName);
        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.StartArray)
          throw JsonExceptions.WrongStructureArrayStartExpected();

        var objects = new List<object>(descriptor.Count);
        var objectCoverter = (JsonConverter<object>) options.GetConverter(typeof(object));
        var i = 0;
        while (reader.Read()) {
          if (reader.TokenType == JsonTokenType.EndArray) {
            break;
          }
          objects.Add(JsonSerializer.Deserialize(ref reader, descriptor[i++], options));
        }

        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();
        propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(IntertalStatesPropertyName))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(IntertalStatesPropertyName), propertyName);
        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.StartArray)
          throw JsonExceptions.WrongStructureArrayStartExpected();

        var states = new List<TupleFieldState>(descriptor.Count);
        while (reader.Read()) {
          if (reader.TokenType == JsonTokenType.EndArray) {
            break;
          }
          states.Add((TupleFieldState) JsonSerializer.Deserialize<int>(ref reader, options));
        }

        _ = reader.Read();
        reader.EnsureEndObject();

        // the only incoming parameter is tuple, and it is the source of data, FastReadOnlyTuple does not cache incoming tuple
        var dummyTuple = Tuples.Tuple.Create(descriptor);
        for (int j = 0, count = objects.Count; j < count; j++) {
          dummyTuple.SetValue(j, objects[j]);
          dummyTuple.SetFieldState(j, states[j]);
        }
        return new FastReadOnlyTuple(dummyTuple);
      }

      /// <inheritdoc/>
      public override void Write(Utf8JsonWriter writer, FastReadOnlyTuple value, JsonSerializerOptions options)
      {
        writer.WriteStartObject();

        writer.WritePropertyName(options.ApplyNamingPolicy(nameof(FastReadOnlyTuple.Descriptor)));
        JsonSerializer.Serialize<TupleDescriptor>(writer, value.Descriptor, options);

        writer.WritePropertyName(options.ApplyNamingPolicy(InternalValuesPropertyName));
        writer.WriteStartArray();

        var states = new TupleFieldState[value.Count];
        for (var i = 0; i < value.Count; i++) {
          var fValue = value.GetValue(i, out var state);
          states[i] = state;
          JsonSerializer.Serialize(writer, fValue, value.Descriptor[i], options);
        }
        writer.WriteEndArray();

        writer.WritePropertyName(options.ApplyNamingPolicy(IntertalStatesPropertyName));
        writer.WriteStartArray();
        for (var i = 0; i < states.Length; i++) {
          writer.WriteNumberValue((int) states[i]);
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
      }
    }

    #endregion

    private static readonly Type RegularTupleType = typeof(Tuples.RegularTuple);
    private static readonly Type PackedTupleType = typeof(Tuples.Packed.PackedTuple);
    private static readonly Type DifferentialTupleType = typeof(Tuples.DifferentialTuple);
    private static readonly Type FastReadOnlyTupleType = typeof(Tuples.FastReadOnlyTuple);
   
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) =>
      typeToConvert.IsAssignableTo(WellKnownOrmTypes.Tuple);

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (typeToConvert.IsAssignableTo(DifferentialTupleType)) return new DifferentialTupleConverter();
      if (typeToConvert.IsAssignableTo(FastReadOnlyTupleType)) return new FastReadOnlyTupleConverter();
      if (typeToConvert.IsAssignableTo(PackedTupleType) 
        || typeToConvert.IsAssignableFrom(RegularTupleType)) return new PackedTupleConverter();

      throw new NotSupportedException($"There is no converter for Tuple descendant {typeToConvert.GetFullName()}");
    }
  }
}
