// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.08

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.ObjectMapping.Model;
using Xtensive.Resources;

namespace Xtensive.ObjectMapping
{
  internal sealed class ObjectExtractor
  {
    private readonly MappingDescription mappingDescription;
    private Queue<object> referencedObjects;

    public void Extract(object root, Dictionary<object,object> resultContainer)
    {
      if (root == null)
        return;
      referencedObjects = new Queue<object>();
      InitializeExtraction(root);
      while (referencedObjects.Count > 0) {
        var current = referencedObjects.Dequeue();
        if (current == null)
          continue;
        var currentType = mappingDescription.GetTargetType(current.GetType());
        switch (currentType.ObjectKind) {
        case ObjectKind.Primitive:
          continue;
        case ObjectKind.Entity:
          if (!AddEntity(current, resultContainer))
            continue;
          goto case ObjectKind.UserStructure;
        case ObjectKind.UserStructure:
          VisitComplexProperties(current, currentType);
          break;
        default:
          throw new ArgumentOutOfRangeException("currentType.ObjectKind");
        }
      }
    }

    private void VisitComplexProperties(object current, TargetTypeDescription currentType)
    {
      foreach (var property in currentType.ComplexProperties.Values) {
        var value = property.SystemProperty.GetValue(current, null);
        if (value==null)
          continue;
        if (!property.IsCollection)
          referencedObjects.Enqueue(value);
        else
          foreach (var obj in (IEnumerable) value)
            referencedObjects.Enqueue(obj);
      }
    }

    private void InitializeExtraction(object root)
    {
      var type = root.GetType();
      if (MappingHelper.IsCollection(type))
          foreach (var obj in (IEnumerable) root) {
            if (obj!=null && MappingHelper.IsCollection(obj.GetType()))
              throw new ArgumentException(Strings.ExNestedCollectionIsNotSupported, "root");
            referencedObjects.Enqueue(obj);
          }
      else
        referencedObjects.Enqueue(root);
    }

    private bool AddEntity(object entity, IDictionary<object, object> resultContainer)
    {
      var key = mappingDescription.ExtractTargetKey(entity);
      if (resultContainer.ContainsKey(key))
        return false;
      resultContainer.Add(key, entity);
      return true;
    }


    // Constructors

    public ObjectExtractor(MappingDescription mappingDescription)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingDescription, "mappingDescription");

      this.mappingDescription = mappingDescription;
    }
  }
}