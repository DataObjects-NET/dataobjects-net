// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Declaration;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  public class IndexProviderCompiler : ProviderCompiler<IndexProvider>
  {
    protected override Provider Compile(IndexProvider provider)
    {
      var index = provider.Index;
      if (index.IsVirtual) {
        if ((index.Attributes & IndexAttributes.Union) > 0)
          return BuildUnionProvider(index);
        if ((index.Attributes & IndexAttributes.Join) > 0)
          return BuildJoinProvider(index);
        if ((index.Attributes & IndexAttributes.Filtered) > 0)
          return BuildFilteredProvider(index);
        throw new NotSupportedException(String.Format(Strings.ExUnsupportedIndex, index.Name, index.Attributes));
      }
      return BuildRealProvider(index);
//      SqlTableRef tableRef = GetTableRef(index);
//      return new QueryBuildResult(tableRef, null, tableRef, GetSqlColumns(index.Columns, tableRef));
    }

    private Provider BuildRealProvider(IndexInfo index)
    {
      throw new NotImplementedException();
    }

    private Provider BuildUnionProvider(IndexInfo index)
    {
      throw new NotImplementedException();
    }

    private Provider BuildJoinProvider(IndexInfo index)
    {
      throw new NotImplementedException();
    }

    private Provider BuildFilteredProvider(IndexInfo index)
    {
      throw new NotImplementedException();
    }

  }
}