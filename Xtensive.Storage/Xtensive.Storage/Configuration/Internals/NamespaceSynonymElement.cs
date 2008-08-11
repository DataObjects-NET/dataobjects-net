// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.Configuration;

namespace Xtensive.Storage.Configuration
{
  internal class NamespaceSynonymElement: ConfigurationCollectionElementBase
  {
    private const string NamespaceElementName = "namespace";
    private const string SynonymElementName = "synonym";

    [ConfigurationProperty(NamespaceElementName, IsRequired = true, IsKey = true)]
    public string Namespace
    {
      get { return (string)this[NamespaceElementName]; }
      set { this[NamespaceElementName] = value; }
    }

    [ConfigurationProperty(SynonymElementName, IsRequired = true, IsKey = false)]
    public string Synonym
    {
      get { return (string)this[SynonymElementName]; }
      set { this[SynonymElementName] = value; }
    }

    public override object GetKey()
    {
      return Namespace;
    }
  }
}