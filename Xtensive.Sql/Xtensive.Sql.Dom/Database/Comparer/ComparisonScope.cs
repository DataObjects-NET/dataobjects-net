// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.01

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class ComparisonScope : Scope<ComparisonContext>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static ComparisonContext CurrentContext
    {
      get { return Scope<ComparisonContext>.CurrentContext; }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public new ComparisonContext Context
    {
      get { return base.Context; }
    }

    public ComparisonScope(ComparisonContext context)
      :base(context)
    {
    }

    public ComparisonScope()
    {
      
    }
  }
}