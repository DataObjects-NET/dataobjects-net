using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xtensive.Orm.Operations.Serialization.Json
{
  internal sealed class CollectionOfIdentifiedEntitiesConverter : JsonConverter<IReadOnlyDictionary<string, Key>>
  {
    public override IReadOnlyDictionary<string, Key> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var dictionary = JsonSerializer.Deserialize<Dictionary<string, Key>>(ref reader, options);
      if (dictionary == null) {
        return null;
      }
      if (dictionary.Count == 0)
        return new Dictionary<string, Key>();// property rewrites empty collections with internal empty instance

      return dictionary.AsReadOnly();
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyDictionary<string, Key> value, JsonSerializerOptions options)
    {
      if (value == null) {
        throw new ArgumentNullException("value");
      }

      var arrayToWrite = value.Count == 0
        ? new Dictionary<string, Key>()
        : value.ToDictionary();
      JsonSerializer.Serialize<Dictionary<string, Key>>(writer, arrayToWrite, options);
    }
  }
}
