// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.01

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class ComparisonContext : Context<ComparisonScope>
  {
    private readonly ComparisonRegistry registry = new ComparisonRegistry();
    private readonly IEnumerable<ComparisonHintBase> hints;
    private readonly ComparisonResultFactory factory;

    public ComparisonResultFactory Factory
    {
      get { return factory; }
    }

    public ComparisonRegistry Registry
    {
      get { return registry; }
    }

    public IEnumerable<ComparisonHintBase> Hints
    {
      get { return hints; }
    }

    /// <inheritdoc/>
    protected override ComparisonScope CreateActiveScope()
    {
      return new ComparisonScope(this);
    }

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return ComparisonScope.CurrentContext==this; }
    }

    public static ComparisonContext Current
    {
      get { return ComparisonScope.CurrentContext; }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="hints">Comparison hints.</param>
    public ComparisonContext(IEnumerable<ComparisonHintBase> hints)
    {
      factory = new ComparisonResultFactory(this);
      this.hints = hints ?? Enumerable.Empty<ComparisonHintBase>();
    }
  }
}