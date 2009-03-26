// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// <see cref="HintSet"/> scope.
  /// </summary>
  public class HintSetScope : Scope<HintSet>
  {
    /// <summary>
    /// Gets the current <see cref="HintSet"/>.
    /// </summary>
    public static HintSet CurrentSet {
      get {
        return CurrentContext ?? HintSet.Empty;
      }
    }
    
    /// <summary>
    /// Gets the associated <see cref="HintSet"/>.
    /// </summary>
    public HintSet Set {
      get {
        return Context;
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context (<see cref="HintSet"/>).</param>
    public HintSetScope(HintSet context)
      : base(context)
    {
    }
  }
}