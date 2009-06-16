using System;
using Npgsql;
using Xtensive.Core;

namespace Xtensive.Sql.Common.PgSql.v8_0
{
  public class PgSqlServerInfoProvider : IPgSqlServerInfoProvider
  {
    public PgSqlServerInfoProvider(Connection conn)
    {
      ArgumentValidator.EnsureArgumentNotNull(conn, "conn");
      NpgsqlConnection nconn = conn.RealConnection as NpgsqlConnection;
      ArgumentValidator.EnsureArgumentNotNull(conn, "conn");
      ServerVersion sv = nconn.PostgreSqlVersion;
      mVersionInfo = new VersionInfo(new Version(sv.Major, sv.Minor, sv.Patch, 0));

      ServerConfig = new ServerConfiguration(nconn);
    }

    #region ServerConfig

    private ServerConfiguration mServerConfig;

    public ServerConfiguration ServerConfig
    {
      get { return mServerConfig; }
      private set { mServerConfig = value; }
    }

    #endregion

    public virtual short MaxDateTimePrecision
    {
      get { return 6; }
    }

    #region IServerInfoProvider Members

    public virtual VersionInfo VersionInfo
    {
      get { return mVersionInfo; }
    }

    public virtual EntityInfo DatabaseInfo
    {
      get
      {
        EntityInfo info = new EntityInfo();
        info.AllowedDdlStatements = DdlStatements.All;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        return info;
      }
    }

    public virtual ServerEntities Entities
    {
      get
      {
        return ServerEntities.Constraints
          | ServerEntities.Domains
            | ServerEntities.Indexes
              | ServerEntities.Schemas
                | ServerEntities.Sequences
                  | ServerEntities.Triggers
                    | ServerEntities.UserDefinedFunctions
                      | ServerEntities.UserDefinedTypes;
      }
    }

    public virtual EntityInfo SchemaInfo
    {
      get
      {
        EntityInfo info = new EntityInfo();
        info.AllowedDdlStatements = DdlStatements.All;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        return info;
      }
    }

    public virtual TableInfo TableInfo
    {
      get
      {
        TableInfo info = new TableInfo();
        info.AllowedDdlStatements = DdlStatements.All;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        info.PartitionMethods = PartitionMethods.None;
        return info;
      }
    }

    public virtual TemporaryTableInfo TemporaryTableInfo
    {
      get
      {
        TemporaryTableInfo info = new TemporaryTableInfo();
        info.AllowedDdlStatements = DdlStatements.All;
        info.Features = TemporaryTableFeatures.Local | TemporaryTableFeatures.DeleteRowsOnCommit | TemporaryTableFeatures.PreserveRowsOnCommit;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        return info;
      }
    }

    public virtual IndexInfo IndexInfo
    {
      get
      {
        IndexInfo info = new IndexInfo();
        info.AllowedDdlStatements = DdlStatements.All;
        info.Features = IndexFeatures;
        info.MaxColumnAmount = ServerConfig.MaxIndexKeys;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        //Pg 8.2: 8191 byte
        info.MaxLength = 2000;
        return info;
      }
    }

    public virtual ColumnInfo ColumnInfo
    {
      get
      {
        ColumnInfo info = new ColumnInfo();
        info.AllowedDdlStatements = DdlStatements.All;
        info.Features = ColumnFeatures.None;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        return info;
      }
    }

    public virtual CheckConstraintInfo CheckConstraintInfo
    {
      get
      {
        CheckConstraintInfo info = new CheckConstraintInfo();
        info.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
        info.Features = ConstraintFeatures.None;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        //TODO: more exactly
        info.MaxExpressionLength = MaxTextLength;
        return info;
      }
    }

    public virtual ConstraintInfo PrimaryKeyInfo
    {
      get
      {
        ConstraintInfo info = new ConstraintInfo();
        info.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
        info.Features = ConstraintFeatures.Clustered;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        return info;
      }
    }

    public virtual ConstraintInfo UniqueConstraintInfo
    {
      get
      {
        ConstraintInfo info = new ConstraintInfo();
        info.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
        info.Features = ConstraintFeatures.Nullable | ConstraintFeatures.Clustered;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        return info;
      }
    }

