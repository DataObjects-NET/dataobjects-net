// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.20

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class IgnoreRulesHandler
  {
    private readonly IgnoreRuleCollection ignoreRules;
    private readonly Dictionary<string, string> databaseAlias;
    private readonly SqlExtractionResult targetModel;
    
    /// <summary>
    /// Runs handling of <see cref="IgnoreRuleCollection"/>
    /// </summary>
    /// <returns>Modified extracted model</returns>
    public SqlExtractionResult Handle()
    {
      foreach (var ignoreRule in ignoreRules) {
        var catalog = GetCatalog(GetRealName(ignoreRule.Database));
        if(catalog!=null)
          VisitCatalog(catalog, ignoreRule);
      }
      return targetModel;
    }

    private void VisitCatalog(Catalog catalog, IgnoreRule rule)
    {
      var schema = string.IsNullOrEmpty(rule.Schema) ? catalog.DefaultSchema : catalog.Schemas.FirstOrDefault(sch => sch.Name==rule.Schema);
      if(schema!=null)
        VisitSchema(schema, rule);
    }

    private void VisitSchema(Schema schema, IgnoreRule rule)
    {
      if (!string.IsNullOrEmpty(rule.Table)) {
        var table = schema.Tables.FirstOrDefault(t => t.Name==rule.Table);
        if(table!=null)
          if (!string.IsNullOrEmpty(rule.Column)) 
            VisitTable(schema.Tables, table, rule);
          else
            RemoveTable(schema.Tables, table);
      }
      else 
        foreach (var table in schema.Tables)
          VisitTable(schema.Tables, table, rule);
    }

    private void VisitTable(PairedNodeCollection<Schema,Table> tables, Table table, IgnoreRule rule)
    {
      var column = table.TableColumns.FirstOrDefault(col => col.Name==rule.Column);
      if(column!=null)
        RemoveColumn(tables, table.TableColumns, column);
    }

    private void RemoveTable(PairedNodeCollection<Schema, Table> tables, Table tableToRemove)
    {
      var fKs = tables.SelectMany(t => t.TableConstraints.OfType<ForeignKey>()).ToList();
      foreach (var fK in fKs) 
        if (fK.Owner==tableToRemove || fK.ReferencedTable==tableToRemove)
          tables.First(table=>table==fK.Owner).TableConstraints.Remove(fK);
      tables.Remove(tableToRemove);
    }

    private void RemoveColumn(PairedNodeCollection<Schema, Table> tables,PairedNodeCollection<Table, TableColumn> columns, TableColumn columnToRemove)
    {
      RemoveForeignKeys(tables, columnToRemove);
      RemoveIndexes(columnToRemove.Table, columnToRemove.Name);
      columns.Remove(columnToRemove);
    }

    private void RemoveForeignKeys(PairedNodeCollection<Schema, Table> tables, TableColumn referencedColumn)
    {
      var fKs = tables.SelectMany(t => t.TableConstraints.OfType<ForeignKey>()).ToList();
      foreach (var fK in fKs) {
        if (fK.ReferencedColumns.Contains(referencedColumn)) {
          tables.First(table=>table==fK.Owner).TableConstraints.Remove(fK);
        }
      }
    }

    private void RemoveIndexes(DataTable table, string columnName)
    {
      var indexes = table.Indexes.Select(index => index)
        .Where(elem=>elem.Columns.Count(el => el.Name==columnName)>0)
        .ToList();
      foreach (var index in indexes) {
        table.Indexes.Remove(index);
      }
    }

    private Catalog GetCatalog(string catalogName)
    {
      return (catalogName == null) ? null : targetModel.Catalogs.FirstOrDefault(cat => cat.Name == catalogName);
    }

    private string GetRealName(string nameDatabase)
    {
      var name = nameDatabase ?? "";
      string realName;
      databaseAlias.TryGetValue(name, out realName);
      if (string.IsNullOrEmpty(realName))
        if (databaseAlias.Values.Contains(name))
          return name;
        else
          return null;
      return realName;
    }

    //Constructor

    /// <summary>
    /// Creates instance of <see cref="IgnoreRuleCollection"/> handler
    /// </summary>
    /// <param name="model">Extracted model</param>
    /// <param name="configuration">Configuration of domain</param>
    public IgnoreRulesHandler(SqlExtractionResult model, DomainConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(model, "model");
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      targetModel = model;
      ignoreRules = configuration.IgnoreRules;
      if (configuration.IsMultidatabase) {
        databaseAlias = configuration.Databases
          .Where(item => !string.IsNullOrEmpty(item.RealName))
          .ToDictionary(item => item.Name, item => item.RealName);
        databaseAlias.Add("", configuration.DefaultDatabase);
      }
      else
        databaseAlias = new Dictionary<string, string> { { "", model.Catalogs[0].Name } };
    }
  }
}
