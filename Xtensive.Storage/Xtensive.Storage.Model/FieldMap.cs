// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.12.27

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class FieldMap: LockableBase, IEnumerable<KeyValuePair<FieldInfo, FieldInfo>>
  {
    private readonly Dictionary<FieldInfo, FieldInfo> map = new Dictionary<FieldInfo, FieldInfo>();
    private readonly Dictionary<FieldInfo, IList<FieldInfo>> reversedMap = new Dictionary<FieldInfo, IList<FieldInfo>>();
    internal static FieldMap Empty;

    public FieldInfo this[FieldInfo interfaceField]
    {
      get { return map[interfaceField]; }
      set { Add(interfaceField, value); }
    }

    public IEnumerable<FieldInfo> GetImplementedInterfaceFields(FieldInfo typeField)
    {
      return reversedMap[typeField];
    }

    public int Count
    {
      get { return map.Count; }
    }

    public bool ContainsKey(FieldInfo interfaceField)
    {
      return map.ContainsKey(interfaceField);
    }

    public void Add(FieldInfo interfaceField, FieldInfo typeField)
    {
      this.EnsureNotLocked();
      map.Add(interfaceField, typeField);
      if (reversedMap.ContainsKey(typeField)) {
        var interfaceFields = reversedMap[typeField];
        interfaceFields.Add(interfaceField);
      }
      else
        reversedMap.Add(typeField, new List<FieldInfo> {interfaceField});
    }

    public bool TryGetValue(FieldInfo interfaceField, out FieldInfo typeField)
    {
      return map.TryGetValue(interfaceField, out typeField);
    }

    IEnumerator<KeyValuePair<FieldInfo, FieldInfo>> IEnumerable<KeyValuePair<FieldInfo, FieldInfo>>.GetEnumerator()
    {
      return map.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<KeyValuePair<FieldInfo, FieldInfo>>)this).GetEnumerator();
    }


    // Constructors

    internal FieldMap()
    {
    }

    static FieldMap()
    {
      Empty = new FieldMap();
      Empty.Lock();
    }
  }
}