// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using Xtensive.Sql.Model;
using Xtensive.Storage.Model;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;

namespace Xtensive.Orm.Providers.Sql.Mappings
{
  /// <summary>
  /// Defines secondary index mapping.
  /// </summary>
  [Serializable]
  public sealed class SecondaryIndexMapping
  {
    public IndexInfo IndexInfo { get; private set; }

    public Index Index { get; private set; }


    // Constructors

    internal SecondaryIndexMapping(IndexInfo indexInfo, Index index)
    {
      IndexInfo = indexInfo;
      Index = index;
    }
  }
}