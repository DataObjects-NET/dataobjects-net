// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Model;
using IndexInfo=Xtensive.Storage.Model.IndexInfo;
using StorageIndexInfo = Xtensive.Storage.Indexing.Model.IndexInfo;
using TypeInfo=Xtensive.Storage.Model.TypeInfo;

namespace Xtensive.Storage.Providers.Memory
{
  public class DomainHandler : Index.DomainHandler
  {
    /// <inheritdoc/>
    protected override Index.IndexStorage CreateLocalStorage(string name)
    {
      return new MemoryIndexStorage(name);
    }
  }
}