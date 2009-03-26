// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Base comparison result.
  /// </summary>
  [Serializable]
  public class Difference : IDifference, IContext<ComparisonScope>
  {
    /// <inheritdoc/>
    public object Source { get; private set; }

    /// <inheritdoc/>
    public object Target { get; private set; }

    /// <inheritdoc/>
    public Difference Parent { get; private set; }

    /// <summary>
    /// Gets the current <see cref="Difference"/> object.
    /// </summary>
    public static Difference Current {
      get {
        return ComparisonScope.CurrentDifference;
      }
    }

    #region IContext<...> members

    /// <inheritdoc/>
    public bool IsActive {
      get { return Current==this; }
    }

    /// <inheritdoc/>
    public ComparisonScope Activate()
    {
      return new ComparisonScope(this);
    }

    /// <inheritdoc/>
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="Source"/> value.</param>
    /// <param name="target">The <see cref="Target"/> value.</param>
    public Difference(object source, object target)
    {
      Source = source;
      Target = target;
      Parent = Current;
    }
  }
}