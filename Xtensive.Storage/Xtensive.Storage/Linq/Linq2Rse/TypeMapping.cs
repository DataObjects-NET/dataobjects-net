// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.24

using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  internal class TypeMapping
  {
    public Dictionary<string, Segment<int>> FieldMapping { get; private set; }
    public Dictionary<string, TypeMapping> JoinedRelations { get; private set; }


    // Constructors

    public TypeMapping(Dictionary<string, Segment<int>> fieldMapping, Dictionary<string, TypeMapping> joinedRelations)
    {
      FieldMapping = fieldMapping;
      JoinedRelations = joinedRelations;
    }
  }
}