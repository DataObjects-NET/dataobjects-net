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
using Xtensive.Integrity.Validation.Interfaces;

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
    public IDisposable OpenInconsistentRegion()
    {
      if (!IsConsistent)
        return null;
      IsConsistent = false;
      if (registry==null)
        registry = new HashSet<Pair<IValidationAware, Action<IValidationAware>>>();
      return 
        new Disposable<ValidationContextBase>(this, (disposing, context) => context.LeaveInconsistentRegion());
    }

    #region Protected methods (to override, if necessary)    

    /// <summary>
    /// Enqueues the object for delayed partial validation.
    /// </summary>
    /// <param name="target">The <see cref="IValidationAware"/> object to enqueue.</param>
    /// <param name="validationDelegate">The validation delegate partially validating the <paramref name="target"/>.
    /// If <see langword="null" />, whole object should be validated.
    /// </param>
    internal protected virtual void EnqueueValidate(IValidationAware target, Action<IValidationAware> validationDelegate)
    {
      if (target.Context!=this)
        throw new ArgumentException(Strings.ExObjectAndContextAreIncompatible, "target");

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
    /// Leaves the inconsistent region.
    /// </summary>
    /// <exception cref="AggregateException">Validation failed.</exception>
    protected virtual void LeaveInconsistentRegion()
    {
      IsConsistent = true;
      if (registry==null)
        return;
      IList<Exception> exceptions = null;
      try {
        foreach (var pair in registry) {
          try {
            if (pair.Second==null)
              pair.First.Validate();
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
        if (exceptions!=null && exceptions.Count > 0)
          throw new AggregateException(Strings.ExValidationFailed, exceptions);
      }
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
