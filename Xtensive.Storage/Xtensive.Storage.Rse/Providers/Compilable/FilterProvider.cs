// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Compilable
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
          compiledPredicate = Predicate.Compile();
        return compiledPredicate;
      }
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Predicate.ToString(true);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
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