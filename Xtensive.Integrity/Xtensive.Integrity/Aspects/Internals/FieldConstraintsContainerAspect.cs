// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.24

using System;
using System.Collections;
using System.Collections.Generic;
using PostSharp.Laos;
using Xtensive.Core.Aspects.Helpers;

namespace Xtensive.Integrity.Aspects.Internals
{
  /// <summary>
  /// Type-level attributes, that contains collection of all <see cref="FieldConstraintAspect"/>s,
  /// applied on type's properties.
  /// </summary>
  [Serializable]
  public class FieldConstraintsContainerAspect : LaosTypeLevelAspect,
    IEnumerable<FieldConstraintAspect>
  {
    private readonly List<FieldConstraintAspect> constraintAspects = 
      new List<FieldConstraintAspect>();

    internal void AddConstraintAspect(FieldConstraintAspect constraintAspect)
    {
      constraintAspects.Add(constraintAspect);
    }

    IEnumerator<FieldConstraintAspect> IEnumerable<FieldConstraintAspect>.GetEnumerator()
    {
      return constraintAspects.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return constraintAspects.GetEnumerator();
    }
  }
}