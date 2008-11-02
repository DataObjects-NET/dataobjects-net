// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.03

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Building;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal class PrototypeProvider
  {
    private readonly Dictionary<TypeInfo, Tuple> cache = new Dictionary<TypeInfo, Tuple>();

    /// <exception cref="InvalidOperationException">Prototype for specified type is not found.</exception>
    public Tuple this[TypeInfo type]
    {
      get
      {
        Tuple result;
        if (cache.TryGetValue(type, out result))
          return result;
        throw new InvalidOperationException(string.Format("Prototype for type '{0}' is not found."));
      }
    }

    public void Build()
    {
      var domain = BuildingContext.Current.Domain;
      foreach (TypeInfo type in domain.Model.Types.Where(t => !t.IsInterface)) {
        BitArray nullableMap = new BitArray(type.TupleDescriptor.Count);
        int i = 0;
        foreach (ColumnInfo column in type.Columns)
          nullableMap[i++] = column.IsNullable;
        Tuple prototype = Tuple.Create(type.TupleDescriptor);
        prototype.Initialize(nullableMap);
        if (type.IsEntity) {
          FieldInfo typeIdField = type.Fields[domain.NameBuilder.TypeIdFieldName];
          prototype.SetValue(typeIdField.MappingInfo.Offset, type.TypeId);
        }
        LogTemplate<Log>.Info("Type '{0}': {1}", type, prototype);
        cache[type] = prototype;
      }
    }
  }
}