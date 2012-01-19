// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.13

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Class, AllowMultiple = false, Inheritance = MulticastInheritance.Multicast)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(InitializableAttribute))]
#if NET40
  [SecuritySafeCritical]
#endif
  internal class EntityAspect : Aspect, IAspectProvider
  {
    public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
    {
      var result = new List<AspectInstance>();
      result.Add(new AspectInstance(targetElement, new ImplementConstructor(typeof(EntityState))));
      result.Add(new AspectInstance(targetElement, new ImplementConstructor(typeof(Session), typeof(EntityState))));
      result.Add(new AspectInstance(targetElement, new ImplementConstructor(typeof(SerializationInfo), typeof(StreamingContext))));
      result.Add(new AspectInstance(targetElement, new ImplementFactoryMethod(typeof(EntityState))));
      result.Add(new AspectInstance(targetElement, new ImplementFactoryMethod(typeof(Session), typeof(EntityState))));
      result.Add(new AspectInstance(targetElement, new ImplementFactoryMethod(typeof(SerializationInfo), typeof(StreamingContext))));
      result.OfType<MulticastAttribute>().ForEach(a => a.AttributeInheritance = MulticastInheritance.None);
      return result;
    }
  }
}