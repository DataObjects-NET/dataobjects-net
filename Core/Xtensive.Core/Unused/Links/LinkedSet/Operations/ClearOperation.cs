// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using Xtensive.Core.Links.LinkedReference.Operations;

namespace Xtensive.Core.Links.LinkedSet.Operations
{
  internal sealed class ClearOperation<TOwner, TItem>
    : SetOperation<TOwner, TItem, SetOperationArg<TOwner, TItem>>
    where TOwner : ILinkOwner
    where TItem : ILinkOwner
  {
    [ThreadStatic] 
    internal static OperationCallRegistrator<SetOperationArg<TOwner, TItem>> callRegistrator;

    private object removeItemOperationKey;
    private object setPropertyOperationKey;

    private OperationCallRegistrator<SetOperationArg<TOwner, TItem>> CallRegistrator
    {
      get
      {
        OperationCallRegistrator<SetOperationArg<TOwner, TItem>> lCallRegistrator = callRegistrator;
        if (lCallRegistrator!=null)
          return lCallRegistrator;
        callRegistrator =
          new OperationCallRegistrator<SetOperationArg<TOwner, TItem>>(ExecutionContextSwitcher.Current, this);
        return callRegistrator;
      }
    }

    public override void Lock(bool recursive)
    {
      setPropertyOperationKey = LinkedOperationRegistry.GetKey(
        typeof (ISetReferenceOperation), LinkedPropertyName);
      removeItemOperationKey = LinkedOperationRegistry.GetKey(
        typeof (IRemoveItemOperation), LinkedPropertyName);
      base.Lock(recursive);
    }

    internal override void ExecuteOperation(ref SetOperationArg<TOwner, TItem> arg)
    {
      foreach (TItem item in arg.LinkedSet) {
        if (item==null)
          continue;
        ISetReferenceOperation setReferenceOp = item.Operations.Find<ISetReferenceOperation>(setPropertyOperationKey);
        IRemoveItemOperation removeOp = item.Operations.Find<IRemoveItemOperation>(removeItemOperationKey);
        if (setReferenceOp!=null || removeOp!=null) {
          if (setReferenceOp!=null)
            setReferenceOp.Execute(item, null, SetReferenceExecuteOption.Default);
          if (removeOp!=null)
            removeOp.Execute(item, arg.LinkedSet.Owner, RemoveItemExecuteOption.Default);
        }
      }
      CallRegistrator.RegisterCall(ref arg);
    }

    public override void ExecuteCall(ref SetOperationArg<TOwner, TItem> arg, ref ExecutionStage executionStage)
    {
      if (executionStage==ExecutionStage.Prologue)
        arg.LinkedSet.OnClearing();
      else if (executionStage==ExecutionStage.Operation)
        arg.LinkedSet.InnerClear();
      else
        arg.LinkedSet.OnClear();
    }
  }
}