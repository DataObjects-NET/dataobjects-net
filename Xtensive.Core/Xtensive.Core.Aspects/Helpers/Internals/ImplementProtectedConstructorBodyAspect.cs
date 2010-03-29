// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.18

using System;
using PostSharp.Aspects;

namespace Xtensive.Core.Aspects.Helpers.Internals
{
  /// <summary>
  /// Internally applied by <see cref="Helpers.ProtectedConstructorAspect"/>.
  /// </summary>
  [Serializable]
  public sealed class ImplementProtectedConstructorBodyAspect : TypeLevelAspect
  {
    private readonly ProtectedConstructorAspect constructorAspect;

    public ProtectedConstructorAspect Aspect
    {
      get { return constructorAspect; }
    }


    // Constructors

    internal ImplementProtectedConstructorBodyAspect(ProtectedConstructorAspect protectedConstructorAspect)
    {
      constructorAspect = protectedConstructorAspect;
    }
  }
}