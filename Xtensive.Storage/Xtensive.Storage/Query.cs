// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.25

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Storage.Linq;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides <see cref="IQueryable{T}"/> queries for 
  /// <see cref="IEntity"/> implementors like entities and persistent interfaces.
  /// </summary>
  /// <typeparam name="T">The type of the content item of the data source. Must be assignable to 
  /// <see cref="Entity"/> or <see cref="IEntity"/> type.
  /// </typeparam>
  public static class Query<T>
    where T : IEntity
  {
    /// <summary>
    /// The "starting point" for any LINQ query -
    /// a <see cref="IQueryable{T}"/> enumerating all the instances
    /// of type <typeparamref name="T"/>.
    /// </summary>
    public static IQueryable<T> All
    {
      get { return new Queryable<T>(); }
    }
    
    /// <summary>
    /// Resolves <see cref="Entity"/> by specified key.
    /// </summary>
    /// <param name="key">Key to resolve entity for.</param>
    /// <returns>Entity resolved from key. <see langword="Null"/> if entity missing or deleted.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="Null"/>.</exception>
    public static T Resolve(Key key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      return (T)(object)key.Resolve();
    }

    /// <summary>
    /// Resolves <see cref="Entity"/> by specified key values.
    /// </summary>
    /// <param name="keyValues">Key values.</param>
    /// <returns>Entity resolved from key values. <see langword="Null"/> if entity missing or deleted.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="keyValues"/> is <see langword="Null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="keyValues"/> is empty.</exception>
    public static T Resolve(params object[] keyValues)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyValues, "keyValues");
      if (keyValues.Length==0) 
        throw new ArgumentException("Key values array is empty.", "keyValues");
      if (keyValues.Length==1 && keyValues[0] is Key)
        return Resolve((Key) keyValues[0]);

      return (T) (object) Key.Create(typeof (T), keyValues).Resolve();
    }
  }
}