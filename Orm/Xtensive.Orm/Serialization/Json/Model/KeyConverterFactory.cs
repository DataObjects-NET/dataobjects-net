// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xtensive.Orm;
using Xtensive.Orm.Internals;

namespace Xtensive.Serialization.Json.Model
{
  /// <summary>
  /// Json converter factory for <see cref="Xtensive.Orm.Key"/>s.
  /// </summary>
  /// <param name="domain">Domain, required for deserialization.</param>
  public class KeyConverterFactory(Domain domain) : JsonConverterFactory
  {
    #region Converters
    private sealed class DefaultKeyConverter(Domain domain) : JsonConverter<Key>
    {
      private readonly Domain domain = domain;

      /// <inheritdoc/>
      public override Key Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        reader.EnsureStartObject();

        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();

        var propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(nameof(Key.NodeId)))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(Key.NodeId)), propertyName);

        _ = reader.Read();
        var nodeId = reader.GetString();

        _ = reader.Read();
        if (reader.TokenType is not JsonTokenType.PropertyName)
          throw JsonExceptions.WrongStructurePropertyExpected();
        propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(nameof(Key.TypeReference)))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(Key.TypeReference)), propertyName);

        _ = reader.Read();
        var typeReference = JsonSerializer.Deserialize<TypeReference>(ref reader, options);

        _ = reader.Read();
        propertyName = reader.GetString();
        if (propertyName != options.ApplyNamingPolicy(nameof(Key.Value)))
          throw JsonExceptions.WrongStructurePropertyNameExpected(options.ApplyNamingPolicy(nameof(Key.Value)), propertyName);

        _ = reader.Read();
        // normally it is type of tuple for key
        var tuple = JsonSerializer.Deserialize<Tuples.RegularTuple>(ref reader, options);

        _ = reader.Read();
        reader.EnsureEndObject();

        // can be optimized if use internal method with TypeInfo parameter instead of System.Type
        var result = Key.Create(domain, nodeId, typeReference.Type.UnderlyingType, typeReference.Accuracy, tuple);

        return result;
      }

      /// <inheritdoc/>
      public override void Write(Utf8JsonWriter writer, Key value, JsonSerializerOptions options)
      {
        writer.WriteStartObject();

        writer.WriteString(options.ApplyNamingPolicy(nameof(Key.NodeId)), value.NodeId);

        writer.WritePropertyName(options.ApplyNamingPolicy(nameof(Key.TypeReference)));
        JsonSerializer.Serialize(writer, value.TypeReference, options);

        writer.WritePropertyName(options.ApplyNamingPolicy(nameof(Key.Value)));
        JsonSerializer.Serialize(writer, value.Value, value.Value.GetType(), options);

        writer.WriteEndObject();
      }
    }

    #endregion

    private readonly Domain domain = domain;

    public override bool CanConvert(Type typeToConvert) =>
      typeToConvert.IsAssignableTo(WellKnownOrmTypes.Key);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      // there might be implementation of per-type converters
      return new DefaultKeyConverter(domain);
    }

    
  }
}
