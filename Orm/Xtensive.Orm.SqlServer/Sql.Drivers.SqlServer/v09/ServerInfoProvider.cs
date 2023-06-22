// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Info;
using SqlServerConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.Drivers.SqlServer.v09
{
  internal class ServerInfoProvider : Info.ServerInfoProvider
  {
    protected const int MaxIdentifierLength = 128;

    public override EntityInfo GetCollationInfo()
    {
      EntityInfo collationInfo = new EntityInfo();
      collationInfo.MaxIdentifierLength = MaxIdentifierLength;
      collationInfo.AllowedDdlStatements = DdlStatements.None;
      return collationInfo;
    }

    public override EntityInfo GetCharacterSetInfo()
    {
      EntityInfo characterSetInfo = new EntityInfo();
      characterSetInfo.MaxIdentifierLength = MaxIdentifierLength;
      characterSetInfo.AllowedDdlStatements = DdlStatements.None;
      return characterSetInfo;
    }

    public override EntityInfo GetTranslationInfo()
    {
      EntityInfo translationInfo = new EntityInfo();
      translationInfo.MaxIdentifierLength = MaxIdentifierLength;
      translationInfo.AllowedDdlStatements = DdlStatements.None;
      return translationInfo;
    }

    public override EntityInfo GetTriggerInfo()
    {
      EntityInfo triggerInfo = new EntityInfo();
      triggerInfo.MaxIdentifierLength = MaxIdentifierLength;
      triggerInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate;
      return triggerInfo;
    }

    public override EntityInfo GetStoredProcedureInfo()
    {
      EntityInfo procedureInfo = new EntityInfo();
      procedureInfo.MaxIdentifierLength = MaxIdentifierLength;
      procedureInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate;
      return procedureInfo;
    }

    public override SequenceInfo GetSequenceInfo()
    {
      return null;
    }

    public override EntityInfo GetDatabaseInfo()
    {
      EntityInfo databaseInfo = new EntityInfo();
      databaseInfo.MaxIdentifierLength = MaxIdentifierLength;
      databaseInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate;
      return databaseInfo;
    }

    public override ColumnInfo GetColumnInfo()
    {
      ColumnInfo columnInfo = new ColumnInfo();
      columnInfo.MaxIdentifierLength = MaxIdentifierLength;
      columnInfo.Features = ColumnFeatures.Identity | ColumnFeatures.Computed;
      columnInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate;
      return columnInfo;
    }

    public override EntityInfo GetViewInfo()
    {
      EntityInfo viewInfo = new EntityInfo();
      viewInfo.MaxIdentifierLength = MaxIdentifierLength;
      viewInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate;
      return viewInfo;
    }

    public override EntityInfo GetSchemaInfo()
    {
      EntityInfo schemaInfo = new EntityInfo();
      schemaInfo.MaxIdentifierLength = MaxIdentifierLength;
      schemaInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate;
      return schemaInfo;
    }

    public override TableInfo GetTableInfo()
    {
      var tableInfo = new TableInfo();
      tableInfo.MaxIdentifierLength = MaxIdentifierLength;
      tableInfo.AllowedDdlStatements = DdlStatements.All;
      tableInfo.PartitionMethods = PartitionMethods.List | PartitionMethods.Range | PartitionMethods.Hash;
      return tableInfo;
    }

    public override TemporaryTableInfo GetTemporaryTableInfo()
    {
      var temporaryTableInfo = new TemporaryTableInfo();
      temporaryTableInfo.MaxIdentifierLength = 116;
      temporaryTableInfo.Features = TemporaryTableFeatures.Global | TemporaryTableFeatures.Local;
      temporaryTableInfo.AllowedDdlStatements = DdlStatements.All;
      return temporaryTableInfo;
    }

    public override CheckConstraintInfo GetCheckConstraintInfo()
    {
      var checkConstraintInfo = new CheckConstraintInfo();
      checkConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      checkConstraintInfo.MaxExpressionLength = 4000;
      checkConstraintInfo.Features = CheckConstraintFeatures.None;
      checkConstraintInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate;
      return checkConstraintInfo;
    }

    public override PrimaryKeyConstraintInfo GetPrimaryKeyInfo()
    {
      var primaryKeyInfo = new PrimaryKeyConstraintInfo();
      primaryKeyInfo.MaxIdentifierLength = MaxIdentifierLength;
      primaryKeyInfo.Features = PrimaryKeyConstraintFeatures.Clustered;
      primaryKeyInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate;
      return primaryKeyInfo;
    }

    public override UniqueConstraintInfo GetUniqueConstraintInfo()
    {
      var uniqueConstraintInfo = new UniqueConstraintInfo();
      uniqueConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      uniqueConstraintInfo.Features = UniqueConstraintFeatures.Clustered | UniqueConstraintFeatures.Nullable;
      uniqueConstraintInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate;
      return uniqueConstraintInfo;
    }

    public override IndexInfo GetIndexInfo()
    {
      var indexInfo = new IndexInfo();
      indexInfo.MaxIdentifierLength = MaxIdentifierLength;
      indexInfo.MaxNumberOfColumns = 16;
      indexInfo.MaxLength = 900;
      indexInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate;
      indexInfo.Features =
        IndexFeatures.Clustered |
        IndexFeatures.FillFactor |
        IndexFeatures.Unique |
        IndexFeatures.NonKeyColumns |
        IndexFeatures.SortOrder | 
        IndexFeatures.FullText;
      indexInfo.PartitionMethods = PartitionMethods.Range;
      return indexInfo;
    }

    public override ForeignKeyConstraintInfo GetForeignKeyConstraintInfo()
    {
      var referenceConstraintInfo = new ForeignKeyConstraintInfo();
      referenceConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      referenceConstraintInfo.Actions =
        ForeignKeyConstraintActions.NoAction |
        ForeignKeyConstraintActions.Cascade |
        ForeignKeyConstraintActions.SetDefault |
        ForeignKeyConstraintActions.SetNull;
      referenceConstraintInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate;
      return referenceConstraintInfo;
    }

    public override FullTextSearchInfo GetFullTextInfo()
    {
      var info = new FullTextSearchInfo();
      info.Features = FullTextSearchFeatures.SingleKeyRankTable;
      return info;
    }

    public override QueryInfo GetQueryInfo()
    {
      var queryInfo = new QueryInfo();
      queryInfo.MaxLength = 60000 * 4000;
      queryInfo.MaxComparisonOperations = 1000;
      queryInfo.MaxNestedSubqueriesAmount = 32;
      queryInfo.ParameterPrefix = "@";
      queryInfo.MaxQueryParameterCount = 2098;
      queryInfo.Features =
        QueryFeatures.NamedParameters |
        QueryFeatures.ParameterPrefix |
        QueryFeatures.Batches |
        QueryFeatures.CrossApply |
        QueryFeatures.UpdateFrom |
        QueryFeatures.UpdateLimit |
        QueryFeatures.DeleteFrom |
        QueryFeatures.DeleteLimit |
        QueryFeatures.Limit |
        QueryFeatures.InsertDefaultValues |
        QueryFeatures.UpdateDefaultValues |
        QueryFeatures.RowNumber |
        QueryFeatures.MultischemaQueries |
        QueryFeatures.MultidatabaseQueries |
        QueryFeatures.ScalarSubquery |
        QueryFeatures.PagingRequiresOrderBy |
        QueryFeatures.ParameterAsColumn;
      return queryInfo;
    }

    public override ServerFeatures GetServerFeatures()
    {
      return ServerFeatures.Savepoints | ServerFeatures.TransactionalDdl | ServerFeatures.FullTextColumnDataTypeSpecification;
    }

    public override IdentityInfo GetIdentityInfo()
    {
      var identityInfo = new IdentityInfo();
      identityInfo.Features = IdentityFeatures.Seed | IdentityFeatures.Increment | IdentityFeatures.AutoIncrement;
      return identityInfo;
    }

    public override DataTypeCollection GetDataTypesInfo()
    {
      var types = new DataTypeCollection();

      var common = DataTypeFeatures.Default | DataTypeFeatures.Nullable | DataTypeFeatures.NonKeyIndexing |
        DataTypeFeatures.Grouping | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple;

      var index = DataTypeFeatures.Indexing | DataTypeFeatures.Clustering |
        DataTypeFeatures.FillFactor | DataTypeFeatures.KeyConstraint;

      var identity = DataTypeFeatures.Identity;

      types.Boolean = DataTypeInfo.Range(SqlType.Boolean, common | index,
        ValueRange.Bool, "bit");
     
      types.UInt8 = DataTypeInfo.Range(SqlType.UInt8, common | index | identity,
        ValueRange.Byte, "tinyint");

      types.Int16 = DataTypeInfo.Range(SqlType.Int16, common | index | identity,
        ValueRange.Int16, "smallint");

      types.Int32 = DataTypeInfo.Range(SqlType.Int32, common | index | identity,
        ValueRange.Int32, "integer", "int");

      types.Int64 = DataTypeInfo.Range(SqlType.Int64, common | index | identity,
        ValueRange.Int64, "bigint");

      types.Decimal = DataTypeInfo.Fractional(SqlType.Decimal, common | index,
        ValueRange.Decimal, 38, "decimal", "numeric");
      
      types.Float = DataTypeInfo.Range(SqlType.Float, common | index,
        ValueRange.Float, "real");

      types.Double = DataTypeInfo.Range(SqlType.Double, common | index,
        ValueRange.Double, "float");

      types.DateTime = DataTypeInfo.Range(SqlType.DateTime, common | index,
        new ValueRange<DateTime>(new DateTime(1753, 1, 1), new DateTime(9999, 12, 31)),
        "datetime", "smalldatetime");
#if NET6_0_OR_GREATER
      types.DateOnly = DataTypeInfo.Range(SqlType.Date, common | index, new ValueRange<DateOnly>(new DateOnly(1, 1, 1), new DateOnly(9999, 12, 31)), "date");
      types.TimeOnly = DataTypeInfo.Range(SqlType.Time, common | index, new ValueRange<TimeOnly>(TimeOnly.MinValue, TimeOnly.MaxValue), "time");
#endif

      types.Char = DataTypeInfo.Stream(SqlType.Char, common | index, 4000, "nchar", "char");
      types.VarChar = DataTypeInfo.Stream(SqlType.VarChar, common | index, 4000, "nvarchar", "varchar");
      types.VarCharMax = DataTypeInfo.Regular(SqlType.VarCharMax, common, "nvarchar(max)", "ntext", "varchar(max)", "text", "xml");

      types.Binary = DataTypeInfo.Stream(SqlType.Binary, common | index, 4000, "binary");
      types.VarBinary = DataTypeInfo.Stream(SqlType.VarBinary, common | index, 4000, "varbinary");
      types.VarBinaryMax = DataTypeInfo.Regular(SqlType.VarBinaryMax, common, "varbinary(max)", "image");
      
      types.Guid = DataTypeInfo.Regular(SqlType.Guid, common | index, "uniqueidentifier");

      return types;
    }

    public override IsolationLevels GetIsolationLevels()
    {
      var levels =
        IsolationLevels.ReadUncommitted |
        IsolationLevels.ReadCommitted |
        IsolationLevels.RepeatableRead |
        IsolationLevels.Serializable |
        IsolationLevels.Snapshot;
      return levels;
    }

    public override EntityInfo GetDomainInfo()
    {
      var domainInfo = new EntityInfo();
      domainInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      domainInfo.MaxIdentifierLength = MaxIdentifierLength;
      return domainInfo;
    }

    public override AssertConstraintInfo GetAssertionInfo()
    {
      var assertConstraintInfo = new AssertConstraintInfo();
      assertConstraintInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      assertConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      return assertConstraintInfo;
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