// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.11.26

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Collections;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A collection of indexes that belongs to a particular <see cref="TypeInfo"/>.
  /// </summary>
  [Serializable]
  public sealed class TypeIndexInfoCollection : IndexInfoCollection
  {
    private IndexInfo primaryIndex;
    private ReadOnlyList<IndexInfo> realPrimaryIndexes;
    private ReadOnlyList<IndexInfo> indexesContainingAllData;

    /// <summary>
    /// Gets the primary index in this instance.
    /// </summary>
    public IndexInfo PrimaryIndex
    {
      [DebuggerStepThrough]
      get { return IsLocked ? primaryIndex : FindPrimaryIndex(); }
    }

    /// <summary>
    /// Gets the list of real primary index in this instance.
    /// </summary>
    public ReadOnlyList<IndexInfo> RealPrimaryIndexes
    {
      [DebuggerStepThrough]
      get {
        return IsLocked 
          ? realPrimaryIndexes
          : new ReadOnlyList<IndexInfo>(FindRealPrimaryIndexes(PrimaryIndex));
      }
    }

    public IndexInfo FindFirst(IndexAttributes indexAttributes)
    {
      var result = Find(indexAttributes);
      if (result.Count!=0) {
        var enumerator = result.GetEnumerator();
        enumerator.MoveNext();
        return enumerator.Current;
      }
      return null;
    }

    [DebuggerStepThrough]
    public IndexInfo GetIndex(string fieldName, params string[] fieldNames)
    {
      var names = new List<string> {fieldName};
      names.AddRange(fieldNames);

      var fields = new List<FieldInfo>();
      foreach (var name in names) {
        FieldInfo field;
        if (primaryIndex.ReflectedType.Fields.TryGetValue(name, out field))
          fields.Add(field);
      }
      if (fields.Count==0)
        return null;

      return GetIndex(fields);
    }

    public IndexInfo GetIndex(FieldInfo field, params FieldInfo[] fields)
    {
      var fieldInfos = new List<FieldInfo> {field};
      fieldInfos.AddRange(fields);
      return GetIndex(fieldInfos);
    }

    /// <inheritdoc/>
    public override void UpdateState(bool recursive)
    {
      base.UpdateState(recursive);
      primaryIndex = FindPrimaryIndex();
      realPrimaryIndexes = new ReadOnlyList<IndexInfo>(FindRealPrimaryIndexes(primaryIndex));
      indexesContainingAllData = new ReadOnlyList<IndexInfo>(FindIndexesContainingAllData());
    }

    private IndexInfo GetIndex(IEnumerable<FieldInfo> fields)
    {
      Action<IEnumerable<FieldInfo>, IList<ColumnInfo>> columnsExtractor = null;
      columnsExtractor = ((fieldsToExtract, extractedColumns) => {
        foreach (var field in fieldsToExtract) {
          if (field.Column==null) {
            if (field.IsEntity || field.IsStructure)
              columnsExtractor(field.Fields, extractedColumns);
          }
          else
            extractedColumns.Add(field.Column);
        }
      });

      var columns = new List<ColumnInfo>();
      columnsExtractor(fields, columns);
      var columnNumber = columns.Count;

      var candidates = this
        .Where(i => i.KeyColumns
          .TakeWhile((_, index) => index < columns.Count)
          .Select((pair, index) => new {column = pair.Key, columnIndex = index})
          .All(p => p.column==columns[p.columnIndex]))
        .OrderByDescending(i => i.Attributes).ToList();

      var result = candidates.Where(c => c.KeyColumns.Count==columnNumber).FirstOrDefault();

      return result ?? candidates.FirstOrDefault();
    }

    /// <summary>
    /// Gets the minimal set of indexes containing all data for the type.
    /// </summary>
    /// <returns></returns>
    public ReadOnlyList<IndexInfo> GetIndexesContainingAllData()
    {
      return IsLocked
        ? indexesContainingAllData
        : new ReadOnlyList<IndexInfo>(FindIndexesContainingAllData());
    }

    private List<IndexInfo> FindIndexesContainingAllData()
    {
      var result = new List<IndexInfo>(Items.Count);
      var virtualIndexes = this.Where(index => index.IsVirtual);
      result.AddRange(virtualIndexes);
      var realIndexes = from index in this where !index.IsVirtual 
                          && (index.Attributes & IndexAttributes.Abstract) == 0
                          && result.Count(virtualIndex => virtualIndex.UnderlyingIndexes.Contains(index)) == 0
                        select index;
      result.AddRange(realIndexes);
      return result;
    }

    private IndexInfo FindPrimaryIndex()
    {
      var result = this.Where(i => i.IsVirtual && i.IsPrimary).OrderByDescending(i => i.Attributes).FirstOrDefault();
      return result ?? FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
    }

    private List<IndexInfo> FindRealPrimaryIndexes(IndexInfo index)
    {
      if (index==null)
        return new List<IndexInfo>();
      if (index.IsPrimary && !index.IsVirtual)
        return new List<IndexInfo>(new[] {index});
      var result = new List<IndexInfo>();
      foreach (IndexInfo underlyingIndex in index.UnderlyingIndexes) {
        if (underlyingIndex.IsPrimary && !underlyingIndex.IsVirtual)
          result.Add(underlyingIndex);
        else
          result.AddRange(FindRealPrimaryIndexes(underlyingIndex));
      }
      return result;
    }


    // Constructors

    /// <inheritdoc/>
    public TypeIndexInfoCollection(Node owner, string name)
      : base(owner, name)
    {
    }
  }
}