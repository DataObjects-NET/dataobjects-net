// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.28

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Possible ways of creating automatic key generators.
  /// </summary>
  public enum KeyGeneratorMode
  {
    /// <summary>
    /// One key generator is created for each key type.
    /// For example, Int32-Generator and Int64-Generator.
    /// </summary>
    PerKeyType = 0,

    /// <summary>
    /// One key generator is created for each hierarchy.
    /// For example, Customer-Generator and Order-Generator.
    /// </summary>
    PerHierarchy = 1,

    /// <summary>
    /// Default mode is <see cref="PerKeyType"/>.
    /// </summary>
    Default = PerKeyType,
  }
}