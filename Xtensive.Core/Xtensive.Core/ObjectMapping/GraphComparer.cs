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
using Xtensive.Core.Threading;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class GraphComparer
  {
    private static ThreadSafeDictionary<TargetTypeDescription, ReadOnlyCollection<TargetPropertyDescription>>
      mutableNonStructureProperties =
      ThreadSafeDictionary<TargetTypeDescription, ReadOnlyCollection<TargetPropertyDescription>>
        .Create(new object());

    private readonly MappingDescription mappingDescription;
    private readonly Action<OperationInfo> subscriber;
    private readonly IExistanceInfoProvider existanceInfoProvider;
    private readonly ObjectExtractor objectExtractor;
    private Dictionary<object, object> originalKeyCache;
    private Dictionary<object, object> modifiedKeyCache;
    private object structureOwner;
    private TargetPropertyDescription[] structurePath;
    private Queue<Triplet<object, object, TargetPropertyDescription[]>> structureLevels;
    private readonly Action<object, object, object, TargetPropertyDescription> userStructureComparer;
    private readonly Action<object, object, object, TargetPropertyDescription> structureLevelsRegistrator;
    
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
        var properties = type.MutableProperties;
        NotifyAboutValuesOfPropertiesOfCreatedObject(createdObject, properties);
      }
    }

    private static IEnumerable<TargetPropertyDescription> GetMutableNonStructureProperties(
      TargetTypeDescription type)
    {
      return mutableNonStructureProperties.GetValue(type,
        t => {
          var result = new List<TargetPropertyDescription>(t.MutableProperties.Count);
          result.AddRange(t.MutableProperties.Where(p => !p.IsUserStructure));
          return new ReadOnlyCollection<TargetPropertyDescription>(result, false);
        });
    }

    private void NotifyAboutValuesOfPropertiesOfCreatedObject(object createdObject,
      IEnumerable<TargetPropertyDescription> properties)
    {
      foreach (var property in properties) {
        var value = property.SystemProperty.GetValue(createdObject, null);
        if (property.IsCollection) {
          InitializeKeyCaches();
          ExtractKeys((IEnumerable) value, originalKeyCache);
          originalKeyCache.Apply(pair =>
            NotifyAboutCollectionModification(createdObject, property, true, pair.Value));
        }
        else if (property.IsUserStructure) {
          var structureType = mappingDescription.TargetTypes[property.SystemProperty.PropertyType];
          var previousStructurePath = structurePath;
          var previousStructureOwner = structureOwner;
          structurePath = structurePath==null ? new[] {property} : AddToStructurePath(property);
          if (structureOwner==null)
            structureOwner = createdObject;
          NotifyAboutValuesOfPropertiesOfCreatedObject(value, GetMutableNonStructureProperties(structureType));
          structureOwner = previousStructureOwner;
          structurePath = previousStructurePath;
        }
        else
          NotifyAboutPropertySetting(createdObject, property, value);
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
        CompareComplexProperties(modifiedObjectPair.Value, originalObject, description,
          userStructureComparer);
        ComparePrimitiveProperties(modifiedObjectPair.Value, originalObject, description);
      }
    }

    private void CompareComplexProperties(object modified, object original, TargetTypeDescription description,
      Action<object, object, object,TargetPropertyDescription> structureComparer)
    {
      foreach (var property in description.ComplexProperties.Values) {
        var modifiedValue = property.SystemProperty.GetValue(modified, null);
        var originalValue = property.SystemProperty.GetValue(original, null);
        if (modifiedValue == null && originalValue == null)
          continue;
        if (property.IsCollection)
          CompareCollections(original, originalValue, modifiedValue, property);
        else if (property.IsUserStructure)
          structureComparer.Invoke(original, originalValue, modifiedValue, property);
        else
          CompareObjects(original, originalValue, modifiedValue, property);
      }
    }

    private void CompareObjects(object original, object originalValue, object modifiedValue,
      TargetPropertyDescription property)
    {
      if (modifiedValue == null || originalValue == null)
        NotifyAboutPropertySetting(original, property, modifiedValue);
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
      if (MappingHelper.IsTypePrimitive(property.ItemType)) {
        ComparePrimitiveCollection(original, originalValue, modifiedValue, property);
        return;
      }
      /*if (property.ItemType.IsValueType)
        throw new InvalidOperationException("Collection of user structures aren't supported.");*/
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

    private void ComparePrimitiveCollection(object original, object originalValue, object modifiedValue,
      TargetPropertyDescription property)
    {
      if (originalValue==null && modifiedValue==null)
        return;
      if (originalValue==null ^ modifiedValue==null) {
        NotifyAboutPropertySetting(original, property, modifiedValue);
        return;
      }
      var originalEnumerator = ((IEnumerable) originalValue).GetEnumerator();
      var modifiedEnumerator = ((IEnumerable) modifiedValue).GetEnumerator();
      using (originalEnumerator as IDisposable) {
        using (modifiedEnumerator as IDisposable) {
          while (originalEnumerator.MoveNext()) {
            if (!modifiedEnumerator.MoveNext() || !Equals(originalEnumerator.Current, modifiedEnumerator.Current)) {
              NotifyAboutPropertySetting(original, property, modifiedValue);
              break;
            }
          }
        }
        if (originalEnumerator.MoveNext())
          NotifyAboutPropertySetting(original, property, modifiedValue);
      }
    }
    
    private void CompareUserStructure(object original, object originalValue, object modifiedValue,
      TargetPropertyDescription property)
    {
      try {
        structureOwner = original;
        if (structureLevels==null)
          structureLevels = new Queue<Triplet<object, object, TargetPropertyDescription[]>>();
        else
          structureLevels.Clear();
        structureLevels.Enqueue(new Triplet<object, object, TargetPropertyDescription[]>(modifiedValue,
          originalValue, new[] {property}));
        while (structureLevels.Count > 0)
          WalkThroughStructureTree(structureLevels.Dequeue());
      }
      finally {
        structureOwner = null;
        structurePath = null;
      }
    }

    private void WalkThroughStructureTree(Triplet<object, object, TargetPropertyDescription[]> level)
    {
      var modifiedValue = level.First;
      var originalValue = level.Second;
      structurePath = level.Third;
      var structureSystemType = structurePath[structurePath.Length - 1].SystemProperty.PropertyType;
      var structureType = mappingDescription.TargetTypes[structureSystemType];
      ComparePrimitiveProperties(modifiedValue, originalValue, structureType);
      CompareComplexProperties(modifiedValue, originalValue, structureType, structureLevelsRegistrator);
    }

    private void RegisterStructureLevels(object original, object modifiedValue, object originalValue,
      TargetPropertyDescription property)
    {
      var newPath = AddToStructurePath(property);
      structureLevels.Enqueue(
        new Triplet<object, object, TargetPropertyDescription[]>(modifiedValue, originalValue, newPath));
    }

    private TargetPropertyDescription[] AddToStructurePath(TargetPropertyDescription property)
    {
      var result = new TargetPropertyDescription[structurePath.Length + 1];
      Array.Copy(structurePath, result, structurePath.Length);
      result[result.Length - 1] = property;
      return result;
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
      foreach (var item in collection) {
        if (item==null)
          continue;
        keyCache.Add(mappingDescription.ExtractTargetKey(item), item);
      }
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
      object target;
      TargetPropertyDescription[] path;
      GetTargetAndPath(obj, property, out target, out path);
      subscriber.Invoke(new OperationInfo(target, OperationType.SetProperty, path, value));
    }

    private void NotifyAboutCollectionModification(object obj, TargetPropertyDescription property,
      bool adding, object item)
    {
      EnsurePropertyIsMutable(property);
      object target;
      TargetPropertyDescription[] path;
      GetTargetAndPath(obj, property, out target, out path);
      subscriber.Invoke(new OperationInfo(target, adding ? OperationType.AddItem : OperationType.RemoveItem,
        path, item));
    }

    private void GetTargetAndPath(object obj, TargetPropertyDescription property, out object target,
      out TargetPropertyDescription[] propertyPath)
    {
      target = structureOwner ?? obj;
      if (structurePath != null) {
        propertyPath = new TargetPropertyDescription[structurePath.Length + 1];
        Array.Copy(structurePath, propertyPath, structurePath.Length);
        propertyPath[propertyPath.Length - 1] = property;
      }
      else
        propertyPath = new[] {property};
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
      userStructureComparer = CompareUserStructure;
      structureLevelsRegistrator = RegisterStructureLevels;
    }
  }
}