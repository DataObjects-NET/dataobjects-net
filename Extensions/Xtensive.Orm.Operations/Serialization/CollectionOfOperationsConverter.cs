using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xtensive.Orm.Operations.Serialization
{
  internal sealed class CollectionOfOperationsConverter : JsonConverter<IReadOnlyList<IOperation>>
  {
    public override IReadOnlyList<IOperation> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var array = JsonSerializer.Deserialize<Operation[]>(ref reader, options);
      if (array == null) {
        return null;
      }
      if (array.Length == 0)
        return Array.Empty<IOperation>();
      return array.Cast<IOperation>().ToArray().AsReadOnly();
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<IOperation> value, JsonSerializerOptions options)
    {
      if (value == null) {
        throw new ArgumentNullException("value");
      }
      var arrayToWrite = value.Count == 0
        ? Array.Empty<Operation>()
        : value.Cast<Operation>().ToArray();
      JsonSerializer.Serialize<Operation[]>(writer, arrayToWrite, options);
    }
  }
}
