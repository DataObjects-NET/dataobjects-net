// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.08

using System;
using System.Collections.Generic;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Queue of <see cref="IFixup"/>s.
  /// </summary>
  public class FixupQueue : Queue<IFixup>
  {
    /// <summary>
    /// Adds new <see cref="IFixup"/> to the queue.
    /// </summary>
    /// <param name="reference">Reference the object is waiting for.</param>
    /// <param name="action">Action after that object deserialization.</param>
    /// <param name="target">Object to execute the <paramref name="action"/> on.</param>
    /// <typeparam name="T">Type of the <paramref name="target"/>.</typeparam>
    public void Enqueue<T>(IReference reference, T target, Action<IReference, T> action) 
    {
      Enqueue(new Fixup<T>(target, reference, action));
    }
  }
}