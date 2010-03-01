// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.07.02

using System;
using Xtensive.Core.Links.LinkedReference.Operations;
using Xtensive.Core.Links.LinkedSet;
using Xtensive.Core.Links.LinkedSet.Operations;

namespace Xtensive.Core.Links.LinkedReference
{
  public class LinkedReference<TOwner, TReference>
    where TOwner : class, ILinkOwner
    where TReference : class, ILinkOwner
  {
    private TOwner owner;
    private string propertyName;
    private SetLinkedReferenceOperation setReferenceOperation;
    private TReference value;



    private sealed class SetLinkedReferenceOperation : Operation<LinkedReferenceChangeArg<TOwner, TReference>>,
      ISetReferenceOperation
    {
      // Cached keys in operations dictionary.
      [ThreadStatic] 
      internal static OperationCallRegistrator<LinkedReferenceChangeArg<TOwner, TReference>> callRegistrator;

      private object addOperationKey;
      private Func<TOwner, LinkedReference<TOwner, TReference>> referenceGetter;
      private object removeOperationKey;
      private object setPropertyOperationKey;

      private OperationCallRegistrator<LinkedReferenceChangeArg<TOwner, TReference>> CallRegistrator
      {
        get
        {
          OperationCallRegistrator<LinkedReferenceChangeArg<TOwner, TReference>> lCallRegistrator = callRegistrator;
          if (lCallRegistrator!=null)
            return lCallRegistrator;
          callRegistrator =
            new OperationCallRegistrator<LinkedReferenceChangeArg<TOwner, TReference>>(
              ExecutionContextSwitcher.Current, this);
          return callRegistrator;
        }
      }

      public override void Lock(bool recursive)
      {
        setPropertyOperationKey = LinkedOperationRegistry.GetKey(typeof (ISetReferenceOperation), LinkedPropertyName);
        removeOperationKey = LinkedOperationRegistry.GetKey(typeof (IRemoveItemOperation), LinkedPropertyName);
        addOperationKey = LinkedOperationRegistry.GetKey(typeof (IAddItemOperation), LinkedPropertyName);
        base.Lock(recursive);
      }

      public override void ExecuteCall(ref LinkedReferenceChangeArg<TOwner, TReference> arg,
        ref ExecutionStage executionStage)
      {
        if (executionStage==ExecutionStage.Prologue)
          arg.LinkedReference.OnChanging(ref arg);
        else if (executionStage==ExecutionStage.Operation)
          arg.LinkedReference.value = arg.NewValue;
        else
          arg.LinkedReference.OnChanged(ref arg);
      }

      internal override void ExecuteOperation(ref LinkedReferenceChangeArg<TOwner, TReference> arg)
      {
        Execute(ref arg, SetReferenceExecuteOption.ExecAll);
      }

      private void Execute(ref LinkedReferenceChangeArg<TOwner, TReference> arg,
        SetReferenceExecuteOption setReferenceExecuteOption)
      {
        // Because dependentObject is new property value.
        if (arg.OldValue==arg.NewValue)
          return;

        // Register operations related to old property value.
        if (arg.OldValue!=null) {
          // Set property value to null
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
              removeOp.Execute(arg.OldValue, arg.LinkedReference.owner, RemoveItemExecuteOption.ExecRemoveItemCall);
            else
              throw new LinkedOperationMissingException(typeof (IRemoveItemOperation),
                LinkedPropertyName, PropertyName, arg.OldValue.GetType());
          }
        }

        // Register operation related to new property value.
        if (arg.NewValue!=null) {
          // Set property value owner to this.
          if (ProtectedLinkType==LinkType.OneToOne &&
            (setReferenceExecuteOption & SetReferenceExecuteOption.ExecNewValueSetProperty)==
              SetReferenceExecuteOption.ExecNewValueSetProperty) {
            ISetReferenceOperation setRefOp =
              arg.NewValue.Operations.Find<ISetReferenceOperation>(setPropertyOperationKey);
            if (setRefOp!=null)
              setRefOp.Execute(arg.NewValue, arg.LinkedReference.owner,
                SetReferenceExecuteOption.ExecOldValueSetProperty);
            else
              throw new LinkedOperationMissingException(typeof (ISetReferenceOperation),
                LinkedPropertyName, PropertyName, arg.NewValue.GetType());
          }

          // Add item to collection.
          if (ProtectedLinkType==LinkType.OneToMany &&
            (setReferenceExecuteOption & SetReferenceExecuteOption.ExecLinkedAddOperation)==
              SetReferenceExecuteOption.ExecLinkedAddOperation) {
            IAddItemOperation addOp = arg.NewValue.Operations.Find<IAddItemOperation>(addOperationKey);
            if (addOp!=null)
              addOp.Execute(arg.NewValue, arg.LinkedReference.owner, AddItemExecuteOption.ExecAddItemCalls);
            else
              throw new LinkedOperationMissingException(
                typeof (IAddItemOperation), LinkedPropertyName, PropertyName,
                arg.NewValue.GetType());
          }
        }
        CallRegistrator.RegisterCall(ref arg);
      }

      #region ISetReferenceOperation Members

      void ISetReferenceOperation.Execute(object operationOwner, object dependentObject,
        SetReferenceExecuteOption setReferenceExecuteOption)
      {
        TOwner owner = (TOwner)operationOwner;
        LinkedReference<TOwner, TReference> linkedReference = referenceGetter(owner);
        LinkedReferenceChangeArg<TOwner, TReference> arg = new LinkedReferenceChangeArg<TOwner, TReference>(
          linkedReference.value, (TReference)dependentObject, linkedReference);
        Execute(ref arg, setReferenceExecuteOption);
      }

      #endregion

      public SetLinkedReferenceOperation(string name, string dependentOperationName,
        LinkType expectedLinkType, Func<TOwner, LinkedReference<TOwner, TReference>> referenceGetter)
      {
        ProtectedLinkType = expectedLinkType;
        PropertyName = name;
        LinkedPropertyName = dependentOperationName;
        this.referenceGetter = referenceGetter;
      }
    }



