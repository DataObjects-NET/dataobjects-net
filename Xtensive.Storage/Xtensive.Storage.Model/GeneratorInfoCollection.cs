// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.13

using System;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class GeneratorInfoCollection : NodeCollection<GeneratorInfo>
  {
    public GeneratorInfo Find(Type generatorType, Type[] keyFieldTypes) 
    {
      foreach (var item in this) {
        if (item.KeyGeneratorType!=generatorType)
          continue;
        var fields = item.KeyInfo.Fields;
        if (fields.Count != keyFieldTypes.Length)
          continue;
        for (int i = 0; i < keyFieldTypes.Length; i++) {
          if (fields[i].Key.ValueType != keyFieldTypes[i])
            goto Next;
        }
        return item;
      Next:
        continue;
      }
      return null;
    }
  }
}