// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.01.26

using System;

namespace Xtensive.Orm.Linq
{
  [Serializable]
  internal enum MemberType
  {
    Default = Unknown,
    Unknown = 0,
    Primitive,
    Key,
    Structure,
    Entity,
    EntitySet,
    Anonymous,
    Grouping,
    Subquery,
    FullTextMatch,
    Array,
  }
}