// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29
// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Storage.Aspects;

namespace Xtensive.Storage.Aspects
{
  /// <summary>
  /// Implementation of epilogue for constructors.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Method)]
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ImplementConstructorEpilogueAspect : LaosMethodLevelAspect
  {
    private Type baseType;

    public Type BaseType
    {
      get { return baseType; }
    }

    /// <inheritdoc/>
    public override PostSharpRequirements GetPostSharpRequirements()
    {
      PostSharpRequirements requirements = base.GetPostSharpRequirements();
      requirements.PlugIns.Add("Xtensive.Storage.Weaver");
      requirements.Tasks.Add("Xtensive.Storage.Weaver.WeaverFactory");
      return requirements;
    }


    // Constructors

    public ImplementConstructorEpilogueAspect(Type baseType)
    {
      this.baseType = baseType;
    }
  }
}