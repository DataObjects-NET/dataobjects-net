// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.SQLite.v3
{
  [Serializable]
  internal class DataTableProxy
  {
    public DataTable Table;

    private List<Pair<int>> columnMappings;

    public void RegisterColumnMapping(int dbColumnIndex, int actualColumnIndex)
    {
      if (columnMappings == null)
        columnMappings = new List<Pair<int>>(1);

      columnMappings.Add(new Pair<int>(dbColumnIndex, actualColumnIndex));
    }

    public DataTableColumn GetColumn(int dbColumnIndex)
    {
      int actualColumnIndex = dbColumnIndex-1;
      var view = Table as View;
      if (view != null)
        return view.ViewColumns[dbColumnIndex];

      var table = (Table)Table;
      if (columnMappings == null || columnMappings[0].First > dbColumnIndex)
        return table.TableColumns[actualColumnIndex];

      int count = columnMappings.Count;
      for (int i = 0; i < count; i++) {
        if (columnMappings[i].First == dbColumnIndex)
          return table.TableColumns[columnMappings[i].Second];
      }
      throw new ArgumentOutOfRangeException("dbColumnIndex");
    }

    public DataTableProxy(DataTable table)
    {
      Table = table;
    }
  }
}