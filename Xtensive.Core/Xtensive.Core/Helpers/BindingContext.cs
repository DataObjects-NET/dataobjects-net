// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.12

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Helpers
{
  /// <summary>
  /// Binds values of type <typeparamref name="T"/> to keys. 
  /// Any binding is active while its binding result (<see cref="Disposable"/>)
  /// isn't disposed.
  /// </summary>
  /// <typeparam name="T">Type of values.</typeparam>
  [Serializable]
  public class BindingContext<T>
  {
    private readonly Dictionary<object, T> bindings = new Dictionary<object, T>();

    /// <summary>
    /// Binds the specified <paramref name="value"/> to <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to bind to.</param>
    /// <param name="value">The value to bind.</param>
    /// <returns>Disposable object that will 
    /// destroy the binding on its disposal.</returns>
    public Disposable Bind(object key, T value)
    {
      T previous;
      if (bindings.TryGetValue(key, out previous)) {
        bindings[key] = value;
        return new Disposable((isDisposing) => bindings[key] = previous);
      }
      else {
        bindings.Add(key, value);
        return new Disposable((isDisposing) => bindings.Remove(key));
      }
    }

    /// <summary>
    /// Replaces the bound value.
    /// </summary>
    /// <param name="key">The binding key.</param>
    /// <param name="value">The new value.</param>
    /// <exception cref="KeyNotFoundException">Key isn't found.</exception>
    public void ReplaceBound(object key, T value)
    {
      if (!bindings.ContainsKey(key))
        throw new KeyNotFoundException();
      bindings[key] = value;
    }

    /// <summary>
    /// Gets the bound value by its key.
    /// </summary>
    /// <param name="key">The key of the bound value to get.</param>
    /// <returns>Bound value.</returns>
    public T GetBound(object key)
    {
      return bindings[key];
    }

    /// <summary>
    /// Gets the bound value by its key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns,
    /// contains the value bound to the specified key, if the key is found;
    /// otherwise, default value for the type of the value parameter.</param>
    /// <returns><see langword="True" /> if the <see cref="BindingContext{T}"/> 
    /// contains an element with the specified key;
    /// otherwise, <see langword="false" />.</returns>
    public bool TryGetBound(object key, out T value)
    {
      return bindings.TryGetValue(key, out value);
    }

    /// <summary>
    /// Gets the sequence of bound keys.
    /// </summary>
    /// <returns>The sequence of bound keys.</returns>
    public IEnumerable GetKeys()
    {
      foreach (var key in bindings.Keys)
        yield return key;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public BindingContext()
    {
    }
  }
}