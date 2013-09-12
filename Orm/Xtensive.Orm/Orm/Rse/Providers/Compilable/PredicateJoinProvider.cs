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
using Xtensive.Collections;
using Xtensive.Core;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Providers
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


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>  
    public PredicateJoinProvider(CompilableProvider left, CompilableProvider right,
      Expression<Func<Tuple, Tuple, bool>> predicate, JoinType joinType)
      : base(ProviderType.PredicateJoin, left, right)
    {
      Predicate = predicate;
      JoinType = joinType;
      Initialize();
    }
  }
}