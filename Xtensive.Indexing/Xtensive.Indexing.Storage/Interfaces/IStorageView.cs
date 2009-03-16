// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using Xtensive.Core.Tuples;
using Xtensive.Indexing.Storage.Model;
using Xtensive.Integrity.Transactions;

namespace Xtensive.Indexing.Storage.Interfaces
{
  public interface IStorageView : 
    IIndexManager,
    IIndexAccessor
  {
    ITransaction Transaction { get; }
  }
}