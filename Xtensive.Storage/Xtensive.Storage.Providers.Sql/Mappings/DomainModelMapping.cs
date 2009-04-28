// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using System.Collections.Generic;
using Xtensive.Sql.Dom.Database;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Sql.Mappings
{
  [Serializable]
  public sealed class DomainModelMapping
  {
    private readonly Dictionary<IndexInfo, PrimaryIndexMapping> primaryIndexMappings = new Dictionary<IndexInfo, PrimaryIndexMapping>();

    internal PrimaryIndexMapping RegisterMapping(IndexInfo primaryIndex, Table table)
    {
      var result = new PrimaryIndexMapping(primaryIndex, table);
      primaryIndexMappings[primaryIndex] = result;
      return result;
    }

    public PrimaryIndexMapping this[IndexInfo primaryIndex]
    {
      get
      {
        PrimaryIndexMapping result;
        primaryIndexMappings.TryGetValue(primaryIndex, out result);
        return result;
      }
    }


    // Constructor

    internal DomainModelMapping()
    {
    }
  }
}