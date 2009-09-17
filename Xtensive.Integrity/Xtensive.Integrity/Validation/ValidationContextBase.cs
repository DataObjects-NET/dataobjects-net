// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.05

using System;
using System.Collections.Generic;
using Xtensive.Core;
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
    private HashSet<Pair<IValidationAware, Action<IValidationAware>>> invalidInstances;

    /// <summary>
    /// Gets the value indicating whether this context is in inconsistent state.
    /// </summary>
    public bool IsConsistent { get; private set; }

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
      if (!IsConsistent)
        return InconsistentRegion.NestedRegionInstance;
      IsConsistent = false;
      return new InconsistentRegion(this);
    }

    /// <summary>
    /// Validates all registered instances even if inconsistent region is open.
    /// </summary>
    /// <exception cref="AggregateException">Validation failed.</exception>
    public void Validate()
    {
      if (registry==null)
        return;
      List<Exception> exceptions = null;
      List<Pair<IValidationAware, Action<IValidationAware>>> invalidItems = null;
      try {
        PerformValidation(registry, out invalidItems, out exceptions);
      }
      finally {
        registry = null;
        AddInvalidItems(invalidItems);
        if (exceptions!=null && exceptions.Count > 0)
          throw new AggregateException(Strings.ExValidationFailed, exceptions);
      }
    }

    private void ValidateInvalidItems()
    {
      if (invalidInstances==null || invalidInstances.Count==0)
        return;
      List<Exception> exceptions = null;
      List<Pair<IValidationAware, Action<IValidationAware>>> invalidItems = null;
      try {
        PerformValidation(invalidInstances, out invalidItems, out exceptions);
      }
      finally {
        registry = null;
        AddInvalidItems(invalidItems);
        if (exceptions!=null && exceptions.Count > 0)
          throw new AggregateException(Strings.ExValidationFailed, exceptions);
      }
    }

    /// <summary>
    /// Validates all registered items, including items which validation already failed.
    /// </summary>
    public void ValidateAll()
    {
      Validate();
      ValidateInvalidItems();
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
      IsConsistent = true;
      if (region.IsCompleted) {
        Validate();
      }
      else {
        if (registry==null)
          return;
        if (invalidInstances==null)
          invalidInstances = new HashSet<Pair<IValidationAware, Action<IValidationAware>>>();
        foreach(var pair in registry)
          invalidInstances.Add(pair);
        registry = null;
      }
    }

    /// <summary>
    /// Resets the state of this context to initial.
    /// </summary>
    protected virtual void Reset()
    {
      registry = null;
      IsConsistent = true;
      invalidInstances = null;
    }

    private void AddInvalidItems(IEnumerable<Pair<IValidationAware, Action<IValidationAware>>> items)
    {
      if (items==null)
        return;
      if (invalidInstances==null)
        invalidInstances = new HashSet<Pair<IValidationAware, Action<IValidationAware>>>();
      foreach (var item in items)
        invalidInstances.Add(item);
    }

    private void PerformValidation(
      IEnumerable<Pair<IValidationAware, Action<IValidationAware>>> itemsToValidate,
      out List<Pair<IValidationAware, Action<IValidationAware>>> invalidItems,
      out List<Exception> exceptions)
    {
      exceptions = null;
      invalidItems = null;
      foreach (var pair in itemsToValidate) {
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
          if (invalidItems==null)
            invalidItems = new List<Pair<IValidationAware, Action<IValidationAware>>>();
          invalidItems.Add(pair);
        }
      }
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
      invalidInstances = null;
    }
  }
}
