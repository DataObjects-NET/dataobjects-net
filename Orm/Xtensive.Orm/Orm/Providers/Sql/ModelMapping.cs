// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Orm.Model;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// Defines model mapping.
  /// </summary>
  public sealed class ModelMapping : LockableBase
  {
    private readonly Dictionary<string, Table> tableMap = new Dictionary<string, Table>();

    public Table this[IndexInfo index] {
      get {
        Table result;
        tableMap.TryGetValue(index.MappingName, out result);
        return result;
      }
    }

    public void Register(IndexInfo primaryIndex, Table table)
    {
      this.EnsureNotLocked();
      tableMap[primaryIndex.MappingName] = table;
    }


    // Constructors

    internal ModelMapping()
    {
    }
  }
}