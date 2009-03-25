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
    private readonly Set<Action> actionIndex;
    private readonly List<Action> actionQueue;
    private readonly Context context;
    private readonly ActionProcessor processor;
    private State state;

    #region Nested type: State

    private enum State
    {
      HasPendingActions = 0,
      NoPendingActions = 1,
      IsLocked = 2,
    }

    #endregion

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
      RegisterAction(assembly, string.Empty);
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

      RegisterAction(assembly, @namespace);
    }

    /// <summary>
    /// Determines whether the specified <see cref="Type"/> is contained in this instance.
    /// </summary>
    /// <param name="value"><see cref="Type"/> to search for.</param>
    /// <returns><see langword="True"/> if the <see cref="Type"/> is found; otherwise, <see langword="false"/>.</returns>
    public bool Contains(Type value)
    {
      ProcessPendingActions();
      return context.Contains(value);
    }

    /// <summary>
    /// Registers the specified action for delayed processing.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="namespace">The name space.</param>
    private void RegisterAction(Assembly assembly, string @namespace)
    {
      Action action = new Action(assembly, @namespace);
      // Skipping duplicate registration calls.
      // If we already have a call to the whole assembly we should skip this call
      if (!actionIndex.Contains(new Action(assembly)) && !actionIndex.Contains(action)) {
        actionIndex.Add(action);
        actionQueue.Add(action);
        state = State.HasPendingActions;
      }
    }

    /// <summary>
    /// Processes waiting actions if any.
    /// </summary>
    private void ProcessPendingActions()
    {
      if (state != State.HasPendingActions)
        return;
      foreach (Action action in actionQueue) {
        processor.Process(context, action);
      }
      actionQueue.Clear();
      state = State.NoPendingActions;
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      ProcessPendingActions();
      state = State.IsLocked;
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

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<Type> GetEnumerator()
    {
      ProcessPendingActions();
      return context.GetEnumerator();
    }

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<Type>)this).GetEnumerator();
    }

    #endregion

    #region ICountable<Type> Members

    /// <summary>
    /// Gets the number of types registered in this instance.
    /// </summary>
    public long Count
    {
      get
      {
        ProcessPendingActions();
        return context.Count;
      }
    }

    #endregion

    internal ReadOnlyCollection<Action> Actions
    {
      get { return new ReadOnlyCollection<Action>(actionIndex); }
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Registry"/> class.
    /// </summary>
    /// <param name="processor">The registration call processor.</param>
    internal Registry(ActionProcessor processor)
    {
      ArgumentValidator.EnsureArgumentNotNull(processor, "processor");
      actionIndex = new Set<Action>();
      actionQueue = new List<Action>();
      context = new Context();
      this.processor = processor;
    }

    private Registry(Registry registry)
    {
      actionIndex = new Set<Action>(registry.actionIndex);
      actionQueue = new List<Action>(registry.actionQueue);
      state = registry.state;
      processor = registry.processor;
      context = (Context)registry.context.Clone();
    }
  }
}