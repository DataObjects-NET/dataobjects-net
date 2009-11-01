// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.09

using System;
using System.Collections.Generic;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// Structure boxing a single <see cref="Value"/>.
  /// </summary>
  /// <typeparam name="T">Type of the <see cref="Value"/>.</typeparam>
  public struct Box<T> : 
    IEquatable<Box<T>>,
    IComparable<Box<T>>
  {
    /// <summary>
    /// Boxed value.
    /// </summary>
    public readonly T Value;

    /// <inheritdoc/>
    public bool Equals(Box<T> other)
    {
      return AdvancedComparerStruct<T>.System.Equals(Value, other.Value);
    }

    /// <inheritdoc/>
    public int CompareTo(Box<T> other)
    {
      return AdvancedComparerStruct<T>.System.Compare(Value, other.Value);
    }

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj is Box<T>) {
        Box<T> other = (Box<T>)obj;
        return Equals(other);
      }
      return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Value.GetHashCode();
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return String.Format(Strings.BoxFormat, Value);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="value">Boxed value.</param>
    public Box(T value)
    {
      Value = value;
    }
  }
}