// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.05

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity.Validation
{
  /// <summary>
  /// Provides consistency validation for see <see cref="IValidationAware"/> implementors.
  /// </summary>
  public abstract class ValidationContextBase
  {
    private HashSet<Pair<IValidationAware, Action<IValidationAware>>> registry;

    /// <summary>
    /// Gets the value indicating whether this context is in inconsistent state.
    /// </summary>
    public bool IsConsistent { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this context is valid.
    /// </summary>
    /// <remarks>
    /// A context becomes invalid when validation fails there, or one of its 
    /// inconsistent regions was not completed.
    /// </remarks>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Opens the "inconsistent region" - the code region, in which Validate method
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
    /// <exception cref="InvalidOperationException">Context <see cref="IsValid">is invalid</see>.</exception>
    public InconsistentRegion OpenInconsistentRegion()
    {
      EnsureIsValid();
      if (!IsConsistent)
        return null;
      IsConsistent = false;
      return new InconsistentRegion(this);
    }

    /// <summary>
    /// Validates all registered instances even if inconsistent region is open.
    /// </summary>
    /// <exception cref="AggregateException">Validation failed.</exception>
    public void Validate()
    {
      EnsureIsValid();
      if (registry==null)
        return;
      IList<Exception> exceptions = null;
      try {
        foreach (var pair in registry) {
          try {
            if (pair.Second==null)
              pair.First.OnValidate();
            else
              if (!registry.Contains(new Pair<IValidationAware, Action<IValidationAware>>(pair.First, null)))
                pair.Second.Invoke(pair.First);
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
        if (exceptions!=null && exceptions.Count > 0) {
          IsValid = false;
          throw new AggregateException(Strings.ExValidationFailed, exceptions);
        }
      }
    }

    #region Protected virtual methods (override, if necessary)

    /// <summary>
    /// Enqueues the object for delayed partial validation.
    /// </summary>
    /// <param name="target">The <see cref="IValidationAware"/> object to enqueue.</param>
    /// <param name="validationDelegate">The validation delegate partially validating the <paramref name="target"/>.
    /// If <see langword="null" />, whole object should be validated.
    /// </param>
    protected internal virtual void EnqueueValidate(IValidationAware target, Action<IValidationAware> validationDelegate)
    {
      EnsureIsValid();
      if (target.Context!=this)
        throw new ArgumentException(Strings.ExObjectAndContextAreIncompatible, "target");
      if (registry==null)
        registry = new HashSet<Pair<IValidationAware, Action<IValidationAware>>>();
      registry.Add(new Pair<IValidationAware, Action<IValidationAware>>(target, validationDelegate));
    }

    /// <summary>
    /// Enqueues the object for delayed validation.
    /// </summary>
    /// <param name="target">The <see cref="IValidationAware"/> object to enqueue.</param>
    protected internal void EnqueueValidate(IValidationAware target)
    {
      EnqueueValidate(target, null);
    }

    /// <summary>
    /// Leaves the inconsistent region.
    /// </summary>
    /// <exception cref="AggregateException">Validation failed.</exception>
    protected internal virtual void LeaveInconsistentRegion(InconsistentRegion region)
    {
      if (region.IsCompleted) {
        IsConsistent = true;
        Validate();
      }
      else {
        IsValid = false;
        return;
      }
    }

    /// <summary>
    /// Resets the state of this context to initial.
    /// </summary>
    protected virtual void Reset()
    {
      registry = null;
      IsConsistent = true;
      IsValid = true;
    }

    #endregion

    #region Protected methods

    /// <summary>
    /// Ensures the context is valid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Context <see cref="IsValid">is invalid</see>.</exception>
    protected void EnsureIsValid()
    {
      if (!IsValid)
        throw new InvalidOperationException(Strings.ExValidationContextIsInvalid);
    }

    #endregion


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected ValidationContextBase()
    {
      registry = null;
      IsConsistent = true;
      IsValid = true;
    }
  }
}
