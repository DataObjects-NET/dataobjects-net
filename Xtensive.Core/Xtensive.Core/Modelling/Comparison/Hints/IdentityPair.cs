// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.06.02

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Pair of node paths or node path and constant.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Source} == {Target}")]
  public class IdentityPair
  {
    /// <summary>
    /// Gets the source node path.
    /// </summary>
    public string Source { get; private set; }

    /// <summary>
    /// Gets the target node path or constant.
    /// </summary>
    public string Target { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Target"/> value is constant.
    /// </summary>
    public bool IsIdentifiedByConstant { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("{0} == {1}", Source, Target ?? "null");
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The source node path.</param>
    /// <param name="target">The target node path.</param>
    /// <param name="isIdentifiedByConstant">if set to <see langword="true"/> the target value is constant.</param>
    public IdentityPair(string source, string target, bool isIdentifiedByConstant)
    {
      Source = source;
      Target = target;
      IsIdentifiedByConstant = isIdentifiedByConstant;
    }
  }
}