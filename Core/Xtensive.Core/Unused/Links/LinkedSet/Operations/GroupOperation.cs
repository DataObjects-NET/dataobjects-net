// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.07.02

using System;

namespace Xtensive.Core.Links.LinkedSet.Operations
{
  internal class GroupOperation<TOwner, TItem, TArg> : SetOperation<TOwner, TItem, TArg>
    where TItem : ILinkOwner
    where TOwner : ILinkOwner
  {
    private Action<LinkedSet<TOwner, TItem>, TArg> realOperation;

    public void Execute(TOwner owner, TArg args)
    {
      ExecutionContextSwitcher.Current.CurrentContext.BeginOperation();
      try {
        realOperation(linkedSetGetter(owner), args);
      }
      finally {
        ExecutionContextSwitcher.Current.CurrentContext.EndOperation();
      }
    }

    public override void ExecuteCall(ref TArg arg, ref ExecutionStage executionStage)
    {
      throw new NotSupportedException();
    }

    internal override void ExecuteOperation(ref TArg arg)
    {
      throw new NotSupportedException();
    }

    public GroupOperation(Action<LinkedSet<TOwner, TItem>, TArg> realOperation)
    {
      ArgumentValidator.EnsureArgumentNotNull(realOperation, "realOperation");
      this.realOperation = realOperation;
    }
  }
}