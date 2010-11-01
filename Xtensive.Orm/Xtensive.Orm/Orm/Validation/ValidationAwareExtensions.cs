// Copyright (C) 2003-2010 Xtensive LLC.
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.30

using System;
using Xtensive.Core;
using Xtensive.Orm.Validation;
using Xtensive.Orm.Validation.Resources;
using AggregateException = Xtensive.Core.AggregateException;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// <see cref="IValidationAware"/> related extensions methods.
  /// </summary>
  public static class ValidationAwareExtensions
  {
    /// <summary>
    /// Partially validates the <paramref name="target"/> with specified delegate, 
    /// or enqueues it for delayed validation.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    /// <param name="validator">The delegate to invoke for validation. 
    /// If <paramref name="validator"/> is <see langword="null"/>, 
    /// the whole object should be validated.</param>
    /// <param name="immediately"><see langword="true" /> if instance should be immediately validated.</param>
    /// <returns>
    /// <see langword="true"/> if validation was performed immediately; 
    /// <see langword="false"/> if it was enqueued.
    /// </returns>
    public static bool Validate(this IValidationAware target, Action<IValidationAware> validator, bool immediately)
    {            
      ValidationContext context = target.Context;

      bool validateNow = immediately || context==null || context.IsConsistent;

      if (validateNow)
        if (validator==null)
          try {
            target.OnValidate();
          }
          catch (Exception e) {
            throw new AggregateException(Strings.ExValidationFailed, e);
          }
        else
          validator.Invoke(target);
      else
        context.EnqueueValidate(target, validator);

      return validateNow;
    }

    /// <summary>
    /// Partially validates the <paramref name="target"/> with specified delegate.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    /// <param name="validator">The delegate to invoke for validation. 
    /// If <paramref name="validator"/> is <see langword="null"/>, 
    /// the whole object should be validated.</param>
    /// <returns>
    /// <see langword="true"/> if validation was performed immediately; 
    /// <see langword="false"/> if it was enqueued.
    /// </returns>
    public static bool Validate(this IValidationAware target, Action<IValidationAware> validator)
    {
      return Validate(target, validator, false);
    }

    /// <summary>
    /// Validates the specified <paramref name="target"/>, or enqueues it for delayed validation.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    /// <param name="immediately"><see langword="true" /> if instance should be immediately validated.</param>
    /// <returns>
    /// <see langword="true"/> if validation was performed immediately; 
    /// <see langword="false"/> if it was enqueued.
    /// </returns>
    public static bool Validate(this IValidationAware target, bool immediately)
    {
      return Validate(target, null, immediately);
    }

    /// <summary>
    /// Validates the specified <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    /// <returns>
    /// <see langword="true"/> if validation was performed immediately; 
    /// <see langword="false"/> if it was enqueued.
    /// </returns>
    public static bool Validate(this IValidationAware target)
    {
      return Validate(target, null, false);
    }

    /// <summary>
    /// Checks all the constraints applied to specified 
    /// <see cref="IValidationAware"/> object.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    public static void CheckConstraints(this IValidationAware target)
    {
      var constraints = ConstraintRegistry.GetConstraints(target.GetType());
      if (constraints.Length > 0) {
        using (var ea = new ExceptionAggregator()) {
          foreach (var constraint in constraints)
            ea.Execute(constraint.Check, target);
          ea.Complete();
        }
      }
    }

    /// <summary>
    /// Gets the validation error for the property with specified <paramref name="propertyName"/>.
    /// </summary>
    /// <param name="target">The object to validate the property of.</param>
    /// <param name="propertyName">Name of the property to get the error for.</param>
    /// <returns>
    /// An exception, if property validation has failed;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public static Exception GetPropertyValidationError(this IValidationAware target, string propertyName)
    {
      var constraints = ConstraintRegistry.GetConstraints(target.GetType());
      foreach (var constraint in constraints)
        if (constraint.Property.Name==propertyName)
          try {
            constraint.Check(target);
          }
          catch (Exception error) {
            return error;
          }
      return null;
    }
  }
}