// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using Xtensive.Core.Links.LinkedSet;
using Xtensive.Core.Links.LinkedSet.Operations;

namespace Xtensive.Core.Links.LinkedReference.Operations
{
  public class SetReferenceOperation<TOwner, TProperty> : Operation<PropertyChangeArg<TOwner, TProperty>>,
    ISetReferenceOperation
    where TOwner : class, ILinkOwner
    where TProperty : class, ILinkOwner
  {
    [ThreadStatic] 
    internal static OperationCallRegistrator<PropertyChangeArg<TOwner, TProperty>> callRegistrator;

    private object addOperationKey;

    private Action<TOwner, TProperty, TProperty> epilogue;
    private Func<TOwner, TProperty> getPropertyValue;
    private Action<TOwner, TProperty, TProperty> operation;
    private Action<TOwner, TProperty, TProperty> prologue;

    // Cached keys in operations dictionary.
    private object removeOperationKey;
    private object setPropertyOperationKey;

    private OperationCallRegistrator<PropertyChangeArg<TOwner, TProperty>> CallRegistrator
    {
      get
      {
        OperationCallRegistrator<PropertyChangeArg<TOwner, TProperty>> lCallRegistrator = callRegistrator;
        if (lCallRegistrator!=null)
          return lCallRegistrator;
        callRegistrator =
          new OperationCallRegistrator<PropertyChangeArg<TOwner, TProperty>>(ExecutionContextSwitcher.Current, this);
        return callRegistrator;
      }
    }

    public Func<TOwner, TProperty> GetPropertyValue
    {
      get { return getPropertyValue; }
    }

    public override void Lock(bool recursive)
    {
      setPropertyOperationKey = LinkedOperationRegistry.GetKey(
        typeof (ISetReferenceOperation), LinkedPropertyName);
      removeOperationKey = LinkedOperationRegistry.GetKey(
        typeof (IRemoveItemOperation), LinkedPropertyName);
      addOperationKey = LinkedOperationRegistry.GetKey(
        typeof (IAddItemOperation), LinkedPropertyName);
      base.Lock(recursive);
    }

    public override void ExecuteCall(ref PropertyChangeArg<TOwner, TProperty> arg, ref ExecutionStage stage)
    {
      if (stage==ExecutionStage.Prologue)
        prologue(arg.Owner, arg.OldValue, arg.NewValue);
      else if (stage==ExecutionStage.Operation)
        operation(arg.Owner, arg.OldValue, arg.NewValue);
      else
        epilogue(arg.Owner, arg.OldValue, arg.NewValue);
    }

    internal override void ExecuteOperation(ref PropertyChangeArg<TOwner, TProperty> arg)
    {
      Execute(ref arg, SetReferenceExecuteOption.ExecAll);
    }

    private void Execute(ref PropertyChangeArg<TOwner, TProperty> arg,
      SetReferenceExecuteOption setReferenceExecuteOption)
    {
      if (arg.OldValue==arg.NewValue)
        return;

      if (arg.OldValue!=null) {
        // Set old value reference to null
        if (ProtectedLinkType==LinkType.OneToOne &&
          (setReferenceExecuteOption & SetReferenceExecuteOption.ExecOldValueSetProperty)==
            SetReferenceExecuteOption.ExecOldValueSetProperty) {
          ISetReferenceOperation oldValueSetRef =
            arg.OldValue.Operations.Find<ISetReferenceOperation>(setPropertyOperationKey);
          if (oldValueSetRef!=null)
            oldValueSetRef.Execute(arg.OldValue, null, SetReferenceExecuteOption.ExecNewValueSetProperty);
          else
            throw new LinkedOperationMissingException(typeof (ISetReferenceOperation),
              LinkedPropertyName, PropertyName, arg.OldValue.GetType());
        }

        // Remove from collection.
        if (ProtectedLinkType==LinkType.OneToMany &&
          (setReferenceExecuteOption & SetReferenceExecuteOption.ExecLinkedRemoveOperation)==
            SetReferenceExecuteOption.ExecLinkedRemoveOperation) {
          IRemoveItemOperation removeOp = arg.OldValue.Operations.Find<IRemoveItemOperation>(removeOperationKey);
          if (removeOp!=null)
            removeOp.Execute(arg.OldValue, arg.Owner, RemoveItemExecuteOption.Default);
          else
            throw new LinkedOperationMissingException(typeof (IRemoveItemOperation),
              LinkedPropertyName, PropertyName, arg.OldValue.GetType());
        }
      }

      if (arg.NewValue!=null) {
        // Set new value reference to this.
        if (ProtectedLinkType==LinkType.OneToOne &&
          (setReferenceExecuteOption & SetReferenceExecuteOption.ExecNewValueSetProperty)==
            SetReferenceExecuteOption.ExecNewValueSetProperty) {
          ISetReferenceOperation setRefOp =
            arg.NewValue.Operations.Find<ISetReferenceOperation>(setPropertyOperationKey);
          if (setRefOp!=null)
            setRefOp.Execute(arg.NewValue, arg.Owner, SetReferenceExecuteOption.ExecOldValueSetProperty);
          else
            throw new LinkedOperationMissingException(
              typeof (ISetReferenceOperation),
              LinkedPropertyName,
              PropertyName,
              arg.NewValue.GetType());
        }

        // Add item to collection.
        if (ProtectedLinkType==LinkType.OneToMany &&
          (setReferenceExecuteOption & SetReferenceExecuteOption.ExecLinkedAddOperation)==
            SetReferenceExecuteOption.ExecLinkedAddOperation) {
          IAddItemOperation addOp = arg.NewValue.Operations.Find<IAddItemOperation>(addOperationKey);
          if (addOp!=null)
            addOp.Execute(arg.NewValue, arg.Owner, AddItemExecuteOption.Default);
          else
            throw new LinkedOperationMissingException(typeof (IAddItemOperation),
              LinkedPropertyName, PropertyName, arg.NewValue.GetType());
        }
      }
      CallRegistrator.RegisterCall(ref arg);
    }



    void ISetReferenceOperation.Execute(object operationOwner, object dependentObject,
      SetReferenceExecuteOption setReferenceExecuteOption)
    {
      TOwner owner = (TOwner)operationOwner;
      PropertyChangeArg<TOwner, TProperty> arg = new PropertyChangeArg<TOwner, TProperty>(owner,
        GetPropertyValue(owner), (TProperty)dependentObject);
      Execute(ref arg, setReferenceExecuteOption);
    }



    public SetReferenceOperation(
      LinkedOperationRegistry linkedOperations,
      string name, string dependentOperationName, LinkType expectedLinkType,
      Action<TOwner, TProperty, TProperty> prologue,
      Action<TOwner, TProperty, TProperty> operation,
      Action<TOwner, TProperty, TProperty> epilogue,
      Func<TOwner, TProperty> getter)
    {
      ArgumentValidator.EnsureArgumentNotNull(linkedOperations, "linkedOperations");
      ArgumentValidator.EnsureArgumentNotNull(getter, "getter");
      ProtectedLinkType = expectedLinkType;
      this.prologue = prologue;
      this.epilogue = epilogue;
      this.operation = operation;
      getPropertyValue = getter;
      PropertyName = name;
      LinkedPropertyName = dependentOperationName;
      Lock();
      linkedOperations.Register(this);
    }
  }
}