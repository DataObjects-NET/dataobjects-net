// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.11

using System;
using System.Diagnostics;

namespace Xtensive.Storage.Manual.Advanced
{
  public static class CustomSqlCompilerStringExtensions
  {
    public static char GetThirdChar(this string source)
    {
      return source[2];
    }

    public static string BuildAddressString(string string1, string string2, string string3)
    {
      return string.Format("{0}, {1}-{2}", string1, string2, string3);
    }
  }
}