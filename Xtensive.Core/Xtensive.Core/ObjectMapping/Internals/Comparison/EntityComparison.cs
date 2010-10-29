// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.22

namespace Xtensive.ObjectMapping.Comparison
{
  internal sealed class EntityComparison : ComparerStateBase
  {
    public void Compare(object original, object modified)
    {
      var description = GraphComparer.MappingDescription.GetTargetType(modified.GetType());
      using (GraphComparer.ComparisonInfo.SaveState()) {
        GraphComparer.ComparisonInfo.Owner = original;
        CompareComplexProperties(original, modified, description);
        ComparePrimitiveProperties(original, modified, description);
      }
    }


    // Constructors

    public EntityComparison(GraphComparer graphComparer)
      : base(graphComparer)
    {}
  }
}