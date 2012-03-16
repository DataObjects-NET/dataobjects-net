// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.24

using System;

namespace Xtensive.Orm.Linq.Expressions
{
  [Serializable]
  internal enum ResultType
  {
    All,
    First,
    FirstOrDefault,
    Single,
    SingleOrDefault
  }
}