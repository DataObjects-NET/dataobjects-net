// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.07.04

using System;

namespace Xtensive.Storage.Providers.Sql
{
  public struct StatementMapping
  {
    private readonly int tablePosition;
    private readonly int tuplePosition;

    public int TablePosition
    {
      get { return tablePosition; }
    }

    public int TuplePosition
    {
      get { return tuplePosition; }
    }

    public StatementMapping(int tablePosition, int tuplePosition)
    {
      this.tablePosition = tablePosition;
      this.tuplePosition = tuplePosition;
    }
  }
}