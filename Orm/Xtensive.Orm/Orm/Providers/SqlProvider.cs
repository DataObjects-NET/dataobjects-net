// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Rse.Providers;
using System;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Unified SQL provider implementation (<see cref="ExecutableProvider"/>).
  /// </summary>
  public class SqlProvider : ExecutableProvider
  {
    protected readonly HandlerAccessor handlers;

    private const string ParameterNamePrefix = "@p";
    private const string ToStringFormat = "[Command: \"{0}\"]";
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
          permanentReference = SqlDml.QueryRef(Request.Statement);
        return permanentReference;
      }
    }

    /// <summary>
    /// Gets the domain handler this provider is bound to.
    /// </summary>
    protected Providers.DomainHandler DomainHandler { get { return handlers.DomainHandler; } }

    /// <inheritdoc/>
    public override IEnumerable<Tuple> OnEnumerate(Rse.Providers.EnumerationContext context)
    {
      var sessionContext = (EnumerationContext) context;
      var sessionHandler = sessionContext.SessionHandler;
      var executor = sessionHandler.GetService<IProviderExecutor>();
      var enumerator = executor.ExecuteTupleReader(Request);
      using (enumerator) {
        while (enumerator.MoveNext()) {
          yield return enumerator.Current;
        }
      }
    }

    #region ToString related methods

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      // No need to show parameters - they are meaningless, since provider is always the same.
      // Finally, they're printed as part of the [Origin: ...]
      return string.Empty;
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