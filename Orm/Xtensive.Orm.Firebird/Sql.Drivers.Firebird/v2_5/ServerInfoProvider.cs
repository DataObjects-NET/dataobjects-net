// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.10

using System;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.Firebird.v2_5
{
  internal class ServerInfoProvider : Info.ServerInfoProvider
  {
    private const int MaxIdentifierLength = 30;
    private const int DoNotKnow = int.MaxValue;
    private const int MaxCharLength = 2000; // physical constraint=32762, but because of http://tracker.firebirdsql.org/browse/CORE-1117;
    // The limit is 64kB for statement text, 64kB for compiled BLR and 48kB for execution plan.
    private const int MaxTextLength = int.MaxValue;

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
      sequenceInfo.Features = SequenceFeatures.None;
      sequenceInfo.MaxIdentifierLength = MaxIdentifierLength;
      return sequenceInfo;
    }

    public override EntityInfo GetDatabaseInfo()
    {
      var info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
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
      viewInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Rename;
      viewInfo.MaxIdentifierLength = MaxIdentifierLength;
      return viewInfo;
    }

    public override EntityInfo GetSchemaInfo()
    {
      var schemaInfo = new EntityInfo();
      schemaInfo.AllowedDdlStatements = DdlStatements.None;
      schemaInfo.MaxIdentifierLength = MaxIdentifierLength;
      return schemaInfo;
    }

    public override TableInfo GetTableInfo()
    {
      var tableInfo = new TableInfo();
      tableInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Rename;
      tableInfo.MaxIdentifierLength = MaxIdentifierLength;
      tableInfo.MaxNumberOfColumns = 256; // not a correct value, Depends on data types used. (Example: 16,384 INTEGER (4-byte) values per row.)
      tableInfo.PartitionMethods = PartitionMethods.None;
      return tableInfo;
    }

    public override TemporaryTableInfo GetTemporaryTableInfo()
    {
      return null;
    }

    public override CheckConstraintInfo GetCheckConstraintInfo()
    {
      var checkContraintsInfo = new CheckConstraintInfo();
      checkContraintsInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      checkContraintsInfo.MaxIdentifierLength = MaxIdentifierLength;
      checkContraintsInfo.MaxExpressionLength = MaxCharLength;
      checkContraintsInfo.Features = CheckConstraintFeatures.None;
      return checkContraintsInfo;
    }

    public override PrimaryKeyConstraintInfo GetPrimaryKeyInfo()
    {
      var primaryKeyInfo = new PrimaryKeyConstraintInfo();
      primaryKeyInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      primaryKeyInfo.MaxIdentifierLength = MaxIdentifierLength;
      primaryKeyInfo.Features = PrimaryKeyConstraintFeatures.None;
      return primaryKeyInfo;
    }

    public override UniqueConstraintInfo GetUniqueConstraintInfo()
    {
      var uniqueContraintInfo = new UniqueConstraintInfo();
      uniqueContraintInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      uniqueContraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      uniqueContraintInfo.Features = UniqueConstraintFeatures.None;
      return uniqueContraintInfo;
    }

    public override IndexInfo GetIndexInfo()
    {
      var indexInfo = new IndexInfo();
      indexInfo.AllowedDdlStatements = DdlStatements.All;
      indexInfo.MaxIdentifierLength = MaxIdentifierLength;
      indexInfo.Features =
        IndexFeatures.Unique |
          IndexFeatures.Expressions;
      indexInfo.MaxNumberOfColumns = 10; // just my opinion
      indexInfo.MaxLength = 4096; // PageSize/4/CharacterSetSizeInByte
      indexInfo.PartitionMethods = PartitionMethods.None;
      return indexInfo;
    }

    public override ForeignKeyConstraintInfo GetForeignKeyConstraintInfo()
    {
      var foreignKeyConstraintInfo = new ForeignKeyConstraintInfo();
      foreignKeyConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      foreignKeyConstraintInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      foreignKeyConstraintInfo.Features = ForeignKeyConstraintFeatures.None;
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
      //      var info = new FullTextSearchInfo();
      //      info.Features = FullTextSearchFeatures.Full;
      //      return info;
    }

    public override QueryInfo GetQueryInfo()
    {
      var queryInfo = new QueryInfo();
      queryInfo.MaxNestedSubqueriesAmount = 100;
      queryInfo.ParameterPrefix = "@";
      queryInfo.MaxLength = MaxCharLength;
      queryInfo.MaxComparisonOperations = DoNotKnow;
      queryInfo.MaxQueryParameterCount = 0;
      queryInfo.Features =
        QueryFeatures.NamedParameters |
          QueryFeatures.ParameterPrefix |
            QueryFeatures.ScalarSubquery |
              QueryFeatures.Paging |
                QueryFeatures.Limit |
                  QueryFeatures.Offset |
                    QueryFeatures.UpdateLimit |
                      QueryFeatures.DeleteLimit;
      return queryInfo;
    }

    public override ServerFeatures GetServerFeatures()
    {
      return ServerFeatures.Savepoints;
    }

    public override IdentityInfo GetIdentityInfo()
    {
      return null;
    }

    public override DataTypeCollection GetDataTypesInfo()
    {
      var commonFeatures =
        DataTypeFeatures.Grouping |
          DataTypeFeatures.Indexing |
            DataTypeFeatures.KeyConstraint |
              DataTypeFeatures.Nullable |
                DataTypeFeatures.Ordering |
                  DataTypeFeatures.Multiple |
                    DataTypeFeatures.Default;
      var lobFeatures =
        DataTypeFeatures.Nullable |
          DataTypeFeatures.Multiple |
            DataTypeFeatures.Default;

      var dtc = new DataTypeCollection();

      dtc.Int16 = DataTypeInfo.Range(SqlType.Int16, commonFeatures,
        ValueRange.Int16,
        "smallint");

      dtc.Int32 = DataTypeInfo.Range(SqlType.Int32, commonFeatures,
        ValueRange.Int32, "integer");

      dtc.Int64 = DataTypeInfo.Range(SqlType.Int64, commonFeatures,
        ValueRange.Int64, "bigint");

      dtc.Decimal = DataTypeInfo.Fractional(SqlType.Decimal, commonFeatures,
        ValueRange.Decimal, 18, "numeric", "decimal");

      dtc.Float = DataTypeInfo.Range(SqlType.Float, commonFeatures,
        ValueRange.Float, "float");

      dtc.Double = DataTypeInfo.Range(SqlType.Double, commonFeatures,
        ValueRange.Double, "double precision");

      dtc.DateTime = DataTypeInfo.Range(SqlType.DateTime, commonFeatures,
        ValueRange.DateTime, "timestamp");

      dtc.Char = DataTypeInfo.Stream(SqlType.Char, commonFeatures, MaxCharLength, "char");
      dtc.VarChar = DataTypeInfo.Stream(SqlType.VarChar, commonFeatures, MaxCharLength, "varchar");
      dtc.VarCharMax = DataTypeInfo.Stream(SqlType.VarCharMax, lobFeatures, MaxTextLength, "blob sub_type 1");
      dtc.VarBinaryMax = DataTypeInfo.Stream(SqlType.VarBinaryMax, lobFeatures, MaxTextLength, "blob sub_type 0");

      return dtc;
    }

    public override IsolationLevels GetIsolationLevels()
    {
      return IsolationLevels.ReadCommitted | IsolationLevels.ReadUncommitted | IsolationLevels.RepeatableRead;
    }

    public override EntityInfo GetDomainInfo()
    {
      var info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = MaxIdentifierLength;
      return info;
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