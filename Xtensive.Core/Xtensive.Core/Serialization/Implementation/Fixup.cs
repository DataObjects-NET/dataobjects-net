// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.08

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Serialization.Implementation
{
  /// <summary>
  /// Default <see cref="IFixup"/> implementation.
  /// </summary>
  /// <typeparam name="T">The type of the object to execute the action on.</typeparam>
  [Serializable]
  public class Fixup<T> : IFixup
  {
    /// <summary>
    /// Gets the object to execute the action on.
    /// </summary>
    public T Source { get; private set; }

    /// <inheritdoc/>
    object IFixup.Source { get { return Source; } }

    /// <inheritdoc/>
    public IReference Reference { get; private set; }

    /// <summary>
    /// Gets the action to execute.
    /// </summary>
    public Action<T, IReference> Action { get; private set; }

    /// <inheritdoc/>
    public void Execute() 
    {
      Action.Invoke(Source, Reference);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    /// <param name="reference">The <see cref="Reference"/> property value.</param>
    /// <param name="action">The <see cref="Action"/> property value.</param>
    public Fixup(T source, IReference reference, Action<T, IReference> action) 
    {
      reference.EnsureNotNull();
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      Source = source;
      Reference = reference;
      Action = action;
    }
  }
}