// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.20

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class IgnoreRulesHandler
  {
    private const string maskSymbol = "*";
    private readonly IgnoreRuleCollection ignoreRules;
    private readonly SchemaExtractionResult targetModel;
    private readonly MappingResolver mappingResolver;
    private readonly StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
    
    /// <summary>
    /// Runs handling of <see cref="IgnoreRuleCollection"/>
    /// </summary>
    /// <returns>Modified extracted model</returns>
    public SchemaExtractionResult Handle()
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
      if (!string.IsNullOrEmpty(rule.Table) && !IsOnlyMaskSymbol(rule.Table)) {
        if(IsContainsMaskSymbol(rule.Table)) {
          var matchedTables = schema.Tables.Where(t => IsMatch(t.Name, rule.Table)).ToList();
          foreach (var table in matchedTables) {
            RemoveTable(schema.Tables, table);
          }
        }
        else {
          var table = schema.Tables[rule.Table];
          if(table!=null)
            if (!string.IsNullOrEmpty(rule.Column) && !IsOnlyMaskSymbol(rule.Column)) 
              VisitTable(schema.Tables, table, rule);
            else
              RemoveTable(schema.Tables, table);
        }
      }
      else 
        foreach (var table in schema.Tables)
          VisitTable(schema.Tables, table, rule);
    }

    private void VisitTable(PairedNodeCollection<Schema,Table> tables, Table table, IgnoreRule rule)
    {
      if(IsContainsMaskSymbol(rule.Column)) {
        var matchedColumns = table.TableColumns.Where(c => IsMatch(c.Name, rule.Column)).ToList();
        foreach (var matchedColumn in matchedColumns) {
          RemoveColumn(tables, table.TableColumns,matchedColumn);
        }
      }
      else {
        var column = table.TableColumns[rule.Column];
        if(column!=null)
          RemoveColumn(tables, table.TableColumns, column);
      }
    }

    private void RemoveTable(PairedNodeCollection<Schema, Table> tables, Table tableToRemove)
    {
      var foreignKeys = tables.SelectMany(t => t.TableConstraints.OfType<ForeignKey>()).ToList();
      foreach (var foreignKey in foreignKeys)
        if (foreignKey.Owner==tableToRemove || foreignKey.ReferencedTable==tableToRemove) {
          if (foreignKey.Owner==tableToRemove) {
            var resolvedTableName = mappingResolver.GetNodeName(foreignKey.ReferencedTable);
            if (!targetModel.LockedTables.ContainsKey(resolvedTableName))
              targetModel.LockedTables.Add(resolvedTableName,string.Format(Strings.ExTableXCantBeRemovedDueToForeignKeyYOfIgnoredTableOrColumn, foreignKey.ReferencedTable.Name, foreignKey.Name));
          }
          tables.First(table => table==foreignKey.Owner).TableConstraints.Remove(foreignKey);
        }
      tables.Remove(tableToRemove);
    }

    private void RemoveColumn(PairedNodeCollection<Schema, Table> tables, PairedNodeCollection<Table, TableColumn> columns, TableColumn columnToRemove)
    {
      RemoveForeignKeys(tables, columnToRemove);
      RemoveIndexes(columnToRemove.Table, columnToRemove.Name);
      var resolvedTableName = mappingResolver.GetNodeName(columnToRemove.Table);
      if(!targetModel.LockedTables.ContainsKey(resolvedTableName))
        targetModel.LockedTables.Add(resolvedTableName, string.Format(Strings.ExTableXCantBeRemovedDueToTheIgnoredColumnY, columnToRemove.Table.Name, columnToRemove.Name));
      columns.Remove(columnToRemove);
    }

    private void RemoveForeignKeys(PairedNodeCollection<Schema, Table> tables, TableColumn referencedColumn)
    {
      var foregnKeys = tables.SelectMany(t => t.TableConstraints.OfType<ForeignKey>()).ToList();
      foreach (var foreignKey in foregnKeys) {
        if (foreignKey.ReferencedColumns.Contains(referencedColumn))
          tables.First(table=>table==foreignKey.Owner).TableConstraints.Remove(foreignKey);

        if (foreignKey.Columns.Contains(referencedColumn)) {
          if (foreignKey.Owner==referencedColumn.Table) {
            var resolvedTableName = mappingResolver.GetNodeName(foreignKey.ReferencedTable);
            if (!targetModel.LockedTables.ContainsKey(resolvedTableName))
              targetModel.LockedTables.Add(resolvedTableName, string.Format(Strings.ExTableXCantBeRemovedDueToForeignKeyYOfIgnoredTableOrColumn, foreignKey.ReferencedTable.Name, foreignKey.Name));
          }
          tables.First(table=>table==foreignKey.Owner).TableConstraints.Remove(foreignKey);
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

    private bool IsMatch(string value, string pattern)
    {
      var regexp = pattern.Replace("*", ".*");
      return Regex.IsMatch(value, regexp);
    }

    private bool IsContainsMaskSymbol(string name)
    {
      return name.Contains(maskSymbol);
    }

    private bool IsOnlyMaskSymbol(string name)
    {
      return (name.Contains(maskSymbol) && name.Length==1);
    }

    //Constructor

    /// <summary>
    /// Creates instance of <see cref="IgnoreRuleCollection"/> handler
    /// </summary>
    /// <param name="model">Extracted model</param>
    /// <param name="configuration">Configuration of domain</param>
    public IgnoreRulesHandler(SchemaExtractionResult model, DomainConfiguration configuration, MappingResolver resolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(model, "model");
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      targetModel = model;
      ignoreRules = configuration.IgnoreRules;
      mappingResolver = resolver;
    }
  }
}
