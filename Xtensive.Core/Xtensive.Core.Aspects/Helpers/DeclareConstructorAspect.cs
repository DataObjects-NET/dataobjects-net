// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.18

using System;
using PostSharp.Laos;

namespace Xtensive.Core.Aspects.Helpers
{
  [Serializable]
  public sealed class DeclareConstructorAspect : LaosTypeLevelAspect,
    ILaosWeavableAspect
  {
    private readonly ConstructorAspect constructorAspect;

    public ConstructorAspect ConstructorAspect
    {
      get { return constructorAspect; }
    }

    int ILaosWeavableAspect.AspectPriority
    {
      get { return (int) ConstructorAspectPriority.Declare; }
    }


    // Constructors

    internal DeclareConstructorAspect(ConstructorAspect constructorAspect)
    {
      this.constructorAspect = constructorAspect;
    }
  }
}