    public virtual ReferenceConstraintInfo ReferentialConstraintInfo
    {
      get
      {
        ReferenceConstraintInfo info = new ReferenceConstraintInfo();
        info.Actions = ConstraintActions.Cascade | ConstraintActions.NoAction | ConstraintActions.Restrict | ConstraintActions.SetDefault | ConstraintActions.SetNull;
        info.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
        info.Features = ConstraintFeatures.Deferrable | ConstraintFeatures.Nullable | ConstraintFeatures.Clustered;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        return info;
      }
    }

    public virtual EntityInfo ViewInfo
    {
      get
      {
        EntityInfo info = new EntityInfo();
        info.AllowedDdlStatements = DdlStatements.All;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        return info;
      }
    }


    public virtual DataTypeCollection DataTypesInfo
    {
      get
      {
        DataTypeFeatures commonFeatures = DataTypeFeatures.Clustering
          | DataTypeFeatures.Grouping | DataTypeFeatures.Indexing
            | DataTypeFeatures.KeyConstraint | DataTypeFeatures.Nullable
              | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple
                | DataTypeFeatures.Default;

        DataTypeCollection dtc = new DataTypeCollection();

        dtc.Char = new StreamDataTypeInfo(SqlDataType.Char, typeof (string), new[] {"character", "char", "bpchar"});
        dtc.Char.Features = commonFeatures;
        dtc.Char.Length = new ValueRange<int>(0, mMaxCharLength, 1);

        dtc.VarChar = new StreamDataTypeInfo(SqlDataType.VarChar, typeof (string), new[] {"character varying", "varchar"});
        dtc.VarChar.Features = commonFeatures;
        dtc.VarChar.Length = new ValueRange<int>(0, mMaxCharLength);

//        dtc.Text = new StreamDataTypeInfo(SqlDataType.Text, typeof (string), new[] {"text"});
//        dtc.Text.Features = commonFeatures;
//        dtc.Text.Length = new ValueRange<int>(0, mMaxTextLength);

        dtc.VarCharMax = new StreamDataTypeInfo(SqlDataType.VarCharMax, typeof (string), new[] {"text"});
        dtc.VarCharMax.Features = commonFeatures;
        dtc.VarCharMax.Length = new ValueRange<int>(0, mMaxTextLength);

        dtc.Boolean = new RangeDataTypeInfo<bool>(SqlDataType.Boolean, new[] {"boolean", "bool"});
        dtc.Boolean.Value = new ValueRange<bool>(false, true);
        dtc.Boolean.Features = commonFeatures;

        dtc.Int16 = new IntegerDataTypeInfo<short>(SqlDataType.Int16, new[] {"smallint", "int2"});
        dtc.Int16.Value = new ValueRange<short>(Int16.MinValue, Int16.MaxValue);
        dtc.Int16.Features = commonFeatures;

        dtc.Int32 = new IntegerDataTypeInfo<int>(SqlDataType.Int32, new[] {"integer", "int4"});
        dtc.Int32.Value = new ValueRange<int>(Int32.MinValue, Int32.MaxValue);
        dtc.Int32.Features = commonFeatures;

        dtc.Int64 = new IntegerDataTypeInfo<long>(SqlDataType.Int64, new[] {"bigint", "int8"});
        dtc.Int64.Value = new ValueRange<long>(Int64.MinValue, Int64.MaxValue);
        dtc.Int64.Features = commonFeatures;

        dtc.Decimal = new FractionalDataTypeInfo<decimal>(SqlDataType.Decimal, new[] {"numeric", "decimal"});
        dtc.Decimal.Value = new ValueRange<decimal>(Decimal.MinValue, Decimal.MaxValue);
        dtc.Decimal.Precision = new ValueRange<short>(1, 1000);
        dtc.Decimal.Scale = new ValueRange<short>(0, 999, 0);
        dtc.Decimal.Features = commonFeatures;

        dtc.Float = new FractionalDataTypeInfo<float>(SqlDataType.Float, new[] {"real", "float4"});
        dtc.Float.Value = new ValueRange<float>(Single.MinValue, Single.MaxValue);
        dtc.Float.Precision = new ValueRange<short>(1, 24, 24);
        dtc.Float.Features = commonFeatures;

        dtc.Double = new FractionalDataTypeInfo<double>(SqlDataType.Double, new[] {"double precision", "float8"});
        dtc.Double.Value = new ValueRange<double>(Double.MinValue, Double.MaxValue);
        dtc.Double.Precision = new ValueRange<short>(25, 53, 53);
        dtc.Double.Features = commonFeatures;

        dtc.DateTime = new RangeDataTypeInfo<DateTime>(SqlDataType.DateTime, new[] {"timestamp"});
        dtc.DateTime.Value = new ValueRange<DateTime>(DateTime.MinValue, DateTime.MaxValue);
        dtc.DateTime.Features = commonFeatures;

        dtc.Interval = new RangeDataTypeInfo<TimeSpan>(SqlDataType.Interval, new[] {"interval"});
        //PostgreSQL's range is bigger (-2^32 days <= value < 2^32 days)
        dtc.Interval.Value = new ValueRange<TimeSpan>(TimeSpan.MinValue, TimeSpan.MaxValue, TimeSpan.FromTicks(0));
        dtc.Interval.Features = commonFeatures;

        dtc.VarBinary = new StreamDataTypeInfo(SqlDataType.VarBinary, typeof (byte[]), new string[0]);
        dtc.VarBinary.Length = new ValueRange<int>(0, mMaxTextLength, 0);
        dtc.VarBinary.Features = commonFeatures;
        
        dtc.VarBinaryMax = new StreamDataTypeInfo(SqlDataType.VarBinaryMax, typeof (byte[]), new[] {"bytea"});
        dtc.VarBinaryMax.Length = new ValueRange<int>(0, mMaxTextLength, 0);
        dtc.VarBinaryMax.Features = commonFeatures;

        return dtc;
      }
    }


