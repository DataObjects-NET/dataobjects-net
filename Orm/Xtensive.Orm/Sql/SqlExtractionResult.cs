// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.06

using Xtensive.Sql.Model;

namespace Xtensive.Sql
{
  /// <summary>
  /// Result of SQL schema extraction.
  /// </summary>
  public sealed class SqlExtractionResult
  {
    public NodeCollection<Catalog> Catalogs { get; set; }

    // Constructors

    public SqlExtractionResult()
    {
      Catalogs = new NodeCollection<Catalog>();
    }
  }
}