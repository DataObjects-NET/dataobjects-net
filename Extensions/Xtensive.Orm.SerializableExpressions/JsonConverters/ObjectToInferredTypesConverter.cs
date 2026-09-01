using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xtensive.Orm.SerializableExpressions.JsonConverters
{
  /// <summary>
  /// Json converter that helps to serialize/deserialize different values which are cast to object.
  /// </summary>
  internal sealed class ObjectToInferredTypesConverter : JsonConverter<object>
  {
    private const string ActualValueProperty = "ActualValue";
    private const string ObjectTypeProperty = "ObjectType";

    /// <inheritdoc />
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      EnsureStartObject(ref reader);

      var runtimeType = GetObjectType(ref reader, options);

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw WrongStructurePropertyExpected();
      var propertyName = reader.GetString();
      var expectedPropertyName = ApplyNamingPolicy(options, ActualValueProperty);
      if (propertyName != expectedPropertyName)
        throw WrongStructurePropertyNameExpected(expectedPropertyName, propertyName);
      var value = JsonSerializer.Deserialize(ref reader, runtimeType, options);

      _ = reader.Read();
      EnsureEndObject( ref reader);

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
      writer.WriteString(ApplyNamingPolicy(options, ObjectTypeProperty), runtimeType.AssemblyQualifiedName);

      writer.WritePropertyName(ApplyNamingPolicy(options,ActualValueProperty));
      JsonSerializer.Serialize(writer, objectToWrite, runtimeType, options);

      writer.WriteEndObject();
    }

    private static Type GetObjectType(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw WrongStructurePropertyExpected();

      string propertyName = reader.GetString();
      var expectedPropertyName = ApplyNamingPolicy(options, ObjectTypeProperty);
      if (propertyName != expectedPropertyName)
        throw WrongStructurePropertyNameExpected(expectedPropertyName, propertyName);

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.String)
        throw WrongStructure("Wrong type of return type property");
      var typeName = reader.GetString();
      return Type.GetType(typeName);
    }

    private static string ApplyNamingPolicy(JsonSerializerOptions options, string propertyName)
    {
      var policy = options.PropertyNamingPolicy;
      if (policy is null)
        return propertyName;
      return options.PropertyNamingPolicy.ConvertName(propertyName);
    }

    private static void EnsureStartObject(ref Utf8JsonReader reader)
    {
      if (reader.TokenType is not JsonTokenType.StartObject)
        throw WrongStructure("Starting of object expected.");
    }

    private static void EnsureEndObject(ref Utf8JsonReader reader)
    {
      if (reader.TokenType != JsonTokenType.EndObject)
        throw WrongStructure("End of object expected.");
    }

    private static JsonException WrongStructurePropertyExpected() => WrongStructure("Property expected");

    private static JsonException WrongStructurePropertyNameExpected(string expected, string actual)
      => WrongStructure($"Property '{expected}' expected, but was '{actual}'");

    private static JsonException WrongStructure(string details) => new ($"Wrong Structure: {details}.");
  }
}
