// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Created:    2007.07.26

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects;
using Xtensive.Core.Links;

namespace Xtensive.Core.Aspects.Helpers
{
  [MulticastAttributeUsage(MulticastTargets.Class)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  [Serializable]
  internal sealed class ImplementLinkOwnerAspect: CompositionAspect
  {
    private static Dictionary<Type, ImplementLinkOwnerAspect> aspects = new Dictionary<Type, ImplementLinkOwnerAspect>();
    private static Dictionary<Type, LinkOwnerImplementation> implementations = new Dictionary<Type, LinkOwnerImplementation>();
    [NonSerialized]
    private LinkOwnerImplementation implementation;
    private Dictionary<FieldInfo, LinkAttribute> links = new Dictionary<FieldInfo, LinkAttribute>();

    public Dictionary<FieldInfo, LinkAttribute> Links
    {
      get { return links; }
    }

    public override CompositionAspectOptions GetOptions()
    {
      return 
        CompositionAspectOptions.GenerateImplementationAccessor | 
        CompositionAspectOptions.IgnoreIfAlreadyImplemented;
    }

    public override object CreateImplementationObject(InstanceBoundLaosEventArgs eventArgs)
    {
      return implementation;
    }

    public override Type GetPublicInterface(Type containerType)
    {
      return typeof (ILinkOwner);
    }

    public override void RuntimeInitialize(Type type)
    {
      lock (implementations) {
        implementation = null;
        implementations.TryGetValue(type, out implementation);
        if (implementation==null) 
        {
          implementation = new LinkOwnerImplementation(type, links, GetBaseImplementation(type));
          implementations.Add(type, implementation);
        }
      }
    }

    public static LinkOwnerImplementation GetBaseImplementation(Type type)
    {
      lock (implementations) {
        Type objectType = typeof (object);
        type = type.BaseType;
        while (type!=objectType) {
          LinkOwnerImplementation implementation = null;
          implementations.TryGetValue(type, out implementation);
          if (implementation != null)
            return implementation;
          type = type.BaseType;
        }
        return null;
      }
    }

    public static ImplementLinkOwnerAspect ApplyOnce(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      return AppliedAspectSet.Add(type, () => new ImplementLinkOwnerAspect());
    }

    
    // Constructors

    private ImplementLinkOwnerAspect()
    {
    }
  }
}