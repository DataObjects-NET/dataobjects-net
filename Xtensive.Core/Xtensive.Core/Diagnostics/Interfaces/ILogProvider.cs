// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Log provider contract.
  /// </summary>
  public interface ILogProvider
  {
    /// <summary>
    /// Gets the <see cref="ILog"/> object by its <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to get the log for.</param>
    /// <returns>The <see cref="ILog"/> object.</returns>
    ILog GetLog(string key);
  }
}