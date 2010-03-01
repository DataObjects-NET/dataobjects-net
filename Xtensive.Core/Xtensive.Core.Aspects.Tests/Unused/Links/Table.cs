// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.01

using Xtensive.Core.Aspects;
using Xtensive.Core.Links;
using Xtensive.Core.Links.LinkedSet;

namespace Xtensive.Core.Aspects.Tests.Links
{
  public class Table: ILinkOwner
  {
    [Link("Columns", "Table", LinkType.OneToMany)]
    private LinkedSet<Table, Column> columns;

    public LinkedSet<Table, Column> Columns
    {
      get { return columns; }
    }

    #region ILinkOwner Members

    public LinkedOperationRegistry Operations
    {
      get { return null; }
    }

    #endregion

    public Table()
    {
      columns = new LinkedSet<Table, Column>(this, "Columns");
    }
  }
}