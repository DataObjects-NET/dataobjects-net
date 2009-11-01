// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created: 7 θώνÿ 2007 γ.


using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Characterizes implementing collection as read-only.
  /// </summary>
  public interface IReadOnlyCollection: ICollection
  {
  }

  /// <summary>
  /// Characterizes implementing collection as read-only.
  /// </summary>
  public interface IReadOnlyCollection<T>: ICollection<T>
  {
  }
}