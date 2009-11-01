// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.05

using System;
using Xtensive.Core;

namespace Xtensive.Integrity.Validation.Interfaces
{
  /// <summary>
  /// Allows to validate the state \ integrity of its implementor.
  /// </summary>
  public interface IValidatable
  {
    /// <summary>
    /// Validates the object state.
    /// </summary>
    /// <remarks>
    /// Throws an exception on validation failure.
    /// </remarks>
    void Validate();
  }
}