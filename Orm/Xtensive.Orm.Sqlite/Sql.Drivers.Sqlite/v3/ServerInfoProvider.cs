// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.Sqlite.v3
{
  internal class ServerInfoProvider : Info.ServerInfoProvider
  {
    private const int MaxIdentifierLength = 64;

    private readonly string databaseName;
    private readonly string defaultSchemaName;

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
      columnInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      return columnInfo;
    }

    public override EntityInfo GetViewInfo()
    {
      EntityInfo viewInfo = new EntityInfo();
      viewInfo.MaxIdentifierLength = MaxIdentifierLength;
      viewInfo.AllowedDdlStatements = DdlStatements.All;
      return viewInfo;
    }

    public override EntityInfo GetSchemaInfo()
    {
      EntityInfo schemaInfo = new EntityInfo();
      schemaInfo.MaxIdentifierLength = MaxIdentifierLength;
      schemaInfo.AllowedDdlStatements = DdlStatements.All;
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
      return null;
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
      indexInfo.Features = IndexFeatures.Unique | IndexFeatures.SortOrder;
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
      queryInfo.Features =
        QueryFeatures.NamedParameters
        | QueryFeatures.ParameterPrefix
        | QueryFeatures.Limit
        | QueryFeatures.Offset
        | QueryFeatures.InsertDefaultValues
        | QueryFeatures.StrictJoinSyntax;
      return queryInfo;
    }

    public override ServerFeatures GetServerFeatures()
    {
      return ServerFeatures.Savepoints | ServerFeatures.TransactionalKeyGenerators;
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

      var common = DataTypeFeatures.Default | DataTypeFeatures.Nullable | DataTypeFeatures.NonKeyIndexing | DataTypeFeatures.Grouping | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple;

      var index = DataTypeFeatures.Indexing | DataTypeFeatures.Clustering | DataTypeFeatures.FillFactor | DataTypeFeatures.KeyConstraint;

      var identity = DataTypeFeatures.Identity;

      types.Boolean = DataTypeInfo.Range(SqlType.Boolean, common | index, ValueRange.Bool, "bit");

      types.UInt8 = DataTypeInfo.Range(SqlType.UInt8, common | index | identity, ValueRange.Byte, "tinyint");

      types.Int16 = DataTypeInfo.Range(SqlType.Int16, common | index | identity, ValueRange.Int16, "smallint");

      types.Int32 = DataTypeInfo.Range(SqlType.Int32, common | index | identity, ValueRange.Int32, "integer", "int");

      types.Int64 = DataTypeInfo.Range(SqlType.Int64, common | index | identity, ValueRange.Int64, "bigint");

      types.Decimal = DataTypeInfo.Fractional(SqlType.Decimal, common | index, ValueRange.Decimal, 38, "decimal", "numeric");

      types.Float = DataTypeInfo.Range(SqlType.Float, common | index, ValueRange.Float, "real");

      types.Double = DataTypeInfo.Range(SqlType.Double, common | index, ValueRange.Double, "float");

      types.DateTime = DataTypeInfo.Range(SqlType.DateTime, common | index, new ValueRange<DateTime>(new DateTime(1753, 1, 1), new DateTime(9999, 12, 31)), "datetime");

      types.Char = DataTypeInfo.Stream(SqlType.Char, common | index, 4000, "nchar", "char");
      types.VarChar = DataTypeInfo.Stream(SqlType.VarChar, common | index, 4000, "varchar", "nvarchar");
      types.VarCharMax = DataTypeInfo.Stream(SqlType.VarCharMax, common, Int32.MaxValue, "text", "xml");

      //types.Binary = DataTypeInfo.Stream(SqlType.Binary, common | index, 4000, "blob");
      types.VarBinaryMax = DataTypeInfo.Stream(SqlType.VarBinaryMax, common, Int32.MaxValue, "blob");

      types.Guid = DataTypeInfo.Regular(SqlType.Guid, common | index, "uniqueidentifier");

      return types;
    }

    public override IsolationLevels GetIsolationLevels()
    {
      var levels = IsolationLevels.ReadUncommitted | IsolationLevels.ReadCommitted | IsolationLevels.RepeatableRead | IsolationLevels.Serializable | IsolationLevels.Snapshot;
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