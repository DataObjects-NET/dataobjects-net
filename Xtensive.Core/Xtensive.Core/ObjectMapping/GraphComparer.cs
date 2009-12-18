// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.07

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class GraphComparer
  {
    private readonly MappingDescription mappingDescription;
    private readonly Action<OperationInfo> subscriber;
    private readonly IExistanceInfoProvider existanceInfoProvider;
    private readonly ObjectExtractor objectExtractor;
    private Dictionary<object, object> originalKeyCache;
    private Dictionary<object, object> modifiedKeyCache;
    
    public void Compare(object original, object modified)
    {
      if (modified==null && original==null)
        return;
      var modifiedObjects = new Dictionary<object, object>();
      objectExtractor.Extract(modified, modifiedObjects);
      var originalObjects = new Dictionary<object, object>();
      objectExtractor.Extract(original, originalObjects);
      IEnumerable<object> createdObjects;
      IEnumerable<object> removedObjects;
      existanceInfoProvider.Get(new ReadOnlyDictionary<object, object>(modifiedObjects, false),
        new ReadOnlyDictionary<object, object>(originalObjects, false), out createdObjects, out removedObjects);
      NotifyAboutCreatedObjects(createdObjects);
      FindChangedObjects(modifiedObjects, originalObjects);
      NotifyAboutRemovedObjects(removedObjects);
    }

    #region Private / internal methods

    private void NotifyAboutCreatedObjects(IEnumerable<object> createdObjects)
    {
      createdObjects.Apply(obj => subscriber.Invoke(new OperationInfo(obj, OperationType.CreateObject,
        null, null)));
      foreach (var createdObject in createdObjects) {
        var type = mappingDescription.TargetTypes[createdObject.GetType()];
        var properties = type.Properties.Select(pair => pair.Value).Cast<TargetPropertyDescription>()
          .Where(p => !p.IsImmutable);
        foreach (var property in properties) {
          var value = property.SystemProperty.GetValue(createdObject, null);
          if (property.IsCollection) {
            InitializeKeyCaches();
            ExtractKeys((IEnumerable) value, originalKeyCache);
            originalKeyCache.Apply(pair =>
              NotifyAboutCollectionModification(createdObject, property, true, pair.Value));
          }
          else
            NotifyAboutPropertySetting(createdObject, property, value);
        }
      }
    }

    private void NotifyAboutRemovedObjects(IEnumerable<object> removedObjects)
    {
      removedObjects.Apply(obj => subscriber.Invoke(new OperationInfo(obj, OperationType.RemoveObject,
        null, null)));
    }

    private void FindChangedObjects(Dictionary<object, object> modifiedObjects,
      Dictionary<object, object> originalObjects)
    {
      foreach (var modifiedObjectPair in modifiedObjects) {
        object originalObject;
        if (!originalObjects.TryGetValue(modifiedObjectPair.Key, out originalObject))
          continue;
        var description = mappingDescription.TargetTypes[modifiedObjectPair.Value.GetType()];
        CompareComplexProperties(modifiedObjectPair.Value, originalObject, description);
        ComparePrimitiveProperties(modifiedObjectPair.Value, originalObject, description);
      }
    }

    private void CompareComplexProperties(object modified, object original, TargetTypeDescription description)
    {
      foreach (var property in description.ComplexProperties.Values) {
        var modifiedValue = property.SystemProperty.GetValue(modified, null);
        var originalValue = property.SystemProperty.GetValue(original, null);
        if (modifiedValue == null && originalValue == null)
          continue;
        if (property.IsCollection)
          CompareCollections(original, originalValue, modifiedValue, property);
        else
          CompareObjects(original, originalValue, modifiedValue, property);
      }
    }

    private void CompareObjects(object original, object originalValue, object modifiedValue,
      TargetPropertyDescription property)
    {
      if (modifiedValue == null)
        subscriber.Invoke(new OperationInfo(original, OperationType.SetProperty,
          property, null));
      else {
        var modifiedKey = mappingDescription.ExtractTargetKey(modifiedValue);
        var originalKey = mappingDescription.ExtractTargetKey(originalValue);
        if (!modifiedKey.Equals(originalKey))
          NotifyAboutPropertySetting(original, property, modifiedValue);
      }
    }

    private void CompareCollections(object original, object originalValue, object modifiedValue,
      TargetPropertyDescription property)
    {
      InitializeKeyCaches();
      ExtractKeys((IEnumerable) originalValue, originalKeyCache);
      ExtractKeys((IEnumerable) modifiedValue, modifiedKeyCache);
      foreach (var objPair in modifiedKeyCache)
        if (!originalKeyCache.ContainsKey(objPair.Key))
          NotifyAboutCollectionModification(original, property, true, objPair.Value);
      foreach (var objPair in originalKeyCache)
        if (!modifiedKeyCache.ContainsKey(objPair.Key))
          NotifyAboutCollectionModification(original, property, false, objPair.Value);
    }

    private void InitializeKeyCaches()
    {
      if (originalKeyCache==null) {
        originalKeyCache = new Dictionary<object, object>();
        modifiedKeyCache = new Dictionary<object, object>();
      }
      else {
        originalKeyCache.Clear();
        modifiedKeyCache.Clear();
      }
    }

    private void ExtractKeys(IEnumerable collection, Dictionary<object, object> keyCache)
    {
      if (collection == null)
        return;
      foreach (var item in collection)
        keyCache.Add(mappingDescription.ExtractTargetKey(item), item);
    }

    private void ComparePrimitiveProperties(object modified, object original, TargetTypeDescription description)
    {
      foreach (var property in description.PrimitiveProperties.Values) {
        var systemProperty = property.SystemProperty;
        var modifiedValue = systemProperty.GetValue(modified, null);
        var originalValue = systemProperty.GetValue(original, null);
        if (!Equals(modifiedValue, originalValue))
          NotifyAboutPropertySetting(original, property, modifiedValue);
      }
    }

    private void NotifyAboutPropertySetting(object obj, TargetPropertyDescription property, object value)
    {
      EnsurePropertyIsMutable(property);
      subscriber.Invoke(new OperationInfo(obj, OperationType.SetProperty, property, value));
    }

    private void NotifyAboutCollectionModification(object obj, TargetPropertyDescription property,
      bool adding, object item)
    {
      EnsurePropertyIsMutable(property);
      subscriber.Invoke(new OperationInfo(obj, adding ? OperationType.AddItem : OperationType.RemoveItem,
            property, item));
    }

    private static void EnsurePropertyIsMutable(TargetPropertyDescription property)
    {
      if (property.IsImmutable)
        throw new InvalidOperationException(String.Format(Strings.ExPropertyXIsImmutable,
          property.SystemProperty));
    }

    #endregion


    // Constructors

    public GraphComparer(MappingDescription mappingDescription, Action<OperationInfo> subscriber,
      IExistanceInfoProvider existanceInfoProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingDescription, "mappingDescription");
      ArgumentValidator.EnsureArgumentNotNull(subscriber, "subscriber");
      ArgumentValidator.EnsureArgumentNotNull(existanceInfoProvider, "existanceInfoProvider");

      this.mappingDescription = mappingDescription;
      this.subscriber = subscriber;
      this.existanceInfoProvider = existanceInfoProvider;
      objectExtractor = new ObjectExtractor(mappingDescription);
    }
  }
}