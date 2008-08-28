// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.28

using Xtensive.Core.Collections;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Returned as service (see <see cref="Provider.GetService{T}"/>) 
  /// by <see cref="SaveProvider"/>.
  /// </summary>
  public interface IProvideNamedResult
  {
    /// <summary>
    /// Gets the result name of context saved data.
    /// </summary>
    /// <returns></returns>
    string GetResultName();
  }
}