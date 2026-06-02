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
  /// Json converter for <see cref="Xtensive.Orm.Model.TypeInfo"/>.
  /// </summary>
  /// <param name="domain">Domain, required for deserialization.</param>
  public sealed class TypeInfoConverter(Domain domain) : JsonConverter<TypeInfo>
  {
    private readonly Domain domain = domain;

    /// <inheritdoc/>
    public override TypeInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      _ = reader.Read();
      var assemblyQualifiedName = reader.GetString();
      var type = Type.GetType(assemblyQualifiedName);
      if (type is null)
        throw JsonExceptions.NoTypeForName(assemblyQualifiedName);
      var typeInfo = domain.Model.Types[type];
      if (typeInfo is null)
        throw new JsonException($"No TypeInfo with underlying type '{assemblyQualifiedName}' found in domain");

      return typeInfo;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TypeInfo objectToWrite, JsonSerializerOptions options)
    {
      writer.WriteString(options.ApplyNamingPolicy(nameof(TypeInfo.UnderlyingType)), objectToWrite.UnderlyingType.AssemblyQualifiedName);
    }
  }
}
