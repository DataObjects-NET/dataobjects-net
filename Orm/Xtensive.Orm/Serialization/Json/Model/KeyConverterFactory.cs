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
        var formattedKey = reader.GetString();
        return Key.Parse(domain, formattedKey);
      }

      /// <inheritdoc/>
      public override void Write(Utf8JsonWriter writer, Key value, JsonSerializerOptions options)
      {
        writer.WriteStringValue(value.Format());
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
