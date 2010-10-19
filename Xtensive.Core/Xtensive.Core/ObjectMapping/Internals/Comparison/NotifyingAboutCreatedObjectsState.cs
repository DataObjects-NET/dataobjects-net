// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.23

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xtensive.ObjectMapping.Model;
using Xtensive.Threading;

namespace Xtensive.ObjectMapping.Comparison
{
  internal sealed class NotifyingAboutCreatedObjectsState : ProcessingCollectionState
  {
    private static ThreadSafeDictionary<TargetTypeDescription, ReadOnlyCollection<TargetPropertyDescription>>
      mutableNonStructureProperties =
      ThreadSafeDictionary<TargetTypeDescription, ReadOnlyCollection<TargetPropertyDescription>>
        .Create(new object());

    public void Notify(IEnumerable<object> createdObjects)
    {
      foreach (var createdObject in createdObjects)
        GraphComparer.Subscriber.Invoke(new Operation(createdObject, OperationType.CreateObject, null, null));
      foreach (var createdObject in createdObjects) {
        using (GraphComparer.ComparisonInfo.SaveState()) {
          GraphComparer.ComparisonInfo.Owner = createdObject;
          var type = GraphComparer.MappingDescription.GetTargetType(createdObject.GetType());
          var properties = type.MutableProperties;
          NotifyAboutValuesOfPropertiesOfCreatedObject(createdObject, properties);
        }
      }
    }

    private static IEnumerable<TargetPropertyDescription> GetMutableNonStructureProperties(
      TargetTypeDescription type)
    {
      return mutableNonStructureProperties.GetValue(type,
        t => {
          var result = new List<TargetPropertyDescription>(t.MutableProperties.Count);
          result.AddRange(t.MutableProperties.Where(p => p.ValueType.ObjectKind!=ObjectKind.UserStructure));
          return new ReadOnlyCollection<TargetPropertyDescription>(result);
        });
    }

    private void NotifyAboutValuesOfPropertiesOfCreatedObject(object createdObj,
      IEnumerable<TargetPropertyDescription> properties)
    {
      foreach (var property in properties) {
        var value = property.SystemProperty.GetValue(createdObj, null);
        if (property.IsCollection) {
          foreach (var item in ExtractModifiedKeys((IEnumerable) value).Values)
            NotifyAboutCollectionModification(property, true, item);
        }
        else if (property.ValueType.ObjectKind==ObjectKind.UserStructure) {
          using (GraphComparer.ComparisonInfo.SaveState()) {
            var structureType = GraphComparer.MappingDescription
              .GetTargetType(property.SystemProperty.PropertyType);
            GraphComparer.ComparisonInfo.StructurePath = ExtendPath(GraphComparer.ComparisonInfo.StructurePath,
              property);
            NotifyAboutValuesOfPropertiesOfCreatedObject(value, GetMutableNonStructureProperties(structureType));
          }
        }
        else
          NotifyAboutPropertySetting(property, value);
      }
    }


    // Constructors

    public NotifyingAboutCreatedObjectsState(GraphComparer graphComparer)
      : base(graphComparer)
    {}
  }
}