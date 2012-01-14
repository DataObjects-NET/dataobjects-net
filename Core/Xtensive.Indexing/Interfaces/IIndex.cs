// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.12

using Xtensive.Collections;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Base index interface.
  /// </summary>
  public interface IIndex: ICountable
  {
    /// <summary>
    /// Clears this instance.
    /// </summary>
    void Clear();
  }
}