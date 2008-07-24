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
  internal class FieldSetterConstraintAspect : OnMethodBoundaryAspect
  {
    private readonly FieldConstraintAspect constraintAspect;

    public override void OnEntry(MethodExecutionEventArgs eventArgs)
    {
      constraintAspect.OnSetValue(
        (IValidationAware) eventArgs.Instance,
        eventArgs.GetReadOnlyArgumentArray()[0]);

      base.OnEntry(eventArgs);
    }

    // Constructor

    public FieldSetterConstraintAspect(FieldConstraintAspect constraintAspect)
    {
      this.constraintAspect = constraintAspect;
    }
  }
}