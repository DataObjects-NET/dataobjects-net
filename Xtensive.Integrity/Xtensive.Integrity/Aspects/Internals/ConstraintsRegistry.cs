// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xtensive.Integrity.Aspects
{
  internal static class ConstraintsRegistry
  {
    private static Dictionary<Type, List<FieldConstraintAspect>> registry = 
      new Dictionary<Type, List<FieldConstraintAspect>>();

    /// <summary>
    /// Registers the constraint in the registry.
    /// </summary>
    /// <param name="targetType">Type of the constraint target.</param>
    /// <param name="constraint">The constraint to register.</param>
    public static void RegisterConstraint(Type targetType, FieldConstraintAspect constraint)
    {
      if (!registry.ContainsKey(targetType))
        registry[targetType] = new List<FieldConstraintAspect>();

      registry[targetType].Add(constraint);      
    }

    /// <summary>
    /// Gets all constraints targeted to the specified type and it's ancestors.
    /// </summary>
    /// <param name="targetType">The target type.</param>
    /// <returns>Enumerable of constraints.</returns>
    public static IEnumerable<FieldConstraintAspect> GetConstraints(Type targetType)
    {
      while (targetType!=null) {  
        if (registry.ContainsKey(targetType)) 
          foreach (var constraint in registry[targetType])
            yield return constraint;

        targetType = targetType.BaseType;        
      }
    }

  }
}