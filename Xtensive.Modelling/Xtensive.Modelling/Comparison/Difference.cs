// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Resources;
using Xtensive.Core.Reflection;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Base comparison result.
  /// </summary>
  [Serializable]
  public abstract class Difference : IDifference, IContext<ComparisonScope>
  {
    /// <summary>
    /// Indent size in <see cref="ToString"/> method.
    /// </summary>
    protected static readonly int ToString_IndentSize = 2;

    /// <inheritdoc/>
    public object Source { get; private set; }

    /// <inheritdoc/>
    public object Target { get; private set; }

    /// <inheritdoc/>
    public Difference Parent { get; private set; }

    /// <summary>
    /// Gets the nearest <see cref="Parent"/> of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="Parent"/> to find.</typeparam>
    /// <returns>The nearest <see cref="Parent"/> of type <typeparamref name="T"/>, if found;
    /// otherwise, <see langword="null" />.</returns>
    public T GetNearestParent<T>()
      where T : Difference
    {
      var current = this;
      while ((current = current.Parent)!=null) {
        var typedCurrent = current as T;
        if (typedCurrent!=null)
          return typedCurrent;
      }
      return null;
    }

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

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.DifferenceFormat,
        GetType().GetShortName(), Source, Target, ParametersToString());
    }

    /// <summary>
    /// Converts parameters to string.
    /// </summary>
    /// <returns>String representation of difference parameters.</returns>
    protected abstract string ParametersToString();


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