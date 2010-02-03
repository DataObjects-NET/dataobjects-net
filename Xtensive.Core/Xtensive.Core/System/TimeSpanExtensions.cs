// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.30

namespace System
{
  /// <summary>
  /// Various extension methods for <see cref="TimeSpan"/>s.
  /// </summary>
  public static class TimeSpanExtensions
  {
    /// <summary>
    /// Converts the specified <see cref="TimeSpan"/> to string using the specified string.
    /// Format string can contain any of these placeholders:
    /// {0} - negative sign, if argument represents a negative <see cref="TimeSpan"/>; <see cref="string.Empty"/>, otherwise.
    /// {1} - absolute value of <see cref="TimeSpan.Days"/> property.
    /// {2} - absolute value of <see cref="TimeSpan.Hours"/> property.
    /// {3} - absolute value of <see cref="TimeSpan.Minutes"/> property.
    /// {4} - absolute value of <see cref="TimeSpan.Seconds"/> property.
    /// {5} - absolute value of <see cref="TimeSpan.Milliseconds"/> property.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The format.</param>
    /// <returns>A string representation of <paramref name="value"/>.</returns>
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