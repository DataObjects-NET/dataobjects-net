// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.09.03

using System;
using System.Linq;

namespace Xtensive.Sql.Model
{
  [Obsolete]
  public class CatalogHelper
  {
    public static void MoveSchemaNodes(Catalog catalog, string sourceSchemaName, string targetSchemaName)
    {
      var sourceSchema = catalog.Schemas[sourceSchemaName];
      var targetSchema = catalog.Schemas[targetSchemaName];

      ChangeSchemaOfNodeCollection(sourceSchema.Sequences, targetSchema);
      ChangeSchemaOfNodeCollection(sourceSchema.Tables, targetSchema);
      ChangeSchemaOfNodeCollection(sourceSchema.Views, targetSchema);
      ChangeSchemaOfNodeCollection(sourceSchema.Collations, targetSchema);
    }

    private static void ChangeSchemaOfNodeCollection<T>(NodeCollection<T> nodeCollection, Schema targetSchema) where T : SchemaNode
    {
      var localCollectionExpression = nodeCollection.ToList();
      foreach (var node in localCollectionExpression)
        node.Schema = targetSchema;
    }
  }
}
