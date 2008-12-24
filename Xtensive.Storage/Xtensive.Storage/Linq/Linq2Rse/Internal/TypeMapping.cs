// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.24

using System.Collections.Generic;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq.Linq2Rse.Internal
{
  internal class TypeMapping
  {
    public TypeInfo Type { get; private set; }
    public Dictionary<string, int> FieldMapping { get; private set; }
    public Dictionary<string, TypeMapping> JoinedRelations { get; private set; }

    public TypeMapping(TypeInfo type, Dictionary<string, int> fieldMapping, Dictionary<string, TypeMapping> joinedRelations)
    {
      Type = type;
      FieldMapping = fieldMapping;
      JoinedRelations = joinedRelations;
    }
  }
}