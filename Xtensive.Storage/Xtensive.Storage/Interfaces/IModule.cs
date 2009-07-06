// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.02

namespace Xtensive.Storage
{
  /// <summary>
  /// The contract of extension module.
  /// </summary>
  public interface IModule
  {
    /// <summary>
    /// Initializes the module.
    /// </summary>
    /// <param name="domain">The domain.</param>
    void OnBuildCompleted(Domain domain);
  }
}