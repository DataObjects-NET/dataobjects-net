// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xtensive.Orm;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Serialization.Json.Model
{
  /// <summary>
  /// Json converter for <see cref="Xtensive.Orm.TypeReference"/>.
  /// </summary>
  public sealed class TypeReferenceConverter : JsonConverter<TypeReference>
  {
    /// <inheritdoc/>
    public override TypeReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      reader.EnsureStartObject();

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw JsonExceptions.WrongStructurePropertyExpected();
      var propertyName = reader.GetString();
      if (propertyName != options.ApplyNamingPolicy(nameof(TypeReference.Accuracy)))
        throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(TypeReference.Accuracy)), propertyName);
      _ = reader.Read();
      var accuracyRawValue = reader.GetInt32();
      var accuracyValue = (TypeReferenceAccuracy) accuracyRawValue;

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw JsonExceptions.WrongStructurePropertyExpected();
      propertyName = reader.GetString();
      if (propertyName != options.ApplyNamingPolicy(nameof(TypeReference.Type)))
        throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(TypeReference.Type)), propertyName);

      _ = reader.Read();
      var typeInfo = JsonSerializer.Deserialize<TypeInfo>(ref reader, options);

      _ = reader.Read();
      reader.EnsureEndObject();

      return new TypeReference(typeInfo, accuracyValue);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TypeReference objectToWrite, JsonSerializerOptions options)
    {
      writer.WriteStartObject();
      writer.WriteNumber(options.ApplyNamingPolicy(nameof(TypeReference.Accuracy)), (int) objectToWrite.Accuracy);
      var type = objectToWrite.Type.UnderlyingType;

      writer.WritePropertyName(options.ApplyNamingPolicy(nameof(TypeReference.Type)));
      JsonSerializer.Serialize<TypeInfo>(writer, objectToWrite.Type, options);

      writer.WriteEndObject();
    }
  }
}
