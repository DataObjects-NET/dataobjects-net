// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;

namespace Xtensive.Modelling.Tests.DatabaseModel
{
  [Serializable]
  public class TableCollection : NodeCollectionBase<Table, Schema>
  {
    internal TableCollection(Schema parent)
      : base(parent, "Tables")
    {
    }
  }
}