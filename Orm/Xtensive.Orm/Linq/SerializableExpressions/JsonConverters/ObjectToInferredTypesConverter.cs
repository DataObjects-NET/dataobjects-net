using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xtensive.Linq.SerializableExpressions.Internals;
using Xtensive.Reflection;

namespace Xtensive.Linq.SerializableExpressions.JsonConverters
{
  public class ObjectToInferredTypesConverter : JsonConverter<object>
  {
    private const string ActualValueProperty = "ActualValue";
    private const string ObjectTypeProperty = "ObjectType";
    private const string WrongStructureExceptionTemplate = "Wrong Structure: {0}.";

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      EnsureStartObject(ref reader);

      var runtimeType = GetObjectType(ref reader, options);

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw new JsonException();
      var valueProperty = reader.GetString();
      if (valueProperty != ConvertName(ActualValueProperty, options))
        throw new JsonException();
      var value = JsonSerializer.Deserialize(ref reader, runtimeType, options);

      _ = reader.Read();
      if (reader.TokenType != JsonTokenType.EndObject)
        throw new JsonException();

      return value;
    }

    public override void Write(Utf8JsonWriter writer, object objectToWrite, JsonSerializerOptions options)
    {
      var runtimeType = objectToWrite.GetType();
      if (runtimeType == WellKnownTypes.Object) {
        writer.WriteStartObject();
        writer.WriteEndObject();
        return;
      }
      writer.WriteStartObject();
      writer.WriteString(ConvertName(ObjectTypeProperty, options), runtimeType.ToSerializableForm());

      writer.WritePropertyName(ConvertName(ActualValueProperty, options));
      JsonSerializer.Serialize(writer, objectToWrite, runtimeType, options);

      writer.WriteEndObject();
    }

    private static void EnsureStartObject(ref Utf8JsonReader reader)
    {
      if (reader.TokenType is not JsonTokenType.StartObject)
        throw WrongStructureException("Starting of object expected.");
    }

    private static Type GetObjectType(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.PropertyName)
        throw WrongStructureException("Property name expected.");

      string propertyName = reader.GetString();
      if (propertyName != ConvertName(ObjectTypeProperty, options))
        throw WrongStructureException($"Expected property is {ObjectTypeProperty} but was {propertyName}.");

      _ = reader.Read();
      if (reader.TokenType is not JsonTokenType.String)
        throw WrongStructureException("Wrong type of SerializableExpression return type property");
      var typeName = reader.GetString();
      return typeName.GetTypeFromSerializableForm();
    }

    private static string ConvertName(string name, JsonSerializerOptions options)
    {
      var policy = options.PropertyNamingPolicy;
      if (policy == null)
        return name;
      return options.PropertyNamingPolicy.ConvertName(name);
    }

    private static JsonException WrongStructureException(string reason) =>
      new JsonException(string.Format(WrongStructureExceptionTemplate, reason));
  }
}
