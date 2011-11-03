// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.02

namespace Xtensive.Storage.Commands
{
  /// <summary>
  /// Void type replacement.
  /// Used within particular <see cref="CommandResult{T}"/> instance as its type parameter.
  /// </summary>
  public enum NoResult
  {
    /// <summary>
    /// The only member of this enumeration.
    /// </summary>
    Default = 0,
  }
}