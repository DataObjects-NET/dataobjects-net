// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.01

using System;
using System.Diagnostics;
using Xtensive.Collections;

namespace Xtensive.Collections
{
  [Serializable]
  internal class CollectionEventBroker<T>
  {
    public event EventHandler<ChangeNotifierEventArgs> OnChanged;
    public event EventHandler<ChangeNotifierEventArgs> OnChanging;
    public event EventHandler<ChangeNotifierEventArgs> OnCleared;
    public event EventHandler<ChangeNotifierEventArgs> OnClearing;
    public event EventHandler<CollectionChangeNotifierEventArgs<T>> OnValidate;
    public event EventHandler<CollectionChangeNotifierEventArgs<T>> OnInserted;
    public event EventHandler<CollectionChangeNotifierEventArgs<T>> OnInserting;
    public event EventHandler<CollectionChangeNotifierEventArgs<T>> OnItemChanged;
    public event EventHandler<CollectionChangeNotifierEventArgs<T>> OnItemChanging;
    public event EventHandler<CollectionChangeNotifierEventArgs<T>> OnRemoved;
    public event EventHandler<CollectionChangeNotifierEventArgs<T>> OnRemoving;

    [DebuggerStepThrough]
    public void RaiseOnChanged(object sender, ChangeNotifierEventArgs args)
    {
      if (OnChanged!=null)
        OnChanged(sender, args);
    }

    [DebuggerStepThrough]
    public void RaiseOnChanging(object sender, ChangeNotifierEventArgs args)
    {
      if (OnChanging!=null)
        OnChanging(sender, args);
    }

    [DebuggerStepThrough]
    public void RaiseOnCleared(object sender, ChangeNotifierEventArgs args)
    {
      if (OnCleared!=null)
        OnCleared(sender, args);
    }

    [DebuggerStepThrough]
    public void RaiseOnClearing(object sender, ChangeNotifierEventArgs args)
    {
      if (OnClearing!=null)
        OnClearing(sender, args);
    }

    [DebuggerStepThrough]
    public void RaiseOnValidate(object sender, CollectionChangeNotifierEventArgs<T> args)
    {
      if (OnValidate!=null)
        OnValidate(sender, args);
    }

    [DebuggerStepThrough]
    public void RaiseOnInserted(object sender, CollectionChangeNotifierEventArgs<T> args)
    {
      if (OnInserted!=null)
        OnInserted(sender, args);
    }

    [DebuggerStepThrough]
    public void RaiseOnInserting(object sender, CollectionChangeNotifierEventArgs<T> args)
    {
      if (OnInserting!=null)
        OnInserting(sender, args);
    }

    [DebuggerStepThrough]
    public void RaiseOnItemChanged(object sender, CollectionChangeNotifierEventArgs<T> args)
    {
      if (OnItemChanged!=null)
        OnItemChanged(sender, args);
    }

    [DebuggerStepThrough]
    public void RaiseOnItemChanging(object sender, CollectionChangeNotifierEventArgs<T> args)
    {
      if (OnItemChanging!=null)
        OnItemChanging(sender, args);
    }

    [DebuggerStepThrough]
    public void RaiseOnRemoved(object sender, CollectionChangeNotifierEventArgs<T> args)
    {
      if (OnRemoved!=null)
        OnRemoved(sender, args);
    }

    [DebuggerStepThrough]
    public void RaiseOnRemoving(object sender, CollectionChangeNotifierEventArgs<T> args)
    {
      if (OnRemoving!=null)
        OnRemoving(sender, args);
    }
  }
}