// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlProvider : ExecutableProvider
  {
    private const string ParameterNamePrefix = "@p";
    private const string ToStringFormat = "[Command: \"{0}\"]";
    protected readonly HandlerAccessor handlers;
    private SqlTable permanentReference;

    /// <summary>
    /// Gets <see cref="SqlQueryRequest"/> associated with this provider.
    /// </summary>
    public SqlQueryRequest Request { get; private set; }

    /// <summary>
    /// Gets the permanent reference (<see cref="SqlQueryRef"/>) for <see cref="SqlSelect"/> associated with this provider.
    /// </summary>
    public SqlTable PermanentReference {
      get {
        if (ReferenceEquals(null, permanentReference))
          permanentReference = SqlDml.QueryRef(Request.SelectStatement);
        return permanentReference;
      }
    }

    /// <inheritdoc/>
    protected override IEnumerable<Tuple> OnEnumerate(Rse.Providers.EnumerationContext context)
    {
      var executor = handlers.SessionHandler.GetService<IQueryExecutor>();
      var enumerator = executor.ExecuteTupleReader(Request);
      using (enumerator) {
        while (enumerator.MoveNext()) {
          yield return enumerator.Current;
        }
      }
    }

    #region ToString related methods

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      // No need to show parameters - they are meaningless, since provider is always the same.
      // Finally, they're printed as part of the [Origin: ...]
      return string.Empty;
    }
    
    /// <inheritdoc/>
    protected override void AppendDescriptionTo(StringBuilder builder, int indent)
    {
      AppendOriginTo(builder, indent);
      var result = Request.Compile((DomainHandler) handlers.DomainHandler);
      AppendCommandTo(result, builder, indent);
    }

    /// <inheritdoc/>
    protected virtual void AppendCommandTo(SqlCompilationResult result, StringBuilder builder, int indent)
    {
      var configuration = new SqlPostCompilerConfiguration();
      int i = 0;
      foreach (var item in Request.ParameterBindings) {
        configuration.PlaceholderValues.Add(item, ParameterNamePrefix + i);
        i++;
      }

      builder.Append(new string(' ', indent))
        .AppendFormat(ToStringFormat, result.GetCommandText(configuration))
        .AppendLine();
    }

    #endregion
    

    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The origin.</param>
    /// <param name="statement">The statement.</param>
    /// <param name="handlers">The handlers.</param>
    /// <param name="sources">The sources.</param>
    public SqlProvider(
      CompilableProvider origin,
      SqlSelect statement,
      HandlerAccessor handlers,
      params ExecutableProvider[] sources)
      : this(origin, statement, handlers, null, null, sources)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The origin.</param>
    /// <param name="statement">The statement.</param>
    /// <param name="handlers">The handlers.</param>
    /// <param name="extraBindings">The extra bindings.</param>
    /// <param name="sources">The sources.</param>
    public SqlProvider(
      CompilableProvider origin,
      SqlSelect statement,
      HandlerAccessor handlers,
      IEnumerable<SqlQueryParameterBinding> extraBindings,
      params ExecutableProvider[] sources)
      : this(origin, statement, handlers, extraBindings, null, sources)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The origin.</param>
    /// <param name="statement">The statement.</param>
    /// <param name="handlers">The handlers.</param>
    /// <param name="extraBindings">The extra bindings.</param>
    /// <param name="allowBatching">The allow batching.</param>
    /// <param name="sources">The sources.</param>
    public SqlProvider(
      CompilableProvider origin,
      SqlSelect statement,
      HandlerAccessor handlers,
      IEnumerable<SqlQueryParameterBinding> extraBindings,
      bool? allowBatching,
      params ExecutableProvider[] sources)
      : base(origin, sources)
    {
      this.handlers = handlers;
      var sqlSources = sources.OfType<SqlProvider>();

      var parameterBindings = sqlSources.SelectMany(p => p.Request.ParameterBindings);
      if (extraBindings!=null)
        parameterBindings = parameterBindings.Concat(extraBindings);

      if (allowBatching==null)
        allowBatching = sqlSources
          .Aggregate(true, (current, provider) => current && provider.Request.AllowBatching);
      var tupleDescriptor = origin.Header.TupleDescriptor;

      if (statement.Columns.Count < origin.Header.TupleDescriptor.Count)
        tupleDescriptor = origin.Header.TupleDescriptor.TrimFields(statement.Columns.Count);

      Request = new SqlQueryRequest(statement, tupleDescriptor, parameterBindings, allowBatching.Value);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="permanentReference">The permanent reference.</param>
    public SqlProvider(SqlProvider provider, SqlTable permanentReference)
      : base(provider.Origin, provider.Sources.Cast<ExecutableProvider>().ToArray())
    {
      this.permanentReference = permanentReference;
      handlers = provider.handlers;
      Request = provider.Request;
    }
  }
}