// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Globalization;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.VistaDb.v3
{
  public class ServerInfoProvider : Info.ServerInfoProvider
  {
    private const int cDefaultIdentifierLength = 128;

    public override ColumnInfo GetColumnInfo()
    {
      ColumnInfo columnInfo = new ColumnInfo();
      columnInfo.MaxIdentifierLength = cDefaultIdentifierLength;
      columnInfo.Features = ColumnFeatures.Identity;
      columnInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      return columnInfo;
    }

    public override CheckConstraintInfo GetCheckConstraintInfo()
    {
      CheckConstraintInfo checkConstraintInfo = new CheckConstraintInfo();
      checkConstraintInfo.MaxIdentifierLength = cDefaultIdentifierLength;
      checkConstraintInfo.MaxExpressionLength = 128;
      checkConstraintInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      return checkConstraintInfo;
    }

    /// <summary>
    /// Gets the primary key info.
    /// </summary>
    /// <returns></returns>
    public override ConstraintInfo GetPrimaryKeyInfo()
    {
      return new EntityInfo() as ConstraintInfo;
    }

    public override EntityInfo GetDatabaseInfo()
    {
      EntityInfo databaseInfo = new EntityInfo();
      databaseInfo.MaxIdentifierLength = cDefaultIdentifierLength;
      databaseInfo.AllowedDdlStatements = DdlStatements.Create;
      return databaseInfo;
    }

    public override IdentityInfo GetIdentityInfo()
    {
      IdentityInfo identityInfo = new IdentityInfo();
      identityInfo.Features = IdentityFeatures.StartValue | IdentityFeatures.Increment;
      return identityInfo;
    }

    public override IndexInfo GetIndexInfo()
    {
      IndexInfo indexInfo = new IndexInfo();
      indexInfo.MaxIdentifierLength = cDefaultIdentifierLength;
      indexInfo.MaxColumnAmount = 256;
      indexInfo.MaxLength = 256;
      indexInfo.Features = IndexFeatures.Clustered | IndexFeatures.Unique | IndexFeatures.SortOrder;
      indexInfo.PartitionMethods = PartitionMethods.None;
      indexInfo.AllowedDdlStatements = DdlStatements.All;
      return indexInfo;
    }

    /// <summary>
    /// Gets the referential constraint info.
    /// </summary>
    /// <returns></returns>
    public override ReferenceConstraintInfo GetReferentialConstraintInfo()
    {
      return new EntityInfo() as ReferenceConstraintInfo;
    }

//    public ConstraintInfo GetPrimaryKeyConstraintInfo()
//    {
//      ConstraintInfo primaryKeyConstraintInfo = new ConstraintInfo();
//      primaryKeyConstraintInfo.MaxIdentifierLength = cDefaultIdentifierLength;
//      primaryKeyConstraintInfo.Features = ConstraintFeatures.Clustered;
//      primaryKeyConstraintInfo.AllowedDdlStatements = DdlStatements.Create|DdlStatements.Drop;
//      return primaryKeyConstraintInfo;
//    }

    public override QueryInfo GetQueryInfo()
    {
      QueryInfo queryInfo = new QueryInfo();
      //obj.MaxLength = 60000 * 4000;
      //obj.MaxComparisonOperations = 1000;
      //obj.MaxNestedQueries = 32;
      queryInfo.ParameterPrefix = "@";
      queryInfo.QuoteToken = "'";
      queryInfo.Features = QueryFeatures.NamedParameters | QueryFeatures.UseParameterPrefix | QueryFeatures.SquareBrackets |
        QueryFeatures.Batches;
      return queryInfo;
    }

//    public ReferenceConstraintInfo GetReferenceConstraintInfo()
//    {
//      ReferenceConstraintInfo referenceConstraintInfo = new ReferenceConstraintInfo();
//      referenceConstraintInfo.MaxIdentifierLength = cDefaultIdentifierLength;
//      referenceConstraintInfo.Actions = ConstraintActions.NoAction|ConstraintActions.Cascade|
//                                        ConstraintActions.SetDefault|
//                                        ConstraintActions.SetNull;
//      referenceConstraintInfo.AllowedDdlStatements = DdlStatements.Create|DdlStatements.Drop;
//      return referenceConstraintInfo;
//    }

    public override TableInfo GetTableInfo()
    {
      TableInfo tableInfo = new TableInfo();
      tableInfo.MaxIdentifierLength = cDefaultIdentifierLength;
      tableInfo.PartitionMethods = PartitionMethods.None;
      tableInfo.AllowedDdlStatements = DdlStatements.All;
      return tableInfo;
    }

    public override TemporaryTableInfo GetTemporaryTableInfo()
    {
      return new EntityInfo() as TemporaryTableInfo;
    }

    public override EntityInfo GetCollationInfo()
    {
      return new EntityInfo() as EntityInfo;
    }

    public override EntityInfo GetCharacterSetInfo()
    {
      return new EntityInfo() as EntityInfo;
    }

    public override EntityInfo GetTranslationInfo()
    {
      return new EntityInfo() as EntityInfo;
    }

    public override EntityInfo GetTriggerInfo()
    {
      EntityInfo triggerInfo = new EntityInfo();
      triggerInfo.MaxIdentifierLength = cDefaultIdentifierLength;
      triggerInfo.AllowedDdlStatements = DdlStatements.All;
      return triggerInfo;
    }

    public override EntityInfo GetStoredProcedureInfo()
    {
      return new EntityInfo() as EntityInfo;
    }

    public override SequenceInfo GetSequenceInfo()
    {
      return new EntityInfo() as SequenceInfo;
    }

    public override ConstraintInfo GetUniqueConstraintInfo()
    {
      ConstraintInfo uniqueConstraintInfo = new ConstraintInfo();
      uniqueConstraintInfo.MaxIdentifierLength = cDefaultIdentifierLength;
      uniqueConstraintInfo.Features = ConstraintFeatures.Clustered | ConstraintFeatures.Nullable;
      uniqueConstraintInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      return uniqueConstraintInfo;
    }

    public override EntityInfo GetViewInfo()
    {
      EntityInfo viewInfo = new EntityInfo();
      viewInfo.MaxIdentifierLength = cDefaultIdentifierLength;
      viewInfo.AllowedDdlStatements = DdlStatements.All;
      return viewInfo;
    }

    /// <summary>
    /// Gets the schema info.
    /// </summary>
    /// <returns></returns>
    public override EntityInfo GetSchemaInfo()
    {
      return new EntityInfo() as EntityInfo;
    }

    public override DataTypeCollection GetDataTypesInfo()
    {
      DataTypeCollection types = new DataTypeCollection();

      DataTypeFeatures common = DataTypeFeatures.Default | DataTypeFeatures.Nullable |
        DataTypeFeatures.Grouping | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple;

      DataTypeFeatures index = DataTypeFeatures.Indexing | DataTypeFeatures.Clustering |
        DataTypeFeatures.KeyConstraint;

      DataTypeFeatures identity = DataTypeFeatures.Identity;

      types.Boolean = DataTypeInfo.Range(SqlType.Boolean, common | index,
        new ValueRange<bool>(false, true),
        "bit");
      
      types.Int8 = DataTypeInfo.Range(SqlType.Int8, common | index | identity,
        new ValueRange<sbyte>(sbyte.MinValue, sbyte.MaxValue),
        "decimal(3)", "numeric(3)");

      types.UInt8 = DataTypeInfo.Range(SqlType.UInt8, common | index | identity,
        new ValueRange<byte>(byte.MinValue, byte.MaxValue),
        "tinyint");

      types.Int16 = DataTypeInfo.Range(SqlType.Int16, common | index | identity,
        new ValueRange<short>(short.MinValue, short.MaxValue),
        "smallint");

      types.UInt16 = DataTypeInfo.Range(SqlType.UInt16, common | index | identity,
        new ValueRange<ushort>(ushort.MinValue, ushort.MaxValue),
        "decimal(6)", "numeric(6)");

      types.Int32 = DataTypeInfo.Range(SqlType.Int32, common | index | identity,
        new ValueRange<int>(int.MinValue, int.MaxValue),
        "integer", "int");

      types.UInt32 = DataTypeInfo.Range(SqlType.UInt32, common | index | identity,
        new ValueRange<uint>(uint.MinValue, uint.MaxValue),
        "decimal(11)", "numeric(11)");

      types.Int64 = DataTypeInfo.Range(SqlType.Int64, common | index | identity,
        new ValueRange<long>(long.MinValue, long.MaxValue),
        "bigint");

      types.UInt64 = DataTypeInfo.Range(SqlType.UInt64, common | index | identity,
        new ValueRange<ulong>(ulong.MinValue, ulong.MaxValue),
        "decimal(20)", "numeric(20)");

      types.Decimal = DataTypeInfo.Fractional(SqlType.Decimal, common | index,
        new ValueRange<decimal>(decimal.MinValue, decimal.MaxValue), 38,
        "decimal", "numeric");
      
      types.Float = DataTypeInfo.Range(SqlType.Float, common | index,
        new ValueRange<float>(float.MinValue, float.MaxValue),
        "real");

      types.Double = DataTypeInfo.Range(SqlType.Double, common | index,
        new ValueRange<double>(double.MinValue, double.MaxValue),
        "float");
      
      types.DateTime = DataTypeInfo.Range(SqlType.DateTime, common | index,
        new ValueRange<DateTime>(DateTime.MinValue, DateTime.MaxValue),
        "datetime");

      types.Char = DataTypeInfo.Stream(SqlType.Char, common | index, 4000, "nchar");
      types.VarChar = DataTypeInfo.Stream(SqlType.VarChar, common | index, 4000, "nvarchar");
      types.VarCharMax = DataTypeInfo.Regular(SqlType.VarCharMax, common, "nvarchar(max)");
      
      types.Binary = DataTypeInfo.Stream(SqlType.Binary, common | index, 4000, "binary");
      types.VarBinary = DataTypeInfo.Stream(SqlType.VarBinary, common | index, 4000, "varbinary");
      types.VarBinaryMax = DataTypeInfo.Regular(SqlType.VarBinaryMax, common, "varbinary(max)");

      types.Guid = DataTypeInfo.Regular(SqlType.Guid, common | index, "uniqueidentifier");
      
      return types;
    }

    public override VersionInfo GetVersionInfo()
    {
      return new VersionInfo(new Version(3, 0));
    }

    public override IsolationLevels GetIsolationLevels()
    {
      IsolationLevels levels =
        IsolationLevels.ReadCommitted |
          IsolationLevels.Snapshot;

      return levels;
    }

    public override EntityInfo GetDomainInfo()
    {
      return new EntityInfo() as EntityInfo;
    }
    
    public override ConstraintInfo GetAssertionInfo()
    {
      return new EntityInfo() as ConstraintInfo;
    }

    public override int GetStringIndexingBase()
    {
      return 1;
    }
  }
}