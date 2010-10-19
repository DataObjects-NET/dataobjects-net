// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.23

using Xtensive.ObjectMapping.Model;

namespace Xtensive.ObjectMapping.Comparison
{
  internal sealed class EntityReferencePropertyComparison : ComparerStateBase
  {
    public void Compare(object originalValue, object modifiedValue, TargetPropertyDescription property)
    {
      if (originalValue == null || modifiedValue == null)
        NotifyAboutPropertySetting(property, modifiedValue);
      else {
        var modifiedKey = GraphComparer.MappingDescription.ExtractTargetKey(modifiedValue);
        var originalKey = GraphComparer.MappingDescription.ExtractTargetKey(originalValue);
        if (!modifiedKey.Equals(originalKey))
          NotifyAboutPropertySetting(property, modifiedValue);
      }
    }


    // Constructors

    public EntityReferencePropertyComparison(GraphComparer graphComparer)
      : base(graphComparer)
    {}
  }
}