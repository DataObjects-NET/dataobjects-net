// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Oracle.DataAccess.Client;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.Oracle.v09
{
  internal class ServerInfoProvider : Info.ServerInfoProvider
  {
    private const int MaxIdentifierLength = 30;
    private const int DoNotKnow = int.MaxValue;
    
    private readonly string databaseName;
    private readonly string defaultSchemaName;

    public override EntityInfo GetCollationInfo()
    {
      return null;
    }

    public override EntityInfo GetCharacterSetInfo()
    {
      return null;
    }

    public override EntityInfo GetTranslationInfo()
    {
      return null;
    }

    public override EntityInfo GetTriggerInfo()
    {
      var triggerInfo = new EntityInfo();
      triggerInfo.AllowedDdlStatements = DdlStatements.All;
      triggerInfo.MaxIdentifierLength = MaxIdentifierLength;
      return triggerInfo;
    }

    public override EntityInfo GetStoredProcedureInfo()
    {
      var storedProcedureInfo = new EntityInfo();
      storedProcedureInfo.AllowedDdlStatements = DdlStatements.All;
      storedProcedureInfo.MaxIdentifierLength = MaxIdentifierLength;
      return storedProcedureInfo;
    }

    public override SequenceInfo GetSequenceInfo()
    {
      var sequenceInfo = new SequenceInfo();
      sequenceInfo.AllowedDdlStatements = DdlStatements.All;
      sequenceInfo.Features = SequenceFeatures.Cache;
      sequenceInfo.MaxIdentifierLength = MaxIdentifierLength;
      return sequenceInfo;
    }

    public override EntityInfo GetDatabaseInfo()
    {
      var info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.None;
      info.MaxIdentifierLength = MaxIdentifierLength;
      return info;
    }

    public override ColumnInfo GetColumnInfo()
    {
      var columnInfo = new ColumnInfo();
      columnInfo.AllowedDdlStatements = DdlStatements.All;
      columnInfo.Features = ColumnFeatures.Computed;
      columnInfo.MaxIdentifierLength = MaxIdentifierLength;
      return columnInfo;
    }

    public override EntityInfo GetViewInfo()
    {
      var viewInfo = new EntityInfo();
      viewInfo.AllowedDdlStatements = DdlStatements.All;
      viewInfo.MaxIdentifierLength = MaxIdentifierLength;
      return viewInfo;
    }

    public override EntityInfo GetSchemaInfo()
    {
      var schemaInfo = new EntityInfo();
      schemaInfo.AllowedDdlStatements = DdlStatements.All;
      schemaInfo.MaxIdentifierLength = MaxIdentifierLength;
      return schemaInfo;
    }

    public override TableInfo GetTableInfo()
    {
      var tableInfo = new TableInfo();
      tableInfo.AllowedDdlStatements = DdlStatements.All;
      tableInfo.MaxIdentifierLength = MaxIdentifierLength;
      tableInfo.PartitionMethods =
        PartitionMethods.Hash |
        PartitionMethods.List |
        PartitionMethods.Range |
        PartitionMethods.Interval;
      return tableInfo;
    }

    public override TemporaryTableInfo GetTemporaryTableInfo()
    {
      var temporaryTableInfo = new TemporaryTableInfo();
      temporaryTableInfo.Features =
        TemporaryTableFeatures.Global |
        TemporaryTableFeatures.DeleteRowsOnCommit |
        TemporaryTableFeatures.PreserveRowsOnCommit;
      temporaryTableInfo.AllowedDdlStatements = DdlStatements.All;
      temporaryTableInfo.MaxIdentifierLength = MaxIdentifierLength;
      return temporaryTableInfo;
    }

    public override CheckConstraintInfo GetCheckConstraintInfo()
    {
      var checkContraintsInfo = new CheckConstraintInfo();
      checkContraintsInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      checkContraintsInfo.MaxIdentifierLength = MaxIdentifierLength;
      checkContraintsInfo.MaxExpressionLength = DoNotKnow;
      checkContraintsInfo.Features = CheckConstraintFeatures.Deferrable;
      return checkContraintsInfo;
    }

    public override PrimaryKeyConstraintInfo GetPrimaryKeyInfo()
    {
      var primaryKeyInfo = new PrimaryKeyConstraintInfo();
      primaryKeyInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      primaryKeyInfo.Features = PrimaryKeyConstraintFeatures.Nullable;
      primaryKeyInfo.MaxIdentifierLength = MaxIdentifierLength;
      return primaryKeyInfo;
    }

    public override UniqueConstraintInfo GetUniqueConstraintInfo()
    {
      var uniqueContraintInfo = new UniqueConstraintInfo();
      uniqueContraintInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      uniqueContraintInfo.Features = UniqueConstraintFeatures.Nullable;
      uniqueContraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      return uniqueContraintInfo;
    }

    public override IndexInfo GetIndexInfo()
    {
      var indexInfo = new IndexInfo();
      indexInfo.AllowedDdlStatements = DdlStatements.All;
      indexInfo.MaxIdentifierLength = MaxIdentifierLength;
      indexInfo.Features =
        IndexFeatures.Unique |
        IndexFeatures.SortOrder;
      indexInfo.PartitionMethods =
        PartitionMethods.Hash |
        PartitionMethods.Interval |
        PartitionMethods.List |
        PartitionMethods.Range;
      return indexInfo;
    }

    public override ForeignKeyConstraintInfo GetForeignKeyConstraintInfo()
    {
      var foreignKeyConstraintInfo = new ForeignKeyConstraintInfo();
      foreignKeyConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      foreignKeyConstraintInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      foreignKeyConstraintInfo.Features = ForeignKeyConstraintFeatures.Deferrable;
      foreignKeyConstraintInfo.Actions =
        ForeignKeyConstraintActions.Cascade |
        ForeignKeyConstraintActions.NoAction |
        ForeignKeyConstraintActions.SetDefault |
        ForeignKeyConstraintActions.SetNull;
      return foreignKeyConstraintInfo;
    }

    public override FullTextSearchInfo GetFullTextInfo()
    {
      return null;
    }

    public override QueryInfo GetQueryInfo()
    {
      var queryInfo = new QueryInfo();
      queryInfo.MaxNestedSubqueriesAmount = 255;
      queryInfo.ParameterPrefix = ":";
      queryInfo.MaxLength = DoNotKnow;
      queryInfo.MaxComparisonOperations = DoNotKnow;
      queryInfo.MaxQueryParameterCount = 63999;
      queryInfo.Features =
        QueryFeatures.NamedParameters |
        QueryFeatures.ParameterPrefix |
        QueryFeatures.Hints |
        QueryFeatures.DmlBatches |
        QueryFeatures.InsertDefaultValues |
        QueryFeatures.UpdateDefaultValues |
        QueryFeatures.MultischemaQueries |
        QueryFeatures.RowNumber;
      return queryInfo;
    }

    public override ServerFeatures GetServerFeatures()
    {
      return ServerFeatures.Savepoints |
        ServerFeatures.LargeObjects |
        ServerFeatures.CursorParameters |
        ServerFeatures.MultipleResultsViaCursorParameters;
    }

    public override IdentityInfo GetIdentityInfo()
    {
      return null;
    }

    public override DataTypeCollection GetDataTypesInfo()
    {
      const DataTypeFeatures common = DataTypeFeatures.Default | DataTypeFeatures.Nullable |
        DataTypeFeatures.NonKeyIndexing | DataTypeFeatures.Grouping | DataTypeFeatures.Ordering |
        DataTypeFeatures.Multiple;
      const DataTypeFeatures index = DataTypeFeatures.Indexing | DataTypeFeatures.Clustering |
        DataTypeFeatures.FillFactor | DataTypeFeatures.KeyConstraint;
      var types = new DataTypeCollection();

      types.Decimal = DataTypeInfo.Fractional(SqlType.Decimal, common | index,
        ValueRange.Decimal, 38, "number");
      types.Float = DataTypeInfo.Range(SqlType.Float, common | index,
        ValueRange.Float, "real");
      types.Double = DataTypeInfo.Range(SqlType.Double, common | index,
        ValueRange.Double, "double precision", "float");
      types.DateTime = DataTypeInfo.Range(SqlType.DateTime, common | index,
        ValueRange.DateTime, "timestamp");
      types.DateTimeOffset = DataTypeInfo.Range(SqlType.DateTimeOffset, common | index,
        ValueRange.DateTimeOffset, "TIMESTAMP WITH TIME ZONE");
      types.Interval = DataTypeInfo.Range(SqlType.Interval, common | index,
        ValueRange.TimeSpan, "interval day to second");

      types.Char = DataTypeInfo.Stream(SqlType.Char,
        common | index | DataTypeFeatures.ZeroLengthValueIsNull, 2000, "nchar");
      types.VarChar = DataTypeInfo.Stream(SqlType.VarChar,
        common | index | DataTypeFeatures.ZeroLengthValueIsNull, 2000, "nvarchar2");
      types.VarCharMax = DataTypeInfo.Regular(SqlType.VarCharMax, common, "nclob");
      types.VarBinaryMax = DataTypeInfo.Regular(SqlType.VarBinaryMax, common, "blob");
      return types;
    }

    public override IsolationLevels GetIsolationLevels()
    {
      return IsolationLevels.ReadCommitted | IsolationLevels.Serializable;
    }

    public override EntityInfo GetDomainInfo()
    {
      return null;
    }

    public override AssertConstraintInfo GetAssertionInfo()
    {
      return null;
    }

    public override int GetStringIndexingBase()
    {
      return 1;
    }

    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}