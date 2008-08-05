// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using PostSharp.Laos;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Aspects
{  
  [Serializable]
  internal class ImplementPropertyConstraintAspect : OnMethodBoundaryAspect,
    ILaosWeavableAspect
  {
    private readonly PropertyConstraintAspect constraintAspect;

    int ILaosWeavableAspect.AspectPriority
    {
      get { return (int)IntegrityAspectPriority.PropertyConstraint; }
    }

    public override void OnEntry(MethodExecutionEventArgs eventArgs)
    {
      constraintAspect.OnSetValue(
        (IValidationAware) eventArgs.Instance,
        eventArgs.GetReadOnlyArgumentArray()[0]);
      
      base.OnEntry(eventArgs);
    }

    public override void RuntimeInitialize(System.Reflection.MethodBase method)
    {
      base.RuntimeInitialize(method);
      constraintAspect.OnRuntimeInitialize();
    }


    // Constructor

    public ImplementPropertyConstraintAspect(PropertyConstraintAspect constraintAspect)
    {
      this.constraintAspect = constraintAspect;
    }
  }
}