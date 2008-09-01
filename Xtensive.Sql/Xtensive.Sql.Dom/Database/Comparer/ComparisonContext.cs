// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.01

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class ComparisonContext : Context<ComparisonScope>
  {
    public IEnumerable<ComparisonHintBase> Hints{ get; private set;}

    /// <inheritdoc/>
    protected override ComparisonScope CreateActiveScope()
    {
      return new ComparisonScope(this);
    }

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return ComparisonScope.CurrentContext == this; }
    }

    public ComparisonContext(IEnumerable<ComparisonHintBase> hints)
    {
      hints = hints ?? Enumerable.Empty<ComparisonHintBase>();
      Hints = hints;
    }
  }
}