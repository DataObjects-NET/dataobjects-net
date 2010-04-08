// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.08

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace Xtensive.Storage.Aspects
{
  [MulticastAttributeUsage(MulticastTargets.Method, AllowMultiple = false, 
    Inheritance = MulticastInheritance.Multicast, 
    TargetMemberAttributes = 
      MulticastAttributes.Instance | 
      MulticastAttributes.UserGenerated | 
      MulticastAttributes.Managed | 
      MulticastAttributes.NonAbstract)]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
  [Serializable]
  public sealed class SessionBoundAspect : MethodLevelAspect,
    IAspectProvider
  {
    public override bool CompileTimeValidate(MethodBase method)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
    {
      throw new NotImplementedException();
    }
  }
}