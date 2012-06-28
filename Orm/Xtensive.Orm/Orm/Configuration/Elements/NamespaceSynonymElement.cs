// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Elements
{
  /// <summary>
  /// Namespace synonym configuration element within a configuration file.
  /// </summary>
  public class NamespaceSynonymElement: ConfigurationCollectionElementBase
  {
    private const string NamespaceElementName = "namespace";
    private const string SynonymElementName = "synonym";

    /// <inheritdoc/>
    public override object Identifier
    {
      get { return Namespace; }
    }

    /// <summary>
    /// Gets or sets the namespace the <see cref="Synonym"/> is defined for.
    /// </summary>
    [ConfigurationProperty(NamespaceElementName, IsRequired = true, IsKey = true)]
    public string Namespace
    {
      get { return (string)this[NamespaceElementName]; }
      set { this[NamespaceElementName] = value; }
    }

    /// <summary>
    /// Gets or sets the synonym for the <see cref="Namespace"/>.
    /// </summary>
    [ConfigurationProperty(SynonymElementName, IsRequired = true, IsKey = false)]
    public string Synonym
    {
      get { return (string)this[SynonymElementName]; }
      set { this[SynonymElementName] = value; }
    }
  }
}