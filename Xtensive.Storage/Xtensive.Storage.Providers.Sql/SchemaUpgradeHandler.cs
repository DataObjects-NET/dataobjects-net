// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.08

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Modelling.Actions;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Model;
using ColumnInfo = Xtensive.Storage.Model.ColumnInfo;
using IndexInfo = Xtensive.Storage.Model.IndexInfo;
using SqlModel = Xtensive.Sql.Dom.Database.Model;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using TypeInfo = Xtensive.Storage.Model.TypeInfo;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Upgrades storage schema.
  /// </summary>
  [Serializable]
  public class SchemaUpgradeHandler : Providers.SchemaUpgradeHandler
  {
    /// <summary>
    /// Gets the session handler.
    /// </summary>
    protected SessionHandler SessionHandler
    {
      get { return BuildingContext.Current.SystemSessionHandler as SessionHandler; }
    }

    /// <summary>
    /// Gets the name builder.
    /// </summary>
    protected NameBuilder NameBuilder
    {
      get { return BuildingContext.Current.NameBuilder; }
    }

    /// <inheritdoc/>
    /// <exception cref="NotImplementedException">
    /// <c>NotImplementedException</c>.</exception>
    public override StorageInfo GetExtractedSchema()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override void UpgradeSchema(ActionSequence upgradeActions, StorageInfo targetSchema)
    {
      ClearStorageSchema();
      UpgradeStorageSchema();
    }
    
    /// <inheritdoc/>
    /// <exception cref="NotImplementedException">
    /// <c>NotImplementedException</c>.</exception>
    protected override bool IsSchemaBoundGenerator(GeneratorInfo generatorInfo)
    {
      throw new NotImplementedException();
    }

    protected void ClearStorageSchema()
    {
      var clearScript = GenerateClearScript();
      if (clearScript.Count > 0)
        SessionHandler.ExecuteNonQuery(clearScript);
    }

    protected void UpgradeStorageSchema()
    {
      var updateScript = GenerateUpgradeScript();
      if (updateScript.Count > 0)
        SessionHandler.ExecuteNonQuery(updateScript);
    }
    
    private SqlBatch GenerateClearScript()
    {
      var schema = ExtractStorageSchema();

      var batch = SqlFactory.Batch();
      foreach (var view in schema.Views)
        batch.Add(SqlFactory.Drop(view));
      foreach (var table in schema.Tables) {
        foreach (var tableConstraint in table.TableConstraints) {
          var fk = tableConstraint as ForeignKey;
          if (fk!=null)
            batch.Add(SqlFactory.Alter(table, SqlFactory.DropConstraint(tableConstraint)));
        }
      }
      foreach (var table in schema.Tables)
        batch.Add(SqlFactory.Drop(table));
      foreach (var sequence in schema.Sequences)
        batch.Add(SqlFactory.Drop(sequence));
      return batch;
    }
    
    private SqlBatch GenerateUpgradeScript()
    {
      var schema = ExtractDomainSchema();

      var batch = SqlFactory.Batch();
      var constraints = new List<Pair<Table, List<TableConstraint>>>();
      foreach (var table in schema.Tables)
      {
        var tableConstraints = new List<TableConstraint>(
          table.TableConstraints.Where(tableConstraint => tableConstraint is ForeignKey));
        constraints.Add(new Pair<Table, List<TableConstraint>>(table, tableConstraints));
        foreach (var foreignKeyConstraint in tableConstraints)
          table.TableConstraints.Remove(foreignKeyConstraint);
        batch.Add(SqlFactory.Create(table));
        foreach (var index in table.Indexes)
          batch.Add(SqlFactory.Create(index));
      }
      foreach (var constraint in constraints)
        foreach (var tableConstraint in constraint.Second)
          batch.Add(SqlFactory.Alter(constraint.First, SqlFactory.AddConstraint(tableConstraint)));
      foreach (var sequence in schema.Sequences)
        batch.Add(SqlFactory.Create(sequence));
      return batch;
    }

    #region ExtractSchema methods

    /// <summary>
    /// Extracts the storage schema.  
    /// </summary>
    /// <returns>The current storage schema.</returns>
    protected virtual Schema ExtractStorageSchema()
    {
      var modelProvider = new SqlModelProvider(SessionHandler.Connection, SessionHandler.Transaction);
      var storageModel = SqlModel.Build(modelProvider);
      return storageModel.DefaultServer.DefaultCatalog.DefaultSchema;
    }

    /// <summary>
    /// Extracts the domain schema.
    /// </summary>
    /// <returns>The domain schema.</returns>
    internal virtual Schema ExtractDomainSchema()
    {
      var domainModel = BuildingContext.Current.Model;
      var tables = new Dictionary<IndexInfo, Table>();
      var modelProvider = new SqlModelProvider(SessionHandler.Connection, SessionHandler.Transaction);
      var existingModel = SqlModel.Build(modelProvider);
      var newModel = new SqlModel(existingModel.Name);
      var schema = newModel
        .CreateServer(existingModel.DefaultServer.Name)
        .CreateCatalog(existingModel.DefaultServer.DefaultCatalog.Name)
        .CreateSchema(existingModel.DefaultServer.DefaultCatalog.DefaultSchema.Name);

      // Tables, columns, indexes.
      foreach (var type in domainModel.Types)
      {
        var primaryIndex = type.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
        if (primaryIndex == null || tables.ContainsKey(primaryIndex))
          continue;
        var table = schema.CreateTable(NameBuilder.BuildTableName(primaryIndex));
        tables.Add(primaryIndex, table);
        CreateColumns(primaryIndex, table);
        CreateSecondaryIndexes(type, primaryIndex, table);
      }

      // Foreign keys.
      if ((Handlers.Domain.Configuration.ForeignKeyMode & ForeignKeyMode.Reference) > 0)
        BuildForeignKeys(domainModel.Associations, tables);
      if ((Handlers.Domain.Configuration.ForeignKeyMode & ForeignKeyMode.Hierarchy) > 0)
        BuildHierarchyReferences(domainModel.Types.Entities, tables);

      // Sequences.
      foreach (var generator in domainModel.Generators)
      {
        if (generator.KeyGeneratorType != typeof(KeyGenerator)
          || (Type.GetTypeCode(generator.KeyGeneratorType) == TypeCode.Object && generator.TupleDescriptor[0] == typeof(Guid)))
          continue;
        BuildSequence(schema, generator);
      }
      return schema;
    }

    /// <summary>
    /// Builds the sequence.
    /// </summary>
    /// <param name="schema">The schema.</param>
    /// <param name="generator">The generator.</param>
    protected virtual void BuildSequence(Schema schema, GeneratorInfo generator)
    {
      var genTable = schema.CreateTable(generator.MappingName);
      var columnType =
        SessionHandler.DomainHandler.ValueTypeMapper.BuildSqlValueType(generator.TupleDescriptor[0], 0);
      var column = genTable.CreateColumn("ID", columnType);
      column.SequenceDescriptor = new SequenceDescriptor(column, generator.CacheSize, generator.CacheSize);
    }

    private void CreateColumns(IndexInfo primaryIndex, Table table)
    {
      var keyColumns = new List<TableColumn>();
      foreach (var columnInfo in primaryIndex.Columns)
      {
        var column = table.CreateColumn(NameBuilder.BuildTableColumnName(columnInfo));
        column.DataType = SessionHandler.DomainHandler.ValueTypeMapper.BuildSqlValueType(columnInfo);
        column.IsNullable = columnInfo.IsNullable;
        if (columnInfo.IsPrimaryKey)
          keyColumns.Add(column);
      }
      if (keyColumns.Count > 0)
        table.CreatePrimaryKey(primaryIndex.Name, keyColumns.ToArray());
    }

    private static void CreateSecondaryIndexes(TypeInfo type, IndexInfo primaryIndex, Table table)
    {
      foreach (var indexInfo in type.Indexes.Find(IndexAttributes.Real).Where(ii => !ii.IsPrimary))
      {
        var index = table.CreateIndex(indexInfo.MappingName);
        index.IsUnique = indexInfo.IsUnique;
        index.FillFactor = (byte)(indexInfo.FillFactor * 100);
        foreach (KeyValuePair<ColumnInfo, Direction> keyColumn in indexInfo.KeyColumns)
        {
          var primaryIndexColumnName = GetPrimaryIndexColumnName(primaryIndex, keyColumn.Key);
          index.CreateIndexColumn(table.TableColumns.First(
            tableColumn => tableColumn.Name == primaryIndexColumnName), keyColumn.Value == Direction.Positive);
        }
        foreach (var nonKeyColumn in indexInfo.IncludedColumns)
        {
          var primaryIndexColumnName = GetPrimaryIndexColumnName(primaryIndex, nonKeyColumn);
          index.NonkeyColumns.Add(table.TableColumns.First(tableColumn => tableColumn.Name == primaryIndexColumnName));
        }
      }
    }

    private void BuildForeignKeys(IEnumerable<AssociationInfo> associations, Dictionary<IndexInfo, Table> tables)
    {
      var domainModel = BuildingContext.Current.Model;

      foreach (var association in associations.Where(associationInfo => associationInfo.IsMaster))
      {
        Table referencingTable;
        Table referencedTable;
        IList<ColumnInfo> referencingColumns;
        ICollection<ColumnInfo> referencedColumns;
        string foreignKeyName;
        if (association.UnderlyingType == null)
        {
          // TODO: Remove comparison with null than Structure association bug fixed.
          if (association.ReferencingType.Indexes.PrimaryIndex == null)
            continue;
          var referencingIndex = FindRealIndex(association.ReferencingType.Indexes.PrimaryIndex, association.ReferencingField);
          referencingTable = tables[referencingIndex];
          referencingColumns = association.ReferencingField.ExtractColumns();
          var referencedIndex = FindRealIndex(association.ReferencedType.Indexes.PrimaryIndex, null);
          referencedTable = tables[referencedIndex];
          referencedColumns = association.ReferencedType.Indexes.PrimaryIndex.KeyColumns.Keys;
          foreignKeyName = NameBuilder.BuildForeignKeyName(association, association.ReferencingField);
          CreateForeignKey(foreignKeyName, referencingTable, referencingColumns, referencedTable, referencedColumns);
        }
        else
        {
          referencingTable = tables[association.UnderlyingType.Indexes.PrimaryIndex];
          foreach (var referencingField in association.UnderlyingType.Fields.Where(fieldInfo => fieldInfo.IsEntity))
          {
            // Build master reference
            var referencedIndex = FindRealIndex(domainModel.Types[referencingField.ValueType].Indexes.PrimaryIndex, null);
            referencedTable = tables[referencedIndex];
            referencedColumns = referencedIndex.KeyColumns.Keys;
            referencingColumns = referencingField.ExtractColumns();
            foreignKeyName = NameBuilder.BuildForeignKeyName(association, referencingField);
            CreateForeignKey(foreignKeyName, referencingTable, referencingColumns, referencedTable, referencedColumns);
          }
        }
      }
    }

    private void BuildHierarchyReferences(IEnumerable<TypeInfo> entities, IDictionary<IndexInfo, Table> tables)
    {
      var indexPairs = new Dictionary<Pair<IndexInfo>, object>();
      foreach (var type in entities)
      {
        if (!type.Indexes.PrimaryIndex.IsVirtual)
          continue;
        var realPrimaryIndexes = type.Indexes.RealPrimaryIndexes;
        for (var i = 0; i < realPrimaryIndexes.Count - 1; i++)
        {
          if (realPrimaryIndexes[i] != realPrimaryIndexes[i + 1])
          {
            var pair = new Pair<IndexInfo>(realPrimaryIndexes[i], realPrimaryIndexes[i + 1]);
            indexPairs[pair] = null;
          }
        }
      }
      foreach (var indexPair in indexPairs.Keys)
      {
        var referencingIndex = indexPair.First;
        IEnumerable<ColumnInfo> referencingColumns = referencingIndex.KeyColumns.Keys;
        var referencedIndex = indexPair.Second;
        IEnumerable<ColumnInfo> referencedColumns = referencedIndex.KeyColumns.Keys;
        var foreignKeyName = NameBuilder.BuildForeignKeyName(referencingIndex.ReflectedType, referencedIndex.ReflectedType);
        CreateForeignKey(foreignKeyName, tables[referencedIndex], referencedColumns, tables[referencingIndex], referencingColumns);
      }
    }

    private void CreateForeignKey(string foreignKeyName, Table referencingTable, IEnumerable<ColumnInfo> referencingColumns, Table referencedTable, IEnumerable<ColumnInfo> referencedColumns)
    {
      var foreignKey = referencingTable.CreateForeignKey(foreignKeyName);
      foreach (var column in referencingColumns)
      {
        var columnName = NameBuilder.BuildTableColumnName(column);
        var tableColumn = FindColumnByName(referencingTable, columnName);
        foreignKey.Columns.Add(tableColumn);
      }
      foreignKey.ReferencedTable = referencedTable;
      foreach (var keyColumn in referencedColumns)
      {
        var columnName = NameBuilder.BuildTableColumnName(keyColumn);
        var tableColumn = FindColumnByName(referencedTable, columnName);
        foreignKey.ReferencedColumns.Add(tableColumn);
      }
    }

    private static TableColumn FindColumnByName(Table referencingTable, string columnName)
    {
      return referencingTable.TableColumns.First(dataTableColumn => dataTableColumn.Name == columnName);
    }

    private static IndexInfo FindRealIndex(IndexInfo index, FieldInfo field)
    {
      if (index.IsVirtual)
      {
        foreach (var underlyingIndex in index.UnderlyingIndexes)
        {
          var result = FindRealIndex(underlyingIndex, field);
          if (result != null)
            return result;
        }
      }
      else
      {
        if (field == null || index.Columns.ContainsAny(field.ExtractColumns()))
          return index;
      }
      return null;
    }

    private static string GetPrimaryIndexColumnName(IndexInfo primaryIndex, ColumnInfo secondaryIndexColumn)
    {
      string primaryIndexColumnName = null;
      foreach (var primaryColumn in primaryIndex.Columns)
      {
        if (primaryColumn.Field.Equals(secondaryIndexColumn.Field))
        {
          primaryIndexColumnName = primaryColumn.Name;
          break;
        }
      }
      return primaryIndexColumnName;
    }

    #endregion

  }
}