// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Common.Mssql.v2000
{
  /// <summary>
  /// Represents a <see cref="ServerInfo"/> provider for MS SQL Server 2000.
  /// </summary>
  public class MssqlServerInfoProvider : IServerInfoProvider
  {
    private int cIdentifierLength = 128;
    private VersionInfo versionInfo;

    /// <summary>
    /// Gets the collation info.
    /// </summary>
    /// <returns></returns>
    public virtual EntityInfo CollationInfo
    {
      get
      {
        EntityInfo collationInfo = new EntityInfo();
        collationInfo.MaxIdentifierLength = cIdentifierLength;
        collationInfo.AllowedDdlStatements = DdlStatements.None;
        return collationInfo;
      }
    }

    /// <summary>
    /// Gets the character set info.
    /// </summary>
    /// <returns></returns>
    public virtual EntityInfo CharacterSetInfo
    {
      get
      {
        EntityInfo characterSetInfo = new EntityInfo();
        characterSetInfo.MaxIdentifierLength = cIdentifierLength;
        characterSetInfo.AllowedDdlStatements = DdlStatements.None;
        return characterSetInfo;
      }
    }

    /// <summary>
    /// Gets the translation info.
    /// </summary>
    /// <returns></returns>
    public virtual EntityInfo TranslationInfo
    {
      get
      {
        EntityInfo translationInfo = new EntityInfo();
        translationInfo.MaxIdentifierLength = cIdentifierLength;
        translationInfo.AllowedDdlStatements = DdlStatements.None;
        return translationInfo;
      }
    }

    /// <summary>
    /// Gets the trigger info.
    /// </summary>
    /// <returns></returns>
    public virtual EntityInfo TriggerInfo
    {
      get
      {
        EntityInfo triggerInfo = new EntityInfo();
        triggerInfo.MaxIdentifierLength = cIdentifierLength;
        triggerInfo.AllowedDdlStatements = DdlStatements.All;
        return triggerInfo;
      }
    }

    /// <summary>
    /// Gets the stored procedure info.
    /// </summary>
    /// <returns></returns>
    public virtual EntityInfo StoredProcedureInfo
    {
      get
      {
        EntityInfo procedureInfo = new EntityInfo();
        procedureInfo.MaxIdentifierLength = cIdentifierLength;
        procedureInfo.AllowedDdlStatements = DdlStatements.All;
        return procedureInfo;
      }
    }

    /// <summary>
    /// Gets the sequence info.
    /// </summary>
    /// <returns></returns>
    public virtual SequenceInfo SequenceInfo
    {
      get
      {
        SequenceInfo sequenceInfo = new SequenceInfo();
        sequenceInfo.MaxIdentifierLength = cIdentifierLength;
        sequenceInfo.AllowedDdlStatements = DdlStatements.All;
        return sequenceInfo;
      }
    }

    /// <summary>
    /// Gets the database info.
    /// </summary>
    /// <returns></returns>
    public virtual EntityInfo DatabaseInfo
    {
      get
      {
        EntityInfo databaseInfo = new EntityInfo();
        databaseInfo.MaxIdentifierLength = cIdentifierLength;
        databaseInfo.AllowedDdlStatements = DdlStatements.All;
        return databaseInfo;
      }
    }

    /// <summary>
    /// Gets the column info.
    /// </summary>
    /// <returns></returns>
    public virtual ColumnInfo ColumnInfo
    {
      get
      {
        ColumnInfo columnInfo = new ColumnInfo();
        columnInfo.MaxIdentifierLength = cIdentifierLength;

        #region ColumnFeatures_

        columnInfo.Features = ColumnFeatures.Identity | ColumnFeatures.Computed;

        #endregion

        columnInfo.AllowedDdlStatements = DdlStatements.All;
        return columnInfo;
      }
    }

    /// <summary>
    /// Gets the view info.
    /// </summary>
    /// <returns></returns>
    public virtual EntityInfo ViewInfo
    {
      get
      {
        EntityInfo viewInfo = new EntityInfo();
        viewInfo.MaxIdentifierLength = cIdentifierLength;
        viewInfo.AllowedDdlStatements = DdlStatements.All;
        return viewInfo;
      }
    }

    /// <summary>
    /// Gets the schema info.
    /// </summary>
    /// <returns></returns>
    public virtual EntityInfo SchemaInfo
    {
      get
      {
        EntityInfo schemaInfo = new EntityInfo();
        schemaInfo.MaxIdentifierLength = cIdentifierLength;
        schemaInfo.AllowedDdlStatements = DdlStatements.All;
        return schemaInfo;
      }
    }

    /// <summary>
    /// Gets the table info.
    /// </summary>
    /// <returns></returns>
    public virtual TableInfo TableInfo
    {
      get
      {
        TableInfo tableInfo = new TableInfo();
        tableInfo.MaxIdentifierLength = cIdentifierLength;
        tableInfo.AllowedDdlStatements = DdlStatements.All;
        return tableInfo;
      }
    }

    /// <summary>
    /// Gets the temporary table info.
    /// </summary>
    /// <returns></returns>
    public virtual TemporaryTableInfo TemporaryTableInfo
    {
      get
      {
        TemporaryTableInfo temporaryTableInfo = new TemporaryTableInfo();
        temporaryTableInfo.MaxIdentifierLength = 116;
        temporaryTableInfo.Features = TemporaryTableFeatures.Global | TemporaryTableFeatures.Local;
        temporaryTableInfo.AllowedDdlStatements = DdlStatements.All;
        return temporaryTableInfo;
      }
    }

    /// <summary>
    /// Gets the check constraint info.
    /// </summary>
    /// <returns></returns>
    public virtual CheckConstraintInfo CheckConstraintInfo
    {
      get
      {
        CheckConstraintInfo checkConstraintInfo = new CheckConstraintInfo();
        checkConstraintInfo.MaxIdentifierLength = cIdentifierLength;
        checkConstraintInfo.MaxExpressionLength = 4000;
        checkConstraintInfo.AllowedDdlStatements = DdlStatements.All;
        return checkConstraintInfo;
      }
    }

    /// <summary>
    /// Gets the primary key info.
    /// </summary>
    /// <returns></returns>
    public virtual ConstraintInfo PrimaryKeyInfo
    {
      get
      {
        ConstraintInfo primaryKeyInfo = new ConstraintInfo();
        primaryKeyInfo.MaxIdentifierLength = cIdentifierLength;
        primaryKeyInfo.Features = ConstraintFeatures.Clustered;
        primaryKeyInfo.AllowedDdlStatements = DdlStatements.All;
        return primaryKeyInfo;
      }
    }

    /// <summary>
    /// Gets the unique constraint info.
    /// </summary>
    /// <returns></returns>
    public virtual ConstraintInfo UniqueConstraintInfo
    {
      get
      {
        ConstraintInfo uniqueConstraintInfo = new ConstraintInfo();
        uniqueConstraintInfo.MaxIdentifierLength = cIdentifierLength;
        uniqueConstraintInfo.Features = ConstraintFeatures.Clustered | ConstraintFeatures.Nullable;
        uniqueConstraintInfo.AllowedDdlStatements = DdlStatements.All;
        return uniqueConstraintInfo;
      }
    }

    /// <summary>
    /// Gets the index info.
    /// </summary>
    /// <returns></returns>
    public virtual IndexInfo IndexInfo
    {
      get
      {
        IndexInfo indexInfo = new IndexInfo();
        indexInfo.MaxIdentifierLength = cIdentifierLength;
        indexInfo.MaxColumnAmount = 16;
        indexInfo.MaxLength = 900;
        indexInfo.AllowedDdlStatements = DdlStatements.All;
        indexInfo.Features = IndexFeatures.Clustered | IndexFeatures.FillFactor |
                             IndexFeatures.Unique;
        MssqlVersionInfo vi = versionInfo as MssqlVersionInfo;
        if (vi != null &&
            (vi.Edition == MssqlEdition.EnterpriseEdition || vi.Edition == MssqlEdition.PersonalEdition ||
             vi.Edition == MssqlEdition.DeveloperEdition || vi.Edition == MssqlEdition.StandardEdition ||
             vi.Edition == MssqlEdition.EnterpriseEvaluationEdition))
          indexInfo.Features |= IndexFeatures.FullText;
        return indexInfo;
      }
    }

    /// <summary>
    /// Gets the referential constraint info.
    /// </summary>
    /// <returns></returns>
    public virtual ReferenceConstraintInfo ReferentialConstraintInfo
    {
      get
      {
        ReferenceConstraintInfo referenceConstraintInfo = new ReferenceConstraintInfo();
        referenceConstraintInfo.MaxIdentifierLength = cIdentifierLength;
        referenceConstraintInfo.Actions = ConstraintActions.NoAction | ConstraintActions.Cascade;
        referenceConstraintInfo.AllowedDdlStatements = DdlStatements.All;
        return referenceConstraintInfo;
      }
    }

    /// <summary>
    /// Gets the query info.
    /// </summary>
    /// <returns></returns>
    public virtual QueryInfo QueryInfo
    {
      get
      {
        QueryInfo queryInfo = new QueryInfo();
        queryInfo.MaxLength = 60000*4000;
        queryInfo.MaxComparisonOperations = 1000;
        queryInfo.MaxNestedSubqueriesAmount = 32;
        queryInfo.ParameterPrefix = "@";
        queryInfo.QuoteToken = "'";
        queryInfo.Features = QueryFeatures.NamedParameters | QueryFeatures.UseParameterPrefix | QueryFeatures.SquareBrackets |
                             QueryFeatures.Batches;
        return queryInfo;
      }
    }

    /// <summary>
    /// Gets the identity info.
    /// </summary>
    /// <returns></returns>
    public virtual IdentityInfo IdentityInfo
    {
      get
      {
        IdentityInfo identityInfo = new IdentityInfo();
        identityInfo.Features = IdentityFeatures.StartValue | IdentityFeatures.Increment;
        return identityInfo;
      }
    }

    /// <summary>
    /// Gets the collection of supported data types.
    /// </summary>
    /// <returns></returns>
    public virtual DataTypeCollection DataTypesInfo
    {
      get
      {
        DataTypeCollection types = new DataTypeCollection();

        types.Xml = DataTypeInfo.Empty as StreamDataTypeInfo;
        types.AnsiVarCharMax = DataTypeInfo.Empty as StreamDataTypeInfo;
        types.VarCharMax = DataTypeInfo.Empty as StreamDataTypeInfo;
        types.VarBinaryMax = DataTypeInfo.Empty as StreamDataTypeInfo;

        DataTypeFeatures common = DataTypeFeatures.Default | DataTypeFeatures.Nullable | DataTypeFeatures.NonKeyIndexing |
                                  DataTypeFeatures.Grouping | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple;

        DataTypeFeatures index = DataTypeFeatures.Indexing | DataTypeFeatures.Clustering |
                                 DataTypeFeatures.FillFactor | DataTypeFeatures.KeyConstraint;

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
        types.DateTime.Value =
          new ValueRange<DateTime>(
            DateTime.ParseExact("1753-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat),
            DateTime.ParseExact("9999-12-31", "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat));
        types.DateTime.Features = common | index;

        types.AnsiChar =
          new StreamDataTypeInfo(SqlDataType.AnsiChar, typeof (string), new string[] {"char", "character"});
        types.AnsiChar.Length = new ValueRange<int>(1, 8000, 1);
        types.AnsiChar.Features = common | index;

        types.AnsiVarChar =
          new StreamDataTypeInfo(
            SqlDataType.AnsiVarChar, typeof (string), new string[] {"varchar", "character varying", "char varying"});
        types.AnsiVarChar.Length = new ValueRange<int>(1, 8000, 1);
        types.AnsiVarChar.Features = common | index;

        types.AnsiText =
          new StreamDataTypeInfo(SqlDataType.AnsiText, typeof (string), new string[] {"text"});
        types.AnsiText.Length = new ValueRange<int>(1, 2147483647, 1);
        types.AnsiText.Features = common;

        types.Char = new StreamDataTypeInfo(SqlDataType.Char, typeof (string), new string[] {"nchar"});
        types.Char.Length = new ValueRange<int>(1, 4000, 1);
        types.Char.Features = common | index;

        types.VarChar = new StreamDataTypeInfo(SqlDataType.VarChar, typeof (string), new string[] {"nvarchar"});
        types.VarChar.Length = new ValueRange<int>(1, 4000, 1);
        types.VarChar.Features = common | index;

        types.Text =
          new StreamDataTypeInfo(SqlDataType.Text, typeof (string), new string[] {"ntext"});
        types.Text.Length = new ValueRange<int>(1, 1073741823, 1);
        types.Text.Features = common;

        types.Binary = new StreamDataTypeInfo(SqlDataType.Binary, typeof (byte[]), new string[] {"binary"});
        types.Binary.Length = new ValueRange<int>(1, 4000, 1);
        types.Binary.Features = common | index;

        types.VarBinary = new StreamDataTypeInfo(SqlDataType.VarBinary, typeof (byte[]), new string[] {"varbinary"});
        types.VarBinary.Length = new ValueRange<int>(1, 4000, 1);
        types.VarBinary.Features = common | index;

        types.Image =
          new StreamDataTypeInfo(SqlDataType.Image, typeof (byte[]), new string[] {"image"});
        types.Image.Length = new ValueRange<int>(1, 1073741823, 1);
        types.Image.Features = common;

        types.Guid = new StreamDataTypeInfo(SqlDataType.Guid, typeof (Guid), new string[] {"uniqueidentifier"});
        types.Guid.Length = new ValueRange<int>(16, 16, 16);
        types.Guid.Features = common | index;

        types.TimeStamp =
          new StreamDataTypeInfo(SqlDataType.TimeStamp, typeof (byte[]), new string[] {"timestamp", "rowversion"});
        types.TimeStamp.Length = new ValueRange<int>(8, 8, 8);
        types.TimeStamp.Features = common | index ^ DataTypeFeatures.Multiple ^ DataTypeFeatures.Default;

        types.Variant = new StreamDataTypeInfo(SqlDataType.Variant, null, new string[] {"sql_variant"});
        types.Variant.Length = new ValueRange<int>(1, 8000, 1);
        types.Variant.Features = common | index;

        return types;
      }
    }

    /// <summary>
    /// Gets the version.
    /// </summary>
    /// <returns></returns>
    public virtual VersionInfo VersionInfo
    {
      get { return versionInfo; }
    }

    /// <summary>
    /// Gets the supported server entities.
    /// </summary>
    /// <returns></returns>
    public virtual ServerEntities Entities
    {
      get
      {
        ServerEntities entities =
          ServerEntities.Collations |
          ServerEntities.Constraints |
          ServerEntities.Schemas |
          ServerEntities.Sequences |
          ServerEntities.StoredProcedures |
          ServerEntities.Synonyms |
          ServerEntities.Filegroups |
          ServerEntities.Triggers |
          ServerEntities.UserDefinedFunctions |
          ServerEntities.UserDefinedTypes;

        return entities;
      }
    }

    /// <summary>
    /// Gets the supported isolation levels.
    /// </summary>
    /// <returns></returns>
    public virtual IsolationLevels IsolationLevels
    {
      get
      {
        IsolationLevels levels =
          IsolationLevels.ReadUncommitted |
          IsolationLevels.ReadCommitted |
          IsolationLevels.RepeatableRead |
          IsolationLevels.Serializable;

        return levels;
      }
    }

    /// <summary>
    /// Gets the domain info.
    /// </summary>
    /// <returns></returns>
    public virtual EntityInfo DomainInfo
    {
      get { return EntityInfo.Empty; }
    }

    /// <summary>
    /// Gets the assertion info.
    /// </summary>
    /// <returns></returns>
    public virtual ConstraintInfo AssertionInfo
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

    /// <summary>
    /// Initializes a new instance of the <see cref="MssqlServerInfoProvider"/> class.
    /// </summary>
    /// <param name="connection">The connection.</param>
    public MssqlServerInfoProvider(Connection connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      try {
        connection.Open();
        using (IDbCommand command = connection.RealConnection.CreateCommand()) {
          command.CommandText =
            "select"+
            "  coalesce(serverProperty('EditionID'),"+
            "    case serverProperty('Edition')"+
            "    when 'Desktop Edition' then "+((long)MssqlEdition.DesktopEdition)+
            "    when 'Express Edition' then "+((long)MssqlEdition.ExpressEdition)+
            "    when 'Standard Edition' then "+((long)MssqlEdition.StandardEdition)+
            "    when 'Workgroup Edition' then "+((long)MssqlEdition.WorkgroupEdition)+
            "    when 'Enterprise Edition' then "+((long)MssqlEdition.EnterpriseEdition)+
            "    when 'Personal Edition' then "+((long)MssqlEdition.PersonalEdition)+
            "    when 'Developer Edition' then "+((long)MssqlEdition.DeveloperEdition)+
            "    when 'Enterprise Evaluation Edition' then "+((long)MssqlEdition.EnterpriseEvaluationEdition)+
            "    when 'Windows Embedded SQL' then "+((long)MssqlEdition.WindowsEmbeddedSql)+
            "    when 'Express Edition with Advanced Services' then "+
            ((long)MssqlEdition.ExpressEditionWithAdvancedServices)+
            "    end"+
            "  ) as EditionID,"+
            "    serverProperty('Edition') as EditionName,"+
            "    serverProperty('ProductVersion') as ProductVersion,"+
            "    serverProperty('ProductLevel') as ProductLevel,"+
            "    serverProperty('EngineEdition') as EngineEdition";
          using (IDataReader reader = command.ExecuteReader()) {
            if (reader.Read()) {
              MssqlVersionInfo v = new MssqlVersionInfo(new Version(reader.GetString(2)));
              long editionID = Convert.ToInt64(reader.GetValue(0));
              if (Enum.IsDefined(typeof(MssqlEdition), editionID))
                v.Edition = ((MssqlEdition)Enum.ToObject(typeof(MssqlEdition), editionID));
              v.EditionName = reader.GetString(1);
              v.ProductLevel = reader.GetString(3);
              int engineEditionID = reader.GetInt32(4);
              if (Enum.IsDefined(typeof(MssqlEdition), (long)engineEditionID))
                v.EngineEdition = (MssqlEngineEdition)Enum.ToObject(typeof(MssqlEngineEdition), engineEditionID);
              versionInfo = v;
            }
            else
              throw new Exception("Unable to obtain version info.");
          }
        }
      }
      catch {
        versionInfo = new MssqlVersionInfo(new Version(8, 0));
      }
      finally {
        try {
          if (connection!=null && connection.State!=ConnectionState.Closed)
            connection.Close();
        }
        catch (DbException) {
        }
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MssqlServerInfoProvider"/> class.
    /// </summary>
    /// <param name="versionInfo">The version info.</param>
    public MssqlServerInfoProvider(MssqlVersionInfo versionInfo)
    {
      this.versionInfo = versionInfo;
    }
  }
}