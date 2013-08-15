// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.MySql.v5_0
{
  internal class ServerInfoProvider : Info.ServerInfoProvider
  {
    private const int MaxIdentifierLength = 64;

    private readonly string databaseName;
    private readonly string defaultSchemaName;

    /// <inheritdoc/>
    public override EntityInfo GetCollationInfo()
    {
      return null;
    }

    /// <inheritdoc/>
    public override EntityInfo GetCharacterSetInfo()
    {
      var characterSetInfo = new EntityInfo();
      characterSetInfo.MaxIdentifierLength = MaxIdentifierLength;
      characterSetInfo.AllowedDdlStatements = DdlStatements.None;
      return characterSetInfo;
    }

    /// <inheritdoc/>
    public override EntityInfo GetTranslationInfo()
    {
      var translationInfo = new EntityInfo();
      translationInfo.MaxIdentifierLength = MaxIdentifierLength;
      translationInfo.AllowedDdlStatements = DdlStatements.None;
      return translationInfo;
    }

    /// <inheritdoc/>
    public override EntityInfo GetTriggerInfo()
    {
      var triggerInfo = new EntityInfo();
      triggerInfo.MaxIdentifierLength = MaxIdentifierLength;
      triggerInfo.AllowedDdlStatements = DdlStatements.All;
      return triggerInfo;
    }

    /// <inheritdoc/>
    public override EntityInfo GetStoredProcedureInfo()
    {
      var procedureInfo = new EntityInfo();
      procedureInfo.MaxIdentifierLength = MaxIdentifierLength;
      procedureInfo.AllowedDdlStatements = DdlStatements.All;
      return procedureInfo;
    }

    /// <inheritdoc/>
    public override SequenceInfo GetSequenceInfo()
    {
      return null;
    }

    /// <inheritdoc/>
    public override EntityInfo GetDatabaseInfo()
    {
      var databaseInfo = new EntityInfo();
      databaseInfo.MaxIdentifierLength = MaxIdentifierLength;
      databaseInfo.AllowedDdlStatements = DdlStatements.All;
      return databaseInfo;
    }

    /// <inheritdoc/>
    public override ColumnInfo GetColumnInfo()
    {
      var columnInfo = new ColumnInfo();
      columnInfo.MaxIdentifierLength = MaxIdentifierLength;
      columnInfo.Features = ColumnFeatures.Identity | ColumnFeatures.Computed;
      columnInfo.AllowedDdlStatements = DdlStatements.All;
      return columnInfo;
    }

    /// <inheritdoc/>
    public override EntityInfo GetViewInfo()
    {
      var viewInfo = new EntityInfo();
      viewInfo.MaxIdentifierLength = MaxIdentifierLength;
      viewInfo.AllowedDdlStatements = DdlStatements.All;
      return viewInfo;
    }

    /// <inheritdoc/>
    public override EntityInfo GetSchemaInfo()
    {
      var schemaInfo = new EntityInfo();
      schemaInfo.MaxIdentifierLength = MaxIdentifierLength;
      schemaInfo.AllowedDdlStatements = DdlStatements.All;
      return schemaInfo;
    }

    /// <inheritdoc/>
    public override TableInfo GetTableInfo()
    {
      var tableInfo = new TableInfo();
      tableInfo.MaxIdentifierLength = MaxIdentifierLength;
      tableInfo.AllowedDdlStatements = DdlStatements.All;
      tableInfo.PartitionMethods = PartitionMethods.None;
      return tableInfo;
    }

    /// <inheritdoc/>
    public override TemporaryTableInfo GetTemporaryTableInfo()
    {
      var temporaryTableInfo = new TemporaryTableInfo();
      temporaryTableInfo.MaxIdentifierLength = 116;
      temporaryTableInfo.Features = TemporaryTableFeatures.Global | TemporaryTableFeatures.Local;
      temporaryTableInfo.AllowedDdlStatements = DdlStatements.All;
      return temporaryTableInfo;
    }

    /// <inheritdoc/>
    public override CheckConstraintInfo GetCheckConstraintInfo()
    {
      var checkConstraintInfo = new CheckConstraintInfo();
      checkConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      checkConstraintInfo.MaxExpressionLength = 4000;
      checkConstraintInfo.Features = CheckConstraintFeatures.None;
      checkConstraintInfo.AllowedDdlStatements = DdlStatements.All;
      return checkConstraintInfo;
    }

    /// <inheritdoc/>
    public override PrimaryKeyConstraintInfo GetPrimaryKeyInfo()
    {
      var primaryKeyInfo = new PrimaryKeyConstraintInfo();
      primaryKeyInfo.MaxIdentifierLength = MaxIdentifierLength;
      primaryKeyInfo.Features = PrimaryKeyConstraintFeatures.Clustered;
      primaryKeyInfo.AllowedDdlStatements = DdlStatements.All;
      primaryKeyInfo.ConstantName = "PRIMARY";
      return primaryKeyInfo;
    }

    /// <inheritdoc/>
    public override UniqueConstraintInfo GetUniqueConstraintInfo()
    {
      var uniqueConstraintInfo = new UniqueConstraintInfo();
      uniqueConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      uniqueConstraintInfo.Features = UniqueConstraintFeatures.Clustered | UniqueConstraintFeatures.Nullable;
      uniqueConstraintInfo.AllowedDdlStatements = DdlStatements.All;
      return uniqueConstraintInfo;
    }

    /// <inheritdoc/>
    public override IndexInfo GetIndexInfo()
    {
      var indexInfo = new IndexInfo();
      indexInfo.MaxIdentifierLength = MaxIdentifierLength;
      indexInfo.MaxNumberOfColumns = 16;
      indexInfo.MaxLength = 900;
      indexInfo.AllowedDdlStatements = DdlStatements.All;
      indexInfo.Features = IndexFeatures.Unique | IndexFeatures.FullText;
      indexInfo.PartitionMethods = PartitionMethods.None;
      return indexInfo;
    }

    /// <inheritdoc/>
    public override ForeignKeyConstraintInfo GetForeignKeyConstraintInfo()
    {
      var referenceConstraintInfo = new ForeignKeyConstraintInfo();
      referenceConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      referenceConstraintInfo.Actions =
        ForeignKeyConstraintActions.NoAction |
        ForeignKeyConstraintActions.Cascade |
        ForeignKeyConstraintActions.SetDefault |
        ForeignKeyConstraintActions.SetNull;
      referenceConstraintInfo.AllowedDdlStatements = DdlStatements.All;
      return referenceConstraintInfo;
    }

    /// <inheritdoc/>
    public override FullTextSearchInfo GetFullTextInfo()
    {
      return null;
    }

    /// <inheritdoc/>
    public override QueryInfo GetQueryInfo()
    {
      var queryInfo = new QueryInfo();
      queryInfo.MaxLength = 60000 * 4000;
      queryInfo.MaxComparisonOperations = 1000;
      queryInfo.MaxNestedSubqueriesAmount = 32; //TODO: Determine max nested sub queries and variables.
      queryInfo.ParameterPrefix = "?";
      queryInfo.Features =
        QueryFeatures.NamedParameters |
        QueryFeatures.ParameterPrefix |
        QueryFeatures.Limit |
        QueryFeatures.Offset |
        QueryFeatures.InsertDefaultValues |
        QueryFeatures.UpdateDefaultValues |
        QueryFeatures.ExplicitJoinOrder |
        QueryFeatures.ScalarSubquery |
        QueryFeatures.NotRemovableSelfForeignKey;

      return queryInfo;
    }

    /// <inheritdoc/>
    public override ServerFeatures GetServerFeatures()
    {
      return ServerFeatures.Savepoints | ServerFeatures.TransactionalDdl;
    }

    /// <inheritdoc/>
    public override IdentityInfo GetIdentityInfo()
    {
      var identityInfo = new IdentityInfo();
      identityInfo.Features = IdentityFeatures.Seed | IdentityFeatures.AutoIncrement;
      return identityInfo;
    }

    /// <inheritdoc/>
    public override DataTypeCollection GetDataTypesInfo()
    {
      var types = new DataTypeCollection();

      DataTypeFeatures common = DataTypeFeatures.Default | DataTypeFeatures.Nullable | DataTypeFeatures.NonKeyIndexing |
        DataTypeFeatures.Grouping | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple;

      DataTypeFeatures index = DataTypeFeatures.Indexing | DataTypeFeatures.KeyConstraint;

      DataTypeFeatures identity = DataTypeFeatures.Identity;

      types.Boolean = DataTypeInfo.Range(SqlType.Boolean, common | index,
        ValueRange.Bool, "boolean");

      types.Int8 = DataTypeInfo.Range(SqlType.Int8, common | index | identity,
        ValueRange.Byte, "tinyint");

      types.Int16 = DataTypeInfo.Range(SqlType.Int16, common | index | identity,
        ValueRange.Int16, "smallint");

      types.Int32 = DataTypeInfo.Range(SqlType.Int32, common | index | identity,
        ValueRange.Int32, "int");

      types.Int64 = DataTypeInfo.Range(SqlType.Int64, common | index | identity,
        ValueRange.Int64, "bigint");

      types.Decimal = DataTypeInfo.Fractional(SqlType.Decimal, common | index,
        ValueRange.Decimal, 38, "decimal", "numeric", "year");

      types.Float = DataTypeInfo.Range(SqlType.Float, common | index,
        ValueRange.Float, "float");

      types.Double = DataTypeInfo.Range(SqlType.Double, common | index,
        ValueRange.Double, "double precision");

      types.DateTime = DataTypeInfo.Range(SqlType.DateTime, common | index,
        new ValueRange<DateTime>(new DateTime(1000, 1, 1), new DateTime(9999, 12, 31)),
        "datetime", "time");

      types.Char = DataTypeInfo.Stream(SqlType.Char, common | index, 255, "char");
      types.VarChar = DataTypeInfo.Stream(SqlType.VarChar, common | index, 4000, "varchar");

      types.VarCharMax = DataTypeInfo.Regular(SqlType.VarCharMax, common, "longtext");

      types.Binary = DataTypeInfo.Stream(SqlType.Binary, common | index, 255, "binary");
      types.VarBinary = DataTypeInfo.Stream(SqlType.VarBinary, common | index, 4000, "varbinary");
      types.VarBinaryMax = DataTypeInfo.Regular(SqlType.VarBinaryMax, common, "longblob");
      //types.Guid = DataTypeInfo.Regular(SqlType.Guid, common | index, "uuid()");

      return types;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override EntityInfo GetDomainInfo()
    {
      var domainInfo = new EntityInfo();
      domainInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      domainInfo.MaxIdentifierLength = MaxIdentifierLength;
      return domainInfo;
    }

    /// <inheritdoc/>
    public override AssertConstraintInfo GetAssertionInfo()
    {
      var assertConstraintInfo = new AssertConstraintInfo();
      assertConstraintInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      assertConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      return assertConstraintInfo;
    }

    /// <inheritdoc/>
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