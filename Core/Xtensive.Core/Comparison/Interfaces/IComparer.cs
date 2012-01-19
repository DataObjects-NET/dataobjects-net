// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.03

namespace Xtensive.Comparison
{
  /// <summary>
  /// Coompares type <typeparamref name="TX"/> with <typeparamref name="TY"/>.
  /// </summary>
  public interface IComparer<TX, TY>
  {
    /// <summary>
    /// Compares <paramref name="x"/> and <paramref name="y"/>.
    /// </summary>
    /// <param name="x">First value to compare.</param>
    /// <param name="y">Second value to compare.</param>
    /// <returns><see langword="-1"/> if <c>x &lt; y</c>, <see langword="1"/> if <c>x &gt; y</c>; 
    /// otherwise, <see langword="0"/>.</returns>
    int Compare(TX x, TY y);
  }
}