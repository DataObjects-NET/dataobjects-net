// Copyright (C) 2008-2021 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.05

using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Core;

namespace Xtensive.Comparison
{
  /// <summary>
  /// Describes how to compare values of comparable objects.
  /// </summary>
  [Serializable]
  public struct ComparisonRule :
    IEquatable<ComparisonRule>,
    ISerializable
  {
    /// <summary>
    /// Predefined rule with <see cref="Direction"/> = <see cref="Core.Direction.None"/>.
    /// </summary>
    public static readonly ComparisonRule None     = new ComparisonRule(Direction.None);
    /// <summary>
    /// Predefined rule with <see cref="Direction"/> = <see cref="Core.Direction.Positive"/>.
    /// </summary>
    public static readonly ComparisonRule Positive = new ComparisonRule(Direction.Positive);
    /// <summary>
    /// Predefined rule with <see cref="Direction"/> = <see cref="Core.Direction.Negative"/>.
    /// </summary>
    public static readonly ComparisonRule Negative = new ComparisonRule(Direction.Negative);

    /// <summary>
    /// Gets <see cref="Core.Direction"/> for the comparison.
    /// </summary>
    public readonly Direction Direction;

    /// <summary>
    /// Gets <see cref="CultureInfo"/> for the comparison.
    /// <see langword="Null"/> means no culture is specified.
    /// <see cref="CultureInfo.InvariantCulture"/> should normally be used
    /// for the comparison in this case.
    /// </summary>
    public readonly CultureInfo Culture;

    /// <summary>
    /// Inverts the direction of the rule.
    /// </summary>
    /// <returns>The same rule, but with inverted direction.</returns>
    public ComparisonRule Invert()
    {
      return new ComparisonRule((Direction)(-((int)Direction)), Culture);     
    }

    /// <summary>
    /// Combines new comparison rule with the rule described by this instance.
    /// </summary>
    /// <param name="rule">Rule to combine.</param>
    /// <returns>Result of the combination.</returns>
    public ComparisonRule Combine(ComparisonRule rule)
    {
      if (Culture == null) {
        if (rule.Culture!=null)
          return new ComparisonRule((Direction) ((int) Direction * (int) rule.Direction), rule.Culture);
        return new ComparisonRule((Direction)((int)Direction * (int)rule.Direction), null);
      }
      if (rule.Culture!=null && !Equals(Culture, rule.Culture))
        throw new InvalidOperationException(Strings.ExCultureOfAppliedRuleShouldBeEitherNullOrTheSameAsOnTarget);
      return new ComparisonRule((Direction)((int)Direction * (int)rule.Direction), Culture);
    }

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public bool Equals(ComparisonRule other)
    {
      if (Direction != other.Direction)
        return false;
      return Equals(Culture, other.Culture);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
      obj is ComparisonRule other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      int result = (int)Direction;
      if (Culture != null)
        result ^= Culture.GetHashCode();
      return result;
    }

    #endregion

    #region Operators, conversion

    /// <summary>
    /// Implements the equality operator.
    /// </summary>
    /// <param name="x">The first argument.</param>
    /// <param name="y">The second argument.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(ComparisonRule x, ComparisonRule y)
    {
      return x.Equals(y);
    }

    /// <summary>
    /// Implements the inequality operator.
    /// </summary>
    /// <param name="x">The first argument.</param>
    /// <param name="y">The second argument.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(ComparisonRule x, ComparisonRule y)
    {
      return !x.Equals(y);
    }

    /// <summary>
    /// Implicit conversion of <see cref="ComparisonRule"/> to <see cref="ComparisonRules"/>.
    /// </summary>
    /// <param name="comparisonRule">The rule to convert.</param>
    /// <returns>Conversion result.</returns>
    public static implicit operator ComparisonRules(ComparisonRule comparisonRule)
    {
      if (comparisonRule.Culture==null) {
        switch (comparisonRule.Direction) {
        case Direction.Positive:
          return ComparisonRules.Positive;
        case Direction.Negative:
          return ComparisonRules.Negative;
        case Direction.None:
          return ComparisonRules.None;
        default:
          throw Exceptions.InvalidArgument(comparisonRule.Direction, "comparisonRule.Direction");
        }
      }
      else
        return new ComparisonRules(comparisonRule);
    }

    /// <summary>
    /// Implicit conversion of <see cref="Core.Direction"/> to <see cref="ComparisonRule"/>.
    /// </summary>
    /// <param name="direction">Direction to convert.</param>
    /// <returns>Conversion result.</returns>
    public static implicit operator ComparisonRule(Direction direction)
    {
      return new ComparisonRule(direction);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.ComparisonRuleFormat,
        Direction, Culture==null ? Strings.AnyCulture : Culture.Name);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="direction">Initial <see cref="Core.Direction"/> property value.</param>
    public ComparisonRule(Direction direction)
      : this(direction, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="direction">Initial <see cref="Core.Direction"/> property value.</param>
    /// <param name="culture">Initial <see cref="Culture"/> property value.</param>
    public ComparisonRule(Direction direction, CultureInfo culture)
    {
      Direction = direction;
      Culture = culture;
    }

    private ComparisonRule(SerializationInfo info, StreamingContext context)
    {
      if (info == null) {
        throw new ArgumentNullException(nameof(info));
      }
      Direction = (Direction) info.GetSByte(nameof(Direction));
      var cultureId = info.GetInt32(nameof(Culture));
      Culture = (cultureId != int.MinValue) ? CultureInfo.GetCultureInfo(cultureId) : null;
    }

    [SecurityCritical]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if(info == null) {
        throw new ArgumentNullException(nameof(info));
      }
      info.AddValue(nameof(Direction), (sbyte) Direction);

      var cultureId = (Culture != null) ? Culture.LCID : int.MinValue;
      info.AddValue(nameof(Culture), cultureId);
    }
  }
}