// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.25

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides <see cref="IQueryable{T}"/> queries for 
  /// <see cref="IEntity"/> implementors and <see cref="Entity"/> descendants.
  /// </summary>
  /// <typeparam name="T">The type of the content item of the data source. Must be assignable to 
  /// <see cref="Entity"/> or <see cref="IEntity"/> type.
  /// </typeparam>
  public static class Query<T>
    where T : class, IEntity
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
    /// Resolves the specified <paramref name="key"/> within the current <see cref="Session"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> the specified <paramref name="key"/> identifies.
    /// </returns>
    /// <exception cref="ArgumentException">Entity for the specified key is not found.</exception>
    public static T Single(Key key)
    {
      return (T) (object) Query.SingleOrDefault(key);
    }
    
    /// <summary>
    /// Resolves the specified <paramref name="key"/> within the current <see cref="Session"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> the specified <paramref name="key"/> identifies.
    /// </returns>
    /// <exception cref="ArgumentException">Entity for the specified key is not found.</exception>
    public static T SingleOrDefault(Key key)
    {
      return (T) (object) Query.SingleOrDefault(key);
    }

    /// <summary>
    /// Resolves <see cref="Entity"/> by specified key values.
    /// </summary>
    /// <param name="keyValues">Key values.</param>
    /// <returns>Entity resolved from key values. <see langword="Null"/> if entity missing or deleted.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="keyValues"/> is <see langword="Null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="keyValues"/> is empty.</exception>
    public static T SingleOrDefault(params object[] keyValues)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyValues, "keyValues");
      if (keyValues.Length==0) 
        throw new ArgumentException(Strings.ExKeyValuesArrayIsEmpty, "keyValues");
      if (keyValues.Length==1 && keyValues[0] is Key)
        return SingleOrDefault((Key) keyValues[0]);

      return (T) (object) Query.SingleOrDefault(Key.Create(typeof (T), keyValues));
    }
  }
}