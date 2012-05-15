// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.09.22

namespace Xtensive.Orm
{
  /// <summary>
  /// Specifies key generator type to use for a particular hierarchy.
  /// </summary>
  public enum KeyGeneratorKind
  {
    /// <summary>
    /// No key generator is provided for hierarchy.
    /// Each <see cref="Entity"/> should supply unique key values
    /// by calling corresponding constructor.
    /// </summary>
    None = 0,

    /// <summary>
    /// Standard key generator is provided for hierarchy.
    /// <see cref="KeyGeneratorAttribute.Name"/> optionally specifies
    /// generator name. If name is not specified key type name is used.
    /// For example if key has <see cref="int"/> type,
    /// then generator will be named "Int32".
    /// Standard key generators handle only single column keys.
    /// </summary>
    Default = 1,

    /// <summary>
    /// Custom key generator is used.
    /// You should provide <see cref="KeyGeneratorAttribute.Name"/>
    /// as well.
    /// </summary>
    Custom = 2
  }
}