// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Storage.Configuration
{
  internal class TypesConfigElement : CollectionConfigElementBase
  {
    [ConfigurationProperty("Assembly", IsRequired = true, IsKey = true)]
    public string Assembly
    {
      get { return (string)this["Assembly"]; }
      set { this["Assembly"] = value; }
    }

    [ConfigurationProperty("Namespace", IsRequired = false, DefaultValue = "", IsKey = true)]
    public string Namespace
    {
      get { return (string)this["Namespace"]; }
      set { this["Namespace"] = value; }
    }

    public override object GetKey()
    {
      return new Pair<string, string>(Assembly, Namespace);
    }
  }
}