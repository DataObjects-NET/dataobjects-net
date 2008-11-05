// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.05

using System;
using System.Diagnostics;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects.Helpers;

namespace Xtensive.Storage.Aspects
{
  /// <summary>
  /// Puts transactional state validation check on method entry point.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Method)]
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)] 
  [Serializable]
  public sealed class UsesTransactionalStateAttribute : ReprocessMethodBoundaryAspect,
    ILaosWeavableAspect
  {   
    int ILaosWeavableAspect.AspectPriority {
      get {
        return (int) StorageAspectPriority.UsesTransactionalState;
      }
    }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(System.Reflection.MethodBase method)
    {
      if (!AspectHelper.ValidateBaseType(this, SeverityType.Error, 
        method.DeclaringType, true, typeof(TransactionalStateContainer)))
        return false;

      if (!AspectHelper.ValidateNotInfrastructure(this, method))
        return false;

      return true;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override object OnEntry(object instance)
    {
      (instance as TransactionalStateContainer).EnsureStateIsActual();
      return null;
    }
  }
}