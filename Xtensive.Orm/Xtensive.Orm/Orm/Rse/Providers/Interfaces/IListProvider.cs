// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.15

using Xtensive.Collections;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Returned as service (see <see cref="ExecutableProvider.GetService{T}"/>) 
  /// by providers supporting random access to their items.
  /// </summary>
  public interface IListProvider: ICountable
  {
    /// <summary>
    /// Gets the item by its index.
    /// </summary>
    /// <param name="index">The index to get the item at.</param>
    /// <returns>The item at the specified index.</returns>
    Tuple GetItem(int index);
  }
}