// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.27

namespace Xtensive.Storage.Configuration.TypeRegistry
{
  /// <summary>
  /// Processes registration actions (<see cref="RegistryAction"/>s) in <see cref="Registry"/>.
  /// </summary>
  public interface IRegistryActionProcessor
  {
    /// <summary>
    /// Processes the specified registration action.
    /// </summary>
    /// <param name="registry">The registry.</param>
    /// <param name="action">The action to process.</param>
    void Process(Registry registry, RegistryAction action);
  }
}