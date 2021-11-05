// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.12.27

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Maps type fields to interface fields and vice versa.
  /// </summary>
  [Serializable]
  public sealed class FieldMap: LockableBase, IEnumerable<KeyValuePair<FieldInfo, FieldInfo>>
  {
    internal static readonly FieldMap Empty;

    private readonly Dictionary<FieldInfo, FieldInfo> map = new Dictionary<FieldInfo, FieldInfo>();
    private readonly Dictionary<FieldInfo, HashSet<FieldInfo>> reversedMap = new Dictionary<FieldInfo, HashSet<FieldInfo>>();

    public FieldInfo this[FieldInfo interfaceField]
    {
      get { return map[interfaceField]; }
    }

    public IEnumerable<FieldInfo> GetImplementedInterfaceFields(FieldInfo typeField)
    {
      HashSet<FieldInfo> value;
      if (!reversedMap.TryGetValue(typeField, out value))
        return Enumerable.Empty<FieldInfo>();
      return value;
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
      if (reversedMap.TryGetValue(typeField, out var interfaceFields)) {
        interfaceFields.Add(interfaceField);
      }
      else
        reversedMap.Add(typeField, new HashSet<FieldInfo> {interfaceField});
    }

    public void Override(FieldInfo interfaceField, FieldInfo typeField)
    {
      this.EnsureNotLocked();
      var oldTypeField = map[interfaceField];
      var interfaceFields = reversedMap[oldTypeField];
      map[interfaceField] = typeField;
      reversedMap.Remove(oldTypeField);
      reversedMap.Add(typeField, interfaceFields);
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
      return ((IEnumerable<KeyValuePair<FieldInfo, FieldInfo>>) this).GetEnumerator();
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