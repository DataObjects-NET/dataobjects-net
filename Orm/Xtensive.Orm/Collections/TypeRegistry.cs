// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using Xtensive.Core;


namespace Xtensive.Collections
{
  /// <summary>
  /// <see cref="Type"/> registration endpoint.
  /// </summary>
  [Serializable]
  public class TypeRegistry : LockableBase,
    IEnumerable<Type>,
    ICloneable
  {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly List<Type> types = new List<Type>();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly HashSet<Type> typeSet = new HashSet<Type>();
    private readonly List<TypeRegistration> actions = new List<TypeRegistration>();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly HashSet<TypeRegistration> actionSet = new HashSet<TypeRegistration>();
    private readonly ITypeRegistrationProcessor processor;
    private bool isProcessingPendingActions = false;
    private readonly Set<Assembly> assemblies = new Set<Assembly>();

    /// <summary>
    /// Gets assemblies containing registered types.
    /// </summary>
    public Set<Assembly> Assemblies{ get { return assemblies; } }

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
        Register(new TypeRegistration(type));
      else {
        if (typeSet.Contains(type))
          return;
        types.Add(type);
        typeSet.Add(type);
        assemblies.Add(type.Assembly);
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
      Register(new TypeRegistration(assembly));
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
      Register(new TypeRegistration(assembly, @namespace));
    }

    /// <summary>
    /// Registers the specified <see cref="TypeRegistration"/> for delayed processing.
    /// </summary>
    /// <param name="action">The type registration to register.</param>
    /// <returns><see langword="true" /> if specified registration was successfully added;
    /// otherwise, <see langword="false" />.</returns>
    public bool Register(TypeRegistration action)
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
      if (IsLocked)
        return;
      isProcessingPendingActions = true;
      try {
        while (true) {
          var oldActions = actions.ToList();
          actions.Clear();
          foreach (var action in oldActions)
            processor.Process(this, action);
          if (actions.Count==0)
            break;
        }
      }
      finally {
        isProcessingPendingActions = false;
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      ProcessPendingActions();
      assemblies.Lock(true);
      base.Lock(recursive);
    }

    #region ICloneable members

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns></returns>
    public virtual object Clone()
    {
      return new TypeRegistry(this);
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

    /// <summary>
    /// Gets the number of types registered in this instance.
    /// </summary>
    public int Count {
      get {
        ProcessPendingActions();
        return types.Count;
      }
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="processor">The registry action processor.</param>
    public TypeRegistry(ITypeRegistrationProcessor processor)
    {
      ArgumentValidator.EnsureArgumentNotNull(processor, "processor");
      this.processor = processor;
    }

    /// <summary>
    /// This constructor is used to clone the instance.
    /// </summary>
    /// <param name="source">The type registry to clone the state of.</param>
    protected TypeRegistry(TypeRegistry source)
    {
      ArgumentValidator.EnsureArgumentIs(source, GetType(), "source");
      actions = new List<TypeRegistration>(source.actions);
      actionSet = new HashSet<TypeRegistration>(source.actionSet);
      types = new List<Type>(source.types);
      typeSet = new HashSet<Type>(source.typeSet);
      processor = source.processor;
      assemblies = new Set<Assembly>(source.assemblies);
    }
  }
}