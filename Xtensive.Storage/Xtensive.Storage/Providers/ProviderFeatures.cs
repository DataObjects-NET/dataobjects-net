// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.28

using System;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Enumerates all the features supported by storage providers.
  /// </summary>
  [Flags]
  public enum ProviderFeatures : long
  {
    None = 0x0,
    DdlBatches = 0x1,
    DmlBatches = 0x2,
    ClusteredIndexes = 0x4,
    Collations = 0x8,
    Apply = 0x10,
    DeferrableConstraints = 0x20,
    ForeignKeyConstraints = 0x40,
    FullFeaturedBooleanExpressions = 0x80,
    IncludedColumns = 0x100,
    KeyColumnSortOrder = 0x200,
    LargeObjects = 0x400,
    NamedParameters = 0x800,
    Sequences = 0x1000,
    TransactionEnlist = 0x2000,
    TreatEmptyBlobAsNull = 0x4000,
    TreatEmptyStringAsNull = 0x8000,
    UpdateFrom = 0x10000,
    Take = 0x20000,
    Skip = 0x40000,
    MultipleActiveResultSets = 0x80000,
    MultipleResultsViaCursorParameters = 0x100000,
    InsertDefaultValues = 0x200000,
    TemporaryTables = 0x400000,
    FullText = 0x800000,
    FullFeaturedFullText = 0x1000000 | FullText,
    SingleKeyRankTableFullText = 0x2000000 | FullText,
    FullTextDdlIsNotTransactional = 0x4000000 | FullText,
    ColumnRename = 0x8000000,
    RowNumber = 0x10000000,
    NativeTake = 0x20000000,
    NativeSkip = 0x40000000,
    Savepoints = 0x80000000,
    ScalarSubqueries = 0x100000000,
    Paging = Take | Skip,
    NativePaging = NativeTake | NativeSkip,
    Batches = DdlBatches | DmlBatches,
  }
}