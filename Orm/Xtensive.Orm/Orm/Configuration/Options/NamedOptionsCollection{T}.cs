// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Options
{
  internal sealed class SessionOptionsCollection : NamedOptionsCollection<SessionConfigurationOptions>
  {
    protected override bool AllowEmptyNames => true;
  }

  internal sealed class DatabaseOptionsCollection : NamedOptionsCollection<DatabaseOptions>
  {

  }

  internal abstract class NamedOptionsCollection<T> : IDictionary<string, T>
    where T: INamedOptionsCollectionElement
  {
    private readonly Dictionary<string, T> internalDictionary = new();
    private readonly List<Exception> exceptionsOccur = new();

    public T this[string key]
    {
      get => internalDictionary[key];
      set {
        
        if (!internalDictionary.ContainsKey(key)) {
          Add(key, value);
        }
        else {
          ValidateName(key);
          value.Name = key;
          ValidateItemOnAdd(value);
          internalDictionary[key] = value;
        }
      }
    }

    public ICollection<string> Keys => internalDictionary.Keys;

    public ICollection<T> Values => internalDictionary.Values;

    public int Count => internalDictionary.Count;

    protected virtual bool AllowEmptyNames => false;

    internal bool AnyExceptionOccur => exceptionsOccur.Any();

    // Section to object materialisation "swollows" exceptions,
    // which means we will get partially populated collections
    // without even knowing that something went wrong.
    // This collection of exceptions helps to understand whether
    // the collection populated without an issues or not.
    internal List<Exception> Exceptions => exceptionsOccur;

    public void Add(string name, T value)
    {
      ValidateName(name);

      if (value == null) {
        throw new ArgumentNullException(nameof(value));
      }
      value.Name = name;

      if (!internalDictionary.TryAdd(name, value)) {
        throw new ArgumentException(string.Format(Strings.ExItemWithNameXAlreadyExists, name));
      }
    }

    public void Clear() => internalDictionary.Clear();

    public bool ContainsKey(string key) => internalDictionary.ContainsKey(key);
    public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => ((ICollection<KeyValuePair<string, T>>) internalDictionary).GetEnumerator();
    public bool Remove(string key) => internalDictionary.Remove(key);
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value) => internalDictionary.TryGetValue(key, out value);


    protected virtual void ValidateItemOnAdd(T item)
    {
      if (item is IValidatableOptions validatableOptions) {
        validatableOptions.Validate();
      }
    }

    protected Exception RegisterValidationException(Exception exception)
    {
      exceptionsOccur.Add(exception);
      return exception;
    }

    private void ValidateName(string name)
    {
      if (!AllowEmptyNames && name.IsNullOrEmpty()) {
        throw RegisterValidationException((name == null)
          ? new ArgumentNullException(nameof(name))
          : new ArgumentException(Strings.ExArgumentCannotBeEmptyString, nameof(name)));
      }
      else {
        if (name == null) {
          throw RegisterValidationException(new ArgumentNullException(nameof(name)));
        }
      }
    }

    bool ICollection<KeyValuePair<string, T>>.IsReadOnly => ((ICollection<KeyValuePair<string, T>>) internalDictionary).IsReadOnly;
    void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item)
    {
      Add(item.Key, item.Value);
    }
    void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) =>
      ((IDictionary<string, T>) internalDictionary).CopyTo(array, arrayIndex);
    bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> item) =>
      ((IDictionary<string, T>) internalDictionary).Contains(item);
    bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item) =>
      ((IDictionary<string, T>) internalDictionary).Remove(item);


    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}
