// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kryuchkov
// Created:    2009.07.07

using System;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.SqlServer.v13
{
  internal class ServerInfoProvider : Info.ServerInfoProvider
  {
    protected const int MaxIdentifierLength = 128;

    public override EntityInfo GetCollationInfo()
    {
      var collationInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.None
      };
      return collationInfo;
    }

    public override EntityInfo GetCharacterSetInfo()
    {
      var characterSetInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.None
      };
      return characterSetInfo;
    }

    public override EntityInfo GetTranslationInfo()
    {
      var translationInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.None
      };
      return translationInfo;
    }

    public override EntityInfo GetTriggerInfo()
    {
      var triggerInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate
      };
      return triggerInfo;
    }

    public override EntityInfo GetStoredProcedureInfo()
    {
      var procedureInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate
      };
      return procedureInfo;
    }

    public override SequenceInfo GetSequenceInfo()
    {
      var info = new SequenceInfo {
        AllowedDdlStatements = DdlStatements.All,
        Features = SequenceFeatures.Cache,
        MaxIdentifierLength = MaxIdentifierLength
      };
      return info;
    }

    public override EntityInfo GetDatabaseInfo()
    {
      var databaseInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate
      };
      return databaseInfo;
    }

    public override ColumnInfo GetColumnInfo()
    {
      var columnInfo = new ColumnInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        Features = ColumnFeatures.Identity | ColumnFeatures.Computed,
        AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate
      };
      return columnInfo;
    }

    public override EntityInfo GetViewInfo()
    {
      var viewInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate
      };
      return viewInfo;
    }

    public override EntityInfo GetSchemaInfo()
    {
      var schemaInfo = new EntityInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate
      };
      return schemaInfo;
    }

    public override TableInfo GetTableInfo()
    {
      var tableInfo = new TableInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.All,
        PartitionMethods = PartitionMethods.List | PartitionMethods.Range | PartitionMethods.Hash
      };
      return tableInfo;
    }

    public override TemporaryTableInfo GetTemporaryTableInfo()
    {
      var temporaryTableInfo = new TemporaryTableInfo {
        MaxIdentifierLength = 116,
        Features = TemporaryTableFeatures.Global | TemporaryTableFeatures.Local,
        AllowedDdlStatements = DdlStatements.All
      };
      return temporaryTableInfo;
    }

    public override CheckConstraintInfo GetCheckConstraintInfo()
    {
      var checkConstraintInfo = new CheckConstraintInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        MaxExpressionLength = 4000,
        Features = CheckConstraintFeatures.None,
        AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate
      };
      return checkConstraintInfo;
    }

    public override PrimaryKeyConstraintInfo GetPrimaryKeyInfo()
    {
      var primaryKeyInfo = new PrimaryKeyConstraintInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        Features = PrimaryKeyConstraintFeatures.Clustered,
        AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate
      };
      return primaryKeyInfo;
    }

    public override UniqueConstraintInfo GetUniqueConstraintInfo()
    {
      var uniqueConstraintInfo = new UniqueConstraintInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        Features = UniqueConstraintFeatures.Clustered | UniqueConstraintFeatures.Nullable,
        AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate
      };
      return uniqueConstraintInfo;
    }

    public override IndexInfo GetIndexInfo()
    {
      var indexInfo = new IndexInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        MaxNumberOfColumns = 16,
        MaxLength = 900,
        AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate,
        Features =
          IndexFeatures.Clustered |
          IndexFeatures.FillFactor |
          IndexFeatures.Unique |
          IndexFeatures.NonKeyColumns |
          IndexFeatures.SortOrder |
          IndexFeatures.FullText |
          IndexFeatures.Filtered,
        PartitionMethods = PartitionMethods.Range
      };
      return indexInfo;
    }

    public override ForeignKeyConstraintInfo GetForeignKeyConstraintInfo()
    {
      var referenceConstraintInfo = new ForeignKeyConstraintInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        Actions =
          ForeignKeyConstraintActions.NoAction |
          ForeignKeyConstraintActions.Cascade |
          ForeignKeyConstraintActions.SetDefault |
          ForeignKeyConstraintActions.SetNull,
        AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Truncate
      };
      return referenceConstraintInfo;
    }

    public override FullTextSearchInfo GetFullTextInfo()
    {
      var info = new FullTextSearchInfo {
        Features = FullTextSearchFeatures.SingleKeyRankTable
      };
      return info;
    }

    public override QueryInfo GetQueryInfo()
    {
      var queryInfo = new QueryInfo {
        MaxLength = 240_000_000, //60000 * 4000
        MaxComparisonOperations = 1000,
        MaxNestedSubqueriesAmount = 32,
        ParameterPrefix = "@",
        MaxQueryParameterCount = 2098,
        Features =
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
          QueryFeatures.ParameterAsColumn |
          QueryFeatures.Offset |
          QueryFeatures.ZeroLimitIsError
      };
      return queryInfo;
    }

    public override ServerFeatures GetServerFeatures()
    {
      return ServerFeatures.Savepoints |
        ServerFeatures.TransactionalDdl |
        ServerFeatures.FullTextColumnDataTypeSpecification;
    }

    public override IdentityInfo GetIdentityInfo()
    {
      var identityInfo = new IdentityInfo {
        Features =
          IdentityFeatures.Seed |
          IdentityFeatures.Increment |
          IdentityFeatures.AutoIncrement
      };
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

      types.DateTimeOffset = DataTypeInfo.Range(SqlType.DateTimeOffset, common | index,
        new ValueRange<DateTimeOffset>(new DateTimeOffset(1, 1, 1, 0, 0, 0, 0, new TimeSpan(0)),
          new DateTimeOffset(9999, 12, 31, 0, 0, 0, 0, new TimeSpan(0))),
        "datetimeoffset");

      types.DateTime = DataTypeInfo.Range(SqlType.DateTime, common | index,
        new ValueRange<DateTime>(new DateTime(1, 1, 1), new DateTime(9999, 12, 31)),
        "datetime2", "datetime", "smalldatetime");
      types.DateOnly = DataTypeInfo.Range(SqlType.Date, common | index, new ValueRange<DateOnly>(new DateOnly(1, 1, 1), new DateOnly(9999, 12, 31)), "date");
      types.TimeOnly = DataTypeInfo.Range(SqlType.Time, common | index, new ValueRange<TimeOnly>(TimeOnly.MinValue, TimeOnly.MaxValue), "time");

      types.Char = DataTypeInfo.Stream(SqlType.Char, common | index, 4000, "nchar", "char");
      types.VarChar = DataTypeInfo.Stream(SqlType.VarChar, common | index, 4000, "nvarchar", "varchar");
      types.VarCharMax = DataTypeInfo.Regular(SqlType.VarCharMax, common, "nvarchar(max)", "ntext", "varchar(max)", "text", "xml");

      types.Binary = DataTypeInfo.Stream(SqlType.Binary, common | index, 4000, "binary");
      types.VarBinary = DataTypeInfo.Stream(SqlType.VarBinary, common | index, 4000, "varbinary");
      types.VarBinaryMax = DataTypeInfo.Regular(SqlType.VarBinaryMax, common, "varbinary(max)", "image");

      types.Guid = DataTypeInfo.Regular(SqlType.Guid, common | index, "uniqueidentifier");

      var geo = DataTypeFeatures.Default | DataTypeFeatures.Nullable | DataTypeFeatures.Multiple | DataTypeFeatures.Spatial;
      types.Add(CustomSqlType.Geometry, DataTypeInfo.Regular(CustomSqlType.Geometry, geo, "geometry"));
      types.Add(CustomSqlType.Geography, DataTypeInfo.Regular(CustomSqlType.Geography, geo, "geography"));

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
      var domainInfo = new EntityInfo {
        AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop,
        MaxIdentifierLength = MaxIdentifierLength
      };
      return domainInfo;
    }

    public override AssertConstraintInfo GetAssertionInfo()
    {
      var assertConstraintInfo = new AssertConstraintInfo {
        AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop,
        MaxIdentifierLength = MaxIdentifierLength
      };
      return assertConstraintInfo;
    }

    public override int GetStringIndexingBase()
    {
      return 1;
    }


    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}