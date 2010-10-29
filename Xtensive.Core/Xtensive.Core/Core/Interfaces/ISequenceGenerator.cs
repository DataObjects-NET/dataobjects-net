// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.11.02

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// Generates a sequence of values of <typeparamref name="T"/> type.
  /// </summary>
  /// <typeparam name="T">Type of value in sequence to produce.</typeparam>
  public interface ISequenceGenerator<T>
    where T: IComparable<T>, IEquatable<T>
  {
    /// <summary>
    /// Produces next value in the suequence.
    /// </summary>
    /// <returns>Next generated value.</returns>
    T Next();
  }
}