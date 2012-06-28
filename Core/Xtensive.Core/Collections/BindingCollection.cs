// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.12

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Notifications;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

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
  public class BindingCollection<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
  {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    protected readonly Dictionary<TKey, TValue> bindings = new Dictionary<TKey, TValue>();
    protected readonly HashSet<TKey> permanentBindings = new HashSet<TKey>();

    /// <summary>
    /// Gets the number of currently bound items.
    /// </summary>
    public virtual int Count {
      [DebuggerStepThrough]
      get { return bindings.Count; }
    }

    /// <summary>
    /// Gets the bound value by its key.
    /// </summary>
    /// <value></value>
    public virtual TValue this[TKey key] {
      [DebuggerStepThrough]
      get { return bindings[key]; }
    }

    /// <summary>
    /// Binds the specified <paramref name="value"/> to <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to bind to.</param>
    /// <param name="value">The value to bind.</param>
    /// <returns>Disposable object that will 
    /// destroy the binding on its disposal.</returns>
    public virtual Disposable Add(TKey key, TValue value)
    {
      TValue previous;
      if (bindings.TryGetValue(key, out previous)) {
        bindings[key] = value;
        return new Disposable((isDisposing) => {
          if (!permanentBindings.Contains(key))
            bindings[key] = previous;
        });
      }
      else {
        bindings.Add(key, value);
        return new Disposable((isDisposing) => {
          if (!permanentBindings.Contains(key))
            bindings.Remove(key);
        });
      }
    }

    /// <summary>
    /// Binds the specified <paramref name="value"/> to <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to bind to.</param>
    /// <param name="value">The value to bind.</param>
    /// <returns><see langword="null" />, so this binding will not be removed.</returns>
    public virtual Disposable PermanentAdd(TKey key, TValue value)
    {
      bindings[key] = value;
      if (!permanentBindings.Contains(key))
        permanentBindings.Add(key);
      return null;
    }

    /// <summary>
    /// Replaces previously bound value.
    /// </summary>
    /// <param name="key">The binding key of the value to replace.</param>
    /// <param name="value">The new value.</param>
    /// <exception cref="KeyNotFoundException">Key isn't found.</exception>
    public virtual void ReplaceBound(TKey key, TValue value)
    {
      if (!bindings.ContainsKey(key))
        throw new KeyNotFoundException();
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
    public virtual bool TryGetValue(TKey key, out TValue value)
    {
      return bindings.TryGetValue(key, out value);
    }

    /// <summary>
    /// Gets the sequence of bound keys.
    /// </summary>
    /// <returns>The sequence of bound keys.</returns>
    public virtual IEnumerable GetKeys()
    {
      foreach (var key in bindings.Keys)
        yield return key;
    }

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      return bindings.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public BindingCollection()
    {
    }
  }
}