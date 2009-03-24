// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

namespace Xtensive.Storage.Rse.Providers
{
  public enum ProviderType
  {
    Index,
    Reindex,
    Store,
    Aggregate,
    Alias,
    Calculate,
    Distinct,
    Filter,
    Join,
    PredicateJoin,
    Sort,
    Range,
    RangeSet,
    Raw,
    Seek,
    Select,
    Skip,
    Take,
    Transfer,
    Apply,
    RowNumber,
    Existence
  }
}