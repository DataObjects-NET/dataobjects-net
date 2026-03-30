// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;

using Xtensive.Modelling.Actions;

using Xtensive.Reflection;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Base comparison result.
  /// </summary>
  [Serializable]
  public abstract class Difference : IDifference
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

    /// <inheritdoc/>
    public abstract bool HasChanges { get; }

    #region ToString implementation

    /// <inheritdoc/>
    public override string ToString()
    {
      var diffType = GetType();
      return string.Format(Strings.DifferenceFormat,
        diffType.IsGenericType ? diffType.GetShortName() : diffType.Name,
        Source, Target, ParametersToString());
    }

    /// <summary>
    /// Converts parameters to string.
    /// </summary>
    /// <returns>String representation of difference parameters.</returns>
    protected abstract string ParametersToString();

    #endregion


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="source">The <see cref="Source"/> value.</param>
    /// <param name="target">The <see cref="Target"/> value.</param>
    /// <exception cref="InvalidOperationException">Both <paramref name="source"/> and 
    /// <paramref name="target"/> are <see langword="null" />.</exception>
    protected Difference(object source, object target)
    {
      Source = source;
      Target = target;
      var any = source ?? target;
      if (any==null)
        throw new InvalidOperationException(Strings.ExBothSourceAndTargetAreNull);
      Parent = Comparer.Current.Context.Difference;
    }
  }
}