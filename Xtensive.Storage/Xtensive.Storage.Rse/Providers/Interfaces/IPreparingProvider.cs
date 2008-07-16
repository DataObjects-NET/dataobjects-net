// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.15

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Returned as service (see <see cref="Provider.GetService{T}"/>) 
  /// by providers preparing (computing) the result before the enumeration.
  /// </summary>
  public interface IPreparingProvider
  {
    /// <summary>
    /// Gets a value indicating whether this provider is "prepared" - 
    /// i.e. its enumeration won't lead to a significant delay needed for the computation (preparation).
    /// </summary>
    bool IsPrepared { get; }

    /// <summary>
    /// Ensures the provider is prepared.
    /// This method can be called more then once.
    /// </summary>
    void Prepare();
  }
}