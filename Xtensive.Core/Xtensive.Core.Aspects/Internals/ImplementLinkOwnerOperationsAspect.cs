// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Created:    2007.07.26

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects;
using Xtensive.Core.Collections;
using Xtensive.Core.Links;

namespace Xtensive.Core.Aspects.Internals
{
  [MulticastAttributeUsage(MulticastTargets.Method)]
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  [Serializable]
  internal sealed class ImplementLinkOwnerOperationsAspect: ImplementMethodAspect
  {
    private ImplementLinkOwnerAspect iloa;

    public override void OnExecution(MethodExecutionEventArgs eventArgs)
    {
      eventArgs.ReturnValue = ((ILinkOwner)iloa.CreateImplementationObject(null)).Operations;
    }

    public override void RuntimeInitialize(MethodBase method)
    {
      iloa.RuntimeInitialize(method.DeclaringType);
    }


    // Constructors

    public ImplementLinkOwnerOperationsAspect(ImplementLinkOwnerAspect iloa)
    {
      this.iloa = iloa;
    }
  }
}