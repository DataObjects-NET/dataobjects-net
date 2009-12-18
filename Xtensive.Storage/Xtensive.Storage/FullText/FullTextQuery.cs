// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.18

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Storage.Linq;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.FullText
{
  /// <summary>
  /// Full-text query access point.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="Entity"/>.</typeparam>
  [Serializable]
  public sealed class FullTextQuery<T> where T : Entity
  {
    /// <summary>
    /// The free-text query.
    /// </summary>
    /// <param name="searchCriteria">The search criteria.</param>
    /// <returns>A query returning <see cref="FullTextMatch{T}"/> instances.</returns>
    public IQueryable<FullTextMatch<T>> FreeText(Func<string> searchCriteria)
    {
      ArgumentValidator.EnsureArgumentNotNull(searchCriteria, "searchCriteria");
      Expression<Func<Func<string>,IQueryable<FullTextMatch<T>>>> lambda = func => FreeText(func);
      var expression = lambda.BindParameters(Expression.Constant(searchCriteria, typeof (Func<string>)));
      return new Queryable<FullTextMatch<T>>(expression);
    }
  }
}