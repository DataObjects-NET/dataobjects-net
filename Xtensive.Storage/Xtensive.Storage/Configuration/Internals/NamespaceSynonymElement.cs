// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.Configuration;

namespace Xtensive.Storage.Configuration
{
  internal class NamespaceSynonymElement: CollectionConfigElementBase
  {
    [ConfigurationProperty("Namespace", IsRequired = true, IsKey = true)]
    public string Namespace
    {
      get { return (string)this["Namespace"]; }
      set { this["Namespace"] = value; }
    }

    [ConfigurationProperty("Synonym", IsRequired = true, IsKey = false)]
    public string Synonym
    {
      get { return (string)this["Synonym"]; }
      set { this["Synonym"] = value; }
    }

    public override object GetKey()
    {
      return Namespace;
    }
  }
}