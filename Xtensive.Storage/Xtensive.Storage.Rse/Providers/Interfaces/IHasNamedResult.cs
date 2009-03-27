// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.28

using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Returned as service (see <see cref="ExecutableProvider.GetService{T}"/>) 
  /// by <see cref="StoreProvider"/>.
  /// </summary>
  public interface IHasNamedResult
  {
    /// <summary>
    /// Gets the scope of the result.
    /// </summary>
    TemporaryDataScope Scope { get; }

    /// <summary>
    /// Gets the name of the saved result.
    /// </summary>
    string Name { get; }
  }
}