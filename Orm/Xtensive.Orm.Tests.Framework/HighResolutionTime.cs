// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.11

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// A timer providing much higher resolution rather then default one.
  /// </summary>
  public class HighResolutionTime
  {
    private static DateTime  startTime  = DateTime.MinValue;
    private static Stopwatch startTimer = null;

    // Static methods

    /// <summary>
    /// Gets current date and time (like <see cref="DateTime.Now">DateTime.Now</see>),
    /// but with the higher precision.
    /// </summary>
    // /// <exception cref="SystemTimeChangedException">Thrown if system date \ time has been changed.</exception>
    public static DateTime Now {
      get {
        lock (startTimer) {
          DateTime now = startTime.Add(startTimer.Elapsed);
          // TODO: Implement system date \ time change notification.
          // Exception isn't good here, since:
          // - How to finally recover from it?
          // - When we should stop throwing it?
//          if (now.Subtract(DateTime.Now).Duration().TotalMilliseconds>100)
//            throw new SystemTimeChangedException();
          return now;
        }
      }
    }


    // Type initializer
    
    static HighResolutionTime()
    {
      startTime  = DateTime.Now;
      startTimer = Stopwatch.StartNew();
    }
  }
}