    public event Action<LinkedReferenceChangeArg<TOwner, TReference>> Changing;
    public event Action<LinkedReferenceChangeArg<TOwner, TReference>> Changed;

    public string PropertyName
    {
      get { return propertyName; }
    }

    public TReference Value
    {
      get { return value; }
      set
      {
        LinkedReferenceChangeArg<TOwner, TReference> arg =
          new LinkedReferenceChangeArg<TOwner, TReference>(this.value, value, this);
        setReferenceOperation.Execute(ref arg);
      }
    }

    protected virtual void OnChanging(ref LinkedReferenceChangeArg<TOwner, TReference> arg)
    {
      if (Changing!=null)
        Changing(arg);
    }

    protected virtual void OnChanged(ref LinkedReferenceChangeArg<TOwner, TReference> arg)
    {
      if (Changed!=null)
        Changed(arg);
    }

    public static void ExportOperations(LinkedOperationRegistry operations,
      string propertyName, string dependentPropertyName, LinkType linkType,
      Func<TOwner, LinkedReference<TOwner, TReference>> referenceGetter)
    {
      SetLinkedReferenceOperation operation = new SetLinkedReferenceOperation(propertyName,
        dependentPropertyName, linkType, referenceGetter);
      operation.Lock();
      operations.Register(operation);
    }

    public static implicit operator TReference(LinkedReference<TOwner, TReference> container)
    {
      return container.value;
    }

    // Constructor

    public LinkedReference(TOwner owner, string propertyName)
    {
      ArgumentValidator.EnsureArgumentNotNull(owner, "owner");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(propertyName, "propertyName");

      this.owner = owner;
      this.propertyName = propertyName;
      object operationKey = LinkedOperationRegistry.GetKey(typeof (SetLinkedReferenceOperation), propertyName);
      setReferenceOperation = owner.Operations.Find<SetLinkedReferenceOperation>(operationKey);
      if (setReferenceOperation==null)
        throw new InvalidOperationException("Set linked reference operation was not registered in operation dictionary.");
    }
  }
}