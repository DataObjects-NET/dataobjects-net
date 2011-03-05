// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.03

using Xtensive.Collections;

namespace Xtensive.Core
{
  /// <summary>
  /// Describes an object having <see cref="Extensions"/> property.
  /// </summary>
  public interface IHasExtensions
  {
    /// <summary>
    /// Gets the collection of extensions bound to the current instance.
    /// </summary>
    IExtensionCollection Extensions { get; }
  }
}