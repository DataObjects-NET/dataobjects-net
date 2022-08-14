// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    public override bool Equals(object obj) =>
      obj is ArithmeticRules other && Equals(other);

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
