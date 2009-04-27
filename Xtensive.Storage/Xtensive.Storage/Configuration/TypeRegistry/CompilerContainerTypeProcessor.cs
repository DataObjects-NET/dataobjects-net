// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.27

using System;
using Xtensive.Core.Linq;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Configuration.TypeRegistry
{
  /// <summary>
  /// <see cref="RegistryAction"/> processor for processing compiler container
  /// types registration in 
  /// <see cref="DomainConfiguration.CompilerContainers"/> registry.
  /// </summary>
  [Serializable]
  public sealed class CompilerContainerTypeProcessor : RegistryActionProcessorBase
  {
    /// <inheritdoc/>
    public override Type BaseType 
    {
      get { return null; }
    }

    /// <inheritdoc/>
    protected override void Process(Registry registry, RegistryAction action, Type type)
    {
      if (registry.Contains(type))
        return;
      registry.Register(type);
    }

    /// <inheritdoc/>
    protected override bool IsAcceptable(Registry registry, RegistryAction action, Type type)
    {
      // base.IsAcceptable(action, type);
      var attributes = type.GetAttributes<CompilerContainerAttribute>(
        AttributeSearchOptions.InheritNone);
      return attributes!=null && attributes.Length > 0;
    }
  }
}