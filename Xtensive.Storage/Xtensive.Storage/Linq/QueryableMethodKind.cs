// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.25

using System;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal enum QueryableMethodKind
  {
    Default = Unknown,
    Unknown = 0,
    Aggregate,
    All,
    Any,
    AsEnumerable,
    AsQueryable,
    Average,
    Cast,
    Concat,
    Contains,
    Count,
    DefaultIfEmpty,
    Distinct,
    ElementAt,
    ElementAtOrDefault,
    Except,
    First,
    FirstOrDefault,
    GroupBy,
    GroupJoin,
    Intersect,
    Join,
    Last,
    LastOrDefault,
    LongCount,
    Max,
    Min,
    OfType,
    OrderBy,
    OrderByDescending,
    Reverse,
    Select,
    SelectMany,
    SequenceEqual,
    Single,
    SingleOrDefault,
    Skip,
    SkipWhile,
    Sum,
    Take,
    TakeWhile,
    ThenBy,
    ThenByDescending,
    ToArray,
    ToList,
    Union,
    Where
  }
}