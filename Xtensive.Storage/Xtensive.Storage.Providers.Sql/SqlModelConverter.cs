// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.31

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Indexing.Model;
using SqlModel = Xtensive.Sql.Dom.Database.Model;
using SqlRefAction = Xtensive.Sql.Dom.ReferentialAction;
using Xtensive.Modelling;
using Xtensive.Sql.Dom.Database;
using IndexInfo = Xtensive.Storage.Indexing.Model.IndexInfo;
using TableInfo = Xtensive.Storage.Indexing.Model.TableInfo;
using ColumnInfo = Xtensive.Storage.Indexing.Model.ColumnInfo;
using Node = Xtensive.Sql.Dom.Database.Node;
using ReferentialAction = Xtensive.Storage.Indexing.Model.ReferentialAction;
using SequenceInfo = Xtensive.Storage.Indexing.Model.SequenceInfo;
using SqlFactory = Xtensive.Sql.Dom.Sql;


namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Converts <see cref="Xtensive.Sql.Dom.Database.Model"/> to indexing storage model.
  /// </summary>
  public class SqlModelConverter : SqlModelVisitor<IPathNode>
  {
    /// <summary>
    /// Gets the storage info.
    /// </summary>
    protected StorageInfo StorageInfo { get; private set; }

    /// <summary>
    /// Gets the provider info.
    /// </summary>
    protected ProviderInfo ProviderInfo {get; private set;}

    /// <summary>
    /// Gets the key fetcher.
    /// </summary>
    protected Func<ISqlCompileUnit, object> CommandExecutor { get; private set; }

    /// <summary>
    /// Gets the type converter.
    /// </summary>
    protected Func<SqlValueType, TypeInfo> ValueTypeConverter { get; private set; }

    /// <summary>
    /// Gets the schema.
    /// </summary>
    protected Schema Schema { get; private set; }

    /// <summary>
    /// Get the result of conversion specified 
    /// <see cref="Schema"/> to <see cref="StorageInfo"/>.
    /// </summary>
    /// <returns>The storage model.</returns>
    public StorageInfo GetConversionResult()
    {
      if (StorageInfo == null) {
        StorageInfo = new StorageInfo();
        Visit(Schema);
      }

      return StorageInfo;
    }

    # region SqlModelVisitor<IPathNode> implementation

    /// <inheritdoc/>
    protected override IPathNode VisitSchema(Schema schema)
    {
      // Build tables, columns and indexes.
      foreach (var table in schema.Tables)
        Visit(table);

      // Build foreign keys.
      var foreignKeys = schema.Tables.SelectMany(
        t => t.TableConstraints.OfType<ForeignKey>());
      foreach (var foreignKey in foreignKeys)
        Visit(foreignKey);
      foreach (var sequence in schema.Sequences)
        VisitSequence(sequence);

      return null;
    }

    /// <inheritdoc/>
    protected override IPathNode Visit(Node node)
    {
      if (!ProviderInfo.SupportSequences) {
        var table = node as Table;
        if (table!=null && IsGeneratorTable(table))
          return VisitGeneratorTable(table);
      }

      return base.Visit(node);
    }

    /// <inheritdoc/>
    protected override IPathNode VisitTable(Table table)
    {
      var tableInfo = new TableInfo(StorageInfo, table.Name);

      foreach (var column in table.TableColumns)
        Visit(column);

      var primaryKey = table.TableConstraints
        .SingleOrDefault(constraint=>constraint is PrimaryKey);
      if (primaryKey != null)
        Visit(primaryKey);

      foreach (var index in table.Indexes)
        Visit(index);

      return tableInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitTableColumn(TableColumn tableColumn)
    {
      var tableInfo = StorageInfo.Tables[tableColumn.Table.Name];
      var columnInfo = new ColumnInfo(tableInfo, tableColumn.Name, ExtractType(tableColumn));
      return columnInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitForeignKey(ForeignKey key)
    {
      var referencingTable = StorageInfo.Tables[key.Table.Name];
      var referencingColumns = new List<ColumnInfo>();
      foreach (var refColumn in key.Columns)
        referencingColumns.Add(referencingTable.Columns[refColumn.Name]);

      var foreignKeyInfo = new ForeignKeyInfo(referencingTable, key.Name) {
        OnUpdateAction = ConvertReferentialAction(key.OnUpdate),
        OnRemoveAction = ConvertReferentialAction(key.OnDelete)
      };

      var referencedTable = StorageInfo.Tables[key.ReferencedTable.Name];
      foreignKeyInfo.PrimaryKey = referencedTable.PrimaryIndex;
      
      var referncingIndex = FindIndex(referencingTable, referencingColumns);
      foreignKeyInfo.ForeignKeyColumns.Set(referncingIndex);

      return foreignKeyInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitPrimaryKey(PrimaryKey key)
    {
      var tableInfo = StorageInfo.Tables[key.Table.Name];
      var primaryIndexInfo = new PrimaryIndexInfo(tableInfo, key.Name);

      foreach (var keyColumn in key.Columns)
        new KeyColumnRef(primaryIndexInfo, tableInfo.Columns[keyColumn.Name],
          Direction.Positive);
      // TODO: Get direction for key columns
      primaryIndexInfo.PopulateValueColumns();

      return primaryIndexInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitIndex(Index index)
    {
      var tableInfo = StorageInfo.Tables[index.DataTable.Name];
      var secondaryIndexInfo = new SecondaryIndexInfo(tableInfo, index.Name) {
        IsUnique = index.IsUnique
      };

      foreach (var keyColumn in index.Columns) {
        var columnInfo = tableInfo.Columns[keyColumn.Column.Name];
        new KeyColumnRef(secondaryIndexInfo,
          columnInfo, keyColumn.Ascending ? Direction.Positive : Direction.Negative);
      }

      foreach (var valueColumn in index.NonkeyColumns) {
        var columnInfo = tableInfo.Columns[valueColumn.Name];
        new IncludedColumnRef(secondaryIndexInfo, columnInfo);
      }

      secondaryIndexInfo.PopulatePrimaryKeyColumns();

      return secondaryIndexInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitSequence(Sequence sequence)
    {
      var sequenceInfo = new SequenceInfo(StorageInfo, sequence.Name) {
        Current = GetNextGeneratorValue(sequence.Name),
        Increment = sequence.SequenceDescriptor.Increment.Value,
        StartValue = sequence.SequenceDescriptor.StartValue.Value,
        Type = ValueTypeConverter.Invoke(sequence.DataType)
      };
      return sequenceInfo;
    }

    # endregion

    /// <summary>
    /// Visits the generator table.
    /// </summary>
    /// <param name="generatorTable">The generator table.</param>
    /// <returns>Visit result.</returns>
    protected virtual IPathNode VisitGeneratorTable(Table generatorTable)
    {
      var idColumn = generatorTable.TableColumns[0];
      var startValue = idColumn.SequenceDescriptor.StartValue;
      var increment = idColumn.SequenceDescriptor.Increment;
      var type = ExtractType(idColumn);
      var currentValue = GetNextGeneratorValue(generatorTable.Name);
      var sequence =
        new SequenceInfo(StorageInfo, generatorTable.Name) {
          StartValue = startValue ?? 0,
          Increment = increment ?? 1,
          Type = type,
          Current = currentValue
        };

      return sequence;
    }

    /// <summary>
    /// Extracts the <see cref="TypeInfo"/> from <see cref="TableColumn"/>.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>Data type.</returns>
    protected virtual TypeInfo ExtractType(TableColumn column)
    {
      if (!ProviderInfo.SupportsRealTimeSpan 
        && column.Domain!=null 
        && column.Domain.Name==WellKnown.TimeSpanDomainName)
        return new TypeInfo(
          column.IsNullable ? typeof (TimeSpan?) : typeof (TimeSpan), column.IsNullable);
      
      var typeInfo = ValueTypeConverter.Invoke(column.DataType);

      if (column.IsNullable) {
        if (typeInfo.Type.IsValueType
          && !typeInfo.Type.IsNullable())
          typeInfo = new TypeInfo(typeInfo.Type.ToNullable(), true, typeInfo.Length);
        else
          typeInfo = new TypeInfo(typeInfo.Type, true, typeInfo.Length);
      }
      return typeInfo;
    }

    /// <summary>
    /// Converts the <see cref="Xtensive.Sql.Dom.ReferentialAction"/> to 
    /// <see cref="Xtensive.Storage.Indexing.Model.ReferentialAction"/>.
    /// </summary>
    /// <param name="toConvert">The action to convert.</param>
    /// <returns>Converted action.</returns>
    protected virtual ReferentialAction ConvertReferentialAction(SqlRefAction toConvert)
    {
      switch (toConvert) {
      case SqlRefAction.NoAction:
        return ReferentialAction.None;
      case SqlRefAction.Restrict:
        return ReferentialAction.Restrict;
      case SqlRefAction.Cascade:
        return ReferentialAction.Cascade;
      case SqlRefAction.SetNull:
        return ReferentialAction.Clear;
      case SqlRefAction.SetDefault:
        return ReferentialAction.Default;
      default:
        return ReferentialAction.Default;
      }
    }

    /// <summary>
    /// Finds the specific index by key columns.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="keyColumns">The key columns.</param>
    /// <returns>The index.</returns>
    protected virtual IndexInfo FindIndex(TableInfo table, List<ColumnInfo> keyColumns)
    {
      var primaryKeyColumns = table.PrimaryIndex.KeyColumns.Select(cr => cr.Value);
      if (primaryKeyColumns.Except(keyColumns)
        .Union(keyColumns.Except(primaryKeyColumns)).Count()==0)
        return table.PrimaryIndex;

      foreach (var index in table.SecondaryIndexes) {
        var secondaryKeyColumns = index.KeyColumns.Select(cr => cr.Value);
        if (secondaryKeyColumns.Except(keyColumns)
          .Union(keyColumns.Except(secondaryKeyColumns)).Count()==0)
          return index;
      }

      return null;
    }

    /// <summary>
    /// Determines whether specific table used as sequence.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns>
    /// <see langword="true"/> if table used as sequence; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool IsGeneratorTable(Table table)
    {
      return table.TableColumns.Count==1 &&
        table.TableColumns[0].SequenceDescriptor!=null;
    }

    /// <summary>
    /// Gets the next generator value.
    /// </summary>
    /// <param name="generatorName">Name of the generator.</param>
    /// <returns>Next value.</returns>
    protected virtual long GetNextGeneratorValue(string generatorName)
    {
      if (ProviderInfo.SupportSequences) {
        var sequence = Schema.Sequences[generatorName];
        var sqlNext = SqlFactory.Select();
        sqlNext.Columns.Add(SqlFactory.NextValue(sequence));
        return (long) CommandExecutor.Invoke(sqlNext);
      }
      else {
        var generatorTable = Schema.Tables[generatorName];
        SqlBatch sqlNext = SqlFactory.Batch();
        SqlInsert insert = SqlFactory.Insert(SqlFactory.TableRef(generatorTable));
        sqlNext.Add(insert);
        SqlSelect select = SqlFactory.Select();
        select.Columns.Add(SqlFactory.Cast(SqlFactory.FunctionCall("SCOPE_IDENTITY"),
          SqlDataType.Int64));
        sqlNext.Add(select);
        SqlDelete delete = SqlFactory.Delete(SqlFactory.TableRef(generatorTable));
        sqlNext.Add(delete);
        return (long) CommandExecutor.Invoke(sqlNext);
      }
    }
    

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="storageSchema">The schema.</param>
    /// <param name="commandExecutor">The key fetcher.</param>
    /// <param name="valueTypeConverter">The value type converter.</param>
    /// <param name="providerInfo">The provider info.</param>
    public SqlModelConverter(Schema storageSchema, Func<SqlValueType, TypeInfo> valueTypeConverter, 
      ProviderInfo providerInfo, Func<ISqlCompileUnit, object> commandExecutor)
    {
      ArgumentValidator.EnsureArgumentNotNull(storageSchema, "schema");
      ArgumentValidator.EnsureArgumentNotNull(commandExecutor, "commandExecutor");
      ArgumentValidator.EnsureArgumentNotNull(valueTypeConverter, "valueTypeConverter");
      ArgumentValidator.EnsureArgumentNotNull(providerInfo, "providerInfo");
      
      CommandExecutor = commandExecutor;
      ValueTypeConverter = valueTypeConverter;
      Schema = storageSchema;
      ProviderInfo = providerInfo;
    }


    #region Not supported

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitUniqueConstraint(UniqueConstraint constraint)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitIndexColumn(IndexColumn indexColumn)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitCatalog(Catalog catalog)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitCharacterSet(CharacterSet characterSet)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitCollation(Collation collation)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitDataTable(DataTable dataTable)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitDataTableColumn(DataTableColumn dataTableColumn)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitDomain(Xtensive.Sql.Dom.Database.Domain domain)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitHashPartition(HashPartition hashPartition)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitListPartition(ListPartition listPartition)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitModel(SqlModel model)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitPartition(Partition partition)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitPartitionDescriptor(PartitionDescriptor partitionDescriptor)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitPartitionFunction(PartitionFunction partitionFunction)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitPartitionSchema(PartitionSchema partitionSchema)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitTableConstraint(TableConstraint constraint)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitDomainConstraint(DomainConstraint constraint)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitConstraint(Constraint constraint)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitCheckConstraint(CheckConstraint constraint)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitRangePartition(RangePartition rangePartition)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitSequenceDescriptor(SequenceDescriptor sequenceDescriptor)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitServer(Server server)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitTemporaryTable(TemporaryTable temporaryTable)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitTranslation(Translation translation)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitUser(User user)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitView(View view)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitViewColumn(ViewColumn viewColumn)
    {
      throw new NotSupportedException();
    }

    #endregion
  }
}