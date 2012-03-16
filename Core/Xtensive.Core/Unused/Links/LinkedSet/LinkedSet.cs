// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Links.LinkedSet.Operations;

namespace Xtensive.Core.Links.LinkedSet
{
  public class LinkedSet<TOwner, TItem> : SetSlim<TItem>
    where TItem : ILinkOwner
    where TOwner : ILinkOwner
  {
    private AddItemOperation<TOwner, TItem> addItemOperation;
    private GroupOperation<TOwner, TItem, IEnumerable> addRangeOperation;
    private ClearOperation<TOwner, TItem> clearOperation;
    private TOwner owner;
    private RemoveItemOperation<TOwner, TItem> removeItemOperation;
    private GroupOperation<TOwner, TItem, IEnumerable> removeRangeOperation;

    public TOwner Owner
    {
      get { return owner; }
    }

    public virtual void OnRemoving(TItem item)
    {
    }

    public override bool Remove(TItem item)
    {
      bool result = Contains(item);
      SetChangedArg<TOwner, TItem> arg = new SetChangedArg<TOwner, TItem>(this, item);
      removeItemOperation.Execute(ref arg);
      return result;
    }

    internal bool InnerRemove(TItem item)
    {
      return base.Remove(item);
    }

    public virtual void OnRemove(TItem item)
    {
    }

    public virtual void OnAdding(TItem item)
    {
    }

    public new virtual void Add(TItem item)
    {
      SetChangedArg<TOwner, TItem> arg = new SetChangedArg<TOwner, TItem>(this, item);
      addItemOperation.Execute(ref arg);
    }

    internal void InnerAdd(TItem item)
    {
      base.Add(item);
    }

    public virtual void OnAdd(TItem item)
    {
    }

    public virtual void OnClearing()
    {
    }

    public virtual void InnerClear()
    {
      base.Clear();
    }

    public new virtual void Clear()
    {
      SetOperationArg<TOwner, TItem> arg = new SetOperationArg<TOwner, TItem>(this);
      clearOperation.Execute(ref arg);
    }

    public virtual void OnClear()
    {
    }

    public void AddRange(IEnumerable collection)
    {
      addRangeOperation.Execute(owner, collection);
    }

    public int RemoveRange(IEnumerable<TItem> collection)
    {
      int beforeCount = Count;
      removeRangeOperation.Execute(owner, collection);
      int afterCount = Count;
      return beforeCount - afterCount;
    }

    public static void ExportOperations(
      LinkedOperationRegistry registry,
      string ownerPropertyName,
      string dependentPropertyName,
      LinkType linkType,
      Func<TOwner, LinkedSet<TOwner, TItem>> getCollectionValueDelegate
      )
    {
      #region Add

      AddItemOperation<TOwner, TItem> addOperation = new AddItemOperation<TOwner, TItem>();
      ExportOperation(registry, addOperation, ownerPropertyName,
        dependentPropertyName, null, getCollectionValueDelegate, linkType);

      #endregion

      #region Remove

      RemoveItemOperation<TOwner, TItem> removeOperation = new RemoveItemOperation<TOwner, TItem>();
      ExportOperation(registry, removeOperation, ownerPropertyName,
        dependentPropertyName, null, getCollectionValueDelegate, linkType);

      #endregion

      #region Clear

      ClearOperation<TOwner, TItem> clearOperation = new ClearOperation<TOwner, TItem>();
      ExportOperation(registry, clearOperation, ownerPropertyName,
        dependentPropertyName, null, getCollectionValueDelegate, linkType);

      #endregion

      #region AddRange

      GroupOperation<TOwner, TItem, IEnumerable> addRangeOperation = new GroupOperation<TOwner, TItem, IEnumerable>(
        delegate(LinkedSet<TOwner, TItem> set, IEnumerable collection) {
          foreach (object o in collection)
            set.Add((TItem)o);
        });
      ExportOperation(registry, addRangeOperation, ownerPropertyName,
        dependentPropertyName, "AddRange", getCollectionValueDelegate, linkType);

      #endregion

      #region RemoveRange

      GroupOperation<TOwner, TItem, IEnumerable> removeRangeOperation = new GroupOperation<TOwner, TItem, IEnumerable>(
        delegate(LinkedSet<TOwner, TItem> set, IEnumerable collection) {
          foreach (object o in collection)
            set.Remove((TItem)o);
        });
      ExportOperation(registry, removeRangeOperation, ownerPropertyName,
        dependentPropertyName, "RemoveRange", getCollectionValueDelegate, linkType);

      #endregion
    }

    private static void ExportOperation<TArg>(LinkedOperationRegistry registry,
      SetOperation<TOwner, TItem, TArg> operation, string ownerPropertyName,
      string dependentPropertyName, string operationName,
      Func<TOwner, LinkedSet<TOwner, TItem>> getCollectionValueDelegate,
      LinkType linkType)
    {
      operation.PropertyName = ownerPropertyName;
      operation.LinkedSetGetter = getCollectionValueDelegate;
      operation.LinkedPropertyName = dependentPropertyName;
      operation.LinkType = linkType;
      operation.Lock();
      registry.Register(operation, operationName);
    }

    // Constructor.

    public LinkedSet(TOwner owner, string ownerPropertyName)
    {
      ArgumentValidator.EnsureArgumentNotNull(owner, "owner");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(ownerPropertyName, "ownerPropertyName");

      this.owner = owner;
      object addItemOperationKey =
        LinkedOperationRegistry.GetKey(typeof (AddItemOperation<TOwner, TItem>), ownerPropertyName);
      addItemOperation = owner.Operations.Find<AddItemOperation<TOwner, TItem>>(addItemOperationKey);
      if (addItemOperation==null)
        throw new LinkedOperationMissingException(typeof (AddItemOperation<TOwner, TItem>), null, ownerPropertyName,
          typeof (TItem));

      object removeItemOperationKey =
        LinkedOperationRegistry.GetKey(typeof (RemoveItemOperation<TOwner, TItem>), ownerPropertyName);
      removeItemOperation = owner.Operations.Find<RemoveItemOperation<TOwner, TItem>>(removeItemOperationKey);

      object clearOperationKey =
        LinkedOperationRegistry.GetKey(typeof (ClearOperation<TOwner, TItem>), ownerPropertyName);
      clearOperation = owner.Operations.Find<ClearOperation<TOwner, TItem>>(clearOperationKey);

      object addRangeOperationKey =
        LinkedOperationRegistry.GetKey(typeof (GroupOperation<TOwner, TItem, IEnumerable>), ownerPropertyName,
          "AddRange");
      addRangeOperation = owner.Operations.Find<GroupOperation<TOwner, TItem, IEnumerable>>(addRangeOperationKey);

      object removeRangeOperationKey =
        LinkedOperationRegistry.GetKey(typeof (GroupOperation<TOwner, TItem, IEnumerable>), ownerPropertyName,
          "RemoveRange");
      removeRangeOperation = owner.Operations.Find<GroupOperation<TOwner, TItem, IEnumerable>>(removeRangeOperationKey);
    }
  }
}