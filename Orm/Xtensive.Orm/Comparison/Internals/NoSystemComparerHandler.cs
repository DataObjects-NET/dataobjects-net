// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.02.10

using System;
using Xtensive.Reflection;


namespace Xtensive.Comparison
{
  internal sealed class NoSystemComparerHandler<T>:
    IComparable<T>,
    IEquatable<T>
  {
    public readonly static NoSystemComparerHandler<T> Instance = 
      new NoSystemComparerHandler<T>();

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    public int CompareTo(T other)
    {
      throw new NotSupportedException(string.Format(
        Strings.ExTypeXMustImplementY, 
        typeof(T).GetShortName(), 
        typeof(IComparable<T>).GetShortName()));
    }

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    public bool Equals(T other)
    {
      throw new NotSupportedException(string.Format(
        Strings.ExTypeXMustImplementY, 
        typeof(T).GetShortName(), 
        typeof(IEquatable<T>).GetShortName()));
    }

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    public override int GetHashCode()
    {
      throw new NotSupportedException(string.Format(
        Strings.ExTypeXMustImplementY,
        typeof(T).GetShortName(),
        typeof(IEquatable<T>).GetShortName()));
    }


    // Constructors

    private NoSystemComparerHandler()
    {
    }
  }
}