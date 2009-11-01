// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.24

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization
{
  [Serializable]
  [DebuggerDisplay("Value = {Value}")]
  public struct Reference: IReference
  {
    public readonly long value;
    public static Reference Empty = new Reference(0L);

    /// <inheritdoc/>
    public bool IsEmpty
    {
      get { return value == 0; }
    }

    /// <inheritdoc/>
    public long Value
    {
      get { return value; }
    }

    /// <inheritdoc/>
    public bool IsResolved
    {
      get { return !IsEmpty && Resolve() != null; }
    }

    /// <inheritdoc/>
    public object Resolve()
    {
      return SerializationScope.CurrentContext.ReferenceManager.Resolve(this);
    }

    /// <inheritdoc/>
    public T Resolve<T>()
    {
      return (T)Resolve();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="value">The value.</param>
    public Reference(long value)
    {
      this.value = value;
    }
  }
}