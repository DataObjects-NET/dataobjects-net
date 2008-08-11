// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.Configuration;

namespace Xtensive.Storage.Configuration
{
  internal abstract class ConfigurationCollectionElementBase : ConfigurationElement
  {
    public abstract object GetKey();
  }
}