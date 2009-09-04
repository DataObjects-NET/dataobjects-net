// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.28

using System;

namespace Xtensive.Storage.Providers
{
  [Flags]
  public enum ProviderFeatures
  {
    None = 0x0,
    Batches = 0x1,
    ClusteredIndexes = 0x2,
    Collations = 0x4,
    CrossApply = 0x8,
    DeferrableConstraints = 0x10,
    ForeignKeyConstraints = 0x20,
    FullFledgedBooleanExpressions = 0x40, // This option is named in honor of Denis Krjuchkov :)
    IncludedColumns = 0x80,
    KeyColumnSortOrder = 0x100,
    LargeObjects = 0x200,
    NamedParameters = 0x400,
    Sequences = 0x1000,
    TransactionEnlist = 0x2000,
    TreatEmptyBlobAsNull = 0x4000,
    TreatEmptyStringAsNull = 0x8000,
    UpdateFrom = 0x10000,
    Limit = 0x20000,
    Offset = 0x40000,
    Paging = Limit | Offset,
  }
}