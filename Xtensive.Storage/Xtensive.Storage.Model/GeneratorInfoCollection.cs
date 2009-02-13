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
    public GeneratorInfo this[Type generatorType, KeyInfo keyInfo]
    {
      get
      {
        foreach (var item in this) {
          if (item.Type!=generatorType)
            continue;
          var fields = item.KeyInfo.Fields;
          if (fields.Count != keyInfo.Fields.Count)
            continue;
          for (int i = 0; i < fields.Count; i++) {
            if (fields[i].Key.ValueType != keyInfo.Fields[i].Key.ValueType)
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
}