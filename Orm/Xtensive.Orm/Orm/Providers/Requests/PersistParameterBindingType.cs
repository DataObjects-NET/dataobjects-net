// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Possible types of <see cref="PersistParameterBinding"/>.
  /// </summary>
  public enum PersistParameterBindingType
  {
    /// <summary>
    /// Indicates that no special handling of parameter is performed.
    /// </summary>
    Regular = 0,
    /// <summary>
    /// Indicates that parameter is a large character object and should be persisted via <see cref="ICharacterLargeObject"/>.
    /// </summary>
    CharacterLob = 1,
    /// <summary>
    /// Indicates that parameter is a large binary object and should be persisted via <see cref="IBinaryLargeObject"/>.
    /// </summary>
    BinaryLob = 2,
  }
}