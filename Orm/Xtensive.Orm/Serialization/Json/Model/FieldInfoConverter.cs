// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xtensive.Orm;
using Xtensive.Orm.Model;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Serialization.Json.Model
{
  /// <summary>
  /// Json converter for <see cref="Xtensive.Orm.Model.FieldInfo"/>.
  /// </summary>
  /// <param name="domain">Domain, required for deserialization.</param>
  public sealed class FieldInfoConverter(Domain domain) : JsonConverter<FieldInfo>
  {
    private readonly Domain domain = domain;

    /// <inheritdoc/>
    public override FieldInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      reader.EnsureStartObject();

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw JsonExceptions.WrongStructurePropertyExpected();
      var propertyName = reader.GetString();
      if (propertyName != options.ApplyNamingPolicy(nameof(FieldInfo.ReflectedType)))
        throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(FieldInfo.ReflectedType)), propertyName);

      _ = reader.Read();
      var reflecteType = JsonSerializer.Deserialize<TypeInfo>(ref reader, options);
      if (reflecteType is null)
        throw new JsonException("Reflected type of the field is null.");
      if (!domain.Model.Types.Contains(reflecteType))
        throw new JsonException("Type Owner of the field belongs to different Domain than the one that provided to the converter.");

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw JsonExceptions.WrongStructurePropertyExpected();
      propertyName = reader.GetString();
      if (propertyName != options.ApplyNamingPolicy(nameof(FieldInfo.Name)))
        throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(FieldInfo.Name)), propertyName);

      _ = reader.Read();
      var name = reader.GetString();

      _ = reader.Read();
      reader.EnsureEndObject();

      if (!reflecteType.Fields.TryGetValue(name, out var field))
        throw new JsonException("Field is not found.");

      return field;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, FieldInfo value, JsonSerializerOptions options)
    {
      writer.WriteStartObject();

      writer.WritePropertyName(nameof(FieldInfo.ReflectedType));
      JsonSerializer.Serialize<TypeInfo>(writer, value.ReflectedType, options);

      writer.WriteString(options.ApplyNamingPolicy(nameof(FieldInfo.Name)), value.Name);

      writer.WriteEndObject();
    }
  }
}
