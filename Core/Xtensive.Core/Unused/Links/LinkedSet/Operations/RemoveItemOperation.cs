// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using Xtensive.Core.Links.LinkedReference.Operations;

namespace Xtensive.Core.Links.LinkedSet.Operations
{
  internal sealed class RemoveItemOperation<TOwner, TItem> : SetOperation<TOwner, TItem,
    SetChangedArg<TOwner, TItem>>, IRemoveItemOperation
    where TItem : ILinkOwner
    where TOwner : ILinkOwner
  {
    [ThreadStatic] 
    internal static OperationCallRegistrator<SetChangedArg<TOwner, TItem>> callRegistrator;

    private object removeItemOperationKey;
    private object setPropertyOperationKey;

    private OperationCallRegistrator<SetChangedArg<TOwner, TItem>> CallRegistrator
    {
      get
      {
        OperationCallRegistrator<SetChangedArg<TOwner, TItem>> lCallRegistrator = callRegistrator;
        if (lCallRegistrator!=null)
          return lCallRegistrator;
        callRegistrator =
          new OperationCallRegistrator<SetChangedArg<TOwner, TItem>>(ExecutionContextSwitcher.Current, this);
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

    internal override void ExecuteOperation(ref SetChangedArg<TOwner, TItem> arg)
    {
      Execute(ref arg, RemoveItemExecuteOption.ExecAll);
    }

    public override void ExecuteCall(ref SetChangedArg<TOwner, TItem> arg, ref ExecutionStage executionStage)
    {
      if (executionStage==ExecutionStage.Prologue)
        arg.LinkedSet.OnRemoving(arg.Item);
      else if (executionStage==ExecutionStage.Operation)
        arg.LinkedSet.InnerRemove(arg.Item);
      else
        arg.LinkedSet.OnRemove(arg.Item);
    }

    public void Execute(ref SetChangedArg<TOwner, TItem> arg, RemoveItemExecuteOption removeItemExecuteOption)
    {
      CallRegistrator.RegisterCall(ref arg);
      if (arg.Item!=null) {
        if (ProtectedLinkType==LinkType.OneToMany &&
          (removeItemExecuteOption & RemoveItemExecuteOption.ExecSetContainerCall)==
            RemoveItemExecuteOption.ExecSetContainerCall) {
          ISetReferenceOperation setReferenceOp =
            arg.Item.Operations.Find<ISetReferenceOperation>(setPropertyOperationKey);
          if (setReferenceOp!=null)
            setReferenceOp.Execute(arg.Item, arg.LinkedSet.Owner,
              SetReferenceExecuteOption.ExecOldValueSetProperty | SetReferenceExecuteOption.ExecNewValueSetProperty);
          else
            throw new LinkedOperationMissingException(
              typeof (ISetReferenceOperation), LinkedPropertyName, PropertyName, arg.Item.GetType());
        }
        if (ProtectedLinkType==LinkType.ManyToMany &&
          (removeItemExecuteOption & RemoveItemExecuteOption.ExecRemoveItemCall)==
            RemoveItemExecuteOption.ExecRemoveItemCall) {
          IRemoveItemOperation removeItemOperation =
            arg.Item.Operations.Find<IRemoveItemOperation>(removeItemOperationKey);
          if (removeItemOperation!=null)
            removeItemOperation.Execute(arg.Item, arg.LinkedSet.Owner, RemoveItemExecuteOption.ExecSetContainerCall);
          else
            throw new LinkedOperationMissingException(typeof (IRemoveItemOperation),
              LinkedPropertyName, PropertyName, arg.Item.GetType());
        }
      }
    }



    public void Execute(ILinkOwner operationOwner, object linkedObject, RemoveItemExecuteOption removeItemExecuteOption)
    {
      SetChangedArg<TOwner, TItem> arg = new SetChangedArg<TOwner, TItem>(
        linkedSetGetter((TOwner)operationOwner), (TItem)linkedObject);
      Execute(ref arg, removeItemExecuteOption);
    }


  }
}