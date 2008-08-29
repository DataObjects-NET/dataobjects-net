// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.31

using System.Collections.Generic;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;
using ColumnGroup=Xtensive.Storage.Rse.ColumnGroup;

namespace Xtensive.Storage.Internals
{
  internal static class RecordSetHeaderExtensions
  {
    public static RecordSetMapping Parse(this RecordSetHeader header, RecordSetHeaderParsingContext context)
    {
      List<HierarchyMapping> mappings = new List<HierarchyMapping>();
      foreach (ColumnGroup group in context.Header.ColumnGroups) {
        HierarchyMapping mapping = Parse(context, group);
        if (mapping != null)
          mappings.Add(mapping);
      }
      return new RecordSetMapping(mappings);
    }

    private static HierarchyMapping Parse(RecordSetHeaderParsingContext context, ColumnGroup group)
    {
      int typeIdIndex = -1;
      Dictionary<ColumnInfo, Column> columnMapping = new Dictionary<ColumnInfo, Column>(group.Columns.Count);

      foreach (int columnIndex in group.Columns) {
        Column column = context.Header.Columns[columnIndex];
        ColumnInfo columnInfo = column.ColumnInfoRef.Resolve(context.Domain.Model);
        columnMapping[columnInfo] = column;
        if (columnInfo.Name==NameBuilder.TypeIdFieldName)
          typeIdIndex = column.Index;
      }

      if (typeIdIndex == -1)
        return null;

      return new HierarchyMapping(typeIdIndex, columnMapping);
    }
  }
}