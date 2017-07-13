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
  /// A resolver responsible for name of full-text catalog.
  /// </summary>
  public interface IFullTextCatalogResolver
  {
    /// <summary>
    /// Determines whether resolver is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Resolves full-text catalog name.
    /// </summary>
    /// <param name="typeInfo"><see cref="TypeInfo">Domain model type.</see></param>
    /// <param name="fulltextTable">Table with full-text index.</param>
    /// <returns></returns>
    string Resolve(TypeInfo typeInfo, TableInfo fulltextTable);
  }
}
