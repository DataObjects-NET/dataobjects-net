// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.07

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class GraphComparer
  {
    private readonly MappingInfo mappingInfo;
    private readonly Action<ModificationDescriptor> subscriber;
    private readonly IExistanceInfoProvider existanceInfoProvider;
    private readonly ObjectExtractor objectExtractor;
    
    public void Compare(object original, object modified)
    {
      if (modified == null && original == null)
        return;
      var modifiedObjects = new Dictionary<object, object>();
      objectExtractor.Extract(modified, modifiedObjects);
      var originalObjects = new Dictionary<object, object>();
      objectExtractor.Extract(original, originalObjects);
      using (existanceInfoProvider.Open(new ReadOnlyDictionary<object, object>(modifiedObjects, false),
        new ReadOnlyDictionary<object, object>(originalObjects, false))) {
        foreach (var createdObject in existanceInfoProvider.GetCreatedObjects())
          subscriber.Invoke(new ModificationDescriptor(createdObject, ModificationType.CreateObject,
            null, null));
        FindChangedObjects(modifiedObjects, originalObjects);
        foreach (var createdObject in existanceInfoProvider.GetRemovedObjects())
          subscriber.Invoke(new ModificationDescriptor(createdObject, ModificationType.RemoveObject,
            null, null));
      }
    }

    private void FindChangedObjects(Dictionary<object, object> modifiedObjects,
      Dictionary<object, object> originalObjects)
    {
      foreach (var modifiedObjectPair in modifiedObjects) {
        object originalObject;
        if (!originalObjects.TryGetValue(modifiedObjectPair.Key, out originalObject))
          continue;
        CompareComplexProperties(modifiedObjectPair.Value, originalObject);
        ComparePrimitiveProperties(modifiedObjectPair.Value, originalObject);
      }
    }

    private void CompareComplexProperties(object modified, object original)
    {
      var properties = mappingInfo.GetTargetComplexProperties(modified.GetType());
      foreach (var property in properties) {
        var modifiedValue = property.GetValue(modified, null);
        var originalValue = property.GetValue(original, null);
        if (modifiedValue == null && originalValue == null)
          continue;
        if (modifiedValue == null)
          subscriber.Invoke(new ModificationDescriptor(original, ModificationType.ChangeProperty,
            property, null));
        else {
          var modifiedKey = mappingInfo.ExtractKey(modifiedValue);
          var originalKey = mappingInfo.ExtractKey(originalValue);
          if (!modifiedKey.Equals(originalKey))
            subscriber.Invoke(new ModificationDescriptor(original, ModificationType.ChangeProperty,
              property, modifiedValue));
        }
      }
    }

    private void ComparePrimitiveProperties(object modified, object original)
    {
      var properties = mappingInfo.GetTargetPrimitiveProperties(modified.GetType());
      foreach (var property in properties) {
        var modifiedValue = property.GetValue(modified, null);
        var originalValue = property.GetValue(original, null);
        if (!Equals(modifiedValue, originalValue))
          subscriber.Invoke(new ModificationDescriptor(original, ModificationType.ChangeProperty,
            property, modifiedValue));
      }
    }


    // Constructors

    public GraphComparer(MappingInfo mappingInfo, Action<ModificationDescriptor> subscriber,
      IExistanceInfoProvider existanceInfoProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingInfo, "mappingInfo");
      ArgumentValidator.EnsureArgumentNotNull(subscriber, "subscriber");
      ArgumentValidator.EnsureArgumentNotNull(existanceInfoProvider, "existanceInfoProvider");

      this.mappingInfo = mappingInfo;
      this.subscriber = subscriber;
      this.existanceInfoProvider = existanceInfoProvider;
      objectExtractor = new ObjectExtractor(mappingInfo);
    }
  }
}