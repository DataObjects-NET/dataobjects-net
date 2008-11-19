// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.18

using System;
using PostSharp.Extensibility;
using PostSharp.Laos;

namespace Xtensive.Core.Aspects.Helpers
{
  [Serializable]
  public sealed class BuildConstructorAspect : LaosTypeLevelAspect,
    ILaosWeavableAspect
  {
    private readonly ConstructorAspect constructorAspect;

    public ConstructorAspect ConstructorAspect
    {
      get { return constructorAspect; }
    }

    int ILaosWeavableAspect.AspectPriority
    {
      get { return (int)ConstructorAspectPriority.Build; }
    }


    // Constructors

    internal BuildConstructorAspect(ConstructorAspect constructorAspect)
    {
      this.constructorAspect = constructorAspect;
    }
  }
}