// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.24

using System.Collections.Generic;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// Applies configured mappings to <see cref="Catalog"/>'s or <see cref="Schema"/>'s name to get actual name.
  /// </summary>
  public sealed class SqlNodeActualizer
  {
    private readonly IReadOnlyDictionary<string, string> databaseMapping;
    private readonly IReadOnlyDictionary<string, string> schemaMapping;

    /// <summary>
    /// Gets actual <see cref="Catalog"/>'s name.
    /// </summary>
    /// <param name="catalog"><see cref="Catalog"/> which name should be actualized.</param>
    /// <returns>Actual name.</returns>
    public string Actualize(Catalog catalog)
    {
      return catalog.GetActualDbName(databaseMapping);
    }

    /// <summary>
    ///  Gets actual <see cref="Schema"/>'s name.
    /// </summary>
    /// <param name="schema"><see cref="Schema"/> which name should be actualized.</param>
    /// <returns>Actual name.</returns>
    public string Actualize(Schema schema)
    {
      return schema.GetActualDbName(schemaMapping);
    }

    /// <summary>
    /// Creates instance of the class.
    /// </summary>
    /// <param name="databaseMapping">Database (or Catalog) mappings.</param>
    /// <param name="schemaMapping">Schema mappings.</param>
    internal SqlNodeActualizer(IReadOnlyDictionary<string, string> databaseMapping, IReadOnlyDictionary<string, string> schemaMapping)
    {
      this.databaseMapping = databaseMapping;
      this.schemaMapping = schemaMapping;
    }
  }
}