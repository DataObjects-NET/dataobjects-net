// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.20

namespace Xtensive.Orm.Upgrade
{
  internal static class MatchingHelper
  {
    public const string WildcardSymbol = "*";

    public static bool ContainsWildcardSymbols(string name)
    {
      return !string.IsNullOrEmpty(name) && name.Contains(WildcardSymbol);
    }

    public static bool IsMatchAll(string name)
    {
      return string.IsNullOrEmpty(name) || name==WildcardSymbol;
    }
  }
}