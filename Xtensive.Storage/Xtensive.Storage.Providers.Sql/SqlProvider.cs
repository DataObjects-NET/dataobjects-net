// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlProvider : ExecutableProvider
  {
    protected readonly HandlerAccessor handlers;
    protected SqlFetchRequest request;
    private const string ToString_Command = "[Command: \"{0}\"]";
    private const string ToString_ParametersBegin = "[Parameters: ";
    private const string ToString_ParametersEnd = "]";
    private const string ToString_Parameter = "{0} = \"{1}\"";

    public SqlFetchRequest Request {
      [DebuggerStepThrough]
      get { return request; }
      [DebuggerStepThrough]
      private set { request = value; }
    }

    public SqlQueryRef PermanentReference { get; private set; }

    /// <inheritdoc/>
    protected override IEnumerable<Tuple> OnEnumerate(Rse.Providers.EnumerationContext context)
    {
      var sessionHandler = (SessionHandler) handlers.SessionHandler;
      sessionHandler.DomainHandler.Compile(request);
      request.BindParameters();
      using (var e = sessionHandler.Execute(request)) {
        while (e.MoveNext())
          yield return e.Current;
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
    protected override void AppendDescriptionTo(StringBuilder sb, int indent)
    {
      AppendOriginTo(sb, indent);
      if (request.CompilationResult == null) {
        var sessionHandler = (SessionHandler) handlers.SessionHandler;
        sessionHandler.DomainHandler.Compile(request);
        request.BindParameters();
      }
      AppendCommandTo(sb, indent);
      AppendParametersTo(sb, indent);
    }

    /// <inheritdoc/>
    protected virtual void AppendCommandTo(StringBuilder sb, int indent)
    {
      sb.Append(new string(' ', indent))
        .AppendFormat(ToString_Command, request.CompiledStatement)
        .AppendLine();
    }

    /// <inheritdoc/>
    private void AppendParametersTo(StringBuilder sb, int indent)
    {
      if (request.ParameterBindings.Count == 0)
        return;
      sb.Append(new string(' ', indent)).Append(ToString_ParametersBegin);
      int i = 0;
      foreach (var item in request.ParameterBindings) {
        if (i > 0)
          sb.Append(", ");
        sb.AppendFormat(ToString_Parameter, item.SqlParameter.ParameterName, item.ValueAccessor());
        i++;
      }
      sb.Append(ToString_ParametersEnd).AppendLine();
    }

    #endregion


    // Constructor

    public SqlProvider(CompilableProvider origin, SqlFetchRequest request, HandlerAccessor handlers, params ExecutableProvider[] sources)
      : base(origin, sources)
    {
      this.request = request;
      this.handlers = handlers;
      var select = request.Statement as SqlSelect;
      if (select != null)
        PermanentReference = Xtensive.Sql.Dom.Sql.QueryRef(select);
      foreach (ExecutableProvider source in sources) {
        var sqlProvider = source as SqlProvider;
        if (sqlProvider!=null && sqlProvider.Request != null)
          request.ParameterBindings.UnionWith(sqlProvider.Request.ParameterBindings);
      }
    }
  }
}