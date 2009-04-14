// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.07

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Default <see cref="IComparer{T}"/> implementation.
  /// </summary>
  /// <typeparam name="T">The type of objects to compare.</typeparam>
  public class Comparer<T> : Comparer,
    IComparer<T>
    where T: IModel
  {
    #region IComparer<T> properties

    /// <inheritdoc/>
    public T Source { 
      get { return (T) base.Source; }
    }

    /// <inheritdoc/>
    public T Target { 
      get { return (T) base.Target; }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    public Comparer(T source, T target)
      : base(source, target)
    {
    }
  }
}