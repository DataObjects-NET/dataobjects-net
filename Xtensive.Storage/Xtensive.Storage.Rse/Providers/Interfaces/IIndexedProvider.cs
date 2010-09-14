// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.15

using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Returned as service (see <see cref="ExecutableProvider.GetService{T}"/>) 
  /// by providers that support indexed access to their records.
  /// </summary>
  public interface IIndexedProvider: 
    IOrderedProvider,
    IOrderedEnumerable<Tuple, Tuple>
  {
  }
}