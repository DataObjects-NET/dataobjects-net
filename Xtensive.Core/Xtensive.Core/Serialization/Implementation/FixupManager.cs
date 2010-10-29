// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.08

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Serialization.Implementation
{
  /// <summary>
  /// <see cref="IFixup"/> manager - a queue of <see cref="IFixup"/>s.
  /// </summary>
  public class FixupManager : IEnumerable<IFixup>
  {
    private readonly Queue<IFixup> queue;

    /// <summary>
    /// Adds new fixup to the queue.
    /// </summary>
    /// <param name="target">Object to execute the <paramref name="action"/> on.</param>
    /// <param name="reference">Reference the object is waiting for.</param>
    /// <param name="action">Action after that object deserialization.</param>
    /// <typeparam name="T">Type of the <paramref name="target"/>.</typeparam>
    public virtual void Add<T>(T target, IReference reference, Action<T, IReference> action) 
    {
      queue.Enqueue(new Fixup<T>(target, reference, action));
    }

    /// <summary>
    /// Executes all the fixups and removes them from itself.
    /// </summary>
    public virtual void Execute()
    {
      while (queue.Count > 0)
        queue.Dequeue().Execute();
    }

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<IFixup> GetEnumerator()
    {
      return queue.GetEnumerator();
    }

    #endregion

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public FixupManager()
      : this (new Queue<IFixup>())
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="queue">The queue to use internally.</param>
    protected FixupManager(Queue<IFixup> queue)
    {
      this.queue = queue;
    }
  }
}