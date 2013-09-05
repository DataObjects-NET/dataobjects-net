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
    public Dictionary<string, string> LockedTablesByForeignKey { get; set; }
    public Dictionary<string, string> LockedTablesByUserColumn { get; set; } 
    public NodeCollection<Catalog> Catalogs { get; set; }

    public SqlExtractionResult ToSqlExtractionResult()
    {
      return new SqlExtractionResult(){Catalogs = this.Catalogs};
    }

    public SchemaExtractionResult()
    {
      LockedTablesByUserColumn = new Dictionary<string, string>();
      LockedTablesByForeignKey = new Dictionary<string, string>();
      Catalogs = new NodeCollection<Catalog>();
    }

    public SchemaExtractionResult(SqlExtractionResult sqlExtractionResult)
    {
      LockedTablesByUserColumn = new Dictionary<string, string>();
      LockedTablesByForeignKey = new Dictionary<string, string>();
      Catalogs = sqlExtractionResult.Catalogs;
    }
  }
}
