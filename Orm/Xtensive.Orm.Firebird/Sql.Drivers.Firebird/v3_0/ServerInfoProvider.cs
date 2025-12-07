// Copyright (C) 2011-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Csaba Beer
// Created:    2011.01.10

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.Firebird.v3_0
{
  internal class ServerInfoProvider : Info.ServerInfoProvider
  {
    // It seems that Firebird 2.5 uses max identifier length constraint only for identifiers of database objects but ignores length of column aliases
    // In Firebird 3.0 this behavior changed and the same length limit is applied for column aliases as well,
    // so we decrease limit by length of "#a." string, similar prefixes are common within queries
    private const int Fb30MaxIdentifierLength = 27;
    private const int DoNotKnow = int.MaxValue;
    private const int MaxCharLength = 2000; // physical constraint=32762, but because of http://tracker.firebirdsql.org/browse/CORE-1117;
    // The limit is 64kB for statement text, 64kB for compiled BLR and 48kB for execution plan.
    private const int MaxTextLength = int.MaxValue;

    protected virtual int MaxIdentifierLength => Fb30MaxIdentifierLength;

    public override EntityInfo GetCollationInfo() => null;

    public override EntityInfo GetCharacterSetInfo() => null;

    public override EntityInfo GetTranslationInfo() => null;

    public override EntityInfo GetTriggerInfo()
    {
      var triggerInfo = new EntityInfo();
      triggerInfo.AllowedDdlStatements = DdlStatements.All;
      triggerInfo.MaxIdentifierLength = MaxIdentifierLength;
      return triggerInfo;
    }

    public override EntityInfo GetStoredProcedureInfo()
    {
      var storedProcedureInfo = new EntityInfo {
        AllowedDdlStatements = DdlStatements.All,
        MaxIdentifierLength = MaxIdentifierLength
      };
      return storedProcedureInfo;
    }

    public override SequenceInfo GetSequenceInfo()
    {
      var sequenceInfo = new SequenceInfo {
        AllowedDdlStatements = DdlStatements.All,
        Features = SequenceFeatures.None,
        MaxIdentifierLength = MaxIdentifierLength
      };
      return sequenceInfo;
    }

    public override EntityInfo GetDatabaseInfo()
    {
      var info = new EntityInfo {
        AllowedDdlStatements = DdlStatements.All,
        MaxIdentifierLength = MaxIdentifierLength
      };
      return info;
    }

    public override ColumnInfo GetColumnInfo()
    {
      var columnInfo = new ColumnInfo {
        AllowedDdlStatements = DdlStatements.All,
        Features = ColumnFeatures.Computed,
        MaxIdentifierLength = MaxIdentifierLength
      };
      return columnInfo;
    }

    public override EntityInfo GetViewInfo()
    {
      var viewInfo = new EntityInfo {
        AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Rename,
        MaxIdentifierLength = MaxIdentifierLength
      };
      return viewInfo;
    }

    public override EntityInfo GetSchemaInfo()
    {
      var schemaInfo = new EntityInfo {
        AllowedDdlStatements = DdlStatements.None,
        MaxIdentifierLength = MaxIdentifierLength
      };
      return schemaInfo;
    }

    public override TableInfo GetTableInfo()
    {
      var tableInfo = new TableInfo {
        AllowedDdlStatements = DdlStatements.All & ~DdlStatements.Rename,
        MaxIdentifierLength = MaxIdentifierLength,
        MaxNumberOfColumns = 256, // not a correct value, Depends on data types used. (Example: 16,384 INTEGER (4-byte) values per row.)
        PartitionMethods = PartitionMethods.None
      };
      return tableInfo;
    }

    public override TemporaryTableInfo GetTemporaryTableInfo() => null;

    public override CheckConstraintInfo GetCheckConstraintInfo()
    {
      var checkContraintsInfo = new CheckConstraintInfo {
        AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop,
        MaxIdentifierLength = MaxIdentifierLength,
        MaxExpressionLength = MaxCharLength,
        Features = CheckConstraintFeatures.None
      };
      return checkContraintsInfo;
    }

    public override PrimaryKeyConstraintInfo GetPrimaryKeyInfo()
    {
      var primaryKeyInfo = new PrimaryKeyConstraintInfo {
        AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop,
        MaxIdentifierLength = MaxIdentifierLength,
        Features = PrimaryKeyConstraintFeatures.None
      };
      return primaryKeyInfo;
    }

    public override UniqueConstraintInfo GetUniqueConstraintInfo()
    {
      var uniqueContraintInfo = new UniqueConstraintInfo {
        AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop,
        MaxIdentifierLength = MaxIdentifierLength,
        Features = UniqueConstraintFeatures.None
      };
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
      var foreignKeyConstraintInfo = new ForeignKeyConstraintInfo {
        MaxIdentifierLength = MaxIdentifierLength,
        AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop,
        Features = ForeignKeyConstraintFeatures.None,
        Actions =
          ForeignKeyConstraintActions.Cascade |
          ForeignKeyConstraintActions.NoAction |
          ForeignKeyConstraintActions.SetDefault |
          ForeignKeyConstraintActions.SetNull
      };
      return foreignKeyConstraintInfo;
    }

    public override FullTextSearchInfo GetFullTextInfo() => null;

    public override QueryInfo GetQueryInfo()
    {
      var queryInfo = new QueryInfo {
        MaxNestedSubqueriesAmount = 100,
        ParameterPrefix = "@",
        MaxLength = MaxCharLength,
        MaxComparisonOperations = DoNotKnow,
        MaxQueryParameterCount = DoNotKnow,
        Features =
          QueryFeatures.NamedParameters |
          QueryFeatures.ParameterPrefix |
          QueryFeatures.ScalarSubquery |
          QueryFeatures.Paging |
          QueryFeatures.Limit |
          QueryFeatures.Offset |
          QueryFeatures.UpdateLimit |
          QueryFeatures.DeleteLimit
      };
      return queryInfo;
    }

    public override ServerFeatures GetServerFeatures() => ServerFeatures.Savepoints;

    public override IdentityInfo GetIdentityInfo() => null;

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

      dtc.DateOnly = DataTypeInfo.Range(SqlType.Date, commonFeatures, ValueRange.DateOnly, "date");
      dtc.TimeOnly = DataTypeInfo.Range(SqlType.Time, commonFeatures, ValueRange.TimeOnly, "time");

      dtc.Char = DataTypeInfo.Stream(SqlType.Char, commonFeatures, MaxCharLength, "char");
      dtc.VarChar = DataTypeInfo.Stream(SqlType.VarChar, commonFeatures, MaxCharLength, "varchar");
      dtc.VarCharMax = DataTypeInfo.Stream(SqlType.VarCharMax, lobFeatures, MaxTextLength, "blob sub_type 1");
      dtc.VarBinaryMax = DataTypeInfo.Stream(SqlType.VarBinaryMax, lobFeatures, MaxTextLength, "blob sub_type 0");

      return dtc;
    }

    public override IsolationLevels GetIsolationLevels()
      => IsolationLevels.ReadCommitted | IsolationLevels.ReadUncommitted | IsolationLevels.RepeatableRead;

    public override EntityInfo GetDomainInfo()
    {
      var info = new EntityInfo {
        AllowedDdlStatements = DdlStatements.All,
        MaxIdentifierLength = MaxIdentifierLength
      };
      return info;
    }

    public override AssertConstraintInfo GetAssertionInfo() => null;

    public override int GetStringIndexingBase() => 1;

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
