// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Text.Json;


namespace Xtensive.Orm.Operations.Serialization.Json
{
  /// <summary>
  /// Contains extension method that relate to <see cref="System.Text.Json.JsonSerializer"/> serialization
  /// </summary>
  public static class JsonSerializationExtensions
  {
    /// <summary>
    /// Adds to <see cref="JsonSerializerOptions.Converters"/> all available converters to serialize/deserialize operations.
    /// </summary>
    /// <param name="options">Options object to which converters should be added.</param>
    /// <param name="domain">Domain instance for converters that require it.</param>
    /// <returns><paramref name="options"/> object with registered converters.</returns>
    public static JsonSerializerOptions AddOperationsConverters(this JsonSerializerOptions options, Domain domain)
    {
      options.Converters.Add(new KeyConverterFactory(domain));
      options.Converters.Add(new FieldInfoConverter(domain));
      options.Converters.Add(new ObjectToInferredTypesConverter());
      return options;
    }

    /// <summary>
    /// Applies <see cref="JsonSerializerOptions.PropertyNamingPolicy"/> (if such exists) to property name,
    /// </summary>
    /// <param name="options">The options</param>
    /// <param name="propertyName">Unmodified property name.</param>
    /// <returns>Adjusted name, if policy exists, or the same value.</returns>
    internal static string ApplyNamingPolicy(this JsonSerializerOptions options, string propertyName)
    {
      var policy = options.PropertyNamingPolicy;
      if (policy == null)
        return propertyName;
      return options.PropertyNamingPolicy.ConvertName(propertyName);
    }

    /// <summary>
    /// Checks that Reader is in the <see cref="JsonTokenType.StartObject"/> position.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <exception cref="JsonException">Reader token has different value</exception>
    internal static void EnsureStartObject(this ref Utf8JsonReader reader)
    {
      if (reader.TokenType is not JsonTokenType.StartObject)
        throw JsonExceptions.WrongStructure("Starting of object expected.");
    }

    /// <summary>
    /// Checks that Reader is in the <see cref="JsonTokenType.EndObject"/> position.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <exception cref="JsonException">Reader token has different value</exception>
    internal static void EnsureEndObject(this ref Utf8JsonReader reader)
    {
      if (reader.TokenType != JsonTokenType.EndObject)
        throw JsonExceptions.WrongStructure("End of object expected.");
    }
  }
}
