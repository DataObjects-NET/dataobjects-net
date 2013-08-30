// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class IgnoreRulesHandler
  {
    private readonly IgnoreRuleCollection ignoreRules;
    private readonly SqlExtractionResult targetModel;
    private readonly MappingResolver mappingResolver;
    private readonly StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
    
    /// <summary>
    /// Runs handling of <see cref="IgnoreRuleCollection"/>
    /// </summary>
    /// <returns>Modified extracted model</returns>
    public SqlExtractionResult Handle()
    {
      foreach (var ignoreRule in ignoreRules) {
        var schema = mappingResolver.GetSchema(targetModel, ignoreRule.Database, ignoreRule.Schema);
        if(schema!=null)
          VisitSchema(schema, ignoreRule);
      }
      return targetModel;
    }

    private void VisitSchema(Schema schema, IgnoreRule rule)
    {
      if (!string.IsNullOrEmpty(rule.Table)) {
        var table = schema.Tables.FirstOrDefault(t => stringComparer.Compare(t.Name, rule.Table)==0);
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
      var column = table.TableColumns.FirstOrDefault(col => stringComparer.Compare(col.Name, rule.Column)==0);
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

    private void RemoveColumn(PairedNodeCollection<Schema, Table> tables, PairedNodeCollection<Table, TableColumn> columns, TableColumn columnToRemove)
    {
      RemoveForeignKeys(tables, columnToRemove);
      RemoveIndexes(columnToRemove.Table, columnToRemove.Name);
      columns.Remove(columnToRemove);
    }

    private void RemoveForeignKeys(PairedNodeCollection<Schema, Table> tables, TableColumn referencedColumn)
    {
      var foregnKeys = tables.SelectMany(t => t.TableConstraints.OfType<ForeignKey>()).ToList();
      foreach (var fK in foregnKeys) {
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

    //Constructor

    /// <summary>
    /// Creates instance of <see cref="IgnoreRuleCollection"/> handler
    /// </summary>
    /// <param name="model">Extracted model</param>
    /// <param name="configuration">Configuration of domain</param>
    public IgnoreRulesHandler(SqlExtractionResult model, DomainConfiguration configuration, MappingResolver resolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(model, "model");
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      targetModel = model;
      ignoreRules = configuration.IgnoreRules;
      mappingResolver = resolver;
    }
  }
}
