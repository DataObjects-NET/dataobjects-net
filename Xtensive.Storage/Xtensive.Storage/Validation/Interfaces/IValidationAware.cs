// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.05

using Xtensive.IoC;

namespace Xtensive.Storage.Validation
{
  /// <summary>
  /// Implemented by objects supporting validation framework.
  /// </summary>
  public interface IValidationAware : IContextBound<ValidationContextBase>
  {
    /// <summary>
    /// Validates the object state right now - i.e. without any delays.
    /// </summary>
    /// <remarks>
    /// Throws an exception on validation failure.
    /// </remarks>
    void OnValidate();
  }
}