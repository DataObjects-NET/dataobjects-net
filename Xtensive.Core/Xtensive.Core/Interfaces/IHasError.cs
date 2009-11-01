// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.07.10

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// Describes an object that has <see cref="Error"/> property of <see cref="Exception"/> type.
  /// </summary>
  public interface IHasError
  {
    /// <summary>
    /// Gets an error <see cref="Exception"/>.
    /// </summary>
    Exception Error { get;}
  }
}