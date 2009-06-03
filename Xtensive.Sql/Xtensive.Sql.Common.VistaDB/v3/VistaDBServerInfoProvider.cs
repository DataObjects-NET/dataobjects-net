// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Globalization;

namespace Xtensive.Sql.Common.VistaDB.v3
{
  public class VistaDBServerInfoProvider : IServerInfoProvider
  {
    private int cDefaultIdentifierLength = 128;

//    /// <summary>
//    /// Initializes this provider.
//    /// </summary>
//    /// <param name="connection">The connection.</param>
//    public void Initialize(Connection connection)
//    {
//    }

    public ColumnInfo ColumnInfo
    {
      get
      {
        ColumnInfo columnInfo = new ColumnInfo();
        columnInfo.MaxIdentifierLength = cDefaultIdentifierLength;
        columnInfo.Features = ColumnFeatures.Identity;
        columnInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
        return columnInfo;
      }
    }

    public CheckConstraintInfo CheckConstraintInfo
    {
      get
      {
        CheckConstraintInfo checkConstraintInfo = new CheckConstraintInfo();
        checkConstraintInfo.MaxIdentifierLength = cDefaultIdentifierLength;
        checkConstraintInfo.MaxExpressionLength = 128;
        checkConstraintInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
        return checkConstraintInfo;
      }
    }

    /// <summary>
    /// Gets the primary key info.
    /// </summary>
    /// <returns></returns>
    public ConstraintInfo PrimaryKeyInfo
    {
      get { return EntityInfo.Empty as ConstraintInfo; }
    }

    public EntityInfo DatabaseInfo
    {
      get
      {
        EntityInfo databaseInfo = new EntityInfo();
        databaseInfo.MaxIdentifierLength = cDefaultIdentifierLength;
        databaseInfo.AllowedDdlStatements = DdlStatements.Create;
        return databaseInfo;
      }
    }

    public IdentityInfo IdentityInfo
    {
      get
      {
        IdentityInfo identityInfo = new IdentityInfo();
        identityInfo.Features = IdentityFeatures.StartValue | IdentityFeatures.Increment;
        return identityInfo;
      }
    }

    public IndexInfo IndexInfo
    {
      get
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
    }

    /// <summary>
    /// Gets the referential constraint info.
    /// </summary>
    /// <returns></returns>
    public ReferenceConstraintInfo ReferentialConstraintInfo
    {
      get { return EntityInfo.Empty as ReferenceConstraintInfo; }
    }

//    public ConstraintInfo GetPrimaryKeyConstraintInfo()
//    {
//      ConstraintInfo primaryKeyConstraintInfo = new ConstraintInfo();
//      primaryKeyConstraintInfo.MaxIdentifierLength = cDefaultIdentifierLength;
//      primaryKeyConstraintInfo.Features = ConstraintFeatures.Clustered;
//      primaryKeyConstraintInfo.AllowedDdlStatements = DdlStatements.Create|DdlStatements.Drop;
//      return primaryKeyConstraintInfo;
//    }

