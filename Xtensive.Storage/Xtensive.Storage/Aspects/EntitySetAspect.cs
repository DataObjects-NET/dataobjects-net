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
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Class, AllowMultiple = false, Inheritance = MulticastInheritance.Multicast)]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(InitializableAttribute))]
  internal class EntitySetAspect : Aspect, IAspectProvider
  {
    public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
    {
      var result = new List<AspectInstance>();
      result.Add(new AspectInstance(targetElement, new ImplementConstructor(typeof(Entity), typeof(FieldInfo))));
      result.Add(new AspectInstance(targetElement, new ImplementConstructor(typeof(SerializationInfo), typeof(StreamingContext))));
      result.Add(new AspectInstance(targetElement, new ImplementFactoryMethod(typeof(Entity), typeof(FieldInfo))));
      result.Add(new AspectInstance(targetElement, new ImplementFactoryMethod(typeof(SerializationInfo), typeof(StreamingContext))));
      result.OfType<MulticastAttribute>().ForEach(a => a.AttributeInheritance = MulticastInheritance.None);
      return result;
    }
  }
}