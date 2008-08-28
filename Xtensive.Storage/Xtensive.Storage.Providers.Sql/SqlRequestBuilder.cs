// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.28

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlRequestBuilder
  {
    public DomainHandler DomainHandler;

    public SqlModificationRequest BuildInsertRequest(TypeInfo type)
    {
      SqlBatch batch = SqlFactory.Batch();
      SqlModificationRequest request = new SqlModificationRequest(batch);
      var parameterMapping = new Dictionary<ColumnInfo, SqlParameter>();
      int j = 0;
      foreach (IndexInfo primaryIndex in type.AffectedIndexes.Where(i => i.IsPrimary)) {
        SqlTableRef tableRef = SqlFactory.TableRef(DomainHandler.GetTable(primaryIndex));
        SqlInsert query = SqlFactory.Insert(tableRef);
        for (int i = 0; i < primaryIndex.Columns.Count; i++) {
          ColumnInfo column = primaryIndex.Columns[i];
          int offset = type.Fields[column.Field.Name].MappingInfo.Offset;
          SqlParameter p;
          if (!parameterMapping.TryGetValue(column, out p)) {
            p = new SqlParameter("p" + j++);
            parameterMapping.Add(column, p);
          }
          request.ParameterBindings[p] = (target => target.IsNull(offset) ? DBNull.Value : target.GetValue(offset));
          query.Values[tableRef[i]] = SqlFactory.ParameterRef(p);
        }
        batch.Add(query);
      }
      request.ExpectedResult = batch.Count;
      request.CompileWith(DomainHandler.Driver);
      return request;
    }


    // Constructor

    public SqlRequestBuilder(DomainHandler domainHandler)
    {
      DomainHandler = domainHandler;
    }
  }
}