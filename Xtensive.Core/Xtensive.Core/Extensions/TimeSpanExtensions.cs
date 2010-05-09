// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.30

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// Various extension methods for <see cref="TimeSpan"/>s.
  /// </summary>
  public static class TimeSpanExtensions
  {
    /// <summary>
    /// Converts the specified <see cref="TimeSpan"/> to string using the specified format string.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The format string.</param>
    /// <returns>Formatted representation of the <paramref name="value"/>.</returns>
    /// <remarks>
    /// Format string can contain any of these placeholders:
    /// <list type="table">
    /// <item><term>{0}</term><description>negative sign, if argument represents a negative <see cref="TimeSpan"/>; <see cref="string.Empty"/>, otherwise.</description></item>
    /// <item><term>{1}</term><description>absolute value of <see cref="TimeSpan.Days"/> property.</description></item>
    /// <item><term>{2}</term><description>absolute value of <see cref="TimeSpan.Hours"/> property.</description></item>
    /// <item><term>{3}</term><description>absolute value of <see cref="TimeSpan.Minutes"/> property.</description></item>
    /// <item><term>{4}</term><description>absolute value of <see cref="TimeSpan.Seconds"/> property.</description></item>
    /// <item><term>{5}</term><description>absolute value of <see cref="TimeSpan.Milliseconds"/> property.</description></item>
    /// </list>
    /// </remarks>
    public static string ToString(this TimeSpan value, string format)
    {
      int days = value.Days;
      int hours = value.Hours;
      int minutes = value.Minutes;
      int seconds = value.Seconds;
      int milliseconds = value.Milliseconds;

      bool negative = false;
      
      if (days < 0) {
        days = -days;
        negative = true;
      }

      if (hours < 0) {
        hours = -hours;
        negative = true;
      }

      if (minutes < 0) {
        minutes = -minutes;
        negative = true;
      }

      if (seconds < 0) {
        seconds = -seconds;
        negative = true;
      }

      if (milliseconds < 0) {
        milliseconds = -milliseconds;
        negative = true;
      }

      return String.Format(format, negative ? "-" : string.Empty,
        days, hours, minutes, seconds, milliseconds);
    }
  }
}