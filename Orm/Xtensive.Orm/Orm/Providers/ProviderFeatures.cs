// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.28

using System;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Enumerates all the features supported by storage providers.
  /// </summary>
  [Flags]
  public enum ProviderFeatures : long
  {
    None = 0,
    DdlBatches = 1L << 0,
    DmlBatches = 1L << 1,
    ClusteredIndexes = 1L << 2,
    Collations = 1L << 3,
    Apply = 1L << 4,
    DeferrableConstraints = 1L << 5,
    ForeignKeyConstraints = 1L << 6,
    FullFeaturedBooleanExpressions = 1L << 7,
    IncludedColumns = 1L << 8,
    KeyColumnSortOrder = 1L << 9,
    LargeObjects = 1L << 10,
    NamedParameters = 1L << 11,
    Sequences = 1L << 12,
    TransactionalDdl = 1L << 13,
    TransactionalFullTextDdl = 1L << 14,
    TreatEmptyBlobAsNull = 1L << 15,
    TreatEmptyStringAsNull = 1L << 16,
    UpdateFrom = 1L << 17,
    Take = 1L << 18,
    Skip = 1L << 19,
    MultipleActiveResultSets = 1L << 20,
    MultipleResultsViaCursorParameters = 1L << 21,
    InsertDefaultValues = 1L << 22,
    UpdateDefaultValues = 1L << 23,
    FullText = 1L << 24,
    FullFeaturedFullText = 1L << 25,
    SingleKeyRankTableFullText = 1L << 26,
    ColumnRename = 1L << 27,
    RowNumber = 1L << 28,
    NativeTake = 1L << 29,
    NativeSkip = 1L << 30,
    Savepoints = 1L << 31,
    ScalarSubqueries = 1L << 32,
    ArbitraryIdentityIncrement = 1L << 33,
    TemporaryTables = 1L << 34,
    TableRename = 1L << 35,
    PartialIndexes = 1L << 36,
    DeleteFrom = 1L << 37,
    Multischema = 1L << 38,
    Multidatabase = 1L << 39,
    PagingRequiresOrderBy = 1L << 40,
    ZeroLimitIsError = 1L << 41,
    TransactionalKeyGenerators = 1L << 42,
    ColumnDrop = 1L << 43,
    ExclusiveWriterConnection = 1L << 44,
    TemporaryTableEmulation = 1L << 45,
    StrictJoinSyntax = 1L << 46,
    SingleConnection = 1L << 47,
    SelfReferencingRowRemovalIsError = 1L << 48,
    FullTextColumnDataTypeSpecification = 1L << 49,
    DateTimeOffset = 1L << 50,
    UpdateLimit = 1L << 52,
    DeleteLimit = 1L << 53,

    // Feature groups
    Paging = Take | Skip,
    NativePaging = NativeTake | NativeSkip,
    Batches = DdlBatches | DmlBatches,

    // Obsolete features
    [Obsolete("Use ProviderFeatures.ExclusiveWriterConnection instead.")]
    SingleSessionAccess = ExclusiveWriterConnection,
  }
}