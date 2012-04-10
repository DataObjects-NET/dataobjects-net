// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Linq.Expressions;
using Xtensive.Core;

using Xtensive.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that declares filtering operation 
  /// over the <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public class FilterProvider : UnaryProvider
  {
    private Func<Tuple, bool> compiledPredicate;

    /// <summary>
    /// Filtering predicate expression.
    /// </summary>
    public Expression<Func<Tuple, bool>> Predicate { get; private set; }

    /// <summary>
    /// Gets the compiled <see cref="Predicate"/>.
    /// </summary>
    public Func<Tuple, bool> CompiledPredicate {
      get {
        if (compiledPredicate==null)
          compiledPredicate = Predicate.CachingCompile();
        return compiledPredicate;
      }
    }

    
    protected override string ParametersToString()
    {
      return Predicate.ToString(true);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The source provider.</param>
    /// <param name="predicate">The predicate.</param>
    public FilterProvider(CompilableProvider source, Expression<Func<Tuple, bool>> predicate)
      : base(ProviderType.Filter, source)
    {
      Predicate = predicate;
    }
  }
}