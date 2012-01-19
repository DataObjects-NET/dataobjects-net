// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.10.13

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// <see cref="LogEventTypes"/> related extension methods.
  /// </summary>
  public static class LogEventTypesExtensions
  {
    /// <summary>
    /// Converts <see cref="LogEventTypes"/> to short string.
    /// </summary>
    /// <param name="logEventType">Type of event to convert.</param>
    /// <returns>Short representation of <paramref name="logEventType"/>.</returns>
    public static string ToShortString(this LogEventTypes logEventType)
    {
      if ((logEventType & LogEventTypes.FatalError)!=0)
        return "Fatal";
      if ((logEventType & LogEventTypes.Error)!=0)
        return "Error";
      if ((logEventType & LogEventTypes.Warning)!=0)
        return "Warn";
      if ((logEventType & LogEventTypes.Info)!=0)
        return "Info";
      if ((logEventType & LogEventTypes.Debug)!=0)
        return "Debug";
      return "None";
    }
  }
}