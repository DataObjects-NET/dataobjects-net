// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Internals.DocTemplates;
using A=Xtensive.Arithmetic;

namespace Xtensive.Arithmetic
{
  /// <summary>
  /// A struct providing faster access for key 
  /// <see cref="Xtensive.Arithmetic.Arithmetic{T}"/> delegates.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IArithmetic{T}"/> generic argument.</typeparam>
  [Serializable]
  public struct ArithmeticStruct<T> : ISerializable
  {
    /// <summary>
    /// Gets <see cref="A.ArithmeticStruct{T}"/> for 
    /// <see cref="A.Arithmetic{T}.Default"/> arithmetic.
    /// </summary>
    public static readonly ArithmeticStruct<T> Default = new ArithmeticStruct<T>(Arithmetic<T>.Default);

    /// <summary>
    /// Gets the underlying arithmetic for this cache.
    /// </summary>
    public readonly Arithmetic<T> Arithmetic;

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
    /// Creates a new instance of <see cref="A.Arithmetic{T}"/> 
    /// with specified arithmetic rules applied.
    /// </summary>
    public Func<ArithmeticRules, Arithmetic<T>> ApplyRules;

    /// <summary>
    /// Implicit conversion of <see cref="A.Arithmetic{T}"/> to 
    /// <see cref="ArithmeticStruct{T}"/>.
    /// </summary>
    /// <param name="arithmetic">Arithmetic to provide the struct for.</param>
    public static implicit operator ArithmeticStruct<T>(Arithmetic<T> arithmetic)
    {
      return new ArithmeticStruct<T>(arithmetic);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="arithmetic">Arithmetic to provide the delegates for.</param>
    private ArithmeticStruct(Arithmetic<T> arithmetic)
    {
      Arithmetic = arithmetic;
      Zero = Arithmetic==null ? default(T) : Arithmetic.Zero;
      One = Arithmetic==null ? default(T) : Arithmetic.One;
      MinValue = Arithmetic == null ? default(T) : Arithmetic.MinValue;
      MaxValue = Arithmetic == null ? default(T) : Arithmetic.MaxValue;
      IsSigned = Arithmetic == null ? true : Arithmetic.IsSigned;
      Add = Arithmetic==null ? null : Arithmetic.Add;
      Subtract = Arithmetic==null ? null : Arithmetic.Subtract;
      Multiply = Arithmetic==null ? null : Arithmetic.Multiply;
      Divide = Arithmetic==null ? null : Arithmetic.Divide;
      Negation = Arithmetic==null ? null : Arithmetic.Negation;
      ApplyRules = Arithmetic==null ? null : Arithmetic.ApplyRules;

    }

    /// <summary>
    /// Deserializes the instance of this class.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    private ArithmeticStruct(SerializationInfo info, StreamingContext context)
    {
      Arithmetic = (Arithmetic<T>)info.GetValue("Arithmetic", typeof (Arithmetic<T>));
      Zero = Arithmetic==null ? default(T) : Arithmetic.Zero;
      One = Arithmetic==null ? default(T) : Arithmetic.One;
      MinValue = Arithmetic == null ? default(T) : Arithmetic.MinValue;
      MaxValue = Arithmetic == null ? default(T) : Arithmetic.MaxValue;
      IsSigned = Arithmetic == null ? true : Arithmetic.IsSigned;
      Add = Arithmetic==null ? null : Arithmetic.Add;
      Subtract = Arithmetic==null ? null : Arithmetic.Subtract;
      Multiply = Arithmetic==null ? null : Arithmetic.Multiply;
      Divide = Arithmetic==null ? null : Arithmetic.Divide;
      Negation = Arithmetic==null ? null : Arithmetic.Negation;
      ApplyRules = Arithmetic==null ? null : Arithmetic.ApplyRules;
    }

    /// <inheritdoc/>
    [SecurityCritical]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Arithmetic", Arithmetic);
    }
  }
}