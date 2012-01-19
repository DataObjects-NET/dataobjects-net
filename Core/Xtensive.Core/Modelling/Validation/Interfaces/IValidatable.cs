// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

namespace Xtensive.Modelling.Validation
{
  /// <summary>
  /// "Validatable object" contract.
  /// </summary>
  public interface IValidatable
  {
    /// <summary>
    /// Validates the instance state.
    /// </summary>
    void Validate();
  }
}