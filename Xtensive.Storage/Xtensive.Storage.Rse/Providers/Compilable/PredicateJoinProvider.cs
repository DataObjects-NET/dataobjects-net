// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.05

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xtensive;
using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Produces join between <see cref="BinaryProvider.Left"/> and 
  /// <see cref="BinaryProvider.Right"/> sources by <see cref="Predicate"/>.
  /// </summary>
  [Serializable]
  public sealed class PredicateJoinProvider : BinaryProvider
  {
    /// <summary>
    /// Join operation type.
    /// </summary>
    public JoinType JoinType { get; private set; }

    /// <summary>
    /// Gets the predicate.
    /// </summary>
    public Expression<Func<Tuple, Tuple, bool>> Predicate { get; private set; }

    /// <inheritdoc/>
    protected override DirectionCollection<int> CreateExpectedColumnsOrdering()
    {
      var result = Left.ExpectedOrder;
      if (Left.ExpectedOrder.Count > 0) {
        var leftHeaderLength = Left.Header.Columns.Count;
        result = new DirectionCollection<int>(
          Enumerable.Union(result, Right.ExpectedOrder.Select(p =>
            new KeyValuePair<int, Direction>(p.Key + leftHeaderLength, p.Value))));
      }
      return result;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>  
    public PredicateJoinProvider(CompilableProvider left, CompilableProvider right,
      Expression<Func<Tuple, Tuple, bool>> predicate, JoinType joinType)
      : base(ProviderType.PredicateJoin, left, right)
    {
      Predicate = predicate;
      JoinType = joinType;
    }
  }
}