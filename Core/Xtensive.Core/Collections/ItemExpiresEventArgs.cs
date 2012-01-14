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
  /// Event arguments for <see cref="IExpiringItemCollection{T}.ItemExpires"/> event.
  /// </summary>
  /// <typeparam name="TItem">The type of collection item.</typeparam>
  public class ItemExpiresEventArgs<TItem> : EventArgs
  {
    private readonly TItem item;
    private bool cancel;

    /// <summary>
    /// <see langword="True"/>, if item must be kept in collection; 
    /// otherwise, <see langword="false"/>.
    /// </summary>
    public bool Cancel
    {
      [DebuggerStepThrough]
      get { return cancel; }
      [DebuggerStepThrough]
      set { cancel = value; }
    }

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
    /// <param name="item">The expiring item.</param>
    public ItemExpiresEventArgs(TItem item)
    {
      this.item = item;
    }
  }
}