// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.23

using System;
using System.Linq.Expressions;

namespace Xtensive.Orm
{
  /// <summary>
  /// A builder of LINQ query roots.
  /// This interface allows to override values returned by <see cref="QueryEndpoint.All{T}"/>
  /// and <see cref="QueryEndpoint.All"/> methods.
  /// Use <see cref="Session.OverrideQueryRoot"/> to attach <see cref="IQueryRootBuilder"/>
  /// to session. Insead of directly implementing this interface you may consider
  /// inheriting <see cref="QueryRootBuilder"/> instead.
  /// </summary>
  public interface IQueryRootBuilder
  {
    /// <summary>
    /// Builds root expression for querying specified entity of the specified type.
    /// </summary>
    /// <param name="entityType">Type of entity to query.</param>
    /// <returns>Expression containing query for entity of the specified type.</returns>
    Expression BuildRootExpression(Type entityType);
  }
}