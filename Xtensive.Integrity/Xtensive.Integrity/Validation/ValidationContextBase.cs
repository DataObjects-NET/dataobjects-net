// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.05

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Integrity.Resources;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Validation
{
  /// <summary>
  /// Provides consistency validation for see <see cref="IValidationAware"/> implementors.
  /// </summary>
  public abstract class ValidationContextBase: Context<ValidationScope>
  {
    private bool isConsistent = true;
    private int  activationCount;
    private ISet<IValidationAware> registry;

    /// <inheritdoc/>
    protected override ValidationScope CreateActiveScope()
    {
      return new ValidationScope(this);
    }

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return ValidationScope.CurrentContext == this; }
    }

    /// <summary>
    /// Gets the value indicating whether this context is in inconsistent state.
    /// </summary>
    public bool IsConsistent {
      get {
        return isConsistent;
      }
      private set {
        if (value==isConsistent)
          return;
        isConsistent = value;
        if (value)
          LeaveInconsistentRegion();
        else
          EnterInconsistentRegion();
      }
    }

    /// <summary>
    /// Creates the "inconsistent region" - the code region, in which <see cref="Validate"/> method
    /// should just queue the validation rather then perform it immediately.
    /// </summary>
    /// <returns>
    /// <see cref="IDisposable"/> object, which disposal will identify the end of the region.
    /// <see langowrd="Null"/>, if <see cref="IsConsistent"/> is <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The beginning of the region is the place where this method is called.
    /// </para>
    /// <para>
    /// The end of the region is the place where returned <see cref="IDisposable"/> object is disposed.
    /// The validation of all queued to-validate objects will be performed during disposal.
    /// </para>
    /// </remarks>
    public IDisposable InconsistentRegion()
    {
      if (!IsConsistent)
        return null;
      IsConsistent = false;
      return new Disposable<ValidationContextBase>(this, (disposing, context) => context.IsConsistent = true);
    }

    /// <summary>
    /// Ensures specified <paramref name="target"/> object is valid.
    /// Either right now (if <see cref="IsConsistent"/> is <see langword="true"/>) 
    /// - by calling it <see cref="IValidationAware.Validate"/> method,
    /// or on inconsistent region leave (if <see cref="IsConsistent"/> is <see langword="false"/>) \ context deactivation
    /// - by calling <see cref="EnqueueValidate"/> method.
    /// </summary>
    /// <param name="target">The <see cref="IValidationAware"/> object to ensure the validity of.</param>
    public virtual void Validate(IValidationAware target)
    {
      if (!target.IsCompatible(this))
        throw new ArgumentException(Strings.ExObjectAndContextAreIncompatible, "target");
      if (IsConsistent)
        target.Validate();
      else
        EnqueueValidate(target);
    }

    #region Protected methods (to override, if necessary)

    /// <summary>
    /// Enqueues the object for delayed validation.
    /// </summary>
    /// <param name="target">The <see cref="IValidationAware"/> object to enqueue.</param>
    protected virtual void EnqueueValidate(IValidationAware target)
    {
      registry.Add(target);
    }

    /// <summary>
    /// Enters the inconsistent region.
    /// </summary>
    protected virtual void EnterInconsistentRegion()
    {
      registry = new SetSlim<IValidationAware>();
    }

    /// <summary>
    /// Leaves the inconsistent region.
    /// </summary>
    protected virtual void LeaveInconsistentRegion()
    {
      IList<Exception> exceptions = null;
      try {
        foreach (IValidationAware validatable in registry) {
          try {
            validatable.Validate();
          }
          catch (Exception e) {
            if (exceptions==null)
              exceptions = new List<Exception>();
            exceptions.Add(e);
          }
        }
      }
      finally {
        registry = null;
        if (exceptions!=null && exceptions.Count > 0)
          throw new AggregateException(exceptions);
      }
    }


    /// <summary>
    /// Called on context activation.
    /// </summary>
    /// <param name="scope">The scope activating this context.</param>
    protected internal virtual void OnActivate(ValidationScope scope)
    {
      activationCount++;
    }

    /// <summary>
    /// Called on context deactivation.
    /// </summary>
    /// <param name="scope">The scope deactivating this context.</param>
    protected internal void OnDeactivate(ValidationScope scope)
    {
      activationCount--;
      if (activationCount==0 && !IsConsistent)
        LeaveInconsistentRegion();
    }

    #endregion
  }
}