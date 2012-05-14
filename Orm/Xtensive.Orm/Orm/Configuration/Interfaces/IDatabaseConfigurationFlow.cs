// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.14

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// <see cref="DatabaseConfiguration"/> construction flow.
  /// </summary>
  public interface IDatabaseConfigurationFlow
  {
    /// <summary>
    /// Sets <see cref="DatabaseConfiguration.Alias"/> for current <see cref="DatabaseConfiguration"/>.
    /// </summary>
    /// <param name="alias">Database alias</param>
    /// <returns>Configuration flow.</returns>
    IDatabaseConfigurationFlow WithAlias(string alias);

    /// <summary>
    /// Sets <see cref="DatabaseConfiguration.TypeIdSeed"/> for current <see cref="DatabaseConfiguration"/>.
    /// </summary>
    /// <param name="value">Type ID seed value.</param>
    /// <returns>Configuration flow.</returns>
    IDatabaseConfigurationFlow WithTypeIdSeed(int value);
  }
}