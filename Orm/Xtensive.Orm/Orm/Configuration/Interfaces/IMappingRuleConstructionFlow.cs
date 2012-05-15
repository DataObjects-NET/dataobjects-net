// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.08

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// <see cref="MappingRule"/> construction flow.
  /// </summary>
  public interface IMappingRuleConstructionFlow
  {
    /// <summary>
    /// Creates <see cref="MappingRule"/> targeting specified <paramref name="database"/>.
    /// </summary>
    /// <param name="database">Database to map to.</param>
    void ToDatabase(string database);

    /// <summary>
    /// Creates <see cref="MappingRule"/> targeting specified <paramref name="schema"/>.
    /// </summary>
    /// <param name="schema">Schema to map to.</param>
    void ToSchema(string schema);

    /// <summary>
    /// Creates <see cref="MappingRule"/> targeting specified <paramref name="schema"/>
    /// in the specified <paramref name="database"/>.
    /// </summary>
    /// <param name="database">Database to map to.</param>
    /// <param name="schema">Schema to map to.</param>
    void To(string database, string schema);
  }
}