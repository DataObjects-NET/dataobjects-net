// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Sql.Info;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.SqlServer.v2005
{
  internal class ServerInfoProvider : Info.ServerInfoProvider
  {
    private const int MaxIdentifierLength = 128;
    private readonly SqlServerVersionInfo versionInfo;

    public override EntityInfo GetCollationInfo()
    {
      EntityInfo collationInfo = new EntityInfo();
      collationInfo.MaxIdentifierLength = MaxIdentifierLength;
      collationInfo.AllowedDdlStatements = DdlStatements.None;
      return collationInfo;
    }

    public override EntityInfo GetCharacterSetInfo()
    {
      EntityInfo characterSetInfo = new EntityInfo();
      characterSetInfo.MaxIdentifierLength = MaxIdentifierLength;
      characterSetInfo.AllowedDdlStatements = DdlStatements.None;
      return characterSetInfo;
    }

    public override EntityInfo GetTranslationInfo()
    {
      EntityInfo translationInfo = new EntityInfo();
      translationInfo.MaxIdentifierLength = MaxIdentifierLength;
      translationInfo.AllowedDdlStatements = DdlStatements.None;
      return translationInfo;
    }

    public override EntityInfo GetTriggerInfo()
    {
      EntityInfo triggerInfo = new EntityInfo();
      triggerInfo.MaxIdentifierLength = MaxIdentifierLength;
      triggerInfo.AllowedDdlStatements = DdlStatements.All;
      return triggerInfo;
    }

    public override EntityInfo GetStoredProcedureInfo()
    {
      EntityInfo procedureInfo = new EntityInfo();
      procedureInfo.MaxIdentifierLength = MaxIdentifierLength;
      procedureInfo.AllowedDdlStatements = DdlStatements.All;
      return procedureInfo;
    }

    public override SequenceInfo GetSequenceInfo()
    {
      return null;
    }

    public override EntityInfo GetDatabaseInfo()
    {
      EntityInfo databaseInfo = new EntityInfo();
      databaseInfo.MaxIdentifierLength = MaxIdentifierLength;
      databaseInfo.AllowedDdlStatements = DdlStatements.All;
      return databaseInfo;
    }

    public override ColumnInfo GetColumnInfo()
    {
      ColumnInfo columnInfo = new ColumnInfo();
      columnInfo.MaxIdentifierLength = MaxIdentifierLength;
      columnInfo.Features = ColumnFeatures.Identity | ColumnFeatures.Computed;
      columnInfo.AllowedDdlStatements = DdlStatements.All;
      return columnInfo;
    }

    public override EntityInfo GetViewInfo()
    {
      EntityInfo viewInfo = new EntityInfo();
      viewInfo.MaxIdentifierLength = MaxIdentifierLength;
      viewInfo.AllowedDdlStatements = DdlStatements.All;
      return viewInfo;
    }

    public override EntityInfo GetSchemaInfo()
    {
      EntityInfo schemaInfo = new EntityInfo();
      schemaInfo.MaxIdentifierLength = MaxIdentifierLength;
      schemaInfo.AllowedDdlStatements = DdlStatements.All;
      return schemaInfo;
    }

    public override TableInfo GetTableInfo()
    {
      var tableInfo = new TableInfo();
      tableInfo.MaxIdentifierLength = MaxIdentifierLength;
      tableInfo.AllowedDdlStatements = DdlStatements.All;

      var vi = versionInfo;
      if (vi!=null && (vi.Edition==SqlServerEdition.EnterpriseEdition || vi.Edition==SqlServerEdition.DeveloperEdition))
        tableInfo.PartitionMethods = PartitionMethods.List | PartitionMethods.Range | PartitionMethods.Hash;
      return tableInfo;
    }

    public override TemporaryTableInfo GetTemporaryTableInfo()
    {
      var temporaryTableInfo = new TemporaryTableInfo();
      temporaryTableInfo.MaxIdentifierLength = 116;
      temporaryTableInfo.Features = TemporaryTableFeatures.Global | TemporaryTableFeatures.Local;
      temporaryTableInfo.AllowedDdlStatements = DdlStatements.All;
      return temporaryTableInfo;
    }

    public override CheckConstraintInfo GetCheckConstraintInfo()
    {
      var checkConstraintInfo = new CheckConstraintInfo();
      checkConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      checkConstraintInfo.MaxExpressionLength = 4000;
      checkConstraintInfo.Features = CheckConstraintFeatures.None;
      checkConstraintInfo.AllowedDdlStatements = DdlStatements.All;
      return checkConstraintInfo;
    }

    public override PrimaryKeyConstraintInfo GetPrimaryKeyInfo()
    {
      var primaryKeyInfo = new PrimaryKeyConstraintInfo();
      primaryKeyInfo.MaxIdentifierLength = MaxIdentifierLength;
      primaryKeyInfo.Features = PrimaryKeyConstraintFeatures.Clustered;
      primaryKeyInfo.AllowedDdlStatements = DdlStatements.All;
      return primaryKeyInfo;
    }

    public override UniqueConstraintInfo GetUniqueConstraintInfo()
    {
      var uniqueConstraintInfo = new UniqueConstraintInfo();
      uniqueConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      uniqueConstraintInfo.Features = UniqueConstraintFeatures.Clustered | UniqueConstraintFeatures.Nullable;
      uniqueConstraintInfo.AllowedDdlStatements = DdlStatements.All;
      return uniqueConstraintInfo;
    }

    public override IndexInfo GetIndexInfo()
    {
      var indexInfo = new IndexInfo();
      indexInfo.MaxIdentifierLength = MaxIdentifierLength;
      indexInfo.MaxNumberOfColumns = 16;
      indexInfo.MaxLength = 900;
      indexInfo.AllowedDdlStatements = DdlStatements.All;
      indexInfo.Features =
        IndexFeatures.Clustered |
        IndexFeatures.FillFactor |
        IndexFeatures.Unique |
        IndexFeatures.NonKeyColumns |
        IndexFeatures.SortOrder;

      if (versionInfo.Edition==SqlServerEdition.EnterpriseEdition || versionInfo.Edition==SqlServerEdition.DeveloperEdition)
        indexInfo.PartitionMethods = PartitionMethods.Range;

      if (versionInfo.Edition==SqlServerEdition.EnterpriseEdition || versionInfo.Edition==SqlServerEdition.DeveloperEdition ||
        versionInfo.Edition==SqlServerEdition.WorkgroupEdition || versionInfo.Edition==SqlServerEdition.StandardEdition)
        indexInfo.Features |= IndexFeatures.FullText;

      return indexInfo;
    }

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

    public override QueryInfo GetQueryInfo()
    {
      var queryInfo = new QueryInfo();
      queryInfo.MaxLength = 60000 * 4000;
      queryInfo.MaxComparisonOperations = 1000;
      queryInfo.MaxNestedSubqueriesAmount = 32;
      queryInfo.ParameterPrefix = "@";
      queryInfo.Features =
        QueryFeatures.NamedParameters |
        QueryFeatures.ParameterPrefix |
        QueryFeatures.Batches |
        QueryFeatures.CrossApply;
      return queryInfo;
    }

    public override IdentityInfo GetIdentityInfo()
    {
      var identityInfo = new IdentityInfo();
      identityInfo.Features = IdentityFeatures.StartValue | IdentityFeatures.Increment;
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
        StandardValueRange.Bool, "bit");
     
      types.UInt8 = DataTypeInfo.Range(SqlType.UInt8, common | index | identity,
        StandardValueRange.Byte, "tinyint");

      types.Int16 = DataTypeInfo.Range(SqlType.Int16, common | index | identity,
        StandardValueRange.Int16, "smallint");

      types.Int32 = DataTypeInfo.Range(SqlType.Int32, common | index | identity,
        StandardValueRange.Int32, "integer", "int");

      types.Int64 = DataTypeInfo.Range(SqlType.Int64, common | index | identity,
        StandardValueRange.Int64, "bigint");

      types.Decimal = DataTypeInfo.Fractional(SqlType.Decimal, common | index,
        StandardValueRange.Decimal, 38, "decimal", "numeric");
      
      types.Float = DataTypeInfo.Range(SqlType.Float, common | index,
        StandardValueRange.Float, "real");

      types.Double = DataTypeInfo.Range(SqlType.Double, common | index,
        StandardValueRange.Double, "float");

      types.DateTime = DataTypeInfo.Range(SqlType.DateTime, common | index,
        new ValueRange<DateTime>(
          DateTime.ParseExact("1753-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat),
          DateTime.ParseExact("9999-12-31", "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat)),
        "datetime");

      types.Char = DataTypeInfo.Stream(SqlType.Char, common | index, 4000, "nchar");
      types.VarChar = DataTypeInfo.Stream(SqlType.VarChar,
        common | index | DataTypeFeatures.ZeroLengthValueIsNull, 4000, "nvarchar");
      types.VarCharMax = DataTypeInfo.Regular(SqlType.VarCharMax, common, "nvarchar(max)", "ntext");

      types.Binary = DataTypeInfo.Stream(SqlType.Binary, common | index, 4000, "binary");
      types.VarBinary = DataTypeInfo.Stream(SqlType.VarBinary, common | index, 4000, "varbinary");
      types.VarBinaryMax = DataTypeInfo.Regular(SqlType.VarBinaryMax, common, "varbinary(max)", "image");
      
      types.Guid = DataTypeInfo.Regular(SqlType.Guid, common | index, "uniqueidentifier");

      return types;
    }

    public override VersionInfo GetVersionInfo()
    {
      return versionInfo;
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
      var domainInfo = new EntityInfo();
      domainInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      domainInfo.MaxIdentifierLength = MaxIdentifierLength;
      return domainInfo;
    }

    public override AssertConstraintInfo GetAssertionInfo()
    {
      var assertConstraintInfo = new AssertConstraintInfo();
      assertConstraintInfo.AllowedDdlStatements = DdlStatements.Create | DdlStatements.Drop;
      assertConstraintInfo.MaxIdentifierLength = MaxIdentifierLength;
      return assertConstraintInfo;
    }

    public override int GetStringIndexingBase()
    {
      return 1;
    }

    public ServerInfoProvider(SqlServerConnection connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");
      using (var command = connection.CreateCommand()) {
        command.CommandText =
          "select"+
          "  coalesce(serverProperty('EditionID'),"+
          "    case serverProperty('Edition')"+
          "    when 'Desktop Edition' then "+((long)SqlServerEdition.DesktopEdition)+
          "    when 'Express Edition' then "+((long)SqlServerEdition.ExpressEdition)+
          "    when 'Standard Edition' then "+((long)SqlServerEdition.StandardEdition)+
          "    when 'Workgroup Edition' then "+((long)SqlServerEdition.WorkgroupEdition)+
          "    when 'Enterprise Edition' then "+((long)SqlServerEdition.EnterpriseEdition)+
          "    when 'Personal Edition' then "+((long)SqlServerEdition.PersonalEdition)+
          "    when 'Developer Edition' then "+((long)SqlServerEdition.DeveloperEdition)+
          "    when 'Enterprise Evaluation Edition' then "+((long)SqlServerEdition.EnterpriseEvaluationEdition)+
          "    when 'Windows Embedded SQL' then "+((long)SqlServerEdition.WindowsEmbeddedSql)+
          "    when 'Express Edition with Advanced Services' then "+((long)SqlServerEdition.ExpressEditionWithAdvancedServices)+
          "    end"+
          "  ) as EditionID,"+
          "    serverProperty('Edition') as EditionName,"+
          "    serverProperty('ProductVersion') as ProductVersion,"+
          "    serverProperty('ProductLevel') as ProductLevel,"+
          "    serverProperty('EngineEdition') as EngineEdition";
        using (var reader = command.ExecuteReader()) {
          if (reader.Read()) {
            var v = new SqlServerVersionInfo(new Version(reader.GetString(2)));
            long editionId = Convert.ToInt64(reader.GetValue(0));
            if (Enum.IsDefined(typeof(SqlServerEdition), editionId))
              v.Edition = ((SqlServerEdition) Enum.ToObject(typeof (SqlServerEdition), editionId));
            v.EditionName = reader.GetString(1);
            v.ProductLevel = reader.GetString(3);
            int engineEditionId = reader.GetInt32(4);
            if (Enum.IsDefined(typeof(SqlServerEdition), (long)engineEditionId))
              v.EngineEdition = (SqlServerEngineEdition) Enum.ToObject(typeof (SqlServerEngineEdition), engineEditionId);
            versionInfo = v;
          }
          else
            throw new Exception("Unable to obtain version info.");
        }
      }
    }
  }
}