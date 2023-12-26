// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.03.12

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;


namespace Xtensive.Collections
{
  /// <summary>
  /// Temporarily binds values of type <typeparamref name="TValue"/> to their keys
  /// and provides access to currently bound values.
  /// Any binding is active while its binding result (<see cref="IDisposable"/> object)
  /// isn't disposed.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class BindingCollection<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>
  {
    public readonly ref struct BindingScope
    {
      public static BindingScope Empty => new BindingScope();

      private readonly BindingCollection<TKey, TValue> owner;
      private readonly TKey key;
      private readonly TValue prevValue;
      private readonly bool prevValueExists;

      public void Dispose()
      {
        if (owner == null) {
          return;
        }

        if (prevValueExists) {
          if (!owner.permanentBindings.Contains(key)) {
            owner.bindings[key] = prevValue;
          }
        }
        else {
          if (!owner.permanentBindings.Contains(key)) {
            owner.bindings.Remove(key);
          }
        }
      }

      public BindingScope(BindingCollection<TKey, TValue> owner, TKey key) : this()
      {
        this.owner = owner;
        this.key = key;
      }

      public BindingScope(BindingCollection<TKey, TValue> owner, TKey key, TValue prevValue)
      {
        this.owner = owner;
        this.key = key;
        this.prevValue = prevValue;
        prevValueExists = true;
      }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly Dictionary<TKey, TValue> bindings = new Dictionary<TKey, TValue>();
    private readonly HashSet<TKey> permanentBindings = new HashSet<TKey>();

    /// <summary>
    /// Gets the number of currently bound items.
    /// </summary>
    public virtual int Count {
      [DebuggerStepThrough]
      get => bindings.Count;
    }

    /// <summary>
    /// Gets the bound value by its key.
    /// </summary>
    /// <value></value>
    public virtual TValue this[TKey key] {
      [DebuggerStepThrough]
      get => bindings[key];
    }

    /// <summary>
    /// Binds the specified <paramref name="value"/> to <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to bind to.</param>
    /// <param name="value">The value to bind.</param>
    /// <returns>Disposable object that will 
    /// destroy the binding on its disposal.</returns>
    public virtual BindingScope Add(TKey key, TValue value)
    {
      if (bindings.TryGetValue(key, out var previous)) {
        bindings[key] = value;
        return new BindingScope(this, key, previous);
      }

      bindings.Add(key, value);
      return new BindingScope(this, key);
    }

    /// <summary>
    /// Binds the specified <paramref name="value"/> to <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to bind to.</param>
    /// <param name="value">The value to bind.</param>
    /// <returns><see langword="null" />, so this binding will not be removed.</returns>
    public virtual void PermanentAdd(TKey key, TValue value)
    {
      bindings[key] = value;
      permanentBindings.Add(key);
    }

    /// <summary>
    /// Replaces previously bound value.
    /// </summary>
    /// <param name="key">The binding key of the value to replace.</param>
    /// <param name="value">The new value.</param>
    /// <exception cref="KeyNotFoundException">Key isn't found.</exception>
    public virtual void ReplaceBound(TKey key, TValue value)
    {
      if (!bindings.ContainsKey(key)) {
        throw new KeyNotFoundException();
      }

      bindings[key] = value;
    }

    /// <summary>
    /// Gets the bound value by its key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns,
    /// contains the value bound to the specified key, if the key is found;
    /// otherwise, default value for the type of the value parameter.</param>
    /// <returns><see langword="True" /> if the <see cref="BindingCollection{TKey,TValue}"/> 
    /// contains an element with the specified key;
    /// otherwise, <see langword="false" />.</returns>
    [DebuggerStepThrough]
    public virtual bool TryGetValue(TKey key, out TValue value) => bindings.TryGetValue(key, out value);

    /// <summary>
    /// Gets the sequence of bound keys.
    /// </summary>
    /// <returns>The sequence of bound keys.</returns>
    public virtual IEnumerable GetKeys()
    {
      foreach (var key in bindings.Keys) {
        yield return key;
      }
    }

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => bindings.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
  }
}