// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.25

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Storage.Fulltext;
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
  [Obsolete("Query<T> is obsolete. Use Query class instead.")]
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
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// <see langword="null" />, if there is no such entity.
    /// </returns>
    public static T Single(Key key)
    {
      return (T) (object) Query.Single(key);
    }
    
    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="keyValues"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <param name="keyValues">Key values.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="keyValues"/> identify.
    /// <see langword="null" />, if there is no such entity.
    /// </returns>
    public static T Single(params object[] keyValues)
    {
      return (T) (object) Query.Single(GetKeyByValues(keyValues));
    }
    
    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// </returns>
    public static T SingleOrDefault(Key key)
    {
      return (T) (object) Query.SingleOrDefault(key);
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="keyValues"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <param name="keyValues">Key values.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="keyValues"/> identify.
    /// </returns>
    public static T SingleOrDefault(params object[] keyValues)
    {
      return (T) (object) Query.SingleOrDefault(GetKeyByValues(keyValues));
    }

    /// <exception cref="ArgumentException"><paramref name="keyValues"/> array is empty.</exception>
    private static Key GetKeyByValues(object[] keyValues)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyValues, "keyValues");
      if (keyValues.Length==0)
        throw new ArgumentException(Strings.ExKeyValuesArrayIsEmpty, "keyValues");
      if (keyValues.Length==1) {
        var keyValue = keyValues[0];
        if (keyValue is Key)
          return keyValue as Key;
        if (keyValue is Entity)
          return (keyValue as Entity).Key;
      }
      return Key.Create(typeof (T), keyValues);
    }
  }
}