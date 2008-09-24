// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.24

using System;
using System.Collections.Generic;
using Xtensive.Sql.Common;

namespace Xtensive.Storage.Providers.Sql.Mappings
{
  [Serializable]
  public sealed class ValueTypeMappingSchema
  {
    private readonly Dictionary<Type, DataTypeInfo> exactMappings = new Dictionary<Type, DataTypeInfo>();
    private readonly Dictionary<Type, List<DataTypeInfo>> ambigiousMappings = new Dictionary<Type, List<DataTypeInfo>>();

    internal void RegisterMapping (Type type, DataTypeInfo dataTypeInfo)
    {
      DataTypeInfo exactMapping;
      if (exactMappings.TryGetValue(type, out exactMapping)) {
        ambigiousMappings.Add(type, new List<DataTypeInfo> {exactMapping, dataTypeInfo});
        exactMappings.Remove(type);
        return;
      }
      List<DataTypeInfo> ambigiousMapping;
      if (ambigiousMappings.TryGetValue(type, out ambigiousMapping)) {
        ambigiousMapping.Add(dataTypeInfo);
        return;
      }
      exactMappings.Add(type, dataTypeInfo);
    }

    public DataTypeInfo GetExactMapping(Type type)
    {
      DataTypeInfo result;
      exactMappings.TryGetValue(type, out result);
      return result;
    }

    public DataTypeInfo[] GetAmbigiousMappings(Type type)
    {
      List<DataTypeInfo> result;
      if (ambigiousMappings.TryGetValue(type, out result))
        return result.ToArray();
      return null;
    }
  }
}