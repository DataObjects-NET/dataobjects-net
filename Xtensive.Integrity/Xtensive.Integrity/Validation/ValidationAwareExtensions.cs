// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.30

using System;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Validation
{
  /// <summary>
  /// <see cref="IValidationAware"/> related extensions methods.
  /// </summary>
  public static class ValidationAwareExtensions
  {
    /// <summary>
    /// Partially validates the <paramref name="target"/> with specified delegate, or enqueues it for delayed validation.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    /// If <paramref name="validationDelegate"/> is <see langword="null"/> whole object should be validated.</param>
    /// <param name="mode">Validation mode to use.</param>
    /// <returns><see langword="true"/> if validation was performed immediately; <see langword="false"/> if it was enqueued.</returns>    
    public static bool Validate(this IValidationAware target, Action validationDelegate, ValidationMode mode)
    {      
      return ValidationScope.CurrentContext.Validate(target, validationDelegate, mode);
    }

    /// <summary>
    /// Partially validates the <paramref name="target"/> with specified delegate using default <see cref="ValidationMode"/>.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    /// <param name="validationDelegate">The delegate partially validating object.
    /// If <paramref name="validationDelegate"/> is <see langword="null"/> whole object should be validated.</param>
    /// <returns><see langword="true"/> if validation was performed immediately; <see langword="false"/> if it was enqueued.</returns>
    public static bool Validate(this IValidationAware target, Action validationDelegate)
    {
      return ValidationScope.CurrentContext.Validate(target, validationDelegate);
    }

    /// <summary>
    /// Validates the specified <paramref name="target"/>, or enqueues it for delayed validation.
    /// </summary>
    /// <param name="target">The object to validate.</param>        
    /// <param name="mode">Validation mode to use.</param>
    /// <returns><see langword="true"/> if validation was performed immediately; <see langword="false"/> if it was enqueued.</returns>    
    public static bool Validate(this IValidationAware target, ValidationMode mode)
    {
      return ValidationScope.CurrentContext.Validate(target, mode);      
    }

    /// <summary>
    /// Validates the specified <paramref name="target"/> using default <see cref="ValidationMode"/>.
    /// </summary>
    /// <param name="target">The object to validate.</param>            
    /// <returns><see langword="true"/> if validation was performed immediately; <see langword="false"/> if it was enqueued.</returns>
    public static bool Validate(this IValidationAware target)
    {
      return ValidationScope.CurrentContext.Validate(target);
    }
  }
}