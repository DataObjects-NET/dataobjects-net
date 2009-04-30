// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.01.26

using System;

namespace Xtensive.Storage.Linq
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
    Array,
  }
}