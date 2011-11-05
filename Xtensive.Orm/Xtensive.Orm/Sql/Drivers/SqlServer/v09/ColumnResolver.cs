// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.12

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.SqlServer.v09
{
  [Serializable]
  internal class ColumnResolver
  {
    public DataTable Table;
    private List<ColumnIndexMapping> columnMappings;

    private class ColumnIndexMapping
    {
      public readonly int DbIndex;
      public readonly int ModelIndex;

      public ColumnIndexMapping(int dbIndex, int modelIndex)
      {
        DbIndex = dbIndex;
        ModelIndex = modelIndex;
      }
    }

    public void RegisterColumnMapping(int dbIndex, int modelIndex)
    {
      if (columnMappings == null)
        columnMappings = new List<ColumnIndexMapping>(1);

      columnMappings.Add(new ColumnIndexMapping(dbIndex, modelIndex));
    }

    public DataTableColumn GetColumn(int dbIndex)
    {
      int modelIndex = dbIndex-1;
      var view = Table as View;
      if (view != null)
        return view.ViewColumns[modelIndex];

      var table = (Table)Table;
      if (columnMappings == null)
        return table.TableColumns[modelIndex];

      var mapping = columnMappings.Where(item => item.DbIndex==dbIndex).FirstOrDefault();
      if (mapping != null)
        return table.TableColumns[mapping.ModelIndex];

      throw new ArgumentOutOfRangeException("dbIndex");
    }

    public ColumnResolver(DataTable table)
    {
      Table = table;
    }
  }
}