// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.05

namespace Xtensive.Indexing
{
  public static class EntireValueTypeExtensions
  {
    public static bool IsInfinity(this EntireValueType valueType)
    {
      return valueType==EntireValueType.PositiveInfinity || valueType==EntireValueType.NegativeInfinity;
    }
  }
}