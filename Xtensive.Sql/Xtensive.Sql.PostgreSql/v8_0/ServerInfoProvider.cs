using System;
using Npgsql;
using Xtensive.Core;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.PostgreSql.v8_0
{
  internal class ServerInfoProvider : Info.ServerInfoProvider
  {
    private const int maxTextLength = (int.MaxValue >> 1) - 1000;
    private const int maxCharLength = 10485760;

    private readonly VersionInfo mVersionInfo;
    private readonly ServerConfiguration serverConfig;

    protected virtual IndexFeatures GetIndexFeatures()
    {
      return IndexFeatures.Clustered | IndexFeatures.Unique;
    }

    protected virtual int GetMaxTextLength()
    {
      return maxTextLength;
    }

    protected virtual int GetMaxCharLength()
    {
      return maxCharLength;
    }
    
    public virtual ServerConfiguration GetServerConfig()
    {
      return serverConfig;
    }

    public virtual short GetMaxDateTimePrecision()
    {
      return 6;
    }

    public override VersionInfo GetVersionInfo()
    {
      return mVersionInfo;
    }

    public override EntityInfo GetDatabaseInfo()
    {
      var info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      return info;
    }

    public override EntityInfo GetSchemaInfo()
    {
      var info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      return info;
    }

    public override TableInfo GetTableInfo()
    {
      var info = new TableInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      info.PartitionMethods = PartitionMethods.None;
      return info;
    }

    public override TemporaryTableInfo GetTemporaryTableInfo()
    {
      var info = new TemporaryTableInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.Features = TemporaryTableFeatures.Local
        | TemporaryTableFeatures.DeleteRowsOnCommit
        | TemporaryTableFeatures.PreserveRowsOnCommit;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      return info;
    }

    public override IndexInfo GetIndexInfo()
    {
      var info = new IndexInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.Features = GetIndexFeatures();
      info.MaxNumberOfColumns = serverConfig.MaxIndexKeys;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      // Pg 8.2: 8191 byte
      info.MaxLength = 2000;
      return info;
    }

    public override ColumnInfo GetColumnInfo()
    {
      var info = new ColumnInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.Features = ColumnFeatures.None;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      return info;
    }

    public override CheckConstraintInfo GetCheckConstraintInfo()
    {
      var info = new CheckConstraintInfo();
      info.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      info.Features = CheckConstraintFeatures.None;
      // TODO: more exactly
      info.MaxExpressionLength = GetMaxTextLength(); 
      return info;
    }

    public override PrimaryKeyConstraintInfo GetPrimaryKeyInfo()
    {
      var info = new PrimaryKeyConstraintInfo();
      info.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      info.Features = PrimaryKeyConstraintFeatures.Clustered;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      return info;
    }

    public override UniqueConstraintInfo GetUniqueConstraintInfo()
    {
      var info = new UniqueConstraintInfo();
      info.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      info.Features = UniqueConstraintFeatures.Nullable | UniqueConstraintFeatures.Clustered;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      return info;
    }

    public override ForeignKeyConstraintInfo GetForeignKeyConstraintInfo()
    {
      var info = new ForeignKeyConstraintInfo();
      info.Actions =
        ForeignKeyConstraintActions.Cascade |
        ForeignKeyConstraintActions.NoAction |
        ForeignKeyConstraintActions.Restrict |
        ForeignKeyConstraintActions.SetDefault |
        ForeignKeyConstraintActions.SetNull;
      info.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      info.Features = ForeignKeyConstraintFeatures.Deferrable;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      return info;
    }

    public override EntityInfo GetViewInfo()
    {
      var info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      return info;
    }


    public override DataTypeCollection GetDataTypesInfo()
    {
      var commonFeatures =
        DataTypeFeatures.Clustering |
        DataTypeFeatures.Grouping |
        DataTypeFeatures.Indexing |
        DataTypeFeatures.KeyConstraint |
        DataTypeFeatures.Nullable |
        DataTypeFeatures.Ordering |
        DataTypeFeatures.Multiple |
        DataTypeFeatures.Default;

      var dtc = new DataTypeCollection();

      dtc.Boolean = DataTypeInfo.Range(SqlType.Boolean, commonFeatures,
        StandardValueRange.Bool, "boolean", "bool");

      dtc.Int16 = DataTypeInfo.Range(SqlType.Int16, commonFeatures,
        StandardValueRange.Int16,
        "smallint", "int2");
      
      dtc.Int32 = DataTypeInfo.Range(SqlType.Int32, commonFeatures,
        StandardValueRange.Int32, "integer", "int4");

      dtc.Int64 = DataTypeInfo.Range(SqlType.Int64, commonFeatures,
        StandardValueRange.Int64, "bigint", "int8");

      dtc.Decimal = DataTypeInfo.Fractional(SqlType.Decimal, commonFeatures,
        StandardValueRange.Decimal, 1000, "numeric", "decimal");
      
      dtc.Float = DataTypeInfo.Range(SqlType.Float, commonFeatures,
        StandardValueRange.Float, "real", "float4");
      
      dtc.Double = DataTypeInfo.Range(SqlType.Double, commonFeatures,
        StandardValueRange.Double, "double precision", "float8");

      dtc.DateTime = DataTypeInfo.Range(SqlType.DateTime, commonFeatures,
        StandardValueRange.DateTime, "timestamp");

      dtc.Interval = DataTypeInfo.Range(SqlType.Interval, commonFeatures,
        StandardValueRange.TimeSpan, "interval");
      
      dtc.Char = DataTypeInfo.Stream(SqlType.Char, commonFeatures, maxCharLength, "character", "char", "bpchar");
      dtc.VarChar = DataTypeInfo.Stream(SqlType.VarChar, commonFeatures, maxCharLength, "character varying", "varchar");
      dtc.VarCharMax = DataTypeInfo.Regular(SqlType.VarCharMax, commonFeatures, "text");
      dtc.VarBinaryMax = DataTypeInfo.Stream(SqlType.VarBinaryMax, commonFeatures, maxTextLength, "bytea");
      
      return dtc;
    }


    public override EntityInfo GetDomainInfo()
    {
      var info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      return info;
    }

    public override SequenceInfo GetSequenceInfo()
    {
      var info = new SequenceInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.Features = SequenceFeatures.Cache;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      return info;
    }

    public override EntityInfo GetStoredProcedureInfo()
    {
      EntityInfo info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      return info;
    }

    public override EntityInfo GetTriggerInfo()
    {
      EntityInfo info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = serverConfig.MaxIdentifierLength;
      return info;
    }


    public override IsolationLevels GetIsolationLevels()
    {
      return IsolationLevels.ReadCommitted | IsolationLevels.Serializable;
    }

    public override QueryInfo GetQueryInfo()
    {
      var info = new QueryInfo();
      info.Features =
        QueryFeatures.Batches |
        QueryFeatures.NamedParameters |
        QueryFeatures.ParameterPrefix |
        QueryFeatures.FullBooleanExpressionSupport |
        QueryFeatures.Paging;
      info.ParameterPrefix = "@";
      info.MaxComparisonOperations = 1000000;
      info.MaxLength = 1000000;
      info.MaxNestedSubqueriesAmount = 100;
      return info;
    }
    
    public override IdentityInfo GetIdentityInfo()
    {
      return null;
    }

    public override AssertConstraintInfo GetAssertionInfo()
    {
      return null;
    }

    public override EntityInfo GetCharacterSetInfo()
    {
      return null;
    }

    public override EntityInfo GetCollationInfo()
    {
      return null;
    }

    public override EntityInfo GetTranslationInfo()
    {
      return null;
    }

    public override int GetStringIndexingBase()
    {
      return 1;
    }

    // Constructors

    public ServerInfoProvider(NpgsqlConnection connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "conn");
#if OLD_NPGSQL
      var sv = connection.PostgreSqlVersion;
      mVersionInfo = new VersionInfo(new Version(sv.Major, sv.Minor, /*sv.Patch*/0, 0));
#else
      mVersionInfo = new VersionInfo(connection.PostgreSqlVersion);
#endif
      serverConfig = new ServerConfiguration(connection);
    }
  }
}