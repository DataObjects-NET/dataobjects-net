// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.12.27

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Maps type fields to interface fields and vice versa.
  /// </summary>
  [Serializable]
  public sealed class FieldMap: LockableBase, IEnumerable<KeyValuePair<FieldInfo, FieldInfo>>
  {
    private readonly Dictionary<FieldInfo, FieldInfo> map = new Dictionary<FieldInfo, FieldInfo>();
    private readonly Dictionary<FieldInfo, HashSet<FieldInfo>> reversedMap = new Dictionary<FieldInfo, HashSet<FieldInfo>>();
    internal static FieldMap Empty;

    /// <summary>
    /// Gets the <see cref="Xtensive.Orm.Model.FieldInfo"/> with the specified interface field.
    /// </summary>
    public FieldInfo this[FieldInfo interfaceField]
    {
      get { return map[interfaceField]; }
    }

    /// <summary>
    /// Gets the implemented interface fields.
    /// </summary>
    /// <param name="typeField">The type field.</param>
    /// <returns></returns>
    public IEnumerable<FieldInfo> GetImplementedInterfaceFields(FieldInfo typeField)
    {
      HashSet<FieldInfo> value;
      if (!reversedMap.TryGetValue(typeField, out value))
        return Enumerable.Empty<FieldInfo>();
      return value;
    }

    /// <summary>
    /// Gets the count.
    /// </summary>
    public int Count
    {
      get { return map.Count; }
    }

    /// <summary>
    /// Determines whether the specified interface field contains key.
    /// </summary>
    /// <param name="interfaceField">The interface field.</param>
    /// <returns>
    ///   <c>true</c> if the specified interface field contains key; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsKey(FieldInfo interfaceField)
    {
      return map.ContainsKey(interfaceField);
    }

    /// <summary>
    /// Adds the specified interface field.
    /// </summary>
    /// <param name="interfaceField">The interface field.</param>
    /// <param name="typeField">The type field.</param>
    public void Add(FieldInfo interfaceField, FieldInfo typeField)
    {
      this.EnsureNotLocked();
      map.Add(interfaceField, typeField);
      if (reversedMap.ContainsKey(typeField)) {
        var interfaceFields = reversedMap[typeField];
        interfaceFields.Add(interfaceField);
      }
      else
        reversedMap.Add(typeField, new HashSet<FieldInfo> {interfaceField});
    }

    /// <summary>
    /// Overrides the specified interface field.
    /// </summary>
    /// <param name="interfaceField">The interface field.</param>
    /// <param name="typeField">The type field.</param>
    public void Override(FieldInfo interfaceField, FieldInfo typeField)
    {
      this.EnsureNotLocked();
      var oldTypeField = map[interfaceField];
      var interfaceFields = reversedMap[oldTypeField];
      map[interfaceField] = typeField;
      reversedMap.Remove(oldTypeField);
      reversedMap.Add(typeField, interfaceFields);
    }

    /// <summary>
    /// Tries the get value.
    /// </summary>
    /// <param name="interfaceField">The interface field.</param>
    /// <param name="typeField">The type field.</param>
    /// <returns></returns>
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
    }
  }
}