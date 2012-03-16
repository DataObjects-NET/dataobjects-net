// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.22

using System;
using System.Collections;
using Xtensive.ObjectMapping.Model;
using Xtensive.Resources;

namespace Xtensive.ObjectMapping.Comparison
{
  internal sealed class CollectionPropertyComparison : ProcessingCollectionState
  {
    public void Compare(object originalValue, object modifiedValue, TargetPropertyDescription property)
    {
      switch (property.ValueType.ObjectKind) {
      case ObjectKind.Primitive:
        ComparePrimitiveCollections(originalValue, modifiedValue, property);
        break;
      case ObjectKind.UserStructure:
        throw new InvalidOperationException(Strings.ExDetectionOfChangesInUserStructureCollectionIsNotSupported);
      case ObjectKind.Entity:
        CompareEntityCollections(originalValue, modifiedValue, property);
        break;
      default:
        throw new ArgumentOutOfRangeException("property.ValueType.ObjectKind");
      }
    }

    private void CompareEntityCollections(object originalValue, object modifiedValue,
      TargetPropertyDescription property)
    {
      var originalKeys = ExtractOriginalKeys((IEnumerable) originalValue);
      var modifiedKeys = ExtractModifiedKeys((IEnumerable) modifiedValue);
      foreach (var objPair in modifiedKeys)
        if (!originalKeys.ContainsKey(objPair.Key))
          NotifyAboutCollectionModification(property, true, objPair.Value);
      foreach (var objPair in originalKeys)
        if (!modifiedKeys.ContainsKey(objPair.Key))
          NotifyAboutCollectionModification(property, false, objPair.Value);
    }

    private void ComparePrimitiveCollections(object originalValue, object modifiedValue,
      TargetPropertyDescription property)
    {
      if (originalValue==null && modifiedValue==null)
        return;
      if (originalValue==null ^ modifiedValue==null) {
        NotifyAboutPropertySetting(property, modifiedValue);
        return;
      }
      var originalEnumerator = ((IEnumerable) originalValue).GetEnumerator();
      var modifiedEnumerator = ((IEnumerable) modifiedValue).GetEnumerator();
      using (originalEnumerator as IDisposable) {
        using (modifiedEnumerator as IDisposable) {
          while (originalEnumerator.MoveNext()) {
            if (!modifiedEnumerator.MoveNext() || !Equals(originalEnumerator.Current, modifiedEnumerator.Current)) {
              NotifyAboutPropertySetting(property, modifiedValue);
              return;
            }
          }
        }
        if (originalEnumerator.MoveNext())
          NotifyAboutPropertySetting(property, modifiedValue);
      }
    }


    // Constructors

    public CollectionPropertyComparison(GraphComparer graphComparer)
      : base(graphComparer)
    {}
  }
}