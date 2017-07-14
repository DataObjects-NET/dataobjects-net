// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.06.12


using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade.Model;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// A name builder responsible for the name of the catalog of every full-text index.
  /// </summary>
  public interface IFullTextCatalogNameBuilder
  {
    /// <summary>
    /// Determines whether builder is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Builds full-text catalog name.
    /// </summary>
    /// <param name="typeInfo"><see cref="TypeInfo">Domain model type.</see></param>
    /// <param name="fulltextTable">Table with full-text index.</param>
    /// <returns></returns>
    string Build(TypeInfo typeInfo, TableInfo fulltextTable);
  }
}
