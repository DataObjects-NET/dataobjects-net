// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.22

using Xtensive.ObjectMapping.Model;

namespace Xtensive.ObjectMapping.Comparison
{
  internal sealed class UserStructurePropertyComparison : ComparerStateBase
  {
    public void Compare(object originalValue, object modifiedValue, TargetPropertyDescription property)
    {
      using (GraphComparer.ComparisonInfo.SaveState()) {
        GraphComparer.ComparisonInfo.StructurePath = ExtendPath(GraphComparer.ComparisonInfo.StructurePath,
          property);
        var structurePath = GraphComparer.ComparisonInfo.StructurePath;
        var structureSystemType = structurePath[structurePath.Length - 1].SystemProperty.PropertyType;
        var structureType = GraphComparer.MappingDescription.GetTargetType(structureSystemType);
        ComparePrimitiveProperties(originalValue, modifiedValue, structureType);
        CompareComplexProperties(originalValue, modifiedValue, structureType);
      }
    }


    // Constructors

    public UserStructurePropertyComparison(GraphComparer graphComparer)
      : base(graphComparer)
    {}
  }
}