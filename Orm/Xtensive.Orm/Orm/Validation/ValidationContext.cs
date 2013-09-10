// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.05

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using AggregateException = Xtensive.Core.AggregateException;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Provides consistency validation for see <see cref="IValidationAware"/> implementors.
  /// </summary>
  public class ValidationContext
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
    /// <exception cref="InvalidOperationException">Context <see cref="IsConsistent">is invalid</see>.</exception>
    public ICompletableScope DisableValidation()
    {
      if (!IsConsistent)
        return new CompletableScope<bool>(false, (notVoid, completed) => {});
      IsConsistent = false;
      return new CompletableScope<bool>(true, (notVoid, completed) => LeaveInconsistentRegion(completed));
    }

    /// <summary>
    /// Validates all registered instances even if inconsistent region is open.
    /// </summary>
    /// <exception cref="AggregateException">Validation failed.</exception>
    public void Validate()
    {
      var exceptions = GetValidationErrors();
      if (exceptions.Count > 0)
        throw new AggregateException(Strings.ExValidationFailed, exceptions);
    }

    /// <summary>
    /// Validates all registered instances and returns
    /// all exceptions occured during validation.
    /// </summary>
    /// <returns>Exceptions thrown during validation.</returns>
    public IList<Exception> ValidateAndGetErrors()
    {
      return GetValidationErrors();
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
    protected internal virtual void LeaveInconsistentRegion(bool isCompleted)
    {
      IsConsistent = true;
      if (isCompleted)
        Validate();
      // Else do nothing, since an exception must be already thrown
    }

    /// <summary>
    /// Resets the state of this context to initial.
    /// </summary>
    public virtual void Reset()
    {
      registry = null;
      IsConsistent = true;
    }

    #endregion

    private List<Exception> GetValidationErrors()
    {
      var exceptions = new List<Exception>();

      if (registry==null)
        return exceptions;

      HashSet<Pair<IValidationAware, Action<IValidationAware>>> newRegistry = null;

      while (registry!=null) {
        var currentRegistry = registry;
        registry = null;
        foreach (var item in currentRegistry) {
          try {
            if (item.Second==null)
              item.First.OnValidate();
            else if (!currentRegistry.Contains(new Pair<IValidationAware, Action<IValidationAware>>(item.First, null)))
              item.Second.Invoke(item.First);
          }
          catch (Exception e) {
            exceptions.Add(e);
            if (newRegistry==null)
              newRegistry = new HashSet<Pair<IValidationAware, Action<IValidationAware>>>();
            newRegistry.Add(item);
          }
        }
      }

      registry = newRegistry;

      return exceptions;
    }

    // Constructor

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public ValidationContext()
    {
      registry = null;
      IsConsistent = true;
    }
  }
}
