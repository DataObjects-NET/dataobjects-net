// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.04

using System;
using System.Diagnostics;
using System.Text;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Comparison
{
  /// <summary>
  /// Ordering rule for <see cref="IAdvancedComparer{T}"/> comparer.
  /// </summary>
  [Serializable]
  public sealed class ComparisonRules :
    IEquatable<ComparisonRules>
  {
    private readonly ComparisonRule    value;
    private readonly ComparisonRules[] composite;
    private volatile int cachedHashCode;

    /// <summary>
    /// Predefined rules with <see cref="Direction"/> = <see cref="Direction.None"/>.
    /// </summary>
    public static readonly ComparisonRules None     = new ComparisonRules(ComparisonRule.None);
    /// <summary>
    /// Predefined rules with <see cref="Direction"/> = <see cref="Direction.Positive"/>.
    /// </summary>
    public static readonly ComparisonRules Positive = new ComparisonRules(ComparisonRule.Positive);
    /// <summary>
    /// Predefined rules with <see cref="Direction"/> = <see cref="Direction.Negative"/>.
    /// </summary>
    public static readonly ComparisonRules Negative = new ComparisonRules(ComparisonRule.Negative);

    /// <summary>
    /// Gets primary <see cref="ComparisonRule"/> value.
    /// </summary>
    public ComparisonRule Value
    {
      [DebuggerStepThrough]
      get { return value; }
    }

    /// <summary>
    /// Gets tail <see cref="ComparisonRules"/>.
    /// Tail rule is considered repeating infinitely 
    /// starting from <see cref="TailIndex"/>.
    /// </summary>
    public ComparisonRules Tail
    {
      [DebuggerStepThrough]
      get { return composite[TailIndex]; }
    }

    /// <summary>
    /// Gets count of <see cref="Composite"/> rules.
    /// </summary>
    public int Count
    {
      [DebuggerStepThrough]
      get { return composite.Length; }
    }

    /// <summary>
    /// Gets <see cref="Tail"/> rule index in <see cref="Composite"/> rules.
    /// Always returns <see cref="Count"/>-1.
    /// </summary>
    public int TailIndex
    {
      [DebuggerStepThrough]
      get { return composite.Length-1; }
    }

    /// <summary>
    /// Indicates whether rule is recursive - 
    /// i.e. its <see cref="Count"/>==<see langword="1"/> and 
    /// <see cref="Composite"/><see langword="[0]"/> returns itself.
    /// </summary>
    public bool IsRecursive {
      [DebuggerStepThrough]
      get { return this==composite[0]; }
    }

    /// <summary>
    /// Gets composite rule for the specified index of composite value.
    /// </summary>
    /// <param name="index">Index of composite rule to get.</param>
    /// <returns>An instance of <see cref="ComparisonRules"/>, 
    /// if rule for specified <paramref name="index"/> is found; 
    /// otherwise, <see cref="Tail"/>.</returns>
    public ComparisonRules this[int index] {
      get {
        int c = TailIndex;
        if (index >= c)
          return composite[c];
        else
          return composite[index];
      }
    }

    /// <summary>
    /// Gets the <see cref="Value"/> of <see cref="Composite"/> rule 
    /// for the specified index of composite rule.
    /// </summary>
    /// <param name="index">Index of composite rule to get the <see cref="Value"/> of.</param>
    /// <returns><see cref="Value"/> of <see cref="Composite"/> rule, 
    /// if rule for specified <paramref name="index"/> is found; 
    /// otherwise, <see cref="Tail"/>.</returns>
    public ComparisonRule GetCompositeValue(int index)
    {
      int c = TailIndex;
      if (index >= c)
        return composite[c].Value;
      else
        return composite[index].Value;
    }

    /// <summary>
    /// Gets the <see cref="Direction"/> of the <see cref="Value"/> of <see cref="Composite"/> rule 
    /// for the specified index of composite rule.
    /// </summary>
    /// <param name="index">Index of composite rule to get the <see cref="Direction"/> of.</param>
    /// <returns><see cref="Direction"/> of the <see cref="Value"/> of <see cref="Composite"/> rule, 
    /// if rule for specified <paramref name="index"/> is found; 
    /// otherwise, <see cref="Tail"/>.</returns>
    public Direction GetDefaultRuleDirection(int index)
    {
      int c = TailIndex;
      if (index >= c)
        return composite[c].Value.Direction;
      else
        return composite[index].Value.Direction;
    }

    /// <summary>
    /// Gets a copy of internal array of composite rules
    /// that are used to order composite values.
    /// </summary>
    public ComparisonRules[] Composite {
      get {
        return composite.Copy();
      }
    }
    
    /// <summary>
    /// Combines new comparison rules with the rules described by this instance.
    /// </summary>
    /// <param name="rules">Rules to combine.</param>
    /// <returns>Result of the combination.</returns>
    public ComparisonRules Combine(ComparisonRules rules)
    {
      ComparisonRule newValue = value.Combine(rules.Value);
      int c = Count;
      if (rules.Count>c)
        c = rules.Count;
      if (c==1 && IsRecursive && rules.IsRecursive)
        return new ComparisonRules(newValue);
      ComparisonRules[] newComposite = new ComparisonRules[c];
      for (int i = 0; i < c; i++)
        newComposite[i] = this[i].Combine(rules[i]);
      return new ComparisonRules(newValue, newComposite, true);
    }

    #region IEquatable methods

    /// <inheritdoc/>
    public bool Equals(ComparisonRules other)
    {
      if (this == other)
        return true;
      if (GetHashCode() != other.GetHashCode())
        return false;
      if (!value.Equals(other.Value))
        return false;
      int c = Count;
      if (other.Count>c)
        c = other.Count;
      if (c==1 && IsRecursive && other.IsRecursive)
        return true;
      for (int i = 0; i < c; i++)
        if (!this[i].Equals(other[i]))
          return false;
      return true;
    }

    #endregion

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj is ComparisonRules)
        return Equals((ComparisonRules)obj);
      return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      if (cachedHashCode==0) lock (composite) if (cachedHashCode==0) {
        int tailIndex = TailIndex;
        int result = value.GetHashCode();
        if (tailIndex==0 && IsRecursive)
          result ^= 29 * 22; // Hash affection by IsRecursive
        else
          result ^= 29 * composite[tailIndex].GetHashCode();
        for (int i = 0; i < tailIndex; i++)
          result ^= (composite[i].GetHashCode() << i);
        cachedHashCode = result;
      }
      return cachedHashCode;
    }

    #endregion

    #region Operators, Conversion

    /// <summary>
    /// Implicit conversion of <see cref="Direction"/> to <see cref="ComparisonRules"/>.
    /// </summary>
    /// <param name="direction">The direction to convert.</param>
    /// <returns>Conversion result.</returns>
    public static implicit operator ComparisonRules(Direction direction)
    {
      switch (direction) {
      case Direction.Positive:
        return Positive;
      case Direction.Negative:
        return Negative;
      case Direction.None:
        return None;
      default:
        throw Exceptions.InvalidArgument(direction, "direction");
      }
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder(32);
      int c = Count;
      if (c==1 && IsRecursive)
        sb.Append(Strings.Recursive);
      else {
        for (int i = 0; i < c; i++) {
          if (i > 0)
            sb.Append(", ");
          sb.Append(composite[i]);
        }
      }
      return string.Format(Strings.ComparisonRulesFormat, value, sb);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="value">Initial <see cref="Value"/> property value.</param>
    public ComparisonRules(ComparisonRule value)
      : this (value, new ComparisonRules[1], true)
    {
      composite[0] = this;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="value">Initial <see cref="Value"/> property value.</param>
    /// <param name="composite">Initial <see cref="Composite"/> property value.
    /// Last composite rule is considered as <see cref="Tail"/> rule.</param>
    public ComparisonRules(ComparisonRule value, params ComparisonRules[] composite)
      : this (value, composite, composite[composite.Length-1])
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="value">Initial <see cref="Value"/> property value.</param>
    /// <param name="composite">Initial <see cref="Composite"/> property value.</param>
    /// <param name="tail">Tail comparison rule (repeated infinitely after composite rules).</param>
    public ComparisonRules(ComparisonRule value, ComparisonRules[] composite, ComparisonRules tail)
    {
      if (value.Direction==Direction.None)
        throw Exceptions.InvalidArgument(value.Direction, "value.Direction");
      ArgumentValidator.EnsureArgumentNotNull(tail, "tail");
      this.value = value;
      if (composite!=null) {
        int tailIndex = composite.Length-1;
        if (composite[tailIndex]!=tail)
          tailIndex++;
        int count = tailIndex + 1;
        this.composite = new ComparisonRules[count];
        for (int i = 0; i < tailIndex; i++)
          this.composite[i] = composite[i].Combine(value);
        this.composite[tailIndex] = tail.Combine(value);
      }
      else {
        this.composite = new ComparisonRules[1];
        this.composite[0] = tail.Combine(value);
      }
    }

    private ComparisonRules(ComparisonRule value, ComparisonRules[] composite, bool ignore)
    {
      this.value  = value;
      this.composite = composite;
    }
  }
}