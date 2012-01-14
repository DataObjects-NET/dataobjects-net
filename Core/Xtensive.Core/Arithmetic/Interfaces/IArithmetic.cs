// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

namespace Xtensive.Arithmetic
{
  /// <summary>
  /// Provides arithmetic operations for specified type.
  /// </summary>
  /// <typeparam name="T">Type to provide arithmetic operations for.</typeparam>
  public interface IArithmetic<T> : IArithmeticBase
  {
    /// <summary>
    /// Gets "<see langword="Zero"/>" value.
    /// </summary>
    T Zero { get; }

    /// <summary>
    /// Gets "<see langword="One"/>" value.
    /// </summary>
    T One { get; }

    /// <summary>
    /// Gets the maximal value.
    /// </summary>
    T MaxValue { get;}

    /// <summary>
    /// Gets the minimal value.
    /// </summary>
    T MinValue { get;}

    /// <summary>
    /// Gets the signed flag.
    /// </summary>
    bool IsSigned { get; }

    /// <summary>
    /// Adds one value to another.
    /// </summary>
    /// <param name="value1">First summand.</param>
    /// <param name="value2">Second summand.</param>
    /// <returns>Sum of <paramref name="value1"/> and <paramref name="value2"/>.</returns>
    T Add(T value1, T value2);

    /// <summary>
    /// Gets negation.
    /// </summary>
    /// <param name="value">Value to get negation for.</param>
    /// <returns>Negation of <paramref name="value"/>.</returns>
    T Negation(T value);

    /// <summary>
    /// Multiplies value by specified factor.
    /// </summary>
    /// <param name="value">Value to multiply.</param>
    /// <param name="factor">Factor.</param>
    /// <returns>Multiplication of <paramref name="value"/> by <paramref name="factor"/>.</returns>
    T Multiply(T value, double factor);

    /// <summary>
    /// Divides value by specified factor.
    /// </summary>
    /// <param name="value">Value to divide.</param>
    /// <param name="factor">Factor.</param>
    /// <returns>Quotient of <paramref name="value"/> by <paramref name="factor"/>.</returns>
    T Divide(T value, double factor);

    /// <summary>
    /// Subtracts one value from another.
    /// </summary>
    /// <param name="value1">Value to subtract from.</param>
    /// <param name="value2">Deduction</param>
    /// <returns>Subtraction of <paramref name="value1"/> and <paramref name="value2"/>.</returns>
    T Subtract(T value1, T value2);

    /// <summary>
    /// Creates a new instance of <see cref="IArithmetic{T}"/> 
    /// with specified arithmetic rules applied.
    /// </summary>
    /// <param name="rules">Rules to apply (relatively to <see cref="ArithmeticRules"/> of this arithmetic).</param>
    /// <returns>New instance of <see cref="IArithmetic{T}"/>.</returns>
    Arithmetic<T> ApplyRules(ArithmeticRules rules);
  }
}