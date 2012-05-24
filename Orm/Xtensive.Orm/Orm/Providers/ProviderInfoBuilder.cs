// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using System.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Info;

namespace Xtensive.Orm.Providers
{
  internal static class ProviderInfoBuilder
  {
    public static ProviderInfo Build(string providerName, SqlDriver driver)
    {
      var coreServerInfo = driver.CoreServerInfo;
      var serverInfo = driver.ServerInfo;
      var queryFeatures = serverInfo.Query.Features;
      var serverFeatures = serverInfo.ServerFeatures;
      var indexFeatures = serverInfo.Index.Features;

      var f = ProviderFeatures.None;
      if (queryFeatures.Supports(QueryFeatures.DdlBatches))
        f |= ProviderFeatures.DdlBatches;
      if (queryFeatures.Supports(QueryFeatures.DmlBatches))
        f |= ProviderFeatures.DmlBatches;
      if (indexFeatures.Supports(IndexFeatures.Clustered))
        f |= ProviderFeatures.ClusteredIndexes;
      if (indexFeatures.Supports(IndexFeatures.Filtered))
        f |= ProviderFeatures.PartialIndexes;
      if (serverInfo.Collation!=null)
        f |= ProviderFeatures.Collations;
      if (serverInfo.ForeignKey!=null) {
        f |= ProviderFeatures.ForeignKeyConstraints;
        if (serverInfo.ForeignKey.Features.Supports(ForeignKeyConstraintFeatures.Deferrable))
          f |= ProviderFeatures.DeferrableConstraints;
      }
      if (indexFeatures.Supports(IndexFeatures.NonKeyColumns))
        f |= ProviderFeatures.IncludedColumns;
      if (indexFeatures.Supports(IndexFeatures.SortOrder))
        f |= ProviderFeatures.KeyColumnSortOrder;
      if (serverInfo.Sequence!=null)
        f |= ProviderFeatures.Sequences;
      else {
        if (serverInfo.Identity!=null && serverInfo.Identity.Features.Supports(IdentityFeatures.Increment))
          f |= ProviderFeatures.ArbitraryIdentityIncrement;
      }
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
      if (queryFeatures.Supports(QueryFeatures.DeleteFrom))
        f |= ProviderFeatures.DeleteFrom;
      if (queryFeatures.Supports(QueryFeatures.Limit))
        f |= ProviderFeatures.Take | ProviderFeatures.NativeTake;
      if (queryFeatures.Supports(QueryFeatures.Offset))
        f |= ProviderFeatures.Skip | ProviderFeatures.NativeSkip;
      if (queryFeatures.Supports(QueryFeatures.RowNumber))
        f |= ProviderFeatures.RowNumber | ProviderFeatures.Paging;
      if (serverFeatures.Supports(ServerFeatures.MultipleResultsViaCursorParameters))
        f |= ProviderFeatures.MultipleResultsViaCursorParameters;
      if (coreServerInfo.MultipleActiveResultSets)
        f |= ProviderFeatures.MultipleActiveResultSets;
      if (queryFeatures.Supports(QueryFeatures.InsertDefaultValues))
        f |= ProviderFeatures.InsertDefaultValues;
      if (queryFeatures.Supports(QueryFeatures.UpdateDefaultValues))
        f |= ProviderFeatures.UpdateDefaultValues;
      if (queryFeatures.Supports(QueryFeatures.ScalarSubquery))
        f |= ProviderFeatures.ScalarSubqueries;
      if (queryFeatures.Supports(QueryFeatures.MultischemaQueries))
        f |= ProviderFeatures.Multischema;
      if (queryFeatures.Supports(QueryFeatures.MultidatabaseQueries))
        f |= ProviderFeatures.Multidatabase;
      if (serverFeatures.Supports(ServerFeatures.TransactionalDdl))
        f |= ProviderFeatures.TransactionalDdl;
      if (serverFeatures.Supports(ServerFeatures.TransactionalFullTextDdl))
        f |= ProviderFeatures.TransactionalFullTextDdl;
      if (queryFeatures.Supports(QueryFeatures.PagingRequiresOrderBy))
        f |= ProviderFeatures.PagingRequiresOrderBy;
      if (queryFeatures.Supports(QueryFeatures.ZeroLimitIsError))
        f |= ProviderFeatures.ZeroLimitIsError;
      if (serverFeatures.Supports(ServerFeatures.TransactionalKeyGenerators))
        f |= ProviderFeatures.TransactionalKeyGenerators;
      if (serverInfo.Column.AllowedDdlStatements.Supports(DdlStatements.Drop))
        f |= ProviderFeatures.ColumnDrop | ProviderFeatures.ColumnTypeChange;
      if (serverFeatures.Supports(ServerFeatures.SingleSessionAccess))
        f |= ProviderFeatures.SingleSessionAccess;

      var temporaryTable = serverInfo.TemporaryTable;
      if (temporaryTable!=null && temporaryTable.Features.Supports(TemporaryTableFeatures.Local))
        f |= ProviderFeatures.TemporaryTables;
      else if (serverFeatures.Supports(ServerFeatures.TemporaryTableEmulation))
        f |= ProviderFeatures.TemporaryTableEmulation;

      if (serverInfo.FullTextSearch!=null) {
        f |= ProviderFeatures.FullText;
        if (serverInfo.FullTextSearch.Features==FullTextSearchFeatures.Full)
          f |= ProviderFeatures.FullFeaturedFullText;
        if (serverInfo.FullTextSearch.Features==FullTextSearchFeatures.SingleKeyRankTable)
          f |= ProviderFeatures.SingleKeyRankTableFullText;
      }

      var column = serverInfo.Column;
      if ((column.AllowedDdlStatements & DdlStatements.Alter)==DdlStatements.Alter)
        f |= ProviderFeatures.ColumnRename | ProviderFeatures.ColumnTypeChange;

      var table = serverInfo.Table;
      if ((table.AllowedDdlStatements & DdlStatements.Rename)==DdlStatements.Rename)
        f |= ProviderFeatures.TableRename;

      var dataTypes = serverInfo.DataTypes;
      var binaryTypeInfo = dataTypes.VarBinary ?? dataTypes.VarBinaryMax;
      if (binaryTypeInfo!=null && binaryTypeInfo.Features.Supports(DataTypeFeatures.ZeroLengthValueIsNull))
        f |= ProviderFeatures.TreatEmptyBlobAsNull;
      var stringTypeInfo = dataTypes.VarChar ?? dataTypes.VarCharMax;
      if (stringTypeInfo!=null && stringTypeInfo.Features.Supports(DataTypeFeatures.ZeroLengthValueIsNull))
        f |= ProviderFeatures.TreatEmptyStringAsNull;

      var storageVersion = coreServerInfo.ServerVersion;
      var maxIdentifierLength = new EntityInfo[] {
        serverInfo.Column,
        serverInfo.ForeignKey,
        serverInfo.Index,
        serverInfo.PrimaryKey,
        serverInfo.Sequence,
        serverInfo.Table,
        serverInfo.TemporaryTable,
        serverInfo.UniqueConstraint
      }
        .Select(e => e==null ? int.MaxValue : e.MaxIdentifierLength)
        .Min();

      return new ProviderInfo(
        providerName, storageVersion, f,
        maxIdentifierLength,
        serverInfo.PrimaryKey.ConstantName,
        coreServerInfo.DatabaseName, coreServerInfo.DefaultSchemaName);
    }
  }
}