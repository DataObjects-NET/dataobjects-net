// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.24

using System;

namespace Xtensive.Storage.Building.DependencyGraph
{
  [Serializable]
  internal enum EdgeKind
  {
    Default = Reference,
    Reference = 0,
    Aggregation = 1,
    Inheritance = 2,
    Implementation = 3,
  }
}