// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.30

using System;
using System.Collections.Generic;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Validation
{
  /// <summary>
  /// <see cref="IValidationAware"/> related extensions methods.
  /// </summary>
  public static class ValidationAwareExtensions
  {
    /// <summary>
    /// Validates the specified <paramref name="target"/>, or enqueues it for delayed validation.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    /// <param name="regions">The set of regions to validate;
    /// <see langword="null" /> means all the regions.</param>
    /// <param name="mode">Validation mode to use.</param>
    /// <returns>Actually used validation mode.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Invalid <paramref name="mode"/> value.</exception>
    public static ValidationMode Validate(this IValidationAware target, HashSet<string> regions, ValidationMode mode)
    {
      switch (mode) {
      case ValidationMode.Immediate:
        target.OnValidate(regions);
        return mode;
      case ValidationMode.Delayed:
        ValidationScope.CurrentContext.Validate(target);
        return mode;
      case ValidationMode.ImmediateOrDelayed:
        if (ValidationScope.CurrentContext.IsConsistent)
          return target.Validate(regions, ValidationMode.Immediate);
        else
          return target.Validate(regions, ValidationMode.Delayed);
      default:
        throw new ArgumentOutOfRangeException("mode");
      }
    }

    public static ValidationMode Validate(this IValidationAware target)
    {

      return ValidationMode.Default;
    }
  }
}