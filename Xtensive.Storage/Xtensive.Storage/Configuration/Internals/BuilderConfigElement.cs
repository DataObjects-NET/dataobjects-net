// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Storage.Configuration
{
  internal class BuilderConfigElement : CollectionConfigElementBase
  {
    [ConfigurationProperty("Type", IsRequired = true, IsKey = true)]
    public string Type
    {
      get { return (string)this["Type"]; }
      set { this["Type"] = value; }
    }

    public override object GetKey()
    {
      return Type;
    }
  }
}