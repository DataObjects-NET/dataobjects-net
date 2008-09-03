// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.02

using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlSaveProviderData
  {
    public ISqlCompileUnit BeforeEnumerate { get; set; }

    public ISqlCompileUnit AfterEnumerate { get; set; }

    public SqlTableRef Table { get; set; }
  }
}