// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.07.10

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// Describes an generic object that has <see cref="Error"/> property.
  /// </summary>
  public interface IHasError<T> : IHasError where T : Exception
  {
    /// <summary>
    /// Gets an error.
    /// </summary>
    new T Error { get;}
  }
}