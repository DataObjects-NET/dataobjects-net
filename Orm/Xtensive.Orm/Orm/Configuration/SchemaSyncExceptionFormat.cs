// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.04

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Format of schema difference in <see cref="SchemaSynchronizationException"/>.
  /// </summary>
  public enum SchemaSyncExceptionFormat
  {
    /// <summary>
    /// Detailed format, all relevant information is shown.
    /// </summary>
    Detailed = 0,

    /// <summary>
    /// Brief format, full schema difference is omitted.
    /// This mode emulates old exception format from DataObjects.Net prior to 4.5
    /// </summary>
    Brief = 1,

    /// <summary>
    /// Default format is <see cref="Detailed"/>.
    /// </summary>
    Default = Detailed,
  }
}