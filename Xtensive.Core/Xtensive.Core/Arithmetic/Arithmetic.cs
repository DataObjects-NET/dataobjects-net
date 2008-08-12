// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Arithmetic
{
  /// <summary>
  /// Provides delegates allowing to call <see cref="IArithmetic{T}"/> methods faster.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IArithmetic{T}"/> generic argument.</typeparam>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  public class Arithmetic<T> : MethodCacheBase<IArithmetic<T>>
  {
    private static readonly object _lock = new object();
    private static volatile Arithmetic<T> @default;

    /// <summary>
    /// Gets default arithmetic for type <typeparamref name="T"/>
    /// (uses <see cref="ArithmeticProvider.Default"/> <see cref="ArithmeticProvider"/>).
    /// </summary>
    public static Arithmetic<T> Default {
      [DebuggerStepThrough]
      get {
        if (@default==null) lock (_lock) if (@default==null) {
          try {
            @default = ArithmeticProvider.Default.GetArithmetic<T>();
          }
          catch {
          }
        }
        return @default;
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
    public Func<ArithmeticRules, Arithmetic<T>> ApplyRules;


    // Constructors

    public Arithmetic(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Provider = Implementation.Provider;
      Zero = Implementation.Zero;
      One = Implementation.One;
      Add = Implementation.Add;
      Subtract = Implementation.Subtract;
      Multiply = Implementation.Multiply;
      Divide = Implementation.Divide;
      Negation = Implementation.Negation;
      ApplyRules = Implementation.ApplyRules;
    }

    public Arithmetic(IArithmetic<T> implementation)
      : base(implementation)
    {
      Provider = Implementation.Provider;
      Zero = Implementation.Zero;
      One = Implementation.One;
      Add = Implementation.Add;
      Subtract = Implementation.Subtract;
      Multiply = Implementation.Multiply;
      Divide = Implementation.Divide;
      Negation = Implementation.Negation;
      ApplyRules = Implementation.ApplyRules;
    }
  }
}