    public virtual EntityInfo DomainInfo
    {
      get
      {
        EntityInfo info = new EntityInfo();
        info.AllowedDdlStatements = DdlStatements.All;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        return info;
      }
    }

    public virtual SequenceInfo SequenceInfo
    {
      get
      {
        SequenceInfo info = new SequenceInfo();
        info.AllowedDdlStatements = DdlStatements.All;
        info.Features = SequenceFeatures.Cache;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        return info;
      }
    }

    public virtual EntityInfo StoredProcedureInfo
    {
      get
      {
        EntityInfo info = new EntityInfo();
        info.AllowedDdlStatements = DdlStatements.All;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        return info;
      }
    }

    public virtual EntityInfo TriggerInfo
    {
      get
      {
        EntityInfo info = new EntityInfo();
        info.AllowedDdlStatements = DdlStatements.All;
        info.MaxIdentifierLength = ServerConfig.MaxIdentifierLength;
        return info;
      }
    }


    public virtual IsolationLevels IsolationLevels
    {
      get { return IsolationLevels.ReadCommitted | IsolationLevels.Serializable; }
    }

    public virtual QueryInfo QueryInfo
    {
      get
      {
        QueryInfo info = new QueryInfo();
        info.Features = QueryFeatures.Batches
          | QueryFeatures.NamedParameters
            | QueryFeatures.UseParameterPrefix;
        info.ParameterPrefix = "@";
        info.MaxComparisonOperations = 1000000;
        info.MaxLength = 1000000;
        info.MaxNestedSubqueriesAmount = 100;
        info.QuoteToken = "'";
        return info;
      }
    }


    public virtual IdentityInfo IdentityInfo
    {
      get { return null; }
    }

    public virtual ConstraintInfo AssertionInfo
    {
      get { return null; }
    }

    public virtual EntityInfo CharacterSetInfo
    {
      get { return null; }
    }

    public virtual EntityInfo CollationInfo
    {
      get { return null; }
    }

    public virtual EntityInfo TranslationInfo
    {
      get { return null; }
    }

    public virtual int StringIndexingBase
    {
      get { return 1; }
    }

    #endregion

    protected virtual IndexFeatures IndexFeatures
    {
      get { return IndexFeatures.Clustered | IndexFeatures.Unique; }
    }

    #region MaxTextLength property

    private int mMaxTextLength = Int32.MaxValue >> 1 - 1000;

    protected virtual int MaxTextLength
    {
      get { return mMaxTextLength; }
    }

    #endregion

    #region MaxCharLength property

    private int mMaxCharLength = 10485760;

    protected virtual int MaxCharLength
    {
      get { return mMaxCharLength; }
    }

    #endregion

    private VersionInfo mVersionInfo;
  }
}