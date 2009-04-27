// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Configuration.TypeRegistry
{
  /// <summary>
  /// <see cref="Type"/> registration endpoint.
  /// </summary>
  [Serializable]
  public sealed class Registry : LockableBase,
    ICountable<Type>,
    ICloneable
  {
    private readonly List<Type> types = new List<Type>();
    private readonly HashSet<Type> typeSet = new HashSet<Type>();
    private readonly List<RegistryAction> actions = new List<RegistryAction>();
    private readonly HashSet<RegistryAction> actionSet = new HashSet<RegistryAction>();
    private readonly IRegistryActionProcessor processor;
    private bool isProcessingPendingActions = false;

    /// <summary>
    /// Determines whether the specified <see cref="Type"/> is contained in this instance.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to search for.</param>
    /// <returns><see langword="True"/> if the <see cref="Type"/> is found; otherwise, <see langword="false"/>.</returns>
    public bool Contains(Type type)
    {
      ProcessPendingActions();
      return typeSet.Contains(type);
    }

    /// <summary>
    /// Registers the specified type.
    /// </summary>
    /// <param name="type">The type to register.</param>
    public void Register(Type type)
    {
      this.EnsureNotLocked();
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      if (!isProcessingPendingActions)
        RegisterAction(new RegistryAction(type));
      else {
        if (typeSet.Contains(type))
          return;
        types.Add(type);
        typeSet.Add(type);
      }
    }

    /// <summary>
    /// Invoke this method to register types from the specified <see cref="Assembly"/>.
    /// Search is restricted by assembly only.
    /// </summary>
    /// <param name="assembly">Assembly to search for types.</param>
    /// <exception cref="InvalidOperationException">When <see cref="Assembly.GetTypes()"/> 
    /// method call has thrown an exception or if no suitable types were found.</exception>
    /// <exception cref="ArgumentNullException">When <paramref name="assembly"/> is null.</exception>
    public void Register(Assembly assembly)
    {
      this.EnsureNotLocked();
      ArgumentValidator.EnsureArgumentNotNull(assembly, "assembly");
      RegisterAction(new RegistryAction(assembly, null));
    }

    /// <summary>
    /// Invoke this method to register types from the specified <see cref="Assembly"/>.
    /// Search is restricted by assembly and namespace.
    /// </summary>
    /// <param name="assembly">Assembly to search for types.</param>
    /// <param name="namespace">Namespace to search for types.</param>
    /// <exception cref="InvalidOperationException">When <see cref="Assembly.GetTypes()"/> 
    /// method call has thrown an exception or if no suitable types were found.</exception>
    /// <exception cref="ArgumentNullException">When <paramref name="assembly"/> is null 
    /// or <paramref name="namespace"/> is empty string.</exception>
    public void Register(Assembly assembly, string @namespace)
    {
      this.EnsureNotLocked();
      ArgumentValidator.EnsureArgumentNotNull(assembly, "assembly");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(@namespace, "@namespace");
      RegisterAction(new RegistryAction(assembly, @namespace));
    }

    /// <summary>
    /// Registers the specified action for delayed processing.
    /// </summary>
    /// <param name="action">The action to register.</param>
    /// <returns><see langword="true" /> if specified action was successfully registered;
    /// otherwise, <see langword="false" />.</returns>
    public bool RegisterAction(RegistryAction action)
    {
      this.EnsureNotLocked();
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      if (actionSet.Contains(action))
        return false;
      actionSet.Add(action);
      actions.Add(action);
      return true;
    }

    private void ProcessPendingActions()
    {
      if (isProcessingPendingActions)
        return;
      isProcessingPendingActions = true;
      try {
        foreach (var action in actions)
          processor.Process(this, action);
        actions.Clear();
      }
      finally {
        isProcessingPendingActions = false;
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      ProcessPendingActions();
    }

    #region ICloneable members

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns></returns>
    public object Clone()
    {
      return new Registry(this);
    }

    #endregion

    #region IEnumerable members

    /// <inheritdoc/>
    public IEnumerator<Type> GetEnumerator()
    {
      ProcessPendingActions();
      return types.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region ICountable<Type> Members

    /// <summary>
    /// Gets the number of types registered in this instance.
    /// </summary>
    public int Count {
      get {
        ProcessPendingActions();
        return types.Count;
      }
    }

    /// <summary>
    /// Gets the number of types registered in this instance.
    /// </summary>
    long ICountable.Count {
      get {
        ProcessPendingActions();
        return types.Count;
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="processor">The registry action processor.</param>
    public Registry(IRegistryActionProcessor processor)
    {
      ArgumentValidator.EnsureArgumentNotNull(processor, "processor");
      this.processor = processor;
    }

    private Registry(Registry registry)
    {
      actions = new List<RegistryAction>(registry.actions);
      actionSet = new HashSet<RegistryAction>(registry.actionSet);
      types = new List<Type>(registry.types);
      typeSet = new HashSet<Type>(registry.typeSet);
      processor = registry.processor;
    }
  }
}