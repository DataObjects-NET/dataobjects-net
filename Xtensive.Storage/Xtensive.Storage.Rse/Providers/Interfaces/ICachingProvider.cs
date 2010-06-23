// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.15

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Returned as service (see <see cref="ExecutableProvider.GetService{T}"/>) 
  /// by providers that compute and cache the result before the enumeration.
  /// </summary>
  public interface ICachingProvider
  {
    /// <summary>
    /// Gets a value indicating whether the result of this provider is cached for the specified <see cref="EnumerationContext"/> - 
    /// i.e. its enumeration won't lead to a significant delay needed for the computation in this context.
    /// </summary>
    bool IsResultCached(EnumerationContext context);

    /// <summary>
    /// Ensures the result is cached in the specified <see cref="EnumerationContext"/>.
    /// This method can be called more then once.
    /// </summary>
    void EnsureResultIsCached(EnumerationContext context);
  }
}