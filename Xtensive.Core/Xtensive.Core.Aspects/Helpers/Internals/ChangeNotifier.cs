// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.09.20

using System;
using Xtensive.Core.Notifications;

namespace Xtensive.Core.Aspects.Helpers.Internals
{
  /// <summary>
  /// Provides implementation of of <see cref="IChangeNotifier"/>
  /// for <see cref="ImplementChangeNotifierAspect"/>.
  /// </summary>
  public class ChangeNotifier: IChangeNotifier
  {
    private object instance;
    private bool isEnabled = true;
    private EventHandler<ChangeNotifierEventArgs> changing;
    private EventHandler<ChangeNotifierEventArgs> changed;

    public object Instance
    {
      get { return instance; }
    }

    public bool IsEnabled
    {
      get { return isEnabled; }
      set { isEnabled = value; }
    }

    public event EventHandler<ChangeNotifierEventArgs> Changing {
      add    { changing += value; }
      remove { changing -= value; }
    }

    public event EventHandler<ChangeNotifierEventArgs> Changed {
      add    { changed += value; }
      remove { changed -= value; }
    }

    public void OnChanging(ChangeNotifierEventArgs args)
    {
      if (changing!=null && isEnabled)
        changing(instance, args);
    }

    public void OnChanged(ChangeNotifierEventArgs args)
    {
      if (changed!=null && isEnabled)
        changed(instance, args);
    }


    // Constructors

    public ChangeNotifier(object instance)
    {
      ArgumentValidator.EnsureArgumentNotNull(instance, "instance");
      this.instance = instance;
    }
  }
}