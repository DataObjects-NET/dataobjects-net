// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.20

using System;

namespace Xtensive.Orm.Logging
{
  internal static class SystemClock
  {
    private static readonly object syncRoot = new object();
    private static int lastTick = -1;
    private static DateTime lastDateTime = DateTime.MinValue;

    public static DateTime Now
    {
      get {
        lock (syncRoot) {
          var tickCount = Environment.TickCount;
          if (lastTick==tickCount)
            return lastDateTime;
          lastTick = tickCount;
          lastDateTime = DateTime.Now;
          return lastDateTime;
        }
      }
    }
  }
}
