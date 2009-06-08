// Copyright (C) 2008 Xtensive LLC.
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.30

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Integrity.Aspects;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity.Validation
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
    /// <param name="mode">Validation mode to use.</param>
    /// <returns>
    /// <see langword="true"/> if validation was performed immediately; 
    /// <see langword="false"/> if it was enqueued.
    /// </returns>
    public static bool Validate(this IValidationAware target, Action<IValidationAware> validator, ValidationMode mode)
    {            
      ValidationContextBase context = target.Context;

      bool immediate = mode==ValidationMode.Immediate || context==null || context.IsConsistent;

      if (immediate)
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

      return immediate;
    }

    /// <summary>
    /// Partially validates the <paramref name="target"/> with specified delegate using default <see cref="ValidationMode"/>.
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
      return Validate(target, validator, ValidationMode.Default);
    }

    /// <summary>
    /// Validates the specified <paramref name="target"/>, or enqueues it for delayed validation.
    /// </summary>
    /// <param name="target">The object to validate.</param>        
    /// <param name="mode">Validation mode to use.</param>
    /// <returns>
    /// <see langword="true"/> if validation was performed immediately; 
    /// <see langword="false"/> if it was enqueued.
    /// </returns>
    public static bool Validate(this IValidationAware target, ValidationMode mode)
    {
      return Validate(target, null, mode);
    }

    /// <summary>
    /// Validates the specified <paramref name="target"/> using default <see cref="ValidationMode"/>.
    /// </summary>
    /// <param name="target">The object to validate.</param>            
    /// <returns>
    /// <see langword="true"/> if validation was performed immediately; 
    /// <see langword="false"/> if it was enqueued.
    /// </returns>
    public static bool Validate(this IValidationAware target)
    {
      return Validate(target, null, ValidationMode.Default);
    }

    /// <summary>
    /// Checks all the constraints applied to specified 
    /// <see cref="IValidationAware"/> object.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    public static void CheckConstraints(this IValidationAware target)
    {
      var constraints = ConstraintRegistry.GetConstraints(target.GetType());
      if (constraints.Length > 0)
        using (var aggregator = new ExceptionAggregator())
          foreach (var constraint in constraints)
            aggregator.Execute(constraint.Check, target);
    }
  }
}