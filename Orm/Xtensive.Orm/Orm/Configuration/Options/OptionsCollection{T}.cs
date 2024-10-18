// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Options
{
  internal sealed class MappingRuleOptionsCollection : OptionsCollection<MappingRuleOptions>
  {
  }

  internal sealed class IgnoreRuleOptionsCollection : OptionsCollection<IgnoreRuleOptions>
  {
  }

  internal sealed class KeyGeneratorOptionsCollection : OptionsCollection<KeyGeneratorOptions>
  {
  }

  internal abstract class OptionsCollection<T> : ICollection<T>
    where T : class, IIdentifyableOptions
  {
    private readonly Dictionary<object, T> map = new();
    private readonly List<Exception> exceptionsOccur = new();

    public int Count => map.Count;

    public bool IsReadOnly => false;

    internal bool AnyExceptionOccur => exceptionsOccur.Any();

    // Section to object materialisation "swollows" exceptions,
    // which means we will get partially populated collections
    // without even knowing that something went wrong.
    // This collection of exceptions helps to understand whether
    // the collection populated without an issues or not.
    internal List<Exception> Exceptions => exceptionsOccur;

    public void Add(T item)
    {
      if (item == null) {
        throw RegisterValidationException(new ArgumentNullException(nameof(item)));
      }

      if (!map.TryAdd(item.Identifier, item)) {
        throw RegisterValidationException(
          new ArgumentException(
            $"Item with the same identifier '{item.Identifier}' has already been declared."));
      }
    }

    public bool Contains(T item)
    {
      if (item == null)
        return false;
      if (map.ContainsKey(item.Identifier))
        return true;
      if (map.Values.Contains(item)) {
        return true;
      }
      return false;
    }

    public void Clear() => map.Clear();

    public bool Remove(T item)
    {
      if (map.Remove(item.Identifier))
        return true;
      var result = map.Where(kv => kv.Value == item).Select(kv => kv.Key).FirstOrDefault();
      if (result != null) {
        return map.Remove(result);
      }
      return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      ArgumentValidator.EnsureArgumentNotNull(array, nameof(array));
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(arrayIndex, 0, nameof(arrayIndex));

      if (array.Length - arrayIndex < map.Count) {
        throw new ArgumentException(
          "The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array");
      }

      var index = arrayIndex;
      foreach (var item in map.Values) {
        array[index] = item;
        index++;
      }
    }

    protected Exception RegisterValidationException(Exception exception)
    {
      exceptionsOccur.Add(exception);
      return exception;
    }

    public IEnumerator<T> GetEnumerator() => map.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}
