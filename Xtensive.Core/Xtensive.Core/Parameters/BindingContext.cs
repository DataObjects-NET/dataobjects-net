// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.12

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Parameters
{
  /// <summary>
  /// Binds values of type <typeparamref name="T"/> to keys. This binding is active within the <see cref="Binding"/> scope.
  /// </summary>
  /// <typeparam name="T">Type of values.</typeparam>
  public class BindingContext<T>
  {
    private readonly Dictionary<object, T> bindings;

    /// <summary>
    /// Binds the specified <paramref name="value"/> to <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns><see cref="Binding"/>.</returns>
    public Binding Bind(object key, T value)
    {
      Action disposeAction;
      T previous;
      if (bindings.TryGetValue(key, out previous)) {
        bindings[key] = value;
        disposeAction = () => bindings[key] = previous;
      }
      else {
        bindings.Add(key, value);
        disposeAction = () => bindings.Remove(key);
      }
      return new Binding(disposeAction);
    }

    /// <summary>
    /// Replaces the bound value.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void ReplaceBound(object key, T value)
    {
      if (!bindings.ContainsKey(key))
        throw new InvalidOperationException();
      bindings[key] = value;
    }

    /// <summary>
    /// Gets the bound value by the key.
    /// </summary>
    /// <param name="key">The key.</param>
    public T GetBound(object key)
    {
      return bindings[key];
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public BindingContext()
    {
      bindings = new Dictionary<object, T>();
    }
  }
}