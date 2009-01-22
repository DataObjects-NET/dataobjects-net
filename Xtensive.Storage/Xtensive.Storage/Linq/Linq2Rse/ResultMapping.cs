// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.24

using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  internal class ResultMapping
  {
    public Dictionary<string, Segment<int>> Fields { get; private set; }
    public Dictionary<string, ResultMapping> JoinedRelations { get; private set; }


    // Constructors

    public ResultMapping(Dictionary<string, Segment<int>> fieldMapping, Dictionary<string, ResultMapping> joinedRelations)
    {
      Fields = fieldMapping;
      JoinedRelations = joinedRelations;
    }
  }
}