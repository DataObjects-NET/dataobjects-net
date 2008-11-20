// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.20

using System;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;

namespace Xtensive.Core.Aspects.Helpers.Internals
{
  [Serializable]
  public sealed class ImplementProtectedConstructorAccessorAspect : LaosTypeLevelAspect,
    ILaosWeavableAspect
  {
    private readonly ProtectedConstructorAccessorAspect protectedConstructorAccessorAspect;

    public ProtectedConstructorAccessorAspect Aspect
    {
      get { return protectedConstructorAccessorAspect; }
    }

    int ILaosWeavableAspect.AspectPriority
    {
      get { return (int)ProtectedConstructorAspectPriority.ImplementBody; }
    }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(Type type)
    {
      ConstructorInfo constructor;
      return AspectHelper.ValidateConstructor(this, SeverityType.Error,
        type.UnderlyingSystemType, false,
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.ExactBinding,
        Aspect.ParameterTypes,
        out constructor);
    }


    // Constructors

    public ImplementProtectedConstructorAccessorAspect(ProtectedConstructorAccessorAspect protectedConstructorAccessorAspect)
    {
      this.protectedConstructorAccessorAspect = protectedConstructorAccessorAspect;
    }
  }
}