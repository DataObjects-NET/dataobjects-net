// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Xtensive.Core;



namespace Xtensive.Arithmetic
{
  /// <summary>
  /// Provides delegates allowing to call <see cref="IArithmetic{T}"/> methods faster.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IArithmetic{T}"/> generic argument.</typeparam>
  [Serializable]
  public sealed class Arithmetic<T> : MethodCacheBase<IArithmetic<T>>
  {
    private static Lazy<Arithmetic<T>> cached =
      new Lazy<Arithmetic<T>>(() => ArithmeticProvider.Default.GetArithmetic<T>());

    /// <summary>
    /// Gets default arithmetic for type <typeparamref name="T"/>
    /// (uses <see cref="ArithmeticProvider.Default"/> <see cref="ArithmeticProvider"/>).
    /// </summary>
    public static Arithmetic<T> Default {
      [DebuggerStepThrough]
      get {
        return cached.Value;
      }
    }

    /// <summary>
    /// Gets the provider underlying arithmetic is associated with.
    /// </summary>
    public readonly IArithmeticProvider Provider;

    /// <summary>
    /// Gets "<see langword="Zero"/>" value.
    /// </summary>
    public readonly T Zero;

    /// <summary>
    /// Gets "<see langword="One"/>" value.
    /// </summary>
    public readonly T One;

    /// <summary>
    /// Gets the maximal value.
    /// </summary>
    public readonly T MaxValue;

    /// <summary>
    /// Gets the minimal value.
    /// </summary>
    public readonly T MinValue;

    /// <summary>
    /// Gets the signed flag.
    /// </summary>
    public readonly bool IsSigned;

    /// <summary>
    /// Adds one value to another.
    /// </summary>
    public readonly Func<T, T, T> Add;

    /// <summary>
    /// Gets negation.
    /// </summary>
    public readonly Func<T, T> Negation;

    /// <summary>
    /// Multiplies value by specified factor.
    /// </summary>
    public readonly Func<T, double, T> Multiply;

    /// <summary>
    /// Divides value by specified factor.
    /// </summary>
    public readonly Func<T, double, T> Divide;

    /// <summary>
    /// Subtracts one value from another.
    /// </summary>
    public readonly Func<T, T, T> Subtract;

    /// <summary>
    /// Creates a new instance of <see cref="Arithmetic{T}"/> 
    /// with specified arithmetic rules applied.
    /// </summary>
    public readonly Func<ArithmeticRules, Arithmetic<T>> ApplyRules;


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <param name="context">The context.</param>
    public Arithmetic(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Provider = Implementation.Provider;
      Zero = Implementation.Zero;
      One = Implementation.One;
      MaxValue = Implementation.MaxValue;
      MinValue = Implementation.MinValue;
      IsSigned = Implementation.IsSigned;
      Add = Implementation.Add;
      Subtract = Implementation.Subtract;
      Multiply = Implementation.Multiply;
      Divide = Implementation.Divide;
      Negation = Implementation.Negation;
      ApplyRules = Implementation.ApplyRules;
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="implementation"><see cref="MethodCacheBase{TImplementation}.Implementation"/> property value.</param>
    public Arithmetic(IArithmetic<T> implementation)
      : base(implementation)
    {
      Provider = Implementation.Provider;
      Zero = Implementation.Zero;
      One = Implementation.One;
      MaxValue = Implementation.MaxValue;
      MinValue = Implementation.MinValue;
      IsSigned = Implementation.IsSigned;
      Add = Implementation.Add;
      Subtract = Implementation.Subtract;
      Multiply = Implementation.Multiply;
      Divide = Implementation.Divide;
      Negation = Implementation.Negation;
      ApplyRules = Implementation.ApplyRules;
    }
  }
}