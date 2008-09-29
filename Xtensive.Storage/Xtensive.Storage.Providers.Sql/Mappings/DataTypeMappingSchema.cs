// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.24

using System;
using System.Collections.Generic;

namespace Xtensive.Storage.Providers.Sql.Mappings
{
  [Serializable]
  public sealed class DataTypeMappingSchema
  {
    private readonly Dictionary<Type, DataTypeMapping> exactMappings = new Dictionary<Type, DataTypeMapping>();
    private readonly Dictionary<Type, List<DataTypeMapping>> ambigiousMappings = new Dictionary<Type, List<DataTypeMapping>>();

    internal void Register(DataTypeMapping mapping)
    {
      DataTypeMapping exactMapping;
      if (exactMappings.TryGetValue(mapping.Type, out exactMapping)) {
        ambigiousMappings.Add(mapping.Type, new List<DataTypeMapping> {exactMapping, mapping});
        exactMappings.Remove(mapping.Type);
        return;
      }
      List<DataTypeMapping> ambigiousMapping;
      if (ambigiousMappings.TryGetValue(mapping.Type, out ambigiousMapping)) {
        ambigiousMapping.Add(mapping);
        return;
      }
      exactMappings.Add(mapping.Type, mapping);
    }

    public DataTypeMapping GetExactMapping(Type type)
    {
      DataTypeMapping result;
      exactMappings.TryGetValue(type, out result);
      return result;
    }

    public DataTypeMapping[] GetAmbigiousMappings(Type type)
    {
      List<DataTypeMapping> result;
      if (ambigiousMappings.TryGetValue(type, out result))
        return result.ToArray();
      return null;
    }
  }
}