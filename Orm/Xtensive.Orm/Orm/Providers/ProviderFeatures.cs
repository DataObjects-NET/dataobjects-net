// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.08.28

using System;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Enumerates all the features supported by storage providers.
  /// </summary>
  [Flags]
  public enum ProviderFeatures : ulong
  {
    None = 0,
    DdlBatches = 1UL << 0,
    DmlBatches = 1UL << 1,
    ClusteredIndexes = 1UL << 2,
    Collations = 1UL << 3,
    Apply = 1UL << 4,
    DeferrableConstraints = 1UL << 5,
    ForeignKeyConstraints = 1UL << 6,
    FullFeaturedBooleanExpressions = 1UL << 7,
    IncludedColumns = 1UL << 8,
    KeyColumnSortOrder = 1UL << 9,
    LargeObjects = 1UL << 10,
    NamedParameters = 1UL << 11,
    Sequences = 1UL << 12,
    TransactionalDdl = 1UL << 13,
    TransactionalFullTextDdl = 1UL << 14,
    TreatEmptyBlobAsNull = 1UL << 15,
    TreatEmptyStringAsNull = 1UL << 16,
    UpdateFrom = 1UL << 17,
    Take = 1UL << 18,
    Skip = 1UL << 19,
    MultipleActiveResultSets = 1UL << 20,
    MultipleResultsViaCursorParameters = 1UL << 21,
    InsertDefaultValues = 1UL << 22,
    UpdateDefaultValues = 1UL << 23,
    FullText = 1UL << 24,
    FullFeaturedFullText = 1UL << 25,
    SingleKeyRankTableFullText = 1UL << 26,
    ColumnRename = 1UL << 27,
    RowNumber = 1UL << 28,
    NativeTake = 1UL << 29,
    NativeSkip = 1UL << 30,
    Savepoints = 1UL << 31,
    ScalarSubqueries = 1UL << 32,
    ArbitraryIdentityIncrement = 1UL << 33,
    TemporaryTables = 1UL << 34,
    TableRename = 1UL << 35,
    PartialIndexes = 1UL << 36,
    DeleteFrom = 1UL << 37,
    Multischema = 1UL << 38,
    Multidatabase = 1UL << 39,
    PagingRequiresOrderBy = 1UL << 40,
    ZeroLimitIsError = 1UL << 41,
    TransactionalKeyGenerators = 1UL << 42,
    ColumnDrop = 1UL << 43,
    ExclusiveWriterConnection = 1UL << 44,
    TemporaryTableEmulation = 1UL << 45,
    StrictJoinSyntax = 1UL << 46,
    SingleConnection = 1UL << 47,
    SelfReferencingRowRemovalIsError = 1UL << 48,
    FullTextColumnDataTypeSpecification = 1UL << 49,
    DateTimeOffset = 1UL << 50,
    AutoIncrementSettingsInMemory = 1UL << 51,
    UpdateLimit = 1UL << 52,
    DeleteLimit = 1UL << 53,
    DateTimeEmulation = 1UL << 54,
    DateTimeOffsetEmulation = 1UL << 55,
    TruncateTable = 1UL << 56,
    UniqueNullTreatment = 1UL << 57,

    // Feature groups
    Paging = Take | Skip,
    NativePaging = NativeTake | NativeSkip,
    Batches = DdlBatches | DmlBatches,
  }
}