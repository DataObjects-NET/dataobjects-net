// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.20

using System;

namespace Xtensive.Orm.Building.DependencyGraph
{
  [Serializable]
  internal enum EdgeWeight
  {
    Default = Normal,
    Low = -1,
    Normal = 0,
    High = 1,
  }
}