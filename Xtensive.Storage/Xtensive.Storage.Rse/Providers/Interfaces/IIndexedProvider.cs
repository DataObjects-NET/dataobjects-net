// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.15

using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers
{
  public interface IIndexedProvider: 
    IOrderedProvider,
    IOrderedEnumerable<Tuple, Tuple>
  {
  }
}