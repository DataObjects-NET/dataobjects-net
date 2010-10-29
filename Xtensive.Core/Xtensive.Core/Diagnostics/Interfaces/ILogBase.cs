// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Common <see cref="ILog"/> and <see cref="IRealLog"/> methods and properties.
  /// </summary>
  public interface ILogBase
  {
    /// <summary>
    /// Gets the name of the log.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the logged text.
    /// </summary>
    string Text { get; }

    /// <summary>
    /// Gets the event types this log logs.
    /// </summary>
    LogEventTypes LoggedEventTypes { get; }

    /// <summary>
    /// Determines whether the specified event type is logged.
    /// </summary>
    /// <param name="eventType">Type of the event to check.</param>
    /// <returns><see langword="True"/> if the specified event type is logged; 
    /// otherwise, <see langword="false"/>.</returns>
    bool IsLogged(LogEventTypes eventType);

    /// <summary>
    /// Updates cached log properties.
    /// </summary>
    void UpdateCachedProperties();
  }
}