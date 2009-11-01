// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.09.20

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects;
using Xtensive.Core.Collections;
using Xtensive.Core.Links;
using Xtensive.Core.Notifications;

namespace Xtensive.Core.Aspects.Internals
{
  [MulticastAttributeUsage(MulticastTargets.Class)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ImplementChangeNotifierAspect: CompositionAspect
  {
    private static Dictionary<Type, ImplementChangeNotifierAspect> aspects = new Dictionary<Type, ImplementChangeNotifierAspect>();

    public override CompositionAspectOptions GetOptions()
    {
      return 
        CompositionAspectOptions.GenerateImplementationAccessor | 
        CompositionAspectOptions.IgnoreIfAlreadyImplemented;
    }

    public override object CreateImplementationObject(InstanceBoundLaosEventArgs eventArgs)
    {
      return new ChangeNotifierImplementation(eventArgs.Instance);
    }

    public override Type GetPublicInterface(Type containerType)
    {
      return typeof (IChangeNotifier);
    }

    public override void RuntimeInitialize(Type type)
    {
    }

    internal static ImplementChangeNotifierAspect FindOrCreate(Type type, out bool isNew)
    {
      lock (aspects) {
        ImplementChangeNotifierAspect aspect = Find(type);
        if (aspect == null) {
          aspect = new ImplementChangeNotifierAspect();
          aspects.Add(type, aspect);
          isNew = true;
        }
        else
          isNew = false;
        return aspect;
      }
    }

    private static ImplementChangeNotifierAspect Find(Type type)
    {
      if (type==typeof(object))
        return null;
      ImplementChangeNotifierAspect aspect = null;
      aspects.TryGetValue(type, out aspect);
      if (aspect!=null)
        return aspect;
      else
        return Find(type.BaseType);
    }

    
    // Constructors

    private ImplementChangeNotifierAspect()
    {
    }
  }
}