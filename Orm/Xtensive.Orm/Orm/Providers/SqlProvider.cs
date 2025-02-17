// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System.Linq;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Rse.Providers;
using System.Threading;
using System.Threading.Tasks;

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
        if (permanentReference is null)
          permanentReference = SqlDml.QueryRef(Request.Statement);
        return permanentReference;
      }
    }

    /// <summary>
    /// Gets the domain handler this provider is bound to.
    /// </summary>
    protected Providers.DomainHandler DomainHandler { get { return handlers.DomainHandler; } }

    /// <inheritdoc/>
    protected internal override DataReader OnEnumerate(Rse.Providers.EnumerationContext context)
    {
      var storageContext = (EnumerationContext) context;
      var executor = storageContext.Session.Services.Demand<IProviderExecutor>();
      return executor.ExecuteTupleReader(Request, storageContext.ParameterContext);
    }

    protected internal async override Task<DataReader> OnEnumerateAsync(Rse.Providers.EnumerationContext context, CancellationToken token)
    {
      var storageContext = (EnumerationContext)context;
      var executor = storageContext.Session.Services.Demand<IProviderExecutor>();
      return await executor.ExecuteTupleReaderAsync(Request, storageContext.ParameterContext, token).ConfigureAwaitFalse();
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
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="request">A <see cref="QueryRequest"/> instance associated with
    /// the newly created <see cref="SqlProvider"/>.
    /// </param>
    /// <param name="origin">The origin.</param>
    /// <param name="handlers">The handlers.</param>
    /// <param name="sources">The sources.</param>
    public SqlProvider(HandlerAccessor handlers, QueryRequest request,
      CompilableProvider origin, ExecutableProvider[] sources)
      : base(origin, sources)
    {
      this.handlers = handlers;
      Request = request;
      if (typeof (SqlProvider)==GetType())
        Initialize();
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="permanentReference">The permanent reference.</param>
    public SqlProvider(SqlProvider provider, SqlTable permanentReference)
      : base(provider.Origin, provider.Sources.Cast<ExecutableProvider>().ToArray(provider.Sources.Count))
    {
      this.permanentReference = permanentReference;
      handlers = provider.handlers;
      Request = provider.Request;
      if (typeof (SqlProvider)==GetType())
        Initialize();
    }
  }
}