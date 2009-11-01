// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.15

using System;

namespace Xtensive.Storage.Rse
{
  [Flags]
  public enum ProviderOptions
  {
    Default = 0,
    Indexed = 1,
    Ordered = 2,
    FastCount = 4,
    RandomAccess = 8,
    FastFirst = 16,
  }
}