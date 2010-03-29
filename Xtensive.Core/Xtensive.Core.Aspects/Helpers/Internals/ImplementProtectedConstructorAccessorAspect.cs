// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.20

using System;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace Xtensive.Core.Aspects.Helpers.Internals
{
  [Serializable]
  public sealed class ImplementProtectedConstructorAccessorAspect : TypeLevelAspect
  {
    private readonly ProtectedConstructorAccessorAspect protectedConstructorAccessorAspect;

    public ProtectedConstructorAccessorAspect Aspect
    {
      get { return protectedConstructorAccessorAspect; }
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