// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.09.25

using System;

namespace Xtensive.Sql.Model
{
  public class SpatialIndex : Index
  {
    public override bool IsSpatial
    {
      get { return true; }
    }

    internal SpatialIndex(DataTable dataTable, string name)
      : base(dataTable, name)
    {
    }
  }
}