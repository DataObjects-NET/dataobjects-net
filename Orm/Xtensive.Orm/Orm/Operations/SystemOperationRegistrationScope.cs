// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;

namespace Xtensive.Orm.Operations
{
  public readonly struct SystemOperationRegistrationScope : IDisposable
  {
    private readonly IOperationRegistry registry;
    private readonly Action<IOperationRegistry, bool> onDispose;
    private readonly bool prevState;

    public SystemOperationRegistrationScope(IOperationRegistry registry, bool newState,
      Action<IOperationRegistry, bool> stateChanger)
    {
      this.registry = registry;
      prevState = registry.IsSystemOperationRegistrationEnabled;
      stateChanger(registry, newState);
      this.onDispose = stateChanger;
    }

    public void Dispose()
    {
      if (onDispose is not null)
        onDispose(registry, prevState);
    }
  }
}