// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Common.Mssql.v2005
{
  /// <summary>
  /// Represents a <see cref="ServerInfo"/> provider for MS SQL Server 2005.
  /// </summary>
  public class MssqlServerInfoProvider : v2000.MssqlServerInfoProvider
  {
    /// <summary>
    /// Gets the table info.
    /// </summary>
    /// <returns></returns>
    public override TableInfo TableInfo
    {
      get
      {
        TableInfo tableInfo = base.TableInfo;
        MssqlVersionInfo vi = base.VersionInfo as MssqlVersionInfo;
        if (vi != null && (vi.Edition == MssqlEdition.EnterpriseEdition || vi.Edition == MssqlEdition.DeveloperEdition))
          tableInfo.PartitionMethods = PartitionMethods.List | PartitionMethods.Range | PartitionMethods.Hash;
        return tableInfo;
      }
    }

    /// <summary>
    /// Gets the index info.
    /// </summary>
    /// <returns></returns>
    public override IndexInfo IndexInfo
    {
      get
      {
        IndexInfo indexInfo = base.IndexInfo;
        MssqlVersionInfo vi = base.VersionInfo as MssqlVersionInfo;
        if (vi != null) {
          if (vi.Edition == MssqlEdition.EnterpriseEdition || vi.Edition == MssqlEdition.DeveloperEdition)
            indexInfo.PartitionMethods = PartitionMethods.Range;
          if (vi.Edition == MssqlEdition.EnterpriseEdition || vi.Edition == MssqlEdition.DeveloperEdition ||
              vi.Edition == MssqlEdition.WorkgroupEdition || vi.Edition == MssqlEdition.StandardEdition)
            indexInfo.Features |= IndexFeatures.FullText;
        }
        indexInfo.Features |= IndexFeatures.NonKeyColumns;
        return indexInfo;
      }
    }

    /// <summary>
    /// Gets the referential constraint info.
    /// </summary>
    /// <returns></returns>
    public override ReferenceConstraintInfo ReferentialConstraintInfo
    {
      get
      {
        ReferenceConstraintInfo referenceConstraintInfo = base.ReferentialConstraintInfo;
        referenceConstraintInfo.Actions |= ConstraintActions.SetDefault | ConstraintActions.SetNull;
        return referenceConstraintInfo;
      }
    }

    /// <summary>
    /// Gets the collection of supported data types.
    /// </summary>
    /// <returns></returns>
    public override DataTypeCollection DataTypesInfo
    {
      get
      {
        DataTypeCollection types = base.DataTypesInfo;

        DataTypeFeatures common = DataTypeFeatures.Default | DataTypeFeatures.Nullable | DataTypeFeatures.NonKeyIndexing |
                                  DataTypeFeatures.Grouping | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple;

        types.AnsiVarCharMax =
          new StreamDataTypeInfo(SqlDataType.AnsiVarCharMax, typeof (string), new string[] {"varchar(max)"});
        types.AnsiVarCharMax.Length = new ValueRange<int>(1, 2147483647, 1);
        types.AnsiVarCharMax.Features = common;

        types.VarCharMax =
          new StreamDataTypeInfo(SqlDataType.VarCharMax, typeof (string), new string[] {"nvarchar(max)"});
        types.VarCharMax.Length = new ValueRange<int>(1, 1073741823, 1);
        types.VarCharMax.Features = common;

        types.VarBinaryMax =
          new StreamDataTypeInfo(SqlDataType.VarBinaryMax, typeof (byte[]), new string[] {"varbinary(max)"});
        types.VarBinaryMax.Length = new ValueRange<int>(1, 1073741823, 1);
        types.VarBinaryMax.Features = common;

        types.Xml = new StreamDataTypeInfo(SqlDataType.Xml, typeof (string), new string[] {"xml"});
        types.Xml.Length = new ValueRange<int>(1, 2147483647, 1);
        types.Xml.Features = common ^ DataTypeFeatures.Ordering;

        return types;
      }
    }

    /// <summary>
    /// Gets the supported isolation levels.
    /// </summary>
    /// <returns></returns>
    public override IsolationLevels IsolationLevels
    {
      get
      {
        IsolationLevels levels = base.IsolationLevels;
        levels |= IsolationLevels.Snapshot;
        return levels;
      }
    }

    public MssqlServerInfoProvider(Connection connection)
      : base(connection)
    {
    }

    public MssqlServerInfoProvider(MssqlVersionInfo versionInfo)
      : base(versionInfo)
    {
    }
  }
}