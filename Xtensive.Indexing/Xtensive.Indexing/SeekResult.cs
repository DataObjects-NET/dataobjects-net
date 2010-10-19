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
  /// Result of <see cref="IOrderedEnumerable{TKey,TItem}.Seek"/> operation.
  /// </summary>
  /// <typeparam name="TItem">Type of the item.</typeparam>
  public struct SeekResult<TItem> :
    IEquatable<SeekResult<TItem>>
  {
    /// <summary>
    /// Seek operation status.
    /// </summary>
    public readonly SeekResultType ResultType;

    /// <summary>
    /// Seek operation result.
    /// If <see cref="ResultType"/> is <see cref="SeekResultType.None"/>,
    /// <see cref="Result"/> has no meaning.
    /// </summary>
    public readonly TItem Result;

    #region Equals, GetHashCode methods

    /// <inheritdoc/>
    public bool Equals(SeekResult<TItem> obj)
    {
      return obj.ResultType==ResultType && Equals(obj.Result, Result);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (SeekResult<TItem>))
        return false;
      return Equals((SeekResult<TItem>) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        return (ResultType.GetHashCode() * 397) ^ Result.GetHashCode();
      }
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.SeekResultFormat, Result, ResultType);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="resultType">Seek operation status.</param>
    /// <param name="result">Seek operation result.</param>
    public SeekResult(SeekResultType resultType, TItem result)
    {
      ResultType = resultType;
      Result = result;
    }
  }
}