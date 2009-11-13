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
    /// Gets <see cref="QueryRequest"/> associated with this provider.
    /// </summary>
    public QueryRequest Request { get; private set; }

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
      var result = Request.GetCompiledStatement((DomainHandler) handlers.DomainHandler);
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
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The origin.</param>
    /// <param name="handlers">The handlers.</param>
    /// <param name="sources">The sources.</param>
    public SqlProvider(HandlerAccessor handlers, QueryRequest request,
      CompilableProvider origin, ExecutableProvider[] sources)
      : base(origin, sources)
    {
      this.handlers = handlers;
      Request = request;
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