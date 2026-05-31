// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xtensive.Tuples;

namespace Xtensive.Serialization.Json.Model
{
  /// <summary>
  /// Json converter for <see cref="Xtensive.Tuples.TupleDescriptor"/>.
  /// </summary>
  public class TupleDescriptorConverter : JsonConverter<TupleDescriptor>
  {
    private const string FieldTypesPropertyName = "FieldTypes";

    /// <inheritdoc/>
    public override TupleDescriptor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      reader.EnsureStartObject();

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw JsonExceptions.WrongStructurePropertyExpected();
      var propertyName = reader.GetString();
      if (propertyName != options.ApplyNamingPolicy(FieldTypesPropertyName))
        throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(FieldTypesPropertyName), propertyName);
      _ = reader.Read();
      var typesArray = JsonSerializer.Deserialize<string[]>(ref reader, options);

      _ = reader.Read();
      reader.EnsureEndObject();

      var types = new Type[typesArray.Length];
      for (var i = 0; i < typesArray.Length; i++) {
        types[i] = Type.GetType(typesArray[i]);
      }
      return TupleDescriptor.Create(types);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TupleDescriptor objectToWrite, JsonSerializerOptions options)
    {
      writer.WriteStartObject();

      var typeNames = new string[objectToWrite.Count];
      for (var i = 0; i < typeNames.Length; i++)
        typeNames[i] = objectToWrite[i].AssemblyQualifiedName;

      writer.WritePropertyName(options.ApplyNamingPolicy(FieldTypesPropertyName));
      JsonSerializer.Serialize<string[]>(writer, typeNames);

      writer.WriteEndObject();
    }
  }
}
