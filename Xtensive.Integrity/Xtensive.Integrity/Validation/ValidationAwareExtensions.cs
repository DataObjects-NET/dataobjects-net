// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.30

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
    /// <param name="useContext">If set to <see langword="true"/>, 
    /// <see cref="ValidationContextBase.Validate"/> methods will be used instead of
    /// <see cref="IValidationAware.Validate"/>.</param>
    public static void Validate(this IValidationAware target, bool useContext)
    {
      if (useContext)
        ValidationScope.CurrentContext.Validate(target);
      else
        target.Validate();
    }
  }
}