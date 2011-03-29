// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using System.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Info;

namespace Xtensive.Storage.Providers.Sql
{
  internal static class ProviderInfoBuilder
  {
    public static ProviderInfo Build(SqlDriver driver)
    {
      var csi = driver.CoreServerInfo;
      var si = driver.ServerInfo;
      var queryFeatures = si.Query.Features;
      var serverFeatures = si.ServerFeatures;
      var indexFeatures = si.Index.Features;
      var foreignKeyFeatures = si.ForeignKey.Features;

      var f = ProviderFeatures.None;
      if (queryFeatures.Supports(QueryFeatures.DdlBatches))
        f |= ProviderFeatures.DdlBatches;
      if (queryFeatures.Supports(QueryFeatures.DmlBatches))
        f |= ProviderFeatures.DmlBatches;
      if (indexFeatures.Supports(IndexFeatures.Clustered))
        f |= ProviderFeatures.ClusteredIndexes;
      if (si.Collation!=null)
        f |= ProviderFeatures.Collations;
      if (si.ForeignKey!=null) {
        f |= ProviderFeatures.ForeignKeyConstraints;
        if (foreignKeyFeatures.Supports(ForeignKeyConstraintFeatures.Deferrable))
          f |= ProviderFeatures.DeferrableConstraints;
      }
      if (indexFeatures.Supports(IndexFeatures.NonKeyColumns))
        f |= ProviderFeatures.IncludedColumns;
      if (indexFeatures.Supports(IndexFeatures.SortOrder))
        f |= ProviderFeatures.KeyColumnSortOrder;
      if (si.Sequence!=null)
        f |= ProviderFeatures.Sequences;
      if (queryFeatures.Supports(QueryFeatures.CrossApply))
        f |= ProviderFeatures.Apply;
      if (serverFeatures.Supports(ServerFeatures.LargeObjects))
        f |= ProviderFeatures.LargeObjects;
      if (serverFeatures.Supports(ServerFeatures.Savepoints))
        f |= ProviderFeatures.Savepoints;
      if (queryFeatures.Supports(QueryFeatures.FullBooleanExpressionSupport))
        f |= ProviderFeatures.FullFeaturedBooleanExpressions;
      if (queryFeatures.Supports(QueryFeatures.NamedParameters))
        f |= ProviderFeatures.NamedParameters;
      if (queryFeatures.Supports(QueryFeatures.UpdateFrom))
        f |= ProviderFeatures.UpdateFrom;
      if (queryFeatures.Supports(QueryFeatures.Limit))
        f |= ProviderFeatures.Take | ProviderFeatures.NativeTake;
      if (queryFeatures.Supports(QueryFeatures.Offset))
        f |= ProviderFeatures.Skip | ProviderFeatures.NativeSkip;
      if (queryFeatures.Supports(QueryFeatures.RowNumber))
        f |= ProviderFeatures.RowNumber | ProviderFeatures.Paging;
      if (serverFeatures.Supports(ServerFeatures.MultipleResultsViaCursorParameters))
        f |= ProviderFeatures.MultipleResultsViaCursorParameters;
      if (csi.MultipleActiveResultSets)
        f |= ProviderFeatures.MultipleActiveResultSets;
      if (queryFeatures.Supports(QueryFeatures.DefaultValues))
        f |= ProviderFeatures.InsertDefaultValues;
      if (queryFeatures.Supports(QueryFeatures.ScalarSubquery))
        f |= ProviderFeatures.ScalarSubqueries;

      var tt = si.TemporaryTable;
      if (tt != null)
        f |= ProviderFeatures.TemporaryTables;
      if (si.FullTextSearch != null) {
        if (si.FullTextSearch.Features==FullTextSearchFeatures.Full)
          f |= ProviderFeatures.FullFeaturedFullText;
        if (si.FullTextSearch.Features==FullTextSearchFeatures.SingleKeyRankTable)
          f |= ProviderFeatures.SingleKeyRankTableFullText | ProviderFeatures.FullTextDdlIsNotTransactional;
      }

      var c = si.Column;
      if ((c.AllowedDdlStatements & DdlStatements.Alter) == DdlStatements.Alter)
        f |= ProviderFeatures.ColumnRename;

      var dataTypes = si.DataTypes;
      var binaryTypeInfo = dataTypes.VarBinary ?? dataTypes.VarBinaryMax;
      if (binaryTypeInfo!=null && binaryTypeInfo.Features.Supports(DataTypeFeatures.ZeroLengthValueIsNull))
        f |= ProviderFeatures.TreatEmptyBlobAsNull;
      var stringTypeInfo = dataTypes.VarChar ?? dataTypes.VarCharMax;
      if (stringTypeInfo!=null && stringTypeInfo.Features.Supports(DataTypeFeatures.ZeroLengthValueIsNull))
        f |= ProviderFeatures.TreatEmptyStringAsNull;

      var storageVersion = csi.ServerVersion;
      var maxIdentifierLength = new EntityInfo[] {
          si.Column,
          si.ForeignKey,
          si.Index,
          si.PrimaryKey,
          si.Sequence,
          si.Table,
          si.TemporaryTable,
          si.UniqueConstraint
        }.Select(e => e==null ? int.MaxValue : e.MaxIdentifierLength).Min();
      return new ProviderInfo(storageVersion, f, si.TemporaryTable == null ? TemporaryTableFeatures.None : si.TemporaryTable.Features, maxIdentifierLength);
    }
  }
}