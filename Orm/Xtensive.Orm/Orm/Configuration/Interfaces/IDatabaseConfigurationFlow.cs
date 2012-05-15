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
    /// Sets <see cref="DatabaseConfiguration.RealName"/> for current <see cref="DatabaseConfiguration"/>.
    /// </summary>
    /// <param name="realName">Real name.</param>
    /// <returns>Configuration flow.</returns>
    IDatabaseConfigurationFlow WithRealName(string realName);

    /// <summary>
    /// Sets <see cref="DatabaseConfiguration.MinTypeId"/> for current <see cref="DatabaseConfiguration"/>.
    /// </summary>
    /// <param name="minValue">Minimal type ID value.</param>
    /// <param name="maxValue">Maximal type ID value.</param>
    /// <returns>Configuration flow.</returns>
    IDatabaseConfigurationFlow WithTypeIdRange(int minValue, int maxValue);
  }
}