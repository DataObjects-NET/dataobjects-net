// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.30

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// An abstract task for <see cref="CommandProcessor"/>.
  /// </summary>
  public abstract class SqlTask
  {
    /// <summary>
    /// Processes this command with the specified <see cref="CommandProcessor"/>.
    /// </summary>
    /// <param name="processor">The processor to use.</param>
    /// <returns>Returns a value indicates whether all command parts are fit parameters count restrictions or not.</returns>
    public abstract bool ProcessWith(ISqlTaskProcessor processor);
  }
}