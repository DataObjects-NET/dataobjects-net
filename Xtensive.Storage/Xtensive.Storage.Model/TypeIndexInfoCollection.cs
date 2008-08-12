// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.11.26

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class TypeIndexInfoCollection : IndexInfoCollection
  {
    private IndexInfo primaryIndex;

    /// <summary>
    /// Gets the primary index in this instance.
    /// </summary>
    public IndexInfo PrimaryIndex {
      [DebuggerStepThrough]
      get { return IsLocked ? primaryIndex : FindPrimaryIndex(); }
    }

    private IndexInfo FindPrimaryIndex()
    {
      IndexInfo result = FindFirst(IndexAttributes.Virtual | IndexAttributes.Primary);
      if (result != null) 
        return result;
      return FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
    }

    public IndexInfo FindFirst(IndexAttributes indexAttributes)
    {
      ICountable<IndexInfo> result = Find(indexAttributes);
      if (result.Count != 0)
      {
        IEnumerator<IndexInfo> enumerator = result.GetEnumerator();
        enumerator.MoveNext();
        return enumerator.Current;
      }
      return null;
    }

    public IndexInfo GetIndex(string fieldName, params string[] fieldNames)
    {
      if (!IsLocked)
        throw new InvalidOperationException();

      var names = new List<string>();
      names.Add(fieldName);
      names.AddRange(fieldNames);

      var fields = new List<FieldInfo>();
      foreach (var name in names) {
        FieldInfo field = primaryIndex.ReflectedType.Fields.TryGetValue(name);
        if (field != null)
          fields.Add(field);
      }
      if (fields.Count == 0)
        return null;

      Action<IEnumerable<FieldInfo>, IList<ColumnInfo>> columnsExtractor = null;
      columnsExtractor = ((fieldsToExtract, extractedColumns) => {
        foreach (var field in fieldsToExtract) {
          if (field.Column==null) {
            if (field.IsEntity)
              columnsExtractor(field.Fields, extractedColumns);
          }
          else
            extractedColumns.Add(field.Column);
        }
      });

      var columns = new List<ColumnInfo>();
      columnsExtractor(fields, columns);

      var result = this
        .Where(i => i.KeyColumns
          .TakeWhile((_, index) => index < columns.Count)
          .Select((pair, index) => new {column = pair.Key, columnIndex = index})
          .All(p => p.column==columns[p.columnIndex]))
        .OrderByDescending(i => i.IsVirtual)
        .FirstOrDefault();

      return result;
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      primaryIndex = FindPrimaryIndex();
      base.Lock(recursive);
    }
  }
}