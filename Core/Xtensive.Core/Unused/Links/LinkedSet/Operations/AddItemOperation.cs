// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.06.28

using System;
using Xtensive.Core.Links.LinkedReference.Operations;

namespace Xtensive.Core.Links.LinkedSet.Operations
{
  internal sealed class AddItemOperation<TOwner, TItem> : SetOperation<TOwner, TItem, SetChangedArg<TOwner, TItem>>,
    IAddItemOperation
    where TItem : ILinkOwner
    where TOwner : ILinkOwner
  {
    [ThreadStatic] 
    internal static OperationCallRegistrator<SetChangedArg<TOwner, TItem>> callRegistrator;

    private object addItemOperationKey;
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
      addItemOperationKey = LinkedOperationRegistry.GetKey(
        typeof (IAddItemOperation), LinkedPropertyName);
      base.Lock(recursive);
    }

    internal override void ExecuteOperation(ref SetChangedArg<TOwner, TItem> arg)
    {
      Execute(ref arg, AddItemExecuteOption.ExecAll);
    }

    public override void ExecuteCall(ref SetChangedArg<TOwner, TItem> arg, ref ExecutionStage executionStage)
    {
      if (executionStage==ExecutionStage.Prologue)
        arg.LinkedSet.OnAdding(arg.Item);
      else if (executionStage==ExecutionStage.Operation)
        arg.LinkedSet.InnerAdd(arg.Item);
      else
        arg.LinkedSet.OnAdd(arg.Item);
    }

    public void Execute(ref SetChangedArg<TOwner, TItem> arg, AddItemExecuteOption addItemExecuteOption)
    {
      if (arg.Item!=null) {
        if (ProtectedLinkType==LinkType.OneToMany &&
          (addItemExecuteOption & AddItemExecuteOption.ExecSetContainerCalls)==
            AddItemExecuteOption.ExecSetContainerCalls) {
          ISetReferenceOperation setReferenceOp =
            arg.Item.Operations.Find<ISetReferenceOperation>(setPropertyOperationKey);
          if (setReferenceOp!=null)
            setReferenceOp.Execute(arg.Item, arg.LinkedSet.Owner,
              SetReferenceExecuteOption.ExecNewValueSetProperty |
                SetReferenceExecuteOption.ExecOldValueSetProperty |
                  SetReferenceExecuteOption.ExecLinkedRemoveOperation);
          else
            throw new LinkedOperationMissingException(typeof (ISetReferenceOperation),
              LinkedPropertyName, PropertyName, arg.Item.GetType());
        }

        if (ProtectedLinkType==LinkType.ManyToMany &&
          (addItemExecuteOption & AddItemExecuteOption.ExecAddItemCalls)==AddItemExecuteOption.ExecAddItemCalls) {
          IAddItemOperation addOperation = arg.Item.Operations.Find<IAddItemOperation>(addItemOperationKey);
          if (addOperation!=null)
            addOperation.Execute(arg.Item, arg.LinkedSet.Owner, AddItemExecuteOption.ExecSetContainerCalls);
          else
            throw new LinkedOperationMissingException(typeof (IAddItemOperation),
              LinkedPropertyName, PropertyName, arg.Item.GetType());
        }
      }
      CallRegistrator.RegisterCall(ref arg);
    }



    public void Execute(ILinkOwner operationOwner, object linkedObject, AddItemExecuteOption addItemExecuteOption)
    {
      SetChangedArg<TOwner, TItem> arg = new SetChangedArg<TOwner, TItem>(
        linkedSetGetter((TOwner)operationOwner), (TItem)linkedObject);
      Execute(ref arg, addItemExecuteOption);
    }


  }
}