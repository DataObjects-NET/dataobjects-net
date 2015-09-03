// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.09.03

namespace Xtensive.Sql.Model
{
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
      foreach (var node in nodeCollection)
        node.Schema = targetSchema;
    }
  }
}
