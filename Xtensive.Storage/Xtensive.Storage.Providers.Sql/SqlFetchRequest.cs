// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.22

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Providers.Sql.Mappings;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Fetch request.
  /// </summary>
  public class SqlFetchRequest : SqlRequest
  {
    /// <summary>
    /// Gets the parameter bindings.
    /// </summary>
    public HashSet<SqlFetchParameterBinding> ParameterBindings { get; private set; }

    /// <summary>
    /// Gets the record set header.
    /// </summary>
    public RecordSetHeader RecordSetHeader { get; private set; }

    /// <summary>
    /// Gets the data reader accessor.
    /// </summary>
    public DbDataReaderAccessor DbDataReaderAccessor { get; private set; }

    internal override void Compile(DomainHandler domainHandler)
    {
      if (CompilationResult!=null)
        return;

      CompileParameters(domainHandler);
      CompileStatement(domainHandler);
      CompileDbReaderAccessor(domainHandler);
    }

    private void CompileDbReaderAccessor(DomainHandler domainHandler)
    {
      var readers = new List<Func<DbDataReader, int, object>>(RecordSetHeader.Columns.Count);
      var converters = new List<Func<object, object>>(RecordSetHeader.Columns.Count);

      foreach (Column column in RecordSetHeader.Columns) {
        DataTypeMapping typeMapping = domainHandler.ValueTypeMapper.GetTypeMapping(column.Type);
        readers.Add(typeMapping.DataReaderAccessor);
        converters.Add(typeMapping.FromSqlValue);
      }
      DbDataReaderAccessor = new DbDataReaderAccessor(readers, converters);
    }

    /// <inheritdoc/>
    protected override IEnumerable<SqlParameterBinding> GetParameterBindings()
    {
      foreach (var binding in ParameterBindings)
        yield return binding;
    }

    /// <summary>
    /// Binds the parameters to actual values.
    /// </summary>
    public void BindParameters()
    {
      foreach (var binding in ParameterBindings)
        BindParameter(binding, binding.ValueAccessor());
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="recordSetHeader">The element descriptor.</param>
    public SqlFetchRequest(ISqlCompileUnit statement, RecordSetHeader recordSetHeader)
      : base(statement)
    {
      ParameterBindings = new HashSet<SqlFetchParameterBinding>();
      RecordSetHeader = recordSetHeader;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="recordSetHeader">The element descriptor.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public SqlFetchRequest(ISqlCompileUnit statement, RecordSetHeader recordSetHeader, IEnumerable<SqlFetchParameterBinding> parameterBindings)
      : this(statement, recordSetHeader)
    {
      ParameterBindings.UnionWith(parameterBindings);
    }

  }
}