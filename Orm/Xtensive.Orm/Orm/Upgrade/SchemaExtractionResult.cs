// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.02

using System.Collections.Generic;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class SchemaExtractionResult
  {
    public Dictionary<string, string> LockedTables { get; set; } 
    public NodeCollection<Catalog> Catalogs { get; set; }

    public SchemaExtractionResult MakeNamesUnreadable()
    {
      foreach (var catalog in Catalogs)
        catalog.MakeNamesUnreadable();
      return this;
    }

    public SchemaExtractionResult()
    {
      LockedTables = new Dictionary<string, string>();
      Catalogs = new NodeCollection<Catalog>();
    }

    public SchemaExtractionResult(SqlExtractionResult sqlExtractionResult)
    {
      LockedTables = new Dictionary<string, string>();
      Catalogs = sqlExtractionResult.Catalogs;
    }
  }
}
