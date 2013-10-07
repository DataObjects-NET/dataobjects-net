// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Collections.Generic;
using Xtensive.Core;

using Xtensive.IoC;

namespace Xtensive.Modelling.Validation
{
  /// <summary>
  /// Model validation context.
  /// </summary>
  [Serializable]
  internal class ValidationContext : IContext<ValidationScope>
  {
    private readonly HashSet<object> validated = new HashSet<object>();

    /// <summary>
    /// Determines whether the specified target is validated.
    /// </summary>
    /// <param name="target">The target to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified target is validated; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsValidated(object target)
    {
      return IsValidated(target, true);
    }

    /// <summary>
    /// Determines whether the specified target is validated.
    /// </summary>
    /// <param name="target">The target to check.</param>
    /// <param name="markAsValidated">If set to <see langword="true"/>, target will be marked as validated.</param>
    /// <returns>
    /// <see langword="true"/> if the specified target is validated; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsValidated(object target, bool markAsValidated)
    {
      if (validated.Contains(target))
        return true;
      if (markAsValidated)
        validated.Add(target);
      return false;
    }

    /// <summary>
    /// Gets the current validation context.
    /// </summary>
    public static ValidationContext Current {
      get {
        return ValidationScope.CurrentContext;
      }
    }

    #region IContext<...> methods

    /// <inheritdoc/>
    public ValidationScope Activate()
    {
      return new ValidationScope(this);
    }

    /// <inheritdoc/>
    public bool IsActive
    {
      get { return Current==this; }
    }

    /// <inheritdoc/>
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public ValidationContext()
    {
    }
  }
}