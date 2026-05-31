// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xtensive.Orm;

namespace Xtensive.Serialization.Json.Model
{
  /// <summary>
  /// Json converter for <see cref="Xtensive.Orm.VersionInfo"/>.
  /// </summary>
  public sealed class VersionInfoConverter : JsonConverter<VersionInfo>
  {
    private const string TupleTypePropertyName = "ValueTupleType";

    /// <inheritdoc/>
    public override VersionInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      reader.EnsureStartObject();

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw JsonExceptions.WrongStructurePropertyExpected();
      var propertyName = reader.GetString();
      if (propertyName != options.ApplyNamingPolicy(nameof(VersionInfo.IsVoid)))
        throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(VersionInfo.IsVoid)), propertyName);
      _ = reader.Read();
      var isVoid = reader.GetBoolean();
      if (isVoid) {
        reader.EnsureEndObject();
        return VersionInfo.Void;
      }

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw JsonExceptions.WrongStructurePropertyExpected();
      propertyName = reader.GetString();
      if (propertyName != options.ApplyNamingPolicy(TupleTypePropertyName))
        throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(TupleTypePropertyName), propertyName);
      _ = reader.Read();
      var tupleTypeName = reader.GetString();
      var tupleType = Type.GetType(tupleTypeName);
      if (tupleType is null)
        throw JsonExceptions.NoTypeForName(tupleTypeName);

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw JsonExceptions.WrongStructurePropertyExpected();
      propertyName = reader.GetString();
      if (propertyName != options.ApplyNamingPolicy(nameof(VersionInfo.Value)))
        throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(VersionInfo.Value)), propertyName);
      _ = reader.Read();
      var tuple = (Tuples.Tuple)JsonSerializer.Deserialize(ref reader, tupleType, options);
      _ = reader.Read();
      reader.EnsureEndObject();

      return new VersionInfo(tuple);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, VersionInfo value, JsonSerializerOptions options)
    {
      writer.WriteStartObject();

      writer.WriteBoolean(options.ApplyNamingPolicy(nameof(VersionInfo.IsVoid)), value.IsVoid);

      if (!value.IsVoid) {
        var tuple = value.Value;
        writer.WriteString(options.ApplyNamingPolicy(TupleTypePropertyName), tuple.GetType().AssemblyQualifiedName);
        writer.WritePropertyName(options.ApplyNamingPolicy(nameof(VersionInfo.Value)));
        JsonSerializer.Serialize(writer, tuple, tuple.GetType(), options);
      }

      writer.WriteEndObject();
    }
  }
}
