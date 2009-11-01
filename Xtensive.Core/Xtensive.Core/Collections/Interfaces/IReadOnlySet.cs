// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created: 7 θώνÿ 2007 γ.


namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Characterizes implementing set as read-only.
  /// </summary>
  public interface IReadOnlySet
    : ISet,
      IReadOnlyCollection
  {
  }

  /// <summary>
  /// Characterizes implementing set as read-only.
  /// </summary>
  public interface IReadOnlySet<T>
    : ISet<T>,
      IReadOnlyCollection<T>
  {
  }
}