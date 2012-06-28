// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.Configuration;

namespace Xtensive.Core
{
  /// <summary>
  /// Abstract base class for a configuration element within a configuration file
  /// that is nested to a collection of similar ones.
  /// </summary>
  public abstract class ConfigurationCollectionElementBase : ConfigurationElement,
    IIdentified
  {
    /// <inheritdoc/>
    public abstract object Identifier { get; }
  }
}