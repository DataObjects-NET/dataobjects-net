// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.06

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Collections;
using Xtensive.Core;



namespace Xtensive.Arithmetic
{
  /// <summary>
  /// Base class for <see cref="IArithmetic{T}"/> implementations.
  /// </summary>
  /// <typeparam name="T">Type to provide arithmetic operations for.</typeparam>
  [Serializable]
  public abstract class ArithmeticBase<T> : IArithmetic<T>,
    IDeserializationCallback
  {
    private IArithmeticProvider provider;
    
    [NonSerialized] 
    private ThreadSafeDictionary<ArithmeticRules, Arithmetic<T>> cachedArithmetics =
      ThreadSafeDictionary<ArithmeticRules, Arithmetic<T>>.Create(new object());

    /// <summary>
    /// Indicates whether overflow is allowed (doesn't lead to an exception)
    /// on arithmetic operations.
    /// </summary>
    [NonSerialized] 
    protected bool OverflowAllowed;

    /// <summary>
    /// Indicates whether <see langword="null"/> value is threated as zero
    /// in arithmetic operations.
    /// </summary>
    [NonSerialized] 
    protected bool NullIsZero;

    /// <summary>
    /// Gets <see cref="ArithmeticRules"/> used by this arithmetic.
    /// </summary>
    protected readonly ArithmeticRules Rules;

    /// <inheritdoc/>
    public IArithmeticProvider Provider
    {
      [DebuggerStepThrough]
      get { return provider; }
    }

    /// <inheritdoc/>
    public abstract T Zero { get; }

    /// <inheritdoc/>
    public abstract T One { get; }

    /// <inheritdoc/>
    public abstract T MaxValue { get; }

    /// <inheritdoc/>
    public abstract T MinValue { get; }

    /// <inheritdoc/>
    public abstract bool IsSigned { get; }

    /// <inheritdoc/>
    public abstract T Add(T value1, T value2);

    /// <inheritdoc/>
    public abstract T Subtract(T value1, T value2);

    /// <summary>
    /// Creates a new instance of <see cref="IArithmetic{T}"/> 
    /// with specified arithmetic rules applied.
    /// </summary>
    /// <param name="rules">Rules to apply (relatively to <see cref="ArithmeticRules"/> of this arithmetic).</param>
    /// <returns>New instance of <see cref="IArithmetic{T}"/>.</returns>
    public Arithmetic<T> ApplyRules(ArithmeticRules rules)
    {
      return cachedArithmetics.GetValue(rules, 
        (_rules, _this) => new Arithmetic<T>(_this.CreateNew(_rules)), 
        this);
    }

    /// <summary>
    /// Creates new arithmetic of the same type, but using different arithmetic rules.
    /// </summary>
    /// <param name="rules">Arithmetic rules for the new arithmetic (relatively to this one).</param>
    /// <returns>New arithmetic of the same type, but using different arithmetic rules.</returns>
    protected abstract IArithmetic<T> CreateNew(ArithmeticRules rules);

    /// <inheritdoc/>
    public abstract T Negation(T value);

    /// <inheritdoc/>
    public abstract T Multiply(T value, double factor);

    /// <inheritdoc/>
    public abstract T Divide(T value, double factor);


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="provider">Arithmetic provider this arithmetic is bound to.</param>
    /// <param name="rules">Arithmetic rules.</param>
    public ArithmeticBase(IArithmeticProvider provider, ArithmeticRules rules)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      this.provider = provider;
      Rules = rules;
      OverflowAllowed = (rules.OverflowBehavior==OverflowBehavior.AllowOverflow);
      NullIsZero = (rules.NullBehavior==NullBehavior.ThreatNullAsZero);
    }

    /// <summary>
    /// Performs post-deserialization actions.
    /// </summary>
    /// <param name="sender"></param>
    public virtual void OnDeserialization(object sender)
    {
      if (provider==null || provider.GetType()==typeof (ArithmeticProvider))
        provider = ArithmeticProvider.Default;
      OverflowAllowed = (Rules.OverflowBehavior==OverflowBehavior.AllowOverflow);
      NullIsZero = (Rules.NullBehavior==NullBehavior.ThreatNullAsZero);
      cachedArithmetics = ThreadSafeDictionary<ArithmeticRules, Arithmetic<T>>.Create(new object());
    }
  }
}