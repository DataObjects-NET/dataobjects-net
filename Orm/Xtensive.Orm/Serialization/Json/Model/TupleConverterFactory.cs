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
    #region Nested Types

    private class GenericTupleConverter : JsonConverter<Tuples.Tuple>
    {
      private const string TypeMarkerPropertyName = "TypeMarker";
      private const string DescriptorPropertyName = "Descriptor";
      private const string ValuesPropertyName = "Values";
      private const string DifferenceValuesPropertyName = "DifferenceValues";

      public override Tuples.Tuple Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        reader.EnsureStartObject();

        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();
        var propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(TypeMarkerPropertyName))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(TypeMarkerPropertyName), propertyName);
        _ = reader.Read();
        Type realType = GetRealType(reader.GetString());

        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();
        propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(DescriptorPropertyName))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(DescriptorPropertyName), propertyName);
        _ = reader.Read();
        var descriptor = JsonSerializer.Deserialize<TupleDescriptor>(ref reader, options);

        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();
        propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(ValuesPropertyName))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(ValuesPropertyName), propertyName);
        _ = reader.Read();
        var formattedValues = reader.GetString();

        var tuple = Tuples.Tuple.Parse(descriptor, formattedValues);
        if (realType == FastReadOnlyTupleType) {
          _ = reader.Read();
          reader.EnsureEndObject();

          return new FastReadOnlyTuple(tuple);
        }
        else if (realType == DifferentialTupleType) {
          _ = reader.Read();
          if (reader.TokenType is not JsonTokenType.PropertyName)
            throw JsonExceptions.WrongStructurePropertyExpected();
          propertyName = reader.GetString();
          if (propertyName != options.ApplyNamingPolicy(DifferenceValuesPropertyName))
            throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(DifferenceValuesPropertyName), propertyName);
          _ = reader.Read();
          if (reader.TokenType is JsonTokenType.Null) {
            _ = reader.Read();
            reader.EnsureEndObject();
            return new DifferentialTuple(tuple);
          }

          var formattedDiffValues = reader.GetString();

          _ = reader.Read();
          reader.EnsureEndObject();

          return new DifferentialTuple(tuple, Tuples.Tuple.Parse(descriptor, formattedDiffValues));
        }
        else {
          _ = reader.Read();
          reader.EnsureEndObject();
          return tuple;
        }
      }

      public override void Write(Utf8JsonWriter writer, Tuples.Tuple value, JsonSerializerOptions options)
      {
        var realType = value.GetType();

        writer.WriteStartObject();
        WriteSharedProperties(writer, value, realType, options);
        WriteSpecificProperties(writer, value, realType, options);

        writer.WriteEndObject();
      }

      private void WriteSharedProperties(Utf8JsonWriter writer, Tuples.Tuple value, Type realType, JsonSerializerOptions options)
      {
        writer.WriteString(options.ApplyNamingPolicy(TypeMarkerPropertyName), realType.Name);
        writer.WritePropertyName(options.ApplyNamingPolicy(DescriptorPropertyName));
        JsonSerializer.Serialize<TupleDescriptor>(writer, value.Descriptor, options);
      }

      private void WriteSpecificProperties(Utf8JsonWriter writer, Tuples.Tuple value, Type realType, JsonSerializerOptions options)
      {
        if (realType == DifferentialTupleType) {
          var diffTuple = value as DifferentialTuple;
          writer.WriteString(options.ApplyNamingPolicy(ValuesPropertyName), diffTuple.Origin.Format());

          if (diffTuple.Difference is null)
            writer.WriteNull(options.ApplyNamingPolicy(DifferenceValuesPropertyName));
          else
            writer.WriteString(options.ApplyNamingPolicy(DifferenceValuesPropertyName), diffTuple.Difference.Format());
        }
        else {
          writer.WriteString(options.ApplyNamingPolicy(ValuesPropertyName), value.Format());
        }
      }

      private static Type GetRealType(string typeMarker)
      {
        return typeMarker switch {
          "DifferentialTuple" => DifferentialTupleType,
          "FastReadOnlyTuple" => FastReadOnlyTupleType,
          _ => PackedTupleType
        };
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
      if (typeToConvert.IsAssignableTo(DifferentialTupleType)
        || typeToConvert.IsAssignableTo(FastReadOnlyTupleType)
        || typeToConvert.IsAssignableTo(PackedTupleType)
        || typeToConvert.IsAssignableFrom(RegularTupleType))
        return new GenericTupleConverter();

      throw new NotSupportedException($"There is no converter for Tuple descendant {typeToConvert.GetFullName()}");
    }


  }
}
