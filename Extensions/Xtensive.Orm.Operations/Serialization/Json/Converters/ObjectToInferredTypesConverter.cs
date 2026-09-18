// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Xtensive.Orm.Operations.Serialization.Json
{
  /// <summary>
  /// Json converter that helps to serialize/deserialize different values which are cast to object.
  /// </summary>
  public class ObjectToInferredTypesConverter : JsonConverter<object>
  {
    private const string ActualValueProperty = "ActualValue";
    private const string ObjectTypeProperty = "ObjectType";

    /// <inheritdoc />
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      reader.EnsureStartObject();

      var runtimeType = GetObjectType(ref reader, options);

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw JsonExceptions.WrongStructurePropertyExpected();
      var propertyName = reader.GetString();
      if (propertyName != options.ApplyNamingPolicy(ActualValueProperty))
        throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(ActualValueProperty), propertyName);
      var value = JsonSerializer.Deserialize(ref reader, runtimeType, options);

      _ = reader.Read();
      reader.EnsureEndObject();

      return value;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, object objectToWrite, JsonSerializerOptions options)
    {
      var runtimeType = objectToWrite.GetType();
      if (runtimeType == typeof(object)) {
        writer.WriteStartObject();
        writer.WriteEndObject();
        return;
      }
      writer.WriteStartObject();
      writer.WriteString(options.ApplyNamingPolicy(ObjectTypeProperty), runtimeType.AssemblyQualifiedName);

      writer.WritePropertyName(options.ApplyNamingPolicy(ActualValueProperty));
      JsonSerializer.Serialize(writer, objectToWrite, runtimeType, options);

      writer.WriteEndObject();
    }

    private static Type GetObjectType(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw JsonExceptions.WrongStructurePropertyExpected();

      string propertyName = reader.GetString();
      if (propertyName != options.ApplyNamingPolicy(ObjectTypeProperty))
        throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(ObjectTypeProperty), propertyName);

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.String)
        throw JsonExceptions.WrongStructure("Wrong type of return type property");
      var typeName = reader.GetString();
      return Type.GetType(typeName);
    }
  }
}
