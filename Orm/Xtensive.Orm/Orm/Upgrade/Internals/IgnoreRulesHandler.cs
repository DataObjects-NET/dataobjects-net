// Copyright (C) 2013-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.08.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class IgnoreRulesHandler
  {
    private readonly IgnoreRuleCollection ignoreRules;
    private readonly SchemaExtractionResult targetModel;
    private readonly MappingResolver mappingResolver;
    private readonly StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

    private List<ForeignKey> foreignKeysOfSchema;

    /// <summary>
    /// Runs handling of <see cref="IgnoreRuleCollection"/>
    /// </summary>
    /// <returns>Modified extracted model</returns>
    public SchemaExtractionResult Handle()
    {
      foreach (var ignoreRule in ignoreRules) {
        var schema = mappingResolver.ResolveSchema(targetModel, ignoreRule.Database, ignoreRule.Schema);
        if (schema != null) {
          VisitSchema(schema, ignoreRule);
        }
      }
      return targetModel;
    }

    private void VisitSchema(Schema schema, IgnoreRule rule)
    {
      foreignKeysOfSchema = schema.Tables.SelectMany(t => t.TableConstraints.OfType<ForeignKey>()).ToList();
      var matcher = RuleMatcher<Table>.Create(rule.Table);
      var matchedTables = matcher.Get(schema.Tables);
      if (!MatchingHelper.IsMatchAll(rule.Column) || !MatchingHelper.IsMatchAll(rule.Index)) {
        foreach (var table in matchedTables) {
          VisitTable(table, rule);
        }
      }
      else {
        foreach (var table in matchedTables) {
          RemoveTable(schema.Tables, table);
        }
      }
    }

    private void VisitTable(Table table, IgnoreRule rule)
    {
      var isColumnRule = !string.IsNullOrEmpty(rule.Column);
      if (isColumnRule) {
        var matcher = RuleMatcher<TableColumn>.Create(rule.Column);
        var matchedColumns = matcher.Get(table.TableColumns);
        foreach (var matchedColumn in matchedColumns) {
          RemoveColumn(table.TableColumns, matchedColumn);
        }
      }
      else {
        var matcher = RuleMatcher<Xtensive.Sql.Model.Index>.Create(rule.Index);
        var matchedIndexes = matcher.Get(table.Indexes);
        foreach (var matchedIndex in matchedIndexes) {
          RemoveIndex(table.Indexes, matchedIndex);
        }
      }
      
    }

    private void RemoveTable(PairedNodeCollection<Schema, Table> tables, Table tableToRemove)
    {
      foreach (var foreignKey in foreignKeysOfSchema) {
        if (foreignKey.Owner == tableToRemove || foreignKey.ReferencedTable == tableToRemove) {
          if (foreignKey.Owner == tableToRemove) {
            var resolvedTableName = mappingResolver.GetNodeName(foreignKey.ReferencedTable);
            if (!targetModel.LockedTables.ContainsKey(resolvedTableName)) {
              targetModel.LockedTables.Add(resolvedTableName,
                string.Format(Strings.ExTableXCantBeRemovedDueToForeignKeyYOfIgnoredTableOrColumn,
                  foreignKey.ReferencedTable.Name, foreignKey.Name));
            }
          }
          _ = foreignKey.Owner.TableConstraints.Remove(foreignKey);
        }
      }

      _ = tables.Remove(tableToRemove);
    }

    private void RemoveColumn(PairedNodeCollection<Table, TableColumn> columns, TableColumn columnToRemove)
    {
      RemoveForeignKeys(columnToRemove);
      RemoveIndexes(columnToRemove.Table, columnToRemove.Name);

      var resolvedTableName = mappingResolver.GetNodeName(columnToRemove.Table);
      if (!targetModel.LockedTables.ContainsKey(resolvedTableName)) {
        targetModel.LockedTables.Add(resolvedTableName,
          string.Format(Strings.ExTableXCantBeRemovedDueToTheIgnoredColumnY,
            columnToRemove.Table.Name, columnToRemove.Name));
      }
      _ = columns.Remove(columnToRemove);
    }

    private void RemoveIndex(PairedNodeCollection<DataTable, Xtensive.Sql.Model.Index> indexes, Xtensive.Sql.Model.Index indexToRemove)
    {
      var resolvedTableName = mappingResolver.GetNodeName((Table)indexToRemove.DataTable);
      if (!targetModel.LockedTables.ContainsKey(resolvedTableName)) {
        targetModel.LockedTables.Add(resolvedTableName,
          string.Format(Strings.ExTableXCantBeRemovedDueToTheIgnoredColumnY,
            ((Table) indexToRemove.DataTable).Name, indexToRemove.Name));
      }
      _ = indexes.Remove(indexToRemove);
    }

    private void RemoveForeignKeys(TableColumn referencedColumn)
    {
      foreach (var foreignKey in foreignKeysOfSchema) {
        if (foreignKey.ReferencedColumns.Contains(referencedColumn)) {
          _ = foreignKey.Owner.TableConstraints.Remove(foreignKey);
        }

        if (foreignKey.Columns.Contains(referencedColumn)) {
          if (foreignKey.Owner == referencedColumn.Table) {
            var resolvedTableName = mappingResolver.GetNodeName(foreignKey.ReferencedTable);
            if (!targetModel.LockedTables.ContainsKey(resolvedTableName)) {
              targetModel.LockedTables.Add(resolvedTableName,
                string.Format(Strings.ExTableXCantBeRemovedDueToForeignKeyYOfIgnoredTableOrColumn,
                  foreignKey.ReferencedTable.Name, foreignKey.Name));
            }
          }
          _ = foreignKey.Owner.TableConstraints.Remove(foreignKey);
        }
      }
    }

    private void RemoveIndexes(DataTable table, string columnName)
    {
      var indexes = table.Indexes.Select(index => index)
        .Where(item => item.Columns.Any(column => stringComparer.Equals(column.Name, columnName)))
        .ToList();
      foreach (var index in indexes) {
        _ = table.Indexes.Remove(index);
      }
    }

    // Constructors

    /// <summary>
    /// Creates instance of <see cref="IgnoreRuleCollection"/> handler
    /// </summary>
    /// <param name="model">Extracted model</param>
    /// <param name="configuration">Configuration of domain</param>
    /// <param name="resolver"><see cref="MappingResolver"/> to be used.</param>
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
