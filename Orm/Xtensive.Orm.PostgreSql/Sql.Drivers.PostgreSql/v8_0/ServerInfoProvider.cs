// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal class ServerInfoProvider : Info.ServerInfoProvider
  {
    // These two options are actually compile-time configurable.
    private const int MaxIdentifierLength = 63;
    private const int MaxIndexKeys = 32;

    private const int MaxTextLength = (int.MaxValue >> 1) - 1000;
    private const int MaxCharLength = 10485760;

    private readonly string defaultSchemaName;
    private readonly string databaseName;

    protected virtual IndexFeatures GetIndexFeatures()
    {
      return IndexFeatures.Unique | IndexFeatures.Filtered | IndexFeatures.Expressions;
    }

    protected virtual int GetMaxTextLength()
    {
      return MaxTextLength;
    }

    protected virtual int GetMaxCharLength()
    {
      return MaxCharLength;
    }
    
    public virtual short GetMaxDateTimePrecision()
    {
      return 6;
    }


    public override EntityInfo GetDatabaseInfo()
    {
      var info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = MaxIdentifierLength;
      return info;
    }

    public override EntityInfo GetSchemaInfo()
    {
      var info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = MaxIdentifierLength;
      return info;
    }

    public override TableInfo GetTableInfo()
    {
      var info = new TableInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = MaxIdentifierLength;
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
      info.MaxIdentifierLength = MaxIdentifierLength;
      return info;
    }

    public override IndexInfo GetIndexInfo()
    {
      var info = new IndexInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.Features = GetIndexFeatures();
      info.MaxNumberOfColumns = MaxIndexKeys;
      info.MaxIdentifierLength = MaxIdentifierLength;
      // Pg 8.2: 8191 byte
      info.MaxLength = 2000;
      return info;
    }

    public override ColumnInfo GetColumnInfo()
    {
      var info = new ColumnInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.Features = ColumnFeatures.None;
      info.MaxIdentifierLength = MaxIdentifierLength;
      return info;
    }

    public override CheckConstraintInfo GetCheckConstraintInfo()
    {
      var info = new CheckConstraintInfo();
      info.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      info.MaxIdentifierLength = MaxIdentifierLength;
      info.Features = CheckConstraintFeatures.None;
      // TODO: more exactly
      info.MaxExpressionLength = GetMaxTextLength(); 
      return info;
    }

    public override PrimaryKeyConstraintInfo GetPrimaryKeyInfo()
    {
      var info = new PrimaryKeyConstraintInfo();
      info.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      info.MaxIdentifierLength = MaxIdentifierLength;
      return info;
    }

    public override FullTextSearchInfo GetFullTextInfo()
    {
      return null;
    }

    public override UniqueConstraintInfo GetUniqueConstraintInfo()
    {
      var info = new UniqueConstraintInfo();
      info.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      info.Features = UniqueConstraintFeatures.Nullable;
      info.MaxIdentifierLength = MaxIdentifierLength;
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
      info.MaxIdentifierLength = MaxIdentifierLength;
      return info;
    }

    public override EntityInfo GetViewInfo()
    {
      var info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = MaxIdentifierLength;
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
        ValueRange.Bool, "boolean", "bool");

      dtc.Int16 = DataTypeInfo.Range(SqlType.Int16, commonFeatures,
        ValueRange.Int16,
        "smallint", "int2");
      
      dtc.Int32 = DataTypeInfo.Range(SqlType.Int32, commonFeatures,
        ValueRange.Int32, "integer", "int4");

      dtc.Int64 = DataTypeInfo.Range(SqlType.Int64, commonFeatures,
        ValueRange.Int64, "bigint", "int8");

      dtc.Decimal = DataTypeInfo.Fractional(SqlType.Decimal, commonFeatures,
        ValueRange.Decimal, 1000, "numeric", "decimal");
      
      dtc.Float = DataTypeInfo.Range(SqlType.Float, commonFeatures,
        ValueRange.Float, "real", "float4");
      
      dtc.Double = DataTypeInfo.Range(SqlType.Double, commonFeatures,
        ValueRange.Double, "double precision", "float8");

      dtc.DateTime = DataTypeInfo.Range(SqlType.DateTime, commonFeatures,
        ValueRange.DateTime, "timestamp");

      dtc.Interval = DataTypeInfo.Range(SqlType.Interval, commonFeatures,
        ValueRange.TimeSpan, "interval");
      
      dtc.Char = DataTypeInfo.Stream(SqlType.Char, commonFeatures, MaxCharLength, "character", "char", "bpchar");
      dtc.VarChar = DataTypeInfo.Stream(SqlType.VarChar, commonFeatures, MaxCharLength, "character varying", "varchar");
      dtc.VarCharMax = DataTypeInfo.Regular(SqlType.VarCharMax, commonFeatures, "text");
      dtc.VarBinaryMax = DataTypeInfo.Stream(SqlType.VarBinaryMax, commonFeatures, MaxTextLength, "bytea");
      dtc.DateTimeOffset = DataTypeInfo.Range(SqlType.DateTimeOffset, commonFeatures,
        new ValueRange<DateTimeOffset>(new DateTimeOffset(1, 1, 1, 0, 0, 0, 0, new TimeSpan(0)),
          new DateTimeOffset(9999, 12, 31, 0, 0, 0, 0, new TimeSpan(0))),
        "timestamp with time zone");
      
      var geo = DataTypeFeatures.Default | DataTypeFeatures.Nullable | DataTypeFeatures.Multiple | DataTypeFeatures.Spatial;

      dtc.Add(CustomSqlType.Point, DataTypeInfo.Regular(CustomSqlType.Point, geo, "point"));
      dtc.Add(CustomSqlType.LSeg, DataTypeInfo.Regular(CustomSqlType.LSeg, geo, "lseg"));
      dtc.Add(CustomSqlType.Box, DataTypeInfo.Regular(CustomSqlType.Box, geo, "box"));
      dtc.Add(CustomSqlType.Path, DataTypeInfo.Regular(CustomSqlType.Path, geo, "path"));
      dtc.Add(CustomSqlType.Polygon, DataTypeInfo.Regular(CustomSqlType.Polygon, geo, "polygon"));
      dtc.Add(CustomSqlType.Circle, DataTypeInfo.Regular(CustomSqlType.Circle, geo, "circle"));

      return dtc;
    }


    public override EntityInfo GetDomainInfo()
    {
      var info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = MaxIdentifierLength;
      return info;
    }

    public override SequenceInfo GetSequenceInfo()
    {
      var info = new SequenceInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.Features = SequenceFeatures.Cache;
      info.MaxIdentifierLength = MaxIdentifierLength;
      return info;
    }

    public override EntityInfo GetStoredProcedureInfo()
    {
      EntityInfo info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = MaxIdentifierLength;
      return info;
    }

    public override EntityInfo GetTriggerInfo()
    {
      EntityInfo info = new EntityInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.MaxIdentifierLength = MaxIdentifierLength;
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
        QueryFeatures.UpdateFrom | 
        QueryFeatures.Limit |
        QueryFeatures.Offset |
        QueryFeatures.MulticolumnIn |
        QueryFeatures.InsertDefaultValues |
        QueryFeatures.UpdateDefaultValues |
        QueryFeatures.MultischemaQueries |
        QueryFeatures.ScalarSubquery;
      info.ParameterPrefix = "@";
      info.MaxComparisonOperations = 1000000;
      info.MaxLength = 1000000;
      info.MaxNestedSubqueriesAmount = 100;
      return info;
    }

    public override ServerFeatures GetServerFeatures()
    {
      return ServerFeatures.Savepoints | ServerFeatures.TransactionalDdl | ServerFeatures.TransactionalFullTextDdl;
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

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}