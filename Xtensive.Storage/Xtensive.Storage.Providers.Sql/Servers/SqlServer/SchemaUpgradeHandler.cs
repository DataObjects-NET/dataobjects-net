// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.07.04

using Xtensive.Sql.Model;

namespace Xtensive.Storage.Providers.Sql.Servers.SqlServer
{
  /// <summary>
  /// A schema upgrade handler specific to Microsoft SQL Server RDBMS.
  /// </summary>
  public class SchemaUpgradeHandler : Sql.SchemaUpgradeHandler
  {
    protected override void SaveNativeExtractedSchema(object schema)
    {
      // We must remove "sysdiagrams" table here
      var typedSchema = (Schema) schema;
      var tables = typedSchema.Tables;
      var sysdiagrams = tables["sysdiagrams"];
      if (sysdiagrams!=null)
        tables.Remove(sysdiagrams);

      base.SaveNativeExtractedSchema(typedSchema);
    }
  }
}