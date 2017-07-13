// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.06.12


using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade.Model;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Default <see cref="IFullTextCatalogResolver"/> implementation.
  /// </summary>
  public class FullTextCatalogResolver : IFullTextCatalogResolver
  {
    private const char NamePartsDelimeter = ':';

    private readonly string defaultDatabase;
    private readonly string defaultSchema;

    public DomainConfiguration DomainConfiguration { get; private set; }

    public NodeConfiguration NodeConfiguration { get; private set; }

    /// <inheritdoc />
    public virtual bool IsEnabled
    {
      get { return true; }
    }

    /// <inheritdoc />
    public string Resolve(Orm.Model.TypeInfo typeInfo, TableInfo tableInfo)
    {
      var nameParts = GetNameParts(tableInfo.Name);
      return Resolve(typeInfo, nameParts[0], nameParts[1], nameParts[2]);
    }

    protected virtual string Resolve(Orm.Model.TypeInfo typeInfo, string databaseName, string schemaName, string typeName)
    {
      return null;
    }

    private string[] GetNameParts(string complexName)
    {
      var tableName = string.Empty;
      var schemaName = defaultSchema;
      var databaseName = defaultDatabase;

      var nameParts = complexName.Split(NamePartsDelimeter);
      if (nameParts.Length == 3) {
        tableName = nameParts[2];
        schemaName = nameParts[1];
        databaseName = nameParts[0];
      }
      else if (nameParts.Length==2) {
        tableName = nameParts[1];
        schemaName = nameParts[0];
      }
      else {
        tableName = nameParts[0];
      }
      return new[] { databaseName, schemaName, tableName };
    }

    public FullTextCatalogResolver()
    {
      var context = UpgradeContext.Demand();
      DomainConfiguration = context.Configuration;
      NodeConfiguration = context.NodeConfiguration;
      defaultDatabase = context.DefaultSchemaInfo.Database;
      defaultSchema = context.DefaultSchemaInfo.Schema;
    }
  }
}
