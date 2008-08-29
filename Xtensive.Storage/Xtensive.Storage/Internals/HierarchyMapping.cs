// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal class HierarchyMapping
  {
    public int TypeIdIndex { get; private set; }

    public IDictionary<ColumnInfo, Column> ColumnInfoMapping { get; private set; }

    public ThreadSafeDictionary<TypeInfo, TypeMapping> TypeMappings { get; private set; }

    public TypeMapping GetTypeMapping(RecordSetHeaderParsingContext context, Tuple tuple)
    {
      throw new NotImplementedException();
    }


    // Constructors

    public HierarchyMapping(int typeIdIndex, Dictionary<ColumnInfo, Column> columnMapping)
    {
      TypeIdIndex = typeIdIndex;
      TypeMappings = new ThreadSafeDictionary<TypeInfo, TypeMapping>();
      ColumnInfoMapping = new ReadOnlyDictionary<ColumnInfo, Column>(columnMapping);
    }
  }
}