// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.07

using System.Reflection;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Mapping rule for persistent types.
  /// </summary>
  public sealed class MappingRule : LockableBase
  {
    private Assembly assembly;
    private string @namespace;
    private string database;
    private string schema;

    /// <summary>
    /// Gets assembly condition.
    /// When type is declared in the specified assembly, this rule is applied.
    /// If this property is set to null value, any assembly matches this rule.
    /// </summary>
    public Assembly Assembly
    {
      get { return assembly; }
      set
      {
        this.EnsureNotLocked();
        assembly = value;
      }
    }

    /// <summary>
    /// Gets namespace condition.
    /// When type has specified namespace or any subnamespace, this rule is applied.
    /// If this property is set to null value, any namespace matches this rule.
    /// </summary>
    public string Namespace
    {
      get { return @namespace; }
      set
      {
        this.EnsureNotLocked();
        @namespace = value;
      }
    }

    /// <summary>
    /// Gets database that is assigned to mapped type when this rule is applied.
    /// If this property is set to null or empty value <see cref="DomainConfiguration.DefaultDatabase"/>
    /// is used instead.
    /// </summary>
    public string Database
    {
      get { return database; }
      set
      {
        this.EnsureNotLocked();
        database = value;
      }
    }

    /// <summary>
    /// Gets schema that is assigned to mapped type when this rule is applied.
    /// If this property is set to null or empty value <see cref="DomainConfiguration.DefaultSchema"/>
    /// is used instead.
    /// </summary>
    public string Schema
    {
      get { return schema; }
      set
      {
        this.EnsureNotLocked();
        schema = value;
      }
    }

    /// <inheritdoc />
    public override string ToString()
    {
      var assemblyPart = Assembly!=null ? Assembly.GetName().Name : "<any assembly>";
      var nsPart = !string.IsNullOrEmpty(Namespace) ? Namespace : "<any namespace>";
      var databasePart = !string.IsNullOrEmpty(Database) ? Database : "<default database>";
      var schemaPart = !string.IsNullOrEmpty(Schema) ? Schema : "<default schema>";
      return string.Format("{0}/{1} -> {2}/{3}", assemblyPart, nsPart, databasePart, schemaPart);
    }

    /// <summary>
    /// Creates a clone of this instance.
    /// </summary>
    /// <returns>Cloned instance.</returns>
    public MappingRule Clone()
    {
      return new MappingRule(assembly, @namespace, database, schema);
    }


    // Constructors

    /// <summary>
    /// Creates new instance of this class.
    /// </summary>
    public MappingRule()
    {
    }

    /// <summary>
    /// Creates new instance of this class.
    /// </summary>
    /// <param name="assembly">Value for <see cref="Assembly"/>.</param>
    /// <param name="namespace">Value for <see cref="Namespace"/>.</param>
    /// <param name="database">Value for <see cref="Database"/>.</param>
    /// <param name="schema">Value for <see cref="Schema"/>.</param>
    public MappingRule(Assembly assembly, string @namespace, string database, string schema)
    {
      Assembly = assembly;
      Namespace = @namespace;
      Database = database;
      Schema = schema;
    }
  }
}