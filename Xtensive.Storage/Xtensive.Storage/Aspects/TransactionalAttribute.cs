// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.21

using System;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Disposable;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Aspects
{
  /// <summary>
  /// Wraps method into transaction, if it is necessary.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Method)]
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)] 
  [Serializable]
  public sealed class TransactionalAttribute : OnMethodBoundaryAspect
  {
    public override bool CompileTimeValidate(System.Reflection.MethodBase method)
    {
      if (!AspectHelper.ValidateContextBoundMethod<Session>(this, method))
        return false;

      if (!AspectHelper.ValidateNotInfrastructure(this, method))
        return false;

      return true;
    }

    /// <inheritdoc/>
    public override void OnEntry(MethodExecutionEventArgs eventArgs)
    {
      Session session = Session.Current;
      if (session==null)
        throw new InvalidOperationException(Strings.SessionIsNotActivated);
      
      TransactionScope scope = session.OpenTransaction();
      eventArgs.MethodExecutionTag = scope;
    }

    /// <inheritdoc/>
    public override void OnSuccess(MethodExecutionEventArgs eventArgs)
    {
      TransactionScope scope = (TransactionScope) eventArgs.MethodExecutionTag;
      scope.Complete();
    }

    /// <inheritdoc/>
    public override void OnExit(MethodExecutionEventArgs eventArgs)
    {
      TransactionScope scope = (TransactionScope) eventArgs.MethodExecutionTag;
      scope.DisposeSafely();
    }
  }
}