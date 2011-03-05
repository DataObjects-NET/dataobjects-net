// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.20

namespace Xtensive.Comparison
{
  /// <summary>
  /// Tagging interface specifying that comparer overrides
  /// <see cref="SystemComparer{T}"/> for type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">Type to override the system comparer for.</typeparam>
  public interface ISystemComparer<T>: IAdvancedComparer<T>
  {
  }
}