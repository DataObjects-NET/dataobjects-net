// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.30

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Integrity.Aspects;
using Xtensive.Integrity.Aspects.Internals;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Validation
{
  /// <summary>
  /// <see cref="IValidatable"/> related extensions methods.
  /// </summary>
  public static class ValidatableExtensions
  {
    /// <summary>
    /// Partially validates the <paramref name="target"/> with specified delegate, or enqueues it for delayed validation.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    /// If <paramref name="validationDelegate"/> is <see langword="null"/> whole object should be validated.</param>
    /// <param name="mode">Validation mode to use.</param>
    /// <returns><see langword="true"/> if validation was performed immediately; <see langword="false"/> if it was enqueued.</returns>    
    public static bool Validate(this IValidatable target, Action validationDelegate, ValidationMode mode)
    {            
      ValidationContextBase context = ValidationScope.CurrentContext;

      bool immedate = mode==ValidationMode.Immediate || context==null || context.IsConsistent;

      if (immedate)
        if (validationDelegate==null)
          target.OnValidate();
        else
          validationDelegate.Invoke();      
      else
        context.EnqueueValidate(target, validationDelegate);        

      return immedate;      
    }

    /// <summary>
    /// Partially validates the <paramref name="target"/> with specified delegate using default <see cref="ValidationMode"/>.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    /// <param name="validationDelegate">The delegate partially validating object.
    /// If <paramref name="validationDelegate"/> is <see langword="null"/> whole object should be validated.</param>
    /// <returns><see langword="true"/> if validation was performed immediately; <see langword="false"/> if it was enqueued.</returns>
    public static bool Validate(this IValidatable target, Action validationDelegate)
    {
      return Validate(target, validationDelegate, ValidationMode.Default);
    }

    /// <summary>
    /// Validates the specified <paramref name="target"/>, or enqueues it for delayed validation.
    /// </summary>
    /// <param name="target">The object to validate.</param>        
    /// <param name="mode">Validation mode to use.</param>
    /// <returns><see langword="true"/> if validation was performed immediately; <see langword="false"/> if it was enqueued.</returns>    
    public static bool Validate(this IValidatable target, ValidationMode mode)
    {
      return Validate(target, null, mode);
    }

    /// <summary>
    /// Validates the specified <paramref name="target"/> using default <see cref="ValidationMode"/>.
    /// </summary>
    /// <param name="target">The object to validate.</param>            
    /// <returns><see langword="true"/> if validation was performed immediately; <see langword="false"/> if it was enqueued.</returns>
    public static bool Validate(this IValidatable target)
    {
      return Validate(target, null, ValidationMode.Default);
    }

    /// <summary>
    /// Checks all constraints applied to this <see cref="IValidatable"/> object.
    /// </summary>
    /// <param name="target">The object to validate.</param>
    public static void CheckConstraints(this IValidatable target)
    {
      using (var ea = new ExceptionAggregator())
        foreach (var constraint in ConstraintsRegistry.GetConstraints(target.GetType()))
          ea.Execute(constraint.OnValidate, target);
    }
  }
}