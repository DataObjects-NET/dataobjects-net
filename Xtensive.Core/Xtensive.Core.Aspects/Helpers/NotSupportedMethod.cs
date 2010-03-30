// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.10.30

using System;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Aspects.Helpers
{
  [MulticastAttributeUsage(MulticastTargets.Default | MulticastTargets.InstanceConstructor | MulticastTargets.Method | MulticastTargets.StaticConstructor)]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public class NotSupportedMethod : ImplementMethodAspect
  {
    public string Text { get; private set; }

    public override void OnExecution(MethodExecutionArgs eventArgs)
    {
      throw new NotSupportedException(Text);
    }

//    int ILaosWeavableAspect.AspectPriority
//    {
//      get
//      {
//        return int.MinValue;
//      }
//    }

//    public override PostSharpRequirements GetPostSharpRequirements()
//    {
//      PostSharpRequirements requirements = base.GetPostSharpRequirements();
//      AspectHelper.AddStandardRequirements(requirements);
//      return requirements;
//    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public NotSupportedMethod()
    {}

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public NotSupportedMethod(string text)
    {
      Text = text;
    }
  }
}