// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.25

using System.Linq;
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
  public static class Query<T> where T : IEntity
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
  }
}