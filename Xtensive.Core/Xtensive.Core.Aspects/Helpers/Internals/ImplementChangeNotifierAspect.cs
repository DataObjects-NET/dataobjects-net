// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.09.20

using System;
using System.Collections.Generic;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using Xtensive.Core.Notifications;

namespace Xtensive.Core.Aspects.Helpers.Internals
{
  /// <summary>
  /// Internally applied by <see cref="ChangerAttribute"/>.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Class)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ImplementChangeNotifierAspect: CompositionAspect
  {
    private static Dictionary<Type, ImplementChangeNotifierAspect> aspects = new Dictionary<Type, ImplementChangeNotifierAspect>();
    
    public override object CreateImplementationObject(AspectArgs args)
    {
      return new ChangeNotifier(args.Instance);
    }

    protected override Type[] GetPublicInterfaces(Type targetType)
    {
      return new[] {typeof(IChangeNotifier)};
    }

//    public override CompositionAspectOptions GetOptions()
//    {
//      return 
//        CompositionAspectOptions.GenerateImplementationAccessor | 
//        CompositionAspectOptions.IgnoreIfAlreadyImplemented;
//    }
//
//    public override object CreateImplementationObject(InstanceBoundLaosEventArgs eventArgs)
//    {
//      return new ChangeNotifier(eventArgs.Instance);
//    }
//
//    public override Type GetPublicInterface(Type containerType)
//    {
//      return typeof (IChangeNotifier);
//    }

    public override void RuntimeInitialize(Type type)
    {
    }

    internal static ImplementChangeNotifierAspect ApplyOnce(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      return AppliedAspectSet.Add(type, () => new ImplementChangeNotifierAspect());
    }

    
    // Constructors

    private ImplementChangeNotifierAspect()
    {
    }
  }
}