    public QueryInfo QueryInfo
    {
      get
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

    public TableInfo TableInfo
    {
      get
      {
        TableInfo tableInfo = new TableInfo();
        tableInfo.MaxIdentifierLength = cDefaultIdentifierLength;
        tableInfo.PartitionMethods = PartitionMethods.None;
        tableInfo.AllowedDdlStatements = DdlStatements.All;
        return tableInfo;
      }
    }

    /// <summary>
    /// Gets the temporary table info.
    /// </summary>
    /// <returns></returns>
    public TemporaryTableInfo TemporaryTableInfo
    {
      get { return EntityInfo.Empty as TemporaryTableInfo; }
    }

    /// <summary>
    /// Gets the collation info.
    /// </summary>
    /// <returns></returns>
    public EntityInfo CollationInfo
    {
      get { return EntityInfo.Empty as EntityInfo; }
    }

    /// <summary>
    /// Gets the character set info.
    /// </summary>
    /// <returns></returns>
    public EntityInfo CharacterSetInfo
    {
      get { return EntityInfo.Empty as EntityInfo; }
    }

    /// <summary>
    /// Gets the translation info.
    /// </summary>
    /// <returns></returns>
    public EntityInfo TranslationInfo
    {
      get { return EntityInfo.Empty as EntityInfo; }
    }

    public EntityInfo TriggerInfo
    {
      get
      {
        EntityInfo triggerInfo = new EntityInfo();
        triggerInfo.MaxIdentifierLength = cDefaultIdentifierLength;
        triggerInfo.AllowedDdlStatements = DdlStatements.All;
        return triggerInfo;
      }
    }

    /// <summary>
    /// Gets the stored procedure info.
    /// </summary>
    /// <returns></returns>
    public EntityInfo StoredProcedureInfo
    {
      get { return EntityInfo.Empty as EntityInfo; }
    }

    /// <summary>
    /// Gets the sequence info.
    /// </summary>
    /// <returns></returns>
    public SequenceInfo SequenceInfo
    {
      get { return EntityInfo.Empty as SequenceInfo; }
    }

    public ConstraintInfo UniqueConstraintInfo
    {
      get
      {
        ConstraintInfo uniqueConstraintInfo = new ConstraintInfo();
        uniqueConstraintInfo.MaxIdentifierLength = cDefaultIdentifierLength;
        uniqueConstraintInfo.Features = ConstraintFeatures.Clustered | ConstraintFeatures.Nullable;
        uniqueConstraintInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
        return uniqueConstraintInfo;
      }
    }

    public EntityInfo ViewInfo
    {
      get
      {
        EntityInfo viewInfo = new EntityInfo();
        viewInfo.MaxIdentifierLength = cDefaultIdentifierLength;
        viewInfo.AllowedDdlStatements = DdlStatements.All;
        return viewInfo;
      }
    }

    /// <summary>
    /// Gets the schema info.
    /// </summary>
    /// <returns></returns>
    public EntityInfo SchemaInfo
    {
      get { return EntityInfo.Empty as EntityInfo; }
    }

    public DataTypeCollection DataTypesInfo
    {
      get
      {
        DataTypeCollection types = new DataTypeCollection();

        DataTypeFeatures common = DataTypeFeatures.Default | DataTypeFeatures.Nullable |
                                  DataTypeFeatures.Grouping | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple;

        DataTypeFeatures index = DataTypeFeatures.Indexing | DataTypeFeatures.Clustering |
                                 DataTypeFeatures.KeyConstraint;

        DataTypeFeatures identity = DataTypeFeatures.Identity;

        types.Boolean = new RangeDataTypeInfo<bool>(SqlDataType.Boolean, new string[] {"bit"});
        types.Boolean.Value = new ValueRange<bool>(false, true);
        types.Boolean.Features = common | index;

        types.SByte = new IntegerDataTypeInfo<sbyte>(SqlDataType.SByte, new string[] {"decimal(3)", "numeric(3)"});
        types.SByte.Value = new ValueRange<sbyte>(sbyte.MinValue, sbyte.MaxValue);
        types.SByte.Features = common | index | identity;

        types.Byte = new IntegerDataTypeInfo<byte>(SqlDataType.Byte, new string[] {"tinyint"});
        types.Byte.Value = new ValueRange<byte>(byte.MinValue, byte.MaxValue);
        types.Byte.Features = common | index | identity;

        types.Int16 = new IntegerDataTypeInfo<short>(SqlDataType.Int16, new string[] {"smallint"});
        types.Int16.Value = new ValueRange<short>(short.MinValue, short.MaxValue);
        types.Int16.Features = common | index | identity;

        types.UInt16 = new IntegerDataTypeInfo<ushort>(SqlDataType.UInt16, new string[] {"decimal(6)", "numeric(6)"});
        types.UInt16.Value = new ValueRange<ushort>(ushort.MinValue, ushort.MaxValue);
        types.UInt16.Features = common | index | identity;

        types.Int32 = new IntegerDataTypeInfo<int>(SqlDataType.Int32, new string[] {"integer", "int"});
        types.Int32.Value = new ValueRange<int>(int.MinValue, int.MaxValue);
        types.Int32.Features = common | index | identity;

        types.UInt32 = new IntegerDataTypeInfo<uint>(SqlDataType.UInt32, new string[] {"decimal(11)", "numeric(11)"});
        types.UInt32.Value = new ValueRange<uint>(uint.MinValue, uint.MaxValue);
        types.UInt32.Features = common | index | identity;

        types.Int64 = new IntegerDataTypeInfo<long>(SqlDataType.Int64, new string[] {"bigint"});
        types.Int64.Value = new ValueRange<long>(long.MinValue, long.MaxValue);
        types.Int64.Features = common | index | identity;

        types.UInt64 =
          new IntegerDataTypeInfo<ulong>(SqlDataType.UInt64, new string[] {"decimal(20)", "numeric(20)"});
        types.UInt64.Value = new ValueRange<ulong>(ulong.MinValue, ulong.MaxValue);
        types.UInt64.Features = common | index | identity;

        types.Decimal =
          new FractionalDataTypeInfo<decimal>(SqlDataType.Decimal, new string[] {"decimal", "numeric"});
        types.Decimal.Value = new ValueRange<decimal>(decimal.MinValue, decimal.MaxValue);
        types.Decimal.Precision = new ValueRange<short>(1, 38, 18);
        types.Decimal.Scale = new ValueRange<short>(0, 38, 0);
        types.Decimal.Features = common | index;

        types.Float = new FractionalDataTypeInfo<float>(SqlDataType.Float, new string[] {"real"});
        types.Float.Value = new ValueRange<float>(float.MinValue, float.MaxValue);
        types.Float.Precision = new ValueRange<short>(1, 24, 24);
        types.Float.Features = common | index;

        types.Double = new FractionalDataTypeInfo<double>(SqlDataType.Double, new string[] {"float"});
        types.Double.Value = new ValueRange<double>(double.MinValue, double.MaxValue);
        types.Double.Precision = new ValueRange<short>(1, 53, 53);
        types.Double.Features = common | index;

        types.SmallMoney = new FractionalDataTypeInfo<decimal>(SqlDataType.SmallMoney, new string[] {"smallmoney"});
        types.SmallMoney.Value = new ValueRange<decimal>(-214748.3648m, 214748.3647m);
        types.SmallMoney.Scale = new ValueRange<short>(4, 4, 4);
        types.SmallMoney.Features = common | index;

        types.Money = new FractionalDataTypeInfo<decimal>(SqlDataType.Money, new string[] {"money"});
        types.Money.Value = new ValueRange<decimal>(-922337203685477.5808m, 922337203685477.5807m);
        types.Money.Scale = new ValueRange<short>(4, 4, 4);
        types.Money.Features = common | index;

        types.SmallDateTime =
          new RangeDataTypeInfo<DateTime>(SqlDataType.SmallDateTime, new string[] {"smalldatetime"});
        types.SmallDateTime.Value =
          new ValueRange<DateTime>(
            DateTime.ParseExact("1900-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat),
            DateTime.ParseExact("2079-06-06", "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat));
        types.SmallDateTime.Features = common | index;

        types.DateTime = new RangeDataTypeInfo<DateTime>(SqlDataType.DateTime, new string[] {"datetime"});
        types.DateTime.Value = new ValueRange<DateTime>(DateTime.MinValue, DateTime.MaxValue);
        types.DateTime.Features = common | index;

        types.AnsiChar = new StreamDataTypeInfo(SqlDataType.AnsiChar, typeof (string), new string[] {"char", "character"});
        types.AnsiChar.Length = new ValueRange<int>(1, 8000, 1);
        types.AnsiChar.Features = common | index;

        types.AnsiVarChar =
          new StreamDataTypeInfo(
            SqlDataType.AnsiVarChar, typeof (string), new string[] {"varchar", "character varying", "char varying"});
        types.AnsiVarChar.Length = new ValueRange<int>(1, 8000, 1);
        types.AnsiVarChar.Features = common | index;

        types.AnsiVarCharMax =
          new StreamDataTypeInfo(SqlDataType.AnsiVarCharMax, typeof (string), new string[] {"varchar(max)"});
        types.AnsiVarCharMax.Length = new ValueRange<int>(1, 2147483647, 1);
        types.AnsiVarCharMax.Features = common;

        types.AnsiText =
          new StreamDataTypeInfo(SqlDataType.AnsiText, typeof (string), new string[] {"text"});
        types.AnsiText.Length = new ValueRange<int>(1, 134213632, 1);
        types.AnsiText.Features = common;

        types.Char = new StreamDataTypeInfo(SqlDataType.Char, typeof (string), new string[] {"nchar"});
        types.Char.Length = new ValueRange<int>(1, 4000, 1);
        types.Char.Features = common | index;

        types.VarChar = new StreamDataTypeInfo(SqlDataType.VarChar, typeof (string), new string[] {"nvarchar"});
        types.VarChar.Length = new ValueRange<int>(1, 4000, 1);
        types.VarChar.Features = common | index;

        types.VarCharMax =
          new StreamDataTypeInfo(SqlDataType.VarCharMax, typeof (string), new string[] {"nvarchar(max)"});
        types.VarCharMax.Length = new ValueRange<int>(1, 1073741823, 1);
        types.VarCharMax.Features = common;

        types.Text =
          new StreamDataTypeInfo(SqlDataType.Text, typeof (string), new string[] {"ntext"});
        types.Text.Length = new ValueRange<int>(1, 67106816, 1);
        types.Text.Features = common;

        types.Binary = new StreamDataTypeInfo(SqlDataType.Binary, typeof (byte[]), new string[] {"binary"});
        types.Binary.Length = new ValueRange<int>(1, 4000, 1);
        types.Binary.Features = common | index;

        types.VarBinary = new StreamDataTypeInfo(SqlDataType.VarBinary, typeof (byte[]), new string[] {"varbinary"});
        types.VarBinary.Length = new ValueRange<int>(1, 4000, 1);
        types.VarBinary.Features = common | index;

        types.VarBinaryMax =
          new StreamDataTypeInfo(SqlDataType.VarBinaryMax, typeof (byte[]), new string[] {"varbinary(max)"});
        types.VarBinaryMax.Length = new ValueRange<int>(1, 1073741823, 1);
        types.VarBinaryMax.Features = common;

        types.Image = new StreamDataTypeInfo(SqlDataType.Image, typeof (byte[]), new string[] {"image"});
        types.Image.Length = new ValueRange<int>(1, 134213632, 1);
        types.Image.Features = common;

        types.Guid = new StreamDataTypeInfo(SqlDataType.Guid, typeof (Guid), new string[] {"uniqueidentifier"});
        types.Guid.Length = new ValueRange<int>(16, 16, 16);
        types.Guid.Features = common | index;

        types.Xml = null;

        types.TimeStamp =
          new StreamDataTypeInfo(SqlDataType.TimeStamp, typeof (byte[]), new string[] {"timestamp", "rowversion"});
        types.TimeStamp.Length = new ValueRange<int>(8, 8, 8);
        types.TimeStamp.Features = common | index ^ DataTypeFeatures.Multiple ^ DataTypeFeatures.Default;

        types.Variant = null;

        return types;
      }
    }

    /// <summary>
    /// Gets the version.
    /// </summary>
    /// <returns></returns>
    public VersionInfo VersionInfo
    {
      get { return new VersionInfo(new Version(3, 0)); }
    }

    /// <summary>
    /// Gets the supported server entities.
    /// </summary>
    /// <returns></returns>
    public ServerEntities Entities
    {
      get
      {
        ServerEntities entities =
          ServerEntities.Constraints
          | ServerEntities.Indexes
          | ServerEntities.Synonyms
          | ServerEntities.Triggers;

        return entities;
      }
    }

    /// <summary>
    /// Gets the supported isolation levels.
    /// </summary>
    /// <returns></returns>
    public IsolationLevels IsolationLevels
    {
      get
      {
        IsolationLevels levels =
          IsolationLevels.ReadCommitted |
          IsolationLevels.Snapshot;

        return levels;
      }
    }

    /// <summary>
    /// Gets the domain info.
    /// </summary>
    /// <returns></returns>
    public EntityInfo DomainInfo
    {
      get { return EntityInfo.Empty as EntityInfo; }
    }

    /// <summary>
    /// Gets the assertion info.
    /// </summary>
    /// <returns></returns>
    public ConstraintInfo AssertionInfo
    {
      get { return EntityInfo.Empty as ConstraintInfo; }
    }

    /// <summary>
    /// Gets the string indexing base.
    /// </summary>
    /// <returns></returns>
    public int StringIndexingBase
    {
      get { return 1; }
    }
  }
}