// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.15

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Describes the result of internal seek (or seek-like) operations.
  /// </summary>
  /// <typeparam name="TPointer">The type of the pointer.</typeparam>
  public struct SeekResultPointer<TPointer> :
    IEquatable<SeekResultPointer<TPointer>>
  {
    /// <summary>
    /// Type of the result.
    /// </summary>
    public SeekResultType ResultType;

    /// <summary>
    /// The pointer to the result.
    /// </summary>
    public TPointer Pointer;

    #region Equals, GetHashCode methods

    /// <inheritdoc/>
    public bool Equals(SeekResultPointer<TPointer> obj)
    {
      return obj.ResultType==ResultType && Equals(obj.Pointer, Pointer);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (SeekResultPointer<TPointer>))
        return false;
      return Equals((SeekResultPointer<TPointer>) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        return (ResultType.GetHashCode() * 397) ^ Pointer.GetHashCode();
      }
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.SeekResultPointerFormat, Pointer, ResultType);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="resultType">Type of the result.</param>
    /// <param name="pointer">The pointer to the result.</param>
    public SeekResultPointer(SeekResultType resultType, TPointer pointer)
    {
      ResultType = resultType;
      Pointer = pointer;
    }
  }
}