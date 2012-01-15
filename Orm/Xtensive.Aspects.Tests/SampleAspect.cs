// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.05.21

using System;
using System.Diagnostics;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;

namespace Xtensive.Aspects.Tests
{
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Method, Inheritance = MulticastInheritance.Multicast, AllowMultiple = false)]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(ReplaceAutoProperty))]
  public sealed class SampleAspect : OnMethodBoundaryAspect
  {
    public override void OnEntry(MethodExecutionArgs args)
    {
      Console.Out.WriteLine("Entry...");
    }
  }
}