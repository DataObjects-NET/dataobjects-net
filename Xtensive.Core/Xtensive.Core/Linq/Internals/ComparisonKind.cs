// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.02

namespace Xtensive.Linq
{
  internal enum ComparisonKind
  {
    Default = 0,
    Equality,
    LikeEndsWith,
    LikeStartsWith,
    ForcedGreaterThan,
    ForcedGreaterThanOrEqual,
    ForcedLessThan,
    ForcedLessThanOrEqual
  }
}