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
  [AttributeUsage(
    AttributeTargets.Property | 
    AttributeTargets.Method | 
    AttributeTargets.Constructor, 
    AllowMultiple = false, Inherited = false)]
  [Serializable]
  public class NotSupportedMethodAspect : MethodLevelAspect
  {
    public string Text { get; private set; }

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
    public NotSupportedMethodAspect()
    {}

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public NotSupportedMethodAspect(string text)
    {
      Text = text;
    }
  }
}