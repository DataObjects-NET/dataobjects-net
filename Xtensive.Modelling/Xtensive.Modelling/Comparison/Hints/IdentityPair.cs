// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.06.02

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Pair of column paths or column path and constant.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Source} == {Target}")]
  public class IdentityPair
  {
    /// <summary>
    /// Gets or sets the source column path.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the target column path or constant.
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Target"/> value is constant.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this <see cref="Target"/> value is constant; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsIdentifiedByConstant { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("{0} == {1}", Source, Target ?? "null");
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <param name="isIdentifiedByConstant">if set to <see langword="true"/> the target value is constant.</param>
    public IdentityPair(string source, string target, bool isIdentifiedByConstant)
    {
      Source = source;
      Target = target;
      IsIdentifiedByConstant = isIdentifiedByConstant;
    }
  }
}