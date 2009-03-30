// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Resources;
using Xtensive.Core.Reflection;
using Xtensive.Core.Helpers;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Base comparison result.
  /// </summary>
  [Serializable]
  public abstract class Difference : IDifference, 
    INodeActionSequenceBuilder,
    IContext<ComparisonScope>
  {
    /// <summary>
    /// Indent size in <see cref="ToString"/> method.
    /// </summary>
    protected static readonly int ToString_IndentSize = 2;

    /// <inheritdoc/>
    public string PropertyName { get; private set; }

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
      get { return ComparisonScope.CurrentDifference; }
    }

    /// <inheritdoc/>
    public abstract void Build(IList<NodeAction> sequence);

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

    #region ToString implementation

    /// <inheritdoc/>
    public override string ToString()
    {
      string proeprtyNamePrefix = PropertyName.IsNullOrEmpty() ? 
        string.Empty : 
        string.Format(Strings.DifferencePropertyNamePrefix, PropertyName);
      return string.Format(Strings.DifferenceFormat,
        proeprtyNamePrefix, GetType().GetShortName(), Source, Target, ParametersToString());
    }

    /// <summary>
    /// Converts parameters to string.
    /// </summary>
    /// <returns>String representation of difference parameters.</returns>
    protected abstract string ParametersToString();

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="propertyName">The <see cref="PropertyName"/> value.</param>
    /// <param name="source">The <see cref="Source"/> value.</param>
    /// <param name="target">The <see cref="Target"/> value.</param>
    public Difference(string propertyName, object source, object target)
    {
      PropertyName = propertyName;
      Source = source;
      Target = target;
      Parent = Current;
    }
  }
}