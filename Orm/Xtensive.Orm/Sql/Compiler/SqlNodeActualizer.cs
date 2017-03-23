// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.24

using System.Collections.Generic;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Compiler
{
  public sealed class SqlNodeActualizer
  {
    private readonly IDictionary<string, string> databaseMapping;
    private readonly IDictionary<string, string> schemaMapping;

    public string Actualize(Catalog catalog)
    {
      return catalog.GetActualDbName(databaseMapping);
    }

    public string Actualize(Schema schema)
    {
      return schema.GetActualDbName(schemaMapping);
    }

    internal SqlNodeActualizer(IDictionary<string, string> databaseMapping, IDictionary<string, string> schemaMapping)
    {
      this.databaseMapping = databaseMapping;
      this.schemaMapping = schemaMapping;
    }
  }
}