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
  [RequirePostSharp("Xtensive.Core.Weaver", "Xtensive.PlugIn")]
  public class NotSupportedAttribute : MethodLevelAspect
  {
    public string Text { get; private set; }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public NotSupportedAttribute()
    {}

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public NotSupportedAttribute(string text)
    {
      Text = text;
    }
  }
}