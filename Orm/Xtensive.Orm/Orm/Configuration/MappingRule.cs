// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.07

using System.Reflection;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Mapping rule for persistent types.
  /// </summary>
  public sealed class MappingRule
  {
    /// <summary>
    /// Gets assembly condition.
    /// When type is declared in the specified assembly, this rule applies.
    /// </summary>
    public Assembly Assembly { get; private set; }

    /// <summary>
    /// Gets namespace condition.
    /// When type has specified namespace or any subnamespace, this rule applies.
    /// </summary>
    public string Namespace { get; private set; }

    /// <summary>
    /// Gets database that is assigned to mapped type when this rule is applied.
    /// </summary>
    public string Database { get; private set; }

    /// <summary>
    /// Gets schema that is assigned to mapped type when this rule is applied.
    /// </summary>
    public string Schema { get; private set; }

    /// <inheritdoc />
    public override string ToString()
    {
      var assembly = Assembly!=null ? Assembly.GetName().Name : "<any assembly>";
      var ns = !string.IsNullOrEmpty(Namespace) ? Namespace : "<any namespace>";
      var database = !string.IsNullOrEmpty(Database) ? Database : "<any database>";
      var schema = !string.IsNullOrEmpty(Schema) ? Schema : "<any schema>";
      return string.Format("{0}/{1} -> {2}/{3}", assembly, ns, database, schema);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="assembly">Value for <see cref="Assembly"/>.</param>
    /// <param name="namespace">Value for <see cref="Namespace"/>.</param>
    /// <param name="database">Value for <see cref="Database"/>.</param>
    /// <param name="schema">Value for <see cref="Schema"/>.</param>
    public MappingRule(Assembly assembly, string @namespace, string database, string schema)
    {
      Assembly = assembly;
      Namespace = @namespace ?? string.Empty;
      Database = database ?? string.Empty;
      Schema = schema ?? string.Empty;
    }
  }
}