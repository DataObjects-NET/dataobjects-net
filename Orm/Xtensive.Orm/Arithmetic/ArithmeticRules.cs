// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.10

using System;

namespace Xtensive.Arithmetic
{
  /// <summary>
  /// Describes how to calculate arithmetics.
  /// </summary>
  [Serializable]
  public struct ArithmeticRules : IEquatable<ArithmeticRules>
  {
    private readonly OverflowBehavior overflowBehavior;
    private readonly NullBehavior nullBehavior;

    /// <summary>
    /// Gets overflow behavior.
    /// </summary>
    public OverflowBehavior OverflowBehavior
    {
      get { return overflowBehavior; }
    }

    /// <summary>
    /// Gets null behavior.
    /// </summary>
    public NullBehavior NullBehavior
    {
      get { return nullBehavior; }
    }

    /// <inheritdoc/>
    public bool Equals(ArithmeticRules other)
    {
      return overflowBehavior==other.overflowBehavior
        && nullBehavior==other.nullBehavior;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj==null)
        return false;
      if (obj is ArithmeticRules)
        return Equals((ArithmeticRules)obj);
      return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return ((byte)overflowBehavior << 8) | (byte)nullBehavior;
    }


    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="ArithmeticRules"/>.
    /// </summary>
    /// <param name="nullBehavior">Null behavior.</param>
    /// <param name="overflowBehavior">Overflow behavior.</param>
    public ArithmeticRules(NullBehavior nullBehavior, OverflowBehavior overflowBehavior)
    {
      this.nullBehavior = nullBehavior;
      this.overflowBehavior = overflowBehavior;
    }
  }
}