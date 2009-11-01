// Copyright (C) 2003-2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.01

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// Describes an object having <see cref="Identifier"/>.
  /// </summary>
  public interface IIdentified
  {
    /// <summary>
    /// Gets object identifier.
    /// </summary>
    object Identifier { get; }
  }

  /// <summary>
  /// Describes an object having <see cref="Identifier"/>.
  /// </summary>
  public interface IIdentified<T>: IIdentified
    // where T: IEquatable<T>
  {
    /// <summary>
    /// Gets object identifier.
    /// </summary>
    new T Identifier { get; }
  }
}