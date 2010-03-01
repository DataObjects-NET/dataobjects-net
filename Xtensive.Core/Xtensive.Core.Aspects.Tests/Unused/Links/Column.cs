// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.01

using Xtensive.Core.Aspects;
using Xtensive.Core.Links;
using Xtensive.Core.Links.LinkedReference;

namespace Xtensive.Core.Aspects.Tests.Links
{
  public class Column: ILinkOwner
  {
    [Link("Table", "Columns", LinkType.OneToMany)]
    protected LinkedReference<Column, Table> table;

    public Table Table
    {
      get { return table.Value; }
      set { table.Value = value; }
    }

    #region ILinkOwner Members

    public LinkedOperationRegistry Operations
    {
      get { return null; }
    }

    #endregion

    public Column()
    {
      table = new LinkedReference<Column, Table>(this, "Table");
    }
  }
}