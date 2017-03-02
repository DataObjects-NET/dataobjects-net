// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.23

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Orm.Upgrade.Internals.Interfaces;
using ReferentialAction = Xtensive.Sql.ReferentialAction;
using TableInfo = Xtensive.Orm.Upgrade.Model.TableInfo;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal sealed class DomainExtractedModelBuilder : ISchemaExtractionResultBuilder
  {
    private readonly string collationName;
    private readonly string typeIdColumnName;
    private readonly DomainConfiguration domainConfiguration;
    private readonly StorageModel model;
    private readonly MappingResolver mappingResolver;
    private readonly ProviderInfo provider;
    private readonly StorageDriver driver;
    private readonly bool makeSharedFinally;

    private readonly SchemaExtractionResult targetResult;

    public SchemaExtractionResult Run()
    {
      BuildCatalogsAndSchemas();
      BuildTables();
      BuildSequences();
      if (makeSharedFinally)
        return targetResult.MakeShared();
      return targetResult;
    }

    private void BuildCatalogsAndSchemas()
    {
      foreach (var group in mappingResolver.GetSchemaTasks().GroupBy(t => t.Catalog)) {
        var catalog = new Catalog(group.Key);
        targetResult.Catalogs.Add(catalog);
        foreach (var task in group)
          catalog.CreateSchema(task.Schema);
      }
    }

    private void BuildTables()
    {
      foreach (var table in model.Tables)
        CreateTable(table);
      foreach (var table in model.Tables)
        CreateForeignKeys(table);
    }

    private void BuildSequences()
    {
      Action<Schema, StorageSequenceInfo, string> sequenceCreator;

      if (provider.Supports(ProviderFeatures.Sequences))
        sequenceCreator = CreateSequence;
      else
        sequenceCreator = CreateGeneratorTable;

      foreach (var sequence in model.Sequences) {
        var resolvedNode = mappingResolver.Resolve(targetResult, sequence.Name);
        sequenceCreator.Invoke(resolvedNode.Schema, sequence, resolvedNode.Name);
      }
    }

    private void CreateTable(TableInfo tableInfo)
    {
      var resolvedNode = mappingResolver.Resolve(targetResult, tableInfo.Name);
      var table = resolvedNode.Schema.CreateTable(resolvedNode.Name);
      CreateColumns(tableInfo, table);
      CreatePrimaryIndex(tableInfo, table);
      CreateSecondaryIndexes(tableInfo, table);
      CreateFullTextIndexes(tableInfo, table);
    }

    private void CreateForeignKeys(TableInfo tableInfo)
    {
      foreach (var foreignKey in tableInfo.ForeignKeys) {
        CreateForeignKey(foreignKey);
      }
    }

    private void CreateColumns(TableInfo tableInfo, Table table)
    {
      foreach (var columnInfo in tableInfo.Columns)
        CreateColumn(columnInfo, table);
    }

    private void CreatePrimaryIndex(TableInfo tableInfo, Table table)
    {
      if (tableInfo.PrimaryIndex==null)
        return;
      var columns = tableInfo.PrimaryIndex.KeyColumns.Select(cr => table.TableColumns[cr.Value.Name]).ToArray();
      var primaryKey = table.CreatePrimaryKey(tableInfo.PrimaryIndex.Name, columns);
      primaryKey.IsClustered = provider.Supports(ProviderFeatures.ClusteredIndexes) && tableInfo.PrimaryIndex.IsClustered;
    }

    private void CreateSecondaryIndexes(TableInfo tableInfo, Table table)
    {
      if (tableInfo.SecondaryIndexes==null || tableInfo.SecondaryIndexes.Count==0)
        return;
      foreach (var secondaryIndex in tableInfo.SecondaryIndexes)
        CreateSecondaryIndex(table, secondaryIndex);
    }

    private void CreateFullTextIndexes(TableInfo tableInfo, Table table)
    {
      if (!provider.Supports(ProviderFeatures.FullText))
        return;
      foreach(var ftIndex in tableInfo.FullTextIndexes)
        CreateFullTextIndex(tableInfo, table, ftIndex);
    }

    private void CreateColumn(StorageColumnInfo columnInfo, Table table)
    {
      var type = columnInfo.Type.NativeType;
      var column = table.CreateColumn(columnInfo.Name, type);
      var isPrimaryKeyColumn =
        columnInfo.Parent.PrimaryIndex != null
        && columnInfo.Parent.PrimaryIndex.KeyColumns.Any(keyColumn => keyColumn.Value == columnInfo);

      if (!column.IsNullable && column.Name != typeIdColumnName)
        if (!isPrimaryKeyColumn)
          column.DefaultValue = GetDefaultValueExpression(columnInfo);
        else if (!string.IsNullOrEmpty(columnInfo.DefaultSqlExpression))
          column.DefaultValue = SqlDml.Native(columnInfo.DefaultSqlExpression);

      column.IsNullable = columnInfo.Type.IsNullable;

      if (columnInfo.Type.Type == typeof(string) && collationName != null)
        column.Collation = table.Schema.Collations[collationName] ?? new Collation(table.Schema, collationName);
    }

    private void CreateSecondaryIndex(Table table, SecondaryIndexInfo indexInfo)
    {
      if (indexInfo.KeyColumns.Count==1) {
        var column = FindColumn(table, indexInfo.KeyColumns[0].Value.Name);
        if (driver.ServerInfo.DataTypes[column.DataType.Type].Features.Supports(DataTypeFeatures.Spatial)) {
          var spatialIndex = table.CreateSpatialIndex(indexInfo.Name);
          spatialIndex.CreateIndexColumn(column);
          return;
        }
      }

      var index = table.CreateIndex(indexInfo.Name);
      index.IsUnique = indexInfo.IsUnique;
      index.IsClustered = indexInfo.IsClustered;
      foreach (var keyColumn in indexInfo.KeyColumns)
        index.CreateIndexColumn(
          FindColumn(table, keyColumn.Value.Name),
          keyColumn.Direction==Direction.Positive);
      index.NonkeyColumns.AddRange(
        indexInfo.IncludedColumns
          .Select(cr => FindColumn(table, cr.Value.Name)).ToArray());
      if (indexInfo.Filter!=null)
        index.Where = SqlDml.Native(indexInfo.Filter.Expression);
    }

    private void CreateFullTextIndex(TableInfo tableInfo, Table table, StorageFullTextIndexInfo fullTextIndexInfo)
    {
      var ftIndex = table.CreateFullTextIndex(fullTextIndexInfo.Name);
      
      ftIndex.UnderlyingUniqueIndex = tableInfo.PrimaryIndex.EscapedName;
      ftIndex.FullTextCatalog = "Default";
      foreach (var columnRef in fullTextIndexInfo.Columns) {
        var configuration = columnRef.Configuration;
        var typeColumnName = columnRef.TypeColumnName;
        var column = columnRef.Value;

        var tableColumn = FindColumn(table, column.Name);
        var ftColumn = ftIndex.CreateIndexColumn(tableColumn);

        ftColumn.TypeColumn = (!typeColumnName.IsNullOrEmpty() && provider.Supports(ProviderFeatures.FullTextColumnDataTypeSpecification))
          ? FindColumn(table, typeColumnName)
          : null;
        ftColumn.Languages.Add(new Language(configuration));
      }
    }

    private void CreateForeignKey(ForeignKeyInfo foreignKeyInfo)
    {
      var referencingTable = FindTable(foreignKeyInfo.Parent);
      var foreignKey = referencingTable.CreateForeignKey(foreignKeyInfo.Name);
      foreignKey.OnUpdate = ConvertReferentialAction(foreignKeyInfo.OnUpdateAction);
      foreignKey.OnDelete = ConvertReferentialAction(foreignKeyInfo.OnRemoveAction);
      foreignKey.IsDeferrable = provider.Supports(ProviderFeatures.DeferrableConstraints);
      foreignKey.IsInitiallyDeferred = foreignKey.IsDeferrable;
      var referencingColumns = foreignKeyInfo.ForeignKeyColumns
        .Select(cr => FindColumn(referencingTable, cr.Value.Name));
      foreignKey.Columns.AddRange(referencingColumns);
      var referencedTable = FindTable(foreignKeyInfo.PrimaryKey.Parent);
      var referencedColumns = foreignKeyInfo.PrimaryKey.KeyColumns
        .Select(cr => FindColumn(referencedTable, cr.Value.Name));
      foreignKey.ReferencedTable = referencedTable;
      foreignKey.ReferencedColumns.AddRange(referencedColumns);
    }


    private void CreateGeneratorTable(Schema schema, StorageSequenceInfo sequenceInfo, string name)
    {
      var sequenceTable = schema.CreateTable(name);
      var idColumn = sequenceTable.CreateColumn(WellKnown.GeneratorColumnName,
        (SqlValueType) sequenceInfo.Type.NativeType);
      idColumn.SequenceDescriptor =
        new SequenceDescriptor(
          idColumn,
          sequenceInfo.Current ?? sequenceInfo.Seed,
          sequenceInfo.Increment);
      sequenceTable.CreatePrimaryKey(string.Format("PK_{0}", sequenceInfo.Name), idColumn);
      if (!provider.Supports(ProviderFeatures.InsertDefaultValues)) {
        var fakeColumn = sequenceTable.CreateColumn(WellKnown.GeneratorFakeColumnName, driver.MapValueType(typeof (int)));
        fakeColumn.IsNullable = true;
      }
    }

    private void CreateSequence(Schema schema, StorageSequenceInfo sequenceInfo, string name)
    {
      var sequence = schema.CreateSequence(name);
      var descriptor = new SequenceDescriptor(sequence, sequenceInfo.Seed, sequenceInfo.Increment) {
        MinValue = sequenceInfo.Seed
      };
      sequence.SequenceDescriptor = descriptor;
    }

    private SqlExpression GetDefaultValueExpression(StorageColumnInfo columnInfo)
    {
      if (!string.IsNullOrEmpty(columnInfo.DefaultSqlExpression))
        return SqlDml.Native(columnInfo.DefaultSqlExpression);
      if (columnInfo.DefaultValue != null)
        return SqlDml.Literal(columnInfo.DefaultValue);
      var type = columnInfo.Type.Type;
      return type.IsValueType && !type.IsNullable() ? SqlDml.Literal(Activator.CreateInstance(type)) : null;
    }

    private Table FindTable(TableInfo tableInfo)
    {
      return mappingResolver.Resolve(targetResult, tableInfo.Name).GetTable();
    }

    private TableColumn FindColumn(Table table, string name)
    {
      return table.TableColumns[name];
    }

    private static ReferentialAction ConvertReferentialAction(Model.ReferentialAction toConvert)
    {
      switch (toConvert) {
        case Model.ReferentialAction.None:
          return ReferentialAction.NoAction;
        case Model.ReferentialAction.Restrict:
          return ReferentialAction.Restrict;
        case Model.ReferentialAction.Cascade:
          return ReferentialAction.Cascade;
        case Model.ReferentialAction.Clear:
          return ReferentialAction.SetNull;
        default:
          return ReferentialAction.Restrict;
      }
    }

    internal DomainExtractedModelBuilder(UpgradeServiceAccessor services, StorageModel model, bool makeSharedFinally)
    {
      ArgumentValidator.EnsureArgumentNotNull(services, "services");
      ArgumentValidator.EnsureArgumentNotNull(model, "model");
      this.model = model;
      this.mappingResolver = services.MappingResolver;
      this.provider = services.ProviderInfo;
      this.driver = services.StorageDriver;
      this.makeSharedFinally = makeSharedFinally;

      this.targetResult = new SchemaExtractionResult();

      if (provider.Supports(ProviderFeatures.Collations)) {
        var collation = services.Configuration.Collation;
        if (!string.IsNullOrEmpty(collation))
          collationName = collation;
      }
    }
  }
}
