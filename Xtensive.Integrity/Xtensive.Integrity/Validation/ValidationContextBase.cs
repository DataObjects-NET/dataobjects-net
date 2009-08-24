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

    public bool IsRegionCompleted { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this context is invalid.
    /// </summary>
    /// <remarks>
    /// Usually context becomes invalid when validation fails or inconsistent region is not completed.
    /// </remarks>
    public bool IsInvalid { get; private set; }

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
    /// <exception cref="InvalidOperationException">Context <see cref="IsInvalid">is invalid</see>.</exception>
    public InconsistentRegionBase OpenInconsistentRegion()
    {
      EnsureContextIsValid();
      if (!IsConsistent)
        return null;
      IsConsistent = false;
      IsRegionCompleted = false;
      return CreateInconsistentRegion();
    }

    /// <summary>
    /// Creates the inconsistent region.
    /// </summary>
    protected virtual InconsistentRegionBase CreateInconsistentRegion()
    {
      return new InconsistentRegionBase(this);
    }

    /// <summary>
    /// Completes the inconsistent region.
    /// </summary>
    protected internal void CompleteInconsistentRegion()
    {
      IsRegionCompleted = true;
    }

    #region Protected methods (to override, if necessary)

    /// <summary>
    /// Ensures the context is valid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Context <see cref="IsInvalid">is invalid</see>.</exception>
    protected virtual void EnsureContextIsValid()
    {
      if (IsInvalid)
        throw new InvalidOperationException(Strings.ExValidationContextIsInvalid);
    }

    /// <summary>
    /// Enqueues the object for delayed partial validation.
    /// </summary>
    /// <param name="target">The <see cref="IValidationAware"/> object to enqueue.</param>
    /// <param name="validationDelegate">The validation delegate partially validating the <paramref name="target"/>.
    /// If <see langword="null" />, whole object should be validated.
    /// </param>
    internal protected virtual void EnqueueValidate(IValidationAware target, Action<IValidationAware> validationDelegate)
    {
      EnsureContextIsValid();
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
    internal protected virtual void EnqueueValidate(IValidationAware target)
    {
      EnqueueValidate(target, null);
    }

    /// <summary>
    /// Validates all registered instances even if inconsistent region is open.
    /// </summary>
    /// <exception cref="AggregateException">Validation failed.</exception>
    public void ValidateAll()
    {
      EnsureContextIsValid();
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
          IsInvalid = true;
          throw new AggregateException(Strings.ExValidationFailed, exceptions);
        }
      }
    }

    /// <summary>
    /// Leaves the inconsistent region.
    /// </summary>
    /// <exception cref="AggregateException">Validation failed.</exception>
    internal virtual void LeaveInconsistentRegion()
    {
      if (!IsRegionCompleted) {
        IsInvalid = true;
        return;
      }
      IsConsistent = true;
      ValidateAll();
    }

    #endregion


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected ValidationContextBase()
    {
      IsConsistent = true;
    }
  }
}
