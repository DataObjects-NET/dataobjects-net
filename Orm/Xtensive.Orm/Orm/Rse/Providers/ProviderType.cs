// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Enumerates all supported provider types.
  /// </summary>
  public enum ProviderType
  {
    Index,
    Store,
    Aggregate,
    Alias,
    Calculate,
    Distinct,
    Filter,
    Join,
    PredicateJoin,
    Sort,
    Raw,
    Seek,
    Select,
    Skip,
    Take,
    Apply,
    RowNumber,
    Existence,
    Concat,
    Intersect,
    Except,
    Union,
    Lock,
    Include,
    FreeText,
    Paging
  }
}