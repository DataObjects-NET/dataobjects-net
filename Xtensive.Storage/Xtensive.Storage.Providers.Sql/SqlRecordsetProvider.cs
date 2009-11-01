// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.27

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Sql.Dom.Database;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlRecordsetProvider : ProviderImplementation
  {
    private readonly IndexInfo indexInfo;
    private readonly RecordHeader recordHeader;
    private readonly SessionHandler handler;
    private Catalog catalog;
    private SqlSelect query;
    private long? count;

    public IndexInfo Index
    {
      get { return indexInfo; }
    }

//    /// <inheritdoc/>
//    public override long Count
//    {
//      get
//      {
//        if (!count.HasValue) {
//          var countSelect = (SqlSelect) query.Clone();
//          countSelect.Columns.Clear();
//          countSelect.Columns.Add(Xtensive.Sql.Dom.Sql.Count(), "Count");
//          using (var reader = handler.ExecuteReader(countSelect)) {
//            reader.Read();
//            count = (int) reader["Count"];
//          }
//        }
//        return count.Value;
//      }
//    }

    /// <inheritdoc/>
    public override IEnumerator<Tuple> GetEnumerator()
    {
      return new SqlReader(Range<IEntire<Tuple>>.Full, this);
    }

//    public override IIndexReader<Tuple, Tuple> CreateReader(Range<IEntire<Tuple>> range)
//    {
//      return new SqlReader(range, this);
//    }
//
//    public override SeekResult<Tuple> Seek(Ray<IEntire<Tuple>> ray)
//    {
//      return base.Seek(ray);
//    }

    public SessionHandler Handler
    {
      get { return handler; }
    }

    #region Private

    #endregion

    public SqlRecordsetProvider(IndexInfo indexInfo, SessionHandler handler)
      : base(null)
    {
      catalog = handler.DomainHandler.Catalog;
      this.indexInfo = indexInfo;
      this.handler = handler;
      recordHeader = new RecordHeader(indexInfo.Columns, null, null);
      query = handler.DomainHandler.BuildQueryInternal(indexInfo);
    }
  }
}