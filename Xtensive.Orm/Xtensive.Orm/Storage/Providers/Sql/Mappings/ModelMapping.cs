// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using System.Collections.Generic;
using Xtensive.Sql.Model;
using Xtensive.Storage.Model;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;

namespace Xtensive.Storage.Providers.Sql.Mappings
{
  /// <summary>
  /// Defines model mapping.
  /// </summary>
  [Serializable]
  public sealed class ModelMapping
  {
    private readonly Dictionary<IndexInfo, PrimaryIndexMapping> primaryIndexMappings = new Dictionary<IndexInfo, PrimaryIndexMapping>();

    public PrimaryIndexMapping this[IndexInfo primaryIndex] {
      get {
        PrimaryIndexMapping result;
        primaryIndexMappings.TryGetValue(primaryIndex, out result);
        return result;
      }
    }

    internal PrimaryIndexMapping RegisterMapping(IndexInfo primaryIndex, Table table)
    {
      var result = new PrimaryIndexMapping(primaryIndex, table);
      primaryIndexMappings[primaryIndex] = result;
      return result;
    }


    // Constructors

    internal ModelMapping()
    {
    }
  }
}