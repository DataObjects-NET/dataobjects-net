// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.05

using System;

namespace Xtensive.Indexing
{
  /// <summary>
  /// <see cref="Index{TKey,TItem}"/> features.
  /// </summary>
  [Flags]
  public enum IndexFeatures
  {
    Default = Serialize + Read + Write,
    Read = 0x1,
    Write = 0x2,
    ReadWrite = Read + Write,
    Serialize = 0x4,
    SerializeAndRead = Serialize + Read,
    Differential = 0x8,
  }
}