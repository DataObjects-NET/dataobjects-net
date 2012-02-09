// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.07

using System.Configuration;
using Xtensive.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Elements
{
  /// <summary>
  /// Database mapping element within a configuration file.
  /// </summary>
  public class MappingRuleElement : ConfigurationCollectionElementBase
  {
    private const string AssemblyElementName = "assembly";
    private const string NamespaceElementName = "namespace";
    private const string DatabaseElementName = "database";
    private const string SchemaElementName = "schema";

    /// <inheritdoc/>
    public override object Identifier {
      get { return new Pair<string>(Assembly ?? string.Empty, Namespace ?? string.Empty); }
    }

    /// <summary>
    /// <see cref="MappingRule.Assembly" copy="true"/>
    /// </summary>
    [ConfigurationProperty(AssemblyElementName, IsKey = true)]
    public string Assembly
    {
      get { return (string)this[AssemblyElementName]; }
      set { this[AssemblyElementName] = value; }
    }

    /// <summary>
    /// <see cref="MappingRule.Namespace" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NamespaceElementName, IsKey = true)]
    public string Namespace
    {
      get { return (string)this[NamespaceElementName]; }
      set { this[NamespaceElementName] = value; }
    }

    /// <summary>
    /// <see cref="MappingRule.Database" copy="true"/>
    /// </summary>
    [ConfigurationProperty(DatabaseElementName)]
    public string Database
    {
      get { return (string)this[DatabaseElementName]; }
      set { this[DatabaseElementName] = value; }
    }

    /// <summary>
    /// <see cref="MappingRule.Schema" copy="true"/>
    /// </summary>
    [ConfigurationProperty(SchemaElementName)]
    public string Schema
    {
      get { return (string)this[SchemaElementName]; }
      set { this[SchemaElementName] = value; }
    }

    /// <summary>
    /// Converts this instance to corresponding <see cref="MappingRule"/>.
    /// </summary>
    /// <returns>Result of conversion.</returns>
    public MappingRule ToNative()
    {
      var assembly = !string.IsNullOrEmpty(Assembly) ? System.Reflection.Assembly.Load(Assembly) : null;
      return new MappingRule(assembly, Namespace, Database, Schema);
    }
  }
}