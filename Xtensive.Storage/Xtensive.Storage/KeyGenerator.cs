// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.10

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage
{
  ///<summary>
  /// Base key generator class.
  ///</summary>
  public abstract class KeyGenerator
  {
    /// <summary>
    /// Gets the hierarchy this instance serves.
    /// </summary>
    public HierarchyInfo Hierarchy { get; private set; }

    /// <summary>
    /// Gets the <see cref="HandlerAccessor"/> providing other available handlers.
    /// </summary>
    protected internal HandlerAccessor Handlers { get; internal set; }

    /// <summary>
    /// Create the <see cref="Tuple"/> with the unique values in key sequence.
    ///  </summary>
    public abstract Tuple Next();

    /// <summary>
    /// Initializer.
    /// </summary>
    public virtual void Initialize()
    {
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="hierarchy">The hierarchy this instance will serve.</param>
    protected KeyGenerator(HierarchyInfo hierarchy)
    {
      Hierarchy = hierarchy;
    }
  }
}