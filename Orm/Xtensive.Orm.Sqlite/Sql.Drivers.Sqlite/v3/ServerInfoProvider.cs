// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.Sqlite.v3
{
  internal class ServerInfoProvider : Info.ServerInfoProvider
  {
    private const int MaxIdentifierLength = 64;

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
      return null;
    }

    public override EntityInfo GetTriggerInfo()
    {
      EntityInfo triggerInfo = new EntityInfo();
      triggerInfo.MaxIdentifierLength = MaxIdentifierLength;
      triggerInfo.AllowedDdlStatements = DdlStatements.All;
      return triggerInfo;
    }

    public override EntityInfo GetStoredProcedureInfo()
    {
      return null;
    }

    public override SequenceInfo GetSequenceInfo()
    {
      return null;
    }

    public override EntityInfo GetDatabaseInfo()
    {
      EntityInfo databaseInfo = new EntityInfo();
      databaseInfo.MaxIdentifierLength = MaxIdentifierLength;
      databaseInfo.AllowedDdlStatements = DdlStatements.All;
      return databaseInfo;
    }

    public override ColumnInfo GetColumnInfo()
    {
      ColumnInfo columnInfo = new ColumnInfo();
      columnInfo.MaxIdentifierLength = MaxIdentifierLength;
      columnInfo.Features = ColumnFeatures.Identity | ColumnFeatures.Computed;
      columnInfo.AllowedDdlStatements = DdlStatements.Create;
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
      tableInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate;
      tableInfo.PartitionMethods = PartitionMethods.List | PartitionMethods.Range | PartitionMethods.Hash;
      return tableInfo;
    }

    public override TemporaryTableInfo GetTemporaryTableInfo()
    {
      var temporaryTableInfo = new TemporaryTableInfo();
      temporaryTableInfo.MaxIdentifierLength = MaxIdentifierLength;
      temporaryTableInfo.AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate;
      temporaryTableInfo.Features = TemporaryTableFeatures.Local;
      return temporaryTableInfo;
    }

    public override CheckConstraintInfo GetCheckConstraintInfo()
    {
      var checkConstraintInfo = new CheckConstraintInfo();
      checkConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      checkConstraintInfo.MaxExpressionLength = 4000;
      checkConstraintInfo.Features = CheckConstraintFeatures.None;
      checkConstraintInfo.AllowedDdlStatements = DdlStatements.All;
      return checkConstraintInfo;
    }

    public override PrimaryKeyConstraintInfo GetPrimaryKeyInfo()
    {
      var primaryKeyInfo = new PrimaryKeyConstraintInfo();
      primaryKeyInfo.MaxIdentifierLength = MaxIdentifierLength;
      primaryKeyInfo.Features = PrimaryKeyConstraintFeatures.Clustered;
      primaryKeyInfo.AllowedDdlStatements = DdlStatements.All;
      primaryKeyInfo.ConstantName = Extractor.PrimaryKeyName;
      return primaryKeyInfo;
    }

    public override FullTextSearchInfo GetFullTextInfo()
    {
      return null;
    }

    public override UniqueConstraintInfo GetUniqueConstraintInfo()
    {
      var uniqueConstraintInfo = new UniqueConstraintInfo();
      uniqueConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      uniqueConstraintInfo.Features = UniqueConstraintFeatures.Nullable;
      uniqueConstraintInfo.AllowedDdlStatements = DdlStatements.All;
      return uniqueConstraintInfo;
    }

    public override IndexInfo GetIndexInfo()
    {
      var indexInfo = new IndexInfo();
      indexInfo.MaxIdentifierLength = MaxIdentifierLength;
      indexInfo.MaxNumberOfColumns = 16;
      indexInfo.MaxLength = 900;
      indexInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      // SQLite supports sort order but this is not enabled by default
      // Also extract is incapable of extracting such information anyway
      indexInfo.Features = IndexFeatures.Unique;
      indexInfo.PartitionMethods = PartitionMethods.None;
      return indexInfo;
    }

    public override ForeignKeyConstraintInfo GetForeignKeyConstraintInfo()
    {
      return null;
    }

    public override QueryInfo GetQueryInfo()
    {
      var queryInfo = new QueryInfo();
      queryInfo.MaxLength = 1000000; //http://www.sqlite.org/limits.html
      queryInfo.MaxComparisonOperations = 1000;
      queryInfo.MaxNestedSubqueriesAmount = 32;
      queryInfo.ParameterPrefix = "@";
      queryInfo.MaxQueryParameterCount = 999;
      queryInfo.Features =
        QueryFeatures.NamedParameters
        | QueryFeatures.ParameterPrefix
        | QueryFeatures.Limit
        | QueryFeatures.Offset
        | QueryFeatures.InsertDefaultValues
        | QueryFeatures.StrictJoinSyntax
        | QueryFeatures.ScalarSubquery
        | QueryFeatures.ParameterAsColumn;
      return queryInfo;
    }

    public override ServerFeatures GetServerFeatures()
    {
      var result = ServerFeatures.Savepoints
                   | ServerFeatures.TransactionalKeyGenerators
                   | ServerFeatures.ExclusiveWriterConnection
                   | ServerFeatures.DateTimeEmulation
                   | ServerFeatures.DateTimeOffsetEmulation;
      var dataSource = Driver.CoreServerInfo.DatabaseName.Trim().ToLowerInvariant();
      if (dataSource==":memory:")
        result |= ServerFeatures.SingleConnection;
      return result;
    }

    public override IdentityInfo GetIdentityInfo()
    {
      var identityInfo = new IdentityInfo();
      identityInfo.Features = IdentityFeatures.None;
      return identityInfo;
    }

    public override DataTypeCollection GetDataTypesInfo()
    {
      var types = new DataTypeCollection();

      var common = DataTypeFeatures.Default | DataTypeFeatures.Nullable
                   | DataTypeFeatures.NonKeyIndexing | DataTypeFeatures.Grouping
                   | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple;

      var index = DataTypeFeatures.Indexing | DataTypeFeatures.Clustering
                  | DataTypeFeatures.FillFactor | DataTypeFeatures.KeyConstraint;

      var identity = DataTypeFeatures.Identity;

      types.Boolean = DataTypeInfo.Range(SqlType.Boolean, common | index, ValueRange.Bool, "bit");

      types.UInt8 = DataTypeInfo.Range(SqlType.UInt8, common | index | identity, ValueRange.Byte, "tinyint");

      types.Int16 = DataTypeInfo.Range(SqlType.Int16, common | index | identity, ValueRange.Int16, "smallint");

      types.Int32 = DataTypeInfo.Range(SqlType.Int32, common | index | identity, ValueRange.Int32, "integer", "int");

      types.Int64 = DataTypeInfo.Range(SqlType.Int64, common | index | identity, ValueRange.Int64, "bigint");

      types.Decimal = DataTypeInfo.Regular(SqlType.Decimal, common | index, "decimal", "numeric");

      types.Float = DataTypeInfo.Range(SqlType.Float, common | index, ValueRange.Float, "real");

      types.Double = DataTypeInfo.Range(SqlType.Double, common | index, ValueRange.Double, "float");

      types.DateTime = DataTypeInfo.Range(SqlType.DateTime, common | index, ValueRange.DateTime, "datetime");

      types.DateTimeOffset = DataTypeInfo.Range(SqlType.DateTimeOffset, common | index, ValueRange.DateTimeOffset, "datetimeoffset");

      types.DateOnly = DataTypeInfo.Range(SqlType.Date, common | index, ValueRange.DateOnly, "date");

      types.TimeOnly = DataTypeInfo.Range(SqlType.Time, common | index, ValueRange.TimeOnly, "time");

      types.VarCharMax = DataTypeInfo.Regular(SqlType.VarCharMax, common | index,
        "varchar", "nvarchar", "nchar", "char", "text", "xml");
      types.VarBinaryMax = DataTypeInfo.Regular(SqlType.VarBinaryMax, common, "blob");

      types.Guid = DataTypeInfo.Regular(SqlType.Guid, common | index, "uniqueidentifier", "guid");

      return types;
    }

    public override IsolationLevels GetIsolationLevels()
    {
      var levels = IsolationLevels.ReadUncommitted
                   | IsolationLevels.ReadCommitted
                   | IsolationLevels.RepeatableRead
                   | IsolationLevels.Serializable
                   | IsolationLevels.Snapshot;
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
