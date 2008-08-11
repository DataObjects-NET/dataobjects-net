// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Storage.Configuration
{
  internal class TypeElement : ConfigurationCollectionElementBase
  {
    private const string AssemblyElementName = "assembly";
    private const string NamespaceElementName = "namespace";

    [ConfigurationProperty(AssemblyElementName, IsRequired = true, IsKey = true)]
    public string Assembly
    {
      get { return (string)this[AssemblyElementName]; }
      set { this[AssemblyElementName] = value; }
    }

    [ConfigurationProperty(NamespaceElementName, IsRequired = false, DefaultValue = "", IsKey = true)]
    public string Namespace
    {
      get { return (string)this[NamespaceElementName]; }
      set { this[NamespaceElementName] = value; }
    }

    public override object GetKey()
    {
      return new Pair<string, string>(Assembly, Namespace);
    }
  }
}