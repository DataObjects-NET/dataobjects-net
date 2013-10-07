// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using Xtensive.Core;

using Xtensive.IoC;

namespace Xtensive.Modelling.Validation
{
  /// <summary>
  /// Model validation scope.
  /// </summary>
  internal sealed class ValidationScope : Scope<ValidationContext>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static ValidationContext CurrentContext {
      get {
        return Scope<ValidationContext>.CurrentContext;
      }
    }
    
    /// <summary>
    /// Gets the associated validation context.
    /// </summary>
    public new ValidationContext Context {
      get {
        return base.Context;
      }
    }
    
    /// <summary>
    /// Opens a validation context and scope.
    /// </summary>
    /// <returns>A new <see cref="ValidationScope"/>, if there is no 
    /// <see cref="ValidationContext.Current"/> validation context;
    /// otherwise, <see langword="null" />.</returns>
    public static ValidationScope Open()
    {
      return CurrentContext==null ? new ValidationContext().Activate() : null;
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="context">The context.</param>
    public ValidationScope(ValidationContext context)
      : base(context)
    {
    }
  }
}