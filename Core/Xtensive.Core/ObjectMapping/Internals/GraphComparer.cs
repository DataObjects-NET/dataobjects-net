// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.07

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.ObjectMapping.Comparison;
using Xtensive.ObjectMapping.Model;
using Xtensive.Threading;

namespace Xtensive.ObjectMapping
{
  internal sealed class GraphComparer
  {
    private static ThreadSafeDictionary<TargetTypeDescription, ReadOnlyCollection<TargetPropertyDescription>>
      mutableNonStructureProperties =
        ThreadSafeDictionary<TargetTypeDescription, ReadOnlyCollection<TargetPropertyDescription>>
          .Create(new object());

    private readonly IExistanceInfoProvider existanceInfoProvider;
    private readonly NotifyingAboutCreatedObjectsState notifyingAboutCreatedObjectsState;
    private readonly EntityComparison entityComparison;
    private readonly EntityReferencePropertyComparison entityReferencePropertyComparison;
    private readonly CollectionPropertyComparison collectionPropertyComparison;
    private readonly UserStructurePropertyComparison userStructurePropertyComparison;
    private readonly Action<object> removedObjectNotificationSender;

    public readonly Action<Operation> Subscriber;

    public readonly MappingDescription MappingDescription;

    public ComparisonInfo ComparisonInfo { get; private set; }
    
    public void Compare(Dictionary<object, object> original, Dictionary<object, object> modified)
    {
      IEnumerable<object> createdObjects;
      IEnumerable<object> removedObjects;
      existanceInfoProvider.Get(new ReadOnlyDictionary<object, object>(modified, false),
        new ReadOnlyDictionary<object, object>(original, false), out createdObjects, out removedObjects);
      ComparisonInfo = new ComparisonInfo();
      notifyingAboutCreatedObjectsState.Notify(createdObjects);
      FindChangedObjects(modified, original);
      NotifyAboutRemovedObjects(removedObjects);
    }

    public void Compare(object originalValue, object modifiedValue, TargetPropertyDescription property,
      ComparisonInfo comparisonInfo)
    {
      if (originalValue==null && modifiedValue==null)
        return;
      if (property.IsCollection)
        collectionPropertyComparison.Compare(originalValue, modifiedValue, property);
      else if (property.ValueType.ObjectKind==ObjectKind.UserStructure)
        userStructurePropertyComparison.Compare(originalValue, modifiedValue, property);
      else if (property.ValueType.ObjectKind==ObjectKind.Entity)
        entityReferencePropertyComparison.Compare(originalValue, modifiedValue, property);
      else
        throw new ArgumentOutOfRangeException("property.ValueType.ObjectKind");
    }

    #region Private / internal methods

    private void NotifyAboutRemovedObjects(IEnumerable<object> removedObjects)
    {
      removedObjects.ForEach(removedObjectNotificationSender);
    }

    private void FindChangedObjects(Dictionary<object, object> modifiedObjects,
      IDictionary<object, object> originalObjects)
    {
      foreach (var modifiedObjectPair in modifiedObjects) {
        object originalObject;
        if (!originalObjects.TryGetValue(modifiedObjectPair.Key, out originalObject))
          continue;
        entityComparison.Compare(originalObject, modifiedObjectPair.Value);
      }
    }

    #endregion


    // Constructors

    public GraphComparer(MappingDescription mappingDescription, Action<Operation> subscriber,
      IExistanceInfoProvider existanceInfoProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingDescription, "mappingDescription");
      ArgumentValidator.EnsureArgumentNotNull(subscriber, "subscriber");
      ArgumentValidator.EnsureArgumentNotNull(existanceInfoProvider, "existanceInfoProvider");

      MappingDescription = mappingDescription;
      Subscriber = subscriber;
      this.existanceInfoProvider = existanceInfoProvider;

      notifyingAboutCreatedObjectsState = new NotifyingAboutCreatedObjectsState(this);
      collectionPropertyComparison = new CollectionPropertyComparison(this);
      entityComparison = new EntityComparison(this);
      entityReferencePropertyComparison = new EntityReferencePropertyComparison(this);
      userStructurePropertyComparison = new UserStructurePropertyComparison(this);

      removedObjectNotificationSender = obj => Subscriber
        .Invoke(new Operation(obj, OperationType.RemoveObject, null, null));
    }
  }
}