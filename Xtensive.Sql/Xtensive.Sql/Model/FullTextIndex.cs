// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2010.01.14

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Model
{
  [Serializable]
  public class FullTextIndex : Index
  {
    private string fullTextCatalog;
    private string underlyingUniqueIndex;

    /// <inheritdoc/>
    public override bool IsFullText { get { return true; } }

    /// <summary>
    /// Gets or sets the full text catalog.
    /// </summary>
    public string FullTextCatalog
    {
      get { return fullTextCatalog; }
      set
      {
        this.EnsureNotLocked();
        fullTextCatalog = value;
      }
    }

    /// <summary>
    /// Gets or sets the underlying unique index name.
    /// </summary>
    public string UnderlyingUniqueIndex
    {
      get { return underlyingUniqueIndex; }
      set
      {
        this.EnsureNotLocked();
        underlyingUniqueIndex = value;
      }
    }

    /// <summary>
    /// Creates the full-text index column.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="language">The language.</param>
    /// <returns></returns>
    public IndexColumn CreateIndexColumn(DataTableColumn column, string language)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      return new IndexColumn(this, column, language);
    }



    internal FullTextIndex(DataTable dataTable, string name)
      : base(dataTable, name)
    {
    }
  }
}