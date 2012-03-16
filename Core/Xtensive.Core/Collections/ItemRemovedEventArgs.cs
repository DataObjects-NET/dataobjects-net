// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.13

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections
{
  /// <summary>
  /// Event arguments for <see cref="IExpiringItemCollection{T}.ItemRemoved"/> and <see cref="IPoolBase{T}.ItemRemoved"/> event.
  /// </summary>
  /// <typeparam name="TItem">The type of collection item.</typeparam>
  public class ItemRemovedEventArgs<TItem> : EventArgs
  {
    private readonly TItem item;

    ///<summary>
    /// Gets the expiring item.
    ///</summary>
    public TItem Item
    {
      [DebuggerStepThrough]
      get { return item; }
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="item">The removing item.</param>
    public ItemRemovedEventArgs(TItem item)
    {
      this.item = item;
    }
  }
}