// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.08

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Describes an action which should be executed after complete deserialization of the graph.
  /// </summary>
  /// <typeparam name="T">The type of the object to execute the action on.</typeparam>
  [Serializable]
  public class Fixup<T> : IFixup
  {
    /// <summary>
    /// Gets the object to execute the action on.
    /// </summary>
    public T Source { get; private set; }

    /// <summary>
    /// Gets the reference this fixup action is defined for.
    /// </summary>
    public IReference Reference { get; private set; }

    /// <summary>
    /// Gets the action to execute.
    /// </summary>
    public Action<IReference, T> Action { get; private set; }

    /// <inheritdoc/>
    public void Execute() 
    {
      Action(Reference, Source);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    /// <param name="reference">The <see cref="Reference"/> property value.</param>
    /// <param name="action">The <see cref="Action"/> property value.</param>
    public Fixup(T source, IReference reference, Action<IReference, T> action) 
    {
      reference.EnsureNotNull();
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      Source = source;
      Reference = reference;
      Action = action;
    }
  }
}