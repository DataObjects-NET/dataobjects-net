// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.23

using System;

namespace Xtensive.Storage.Model
{
  [Flags]
  public enum NestingLevel
  {
    Default = 1,

    Root = Default,

    Nested = 2,

    All = Root | Nested,
  }
}