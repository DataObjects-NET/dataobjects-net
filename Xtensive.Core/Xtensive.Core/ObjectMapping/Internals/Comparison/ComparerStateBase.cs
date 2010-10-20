// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.22

using System;
using Xtensive.Core;
using Xtensive.Resources;
using Xtensive.ObjectMapping.Model;

namespace Xtensive.ObjectMapping.Comparison
{
  internal abstract class ComparerStateBase
  {
    protected readonly GraphComparer GraphComparer;

    //public abstract void Compare(object original, object modified, ComparisonInfo comparisonInfo);

    protected void CompareComplexProperties(object original, object modified, TargetTypeDescription description)
    {
      using (GraphComparer.ComparisonInfo.SaveState()) {
        foreach (var property in description.ComplexProperties.Values) {
          if (property.IsChangeTrackingDisabled)
            continue;
          var modifiedValue = property.SystemProperty.GetValue(modified, null);
          var originalValue = property.SystemProperty.GetValue(original, null);
          if (modifiedValue==null && originalValue==null)
            continue;
          GraphComparer.Compare(originalValue, modifiedValue, property, GraphComparer.ComparisonInfo);
        }
      }
    }

    protected void ComparePrimitiveProperties(object original, object modified,
      TargetTypeDescription description)
    {
      foreach (var property in description.PrimitiveProperties.Values) {
        if (property.IsChangeTrackingDisabled)
            continue;
        var systemProperty = property.SystemProperty;
        var modifiedValue = systemProperty.GetValue(modified, null);
        var originalValue = systemProperty.GetValue(original, null);
        if (!Equals(modifiedValue, originalValue))
          NotifyAboutPropertySetting(property, modifiedValue);
      }
    }

    protected void NotifyAboutPropertySetting(TargetPropertyDescription property, object value)
    {
      var path = GetFullPath(property);
      GraphComparer.Subscriber.Invoke(new Operation(GraphComparer.ComparisonInfo.Owner,
        OperationType.SetProperty, path, value));
    }

    protected TargetPropertyDescription[] GetFullPath(TargetPropertyDescription property)
    {
      if (GraphComparer.ComparisonInfo.StructurePath != null)
        return ExtendPath(GraphComparer.ComparisonInfo.StructurePath, property);
      return new[] {property};
    }

    protected static TargetPropertyDescription[] ExtendPath(TargetPropertyDescription[] path,
      TargetPropertyDescription property)
    {
      TargetPropertyDescription[] result;
      if (path==null)
        result = new[] {property};
      else {
        result = new TargetPropertyDescription[path.Length + 1];
        Array.Copy(path, result, path.Length);
        result[result.Length - 1] = property;
      }
      return result;
    }


    // Constructors

    protected ComparerStateBase(GraphComparer graphComparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(graphComparer, "graphComparer");

      GraphComparer = graphComparer;
    }
  }
}