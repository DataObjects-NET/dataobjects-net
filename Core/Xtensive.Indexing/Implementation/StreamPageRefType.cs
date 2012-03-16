// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.05

namespace Xtensive.Indexing.Implementation
{
  /// <summary>
  /// <see cref="StreamPageRef{TKey, TValue}"/> types.
  /// </summary>
  public enum StreamPageRefType
  {
    Normal = 0,
    Null = -1,
    Descriptor = -2,
    Undefined = int.MinValue,
  }
}