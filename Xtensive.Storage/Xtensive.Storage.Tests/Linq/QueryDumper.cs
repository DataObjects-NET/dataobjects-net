// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.27

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [Serializable]
  public static class QueryDumper
  {
    public static void Dump(IEnumerable query)
    {
      foreach (var item in query) {
       Dump(item);
      }
    }

    public static void Dump(object value)
    {
      if (value is IEnumerable)
        Dump((IEnumerable) value);
      Log.Info(value == null ? "NULL" : value.ToString());
    }
  }
}