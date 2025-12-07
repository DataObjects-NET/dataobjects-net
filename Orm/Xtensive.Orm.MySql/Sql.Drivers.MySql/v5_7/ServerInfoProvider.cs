// Copyright (C) 2011-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.MySql.v5_7
{
  internal class ServerInfoProvider : Info.ServerInfoProvider
  {
    private const int MaxIdentifierLength = 64;
    private const int MaxTableIdentifierLength = 128;
    private const int MaxTempTableIdentifierLength = 116;
    private const int DoNotKnow = int.MaxValue;

    /// <inheritdoc/>
    public override EntityInfo GetCollationInfo() => null;

    /// <inheritdoc/>
    public override EntityInfo GetCharacterSetInfo()
    {
      var characterSetInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.None
      };
      return characterSetInfo;
    }

    /// <inheritdoc/>
    public override EntityInfo GetTranslationInfo()
    {
      var translationInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.None
      };
      return translationInfo;
    }

    /// <inheritdoc/>
    public override EntityInfo GetTriggerInfo()
    {
      var triggerInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.All
      };
      return triggerInfo;
    }

    /// <inheritdoc/>
    public override EntityInfo GetStoredProcedureInfo()
    {
      var procedureInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.All
      };
      return procedureInfo;
    }

    /// <inheritdoc/>
    public override SequenceInfo GetSequenceInfo() => null;

    /// <inheritdoc/>
    public override EntityInfo GetDatabaseInfo()
    {
      var databaseInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.All
      };
      return databaseInfo;
    }

    /// <inheritdoc/>
    public override ColumnInfo GetColumnInfo()
    {
      var columnInfo = new ColumnInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        Features = ColumnFeatures.Identity | ColumnFeatures.Computed,
        AllowedDdlStatements = DdlStatements.All
      };
      return columnInfo;
    }

    /// <inheritdoc/>
    public override EntityInfo GetViewInfo()
    {
      var viewInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.All
      };
      return viewInfo;
    }

    /// <inheritdoc/>
    public override EntityInfo GetSchemaInfo()
    {
      var schemaInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.All
      };
      return schemaInfo;
    }

    /// <inheritdoc/>
    public override TableInfo GetTableInfo()
    {
      var tableInfo = new TableInfo {
        MaxIdentifierLength = MaxTableIdentifierLength,
        AllowedDdlStatements = DdlStatements.All,
        // From version  5.1.14
        PartitionMethods = PartitionMethods.Hash | PartitionMethods.Range | PartitionMethods.List
      };
      return tableInfo;
    }

    /// <inheritdoc/>
    public override TemporaryTableInfo GetTemporaryTableInfo()
    {
      var temporaryTableInfo = new TemporaryTableInfo {
        MaxIdentifierLength = MaxTempTableIdentifierLength,
        Features = TemporaryTableFeatures.Global | TemporaryTableFeatures.Local,
        AllowedDdlStatements = DdlStatements.All
      };
      return temporaryTableInfo;
    }

    /// <inheritdoc/>
    public override CheckConstraintInfo GetCheckConstraintInfo()
    {
      var checkConstraintInfo = new CheckConstraintInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        MaxExpressionLength = 4000,
        Features = CheckConstraintFeatures.None,
        AllowedDdlStatements = DdlStatements.All
      };
      return checkConstraintInfo;
    }

    /// <inheritdoc/>
    public override PrimaryKeyConstraintInfo GetPrimaryKeyInfo()
    {
      var primaryKeyInfo = new PrimaryKeyConstraintInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        Features = PrimaryKeyConstraintFeatures.Clustered,
        AllowedDdlStatements = DdlStatements.All,
        ConstantName = "PRIMARY"
      };
      return primaryKeyInfo;
    }

    /// <inheritdoc/>
    public override UniqueConstraintInfo GetUniqueConstraintInfo()
    {
      var uniqueConstraintInfo = new UniqueConstraintInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        Features = UniqueConstraintFeatures.Clustered | UniqueConstraintFeatures.Nullable,
        AllowedDdlStatements = DdlStatements.All
      };
      return uniqueConstraintInfo;
    }

    /// <inheritdoc/>
    public override IndexInfo GetIndexInfo()
    {
      var indexInfo = new IndexInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        MaxNumberOfColumns = 16,
        MaxLength = 900,
        AllowedDdlStatements = DdlStatements.All,
        Features = IndexFeatures.Unique | IndexFeatures.FullText,
        PartitionMethods = PartitionMethods.None
      };
      return indexInfo;
    }

    /// <inheritdoc/>
    public override ForeignKeyConstraintInfo GetForeignKeyConstraintInfo()
    {
      var referenceConstraintInfo = new ForeignKeyConstraintInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        Actions =
          ForeignKeyConstraintActions.NoAction |
          ForeignKeyConstraintActions.Cascade |
          ForeignKeyConstraintActions.SetDefault |
          ForeignKeyConstraintActions.SetNull,
        AllowedDdlStatements = DdlStatements.All
      };
      return referenceConstraintInfo;
    }

    /// <inheritdoc/>
    public override FullTextSearchInfo GetFullTextInfo() => null;

    /// <inheritdoc/>
    public override QueryInfo GetQueryInfo()
    {
      var queryInfo = new QueryInfo {
        MaxLength = 240_000_000, // 60000 * 4000, whatever that means
        MaxComparisonOperations = 1000,
        MaxNestedSubqueriesAmount = 32, //TODO: Determine max nested sub queries and variables.
        ParameterPrefix = "?",
        MaxQueryParameterCount = DoNotKnow,
        Features =
          QueryFeatures.NamedParameters |
          QueryFeatures.ParameterPrefix |
          QueryFeatures.Limit |
          QueryFeatures.Offset |
          QueryFeatures.InsertDefaultValues |
          QueryFeatures.UpdateDefaultValues |
          QueryFeatures.UpdateLimit |
          QueryFeatures.DeleteLimit |
          QueryFeatures.ExplicitJoinOrder |
          QueryFeatures.ScalarSubquery |
          QueryFeatures.SelfReferencingRowRemovalIsError |
          QueryFeatures.StrictJoinSyntax |
          QueryFeatures.Batches |
          QueryFeatures.ParameterAsColumn
      };
      return queryInfo;
    }

    /// <inheritdoc/>
    public override ServerFeatures GetServerFeatures() => ServerFeatures.Savepoints | ServerFeatures.TransactionalDdl;

    /// <inheritdoc/>
    public override IdentityInfo GetIdentityInfo()
    {
      var identityInfo = new IdentityInfo {
        Features =
          IdentityFeatures.Seed |
          IdentityFeatures.AutoIncrement |
          IdentityFeatures.AutoIncrementSettingsInMemory
      };
      return identityInfo;
    }

    /// <inheritdoc/>
    public override DataTypeCollection GetDataTypesInfo()
    {
      var types = new DataTypeCollection();

      var commonFeatures =
        DataTypeFeatures.Default |
        DataTypeFeatures.Nullable |
        DataTypeFeatures.NonKeyIndexing |
        DataTypeFeatures.Grouping |
        DataTypeFeatures.Ordering |
        DataTypeFeatures.Multiple;

      var indexFeatures = DataTypeFeatures.Indexing | DataTypeFeatures.KeyConstraint;

      var identityFeatures = DataTypeFeatures.Identity;

      types.Boolean = DataTypeInfo.Range(SqlType.Boolean, commonFeatures | indexFeatures,
        ValueRange.Bool, "boolean");

      types.Int8 = DataTypeInfo.Range(SqlType.Int8, commonFeatures | indexFeatures | identityFeatures,
        ValueRange.Byte, "tinyint");

      types.Int16 = DataTypeInfo.Range(SqlType.Int16, commonFeatures | indexFeatures | identityFeatures,
        ValueRange.Int16, "smallint");

      types.Int32 = DataTypeInfo.Range(SqlType.Int32, commonFeatures | indexFeatures | identityFeatures,
        ValueRange.Int32, "int");

      types.Int64 = DataTypeInfo.Range(SqlType.Int64, commonFeatures | indexFeatures | identityFeatures,
        ValueRange.Int64, "bigint");

      types.Decimal = DataTypeInfo.Fractional(SqlType.Decimal, commonFeatures | indexFeatures,
        ValueRange.Decimal, 38, "decimal", "numeric", "year");

      types.Float = DataTypeInfo.Range(SqlType.Float, commonFeatures | indexFeatures,
        ValueRange.Float, "float");

      types.Double = DataTypeInfo.Range(SqlType.Double, commonFeatures | indexFeatures,
        ValueRange.Double, "double precision");

      types.DateTime = DataTypeInfo.Range(SqlType.DateTime, commonFeatures | indexFeatures,
        new ValueRange<DateTime>(new DateTime(1000, 1, 1), new DateTime(9999, 12, 31)),
        "datetime(6)");

      types.DateOnly = DataTypeInfo.Range(SqlType.Date, commonFeatures | indexFeatures,
        new ValueRange<DateOnly>(new DateOnly(1000, 1, 1), new DateOnly(9999, 12, 31)),
        "date");

      types.TimeOnly = DataTypeInfo.Range(SqlType.Time, commonFeatures | indexFeatures, ValueRange.TimeOnly, "time(6)");

      types.Char = DataTypeInfo.Stream(SqlType.Char, commonFeatures | indexFeatures, 255, "char");
      types.VarChar = DataTypeInfo.Stream(SqlType.VarChar, commonFeatures | indexFeatures, 4000, "varchar");

      types.VarCharMax = DataTypeInfo.Regular(SqlType.VarCharMax, commonFeatures, "longtext");

      types.Binary = DataTypeInfo.Stream(SqlType.Binary, commonFeatures | indexFeatures, 255, "binary");
      types.VarBinary = DataTypeInfo.Stream(SqlType.VarBinary, commonFeatures | indexFeatures, 4000, "varbinary");
      types.VarBinaryMax = DataTypeInfo.Regular(SqlType.VarBinaryMax, commonFeatures, "longblob");
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
      var domainInfo = new EntityInfo {
        AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop,
        MaxIdentifierLength = MaxIdentifierLength
      };
      return domainInfo;
    }

    /// <inheritdoc/>
    public override AssertConstraintInfo GetAssertionInfo()
    {
      var assertConstraintInfo = new AssertConstraintInfo {
        AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop,
        MaxIdentifierLength = MaxIdentifierLength
      };
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