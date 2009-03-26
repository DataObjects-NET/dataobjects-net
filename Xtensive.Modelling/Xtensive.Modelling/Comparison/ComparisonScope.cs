// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Comparison scope.
  /// </summary>
  public class ComparisonScope : Scope<Difference>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public static Difference CurrentDifference {
      get {
        return CurrentContext;
      }
    }
    
    /// <summary>
    /// Gets the associated comparison context.
    /// </summary>
    public Difference Difference {
      get {
        return Context;
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context (<see cref="Difference"/>).</param>
    public ComparisonScope(Difference context)
      : base(context)
    {
    }
  }
}