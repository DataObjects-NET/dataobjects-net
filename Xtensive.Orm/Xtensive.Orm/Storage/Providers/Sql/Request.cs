// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.27

using Xtensive.Internals.DocTemplates;
using Xtensive.Threading;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Base class for any SQL request.
  /// </summary>
  public abstract class Request
  {
    private ThreadSafeCached<SqlCompilationResult> compilationResult = ThreadSafeCached<SqlCompilationResult>.Create(new object());
    
    /// <summary>
    /// Gets the statement of this request.
    /// </summary>
    protected internal ISqlCompileUnit Statement { get; private set; }

    /// <summary>
    /// Gets the options of this request.
    /// </summary>
    public RequestOptions Options { get; private set; }
    
    /// <summary>
    /// Gets the compiled statement.
    /// </summary>
    /// <param name="domainHandler">The domain handler.</param>
    /// <returns>Compiled statement.</returns>
    public SqlCompilationResult GetCompiledStatement(DomainHandler domainHandler)
    {
      return compilationResult.GetValue(
        (driver, _this) => driver.Compile(_this.Statement), domainHandler.Driver, this);
    }

    /// <summary>
    /// Checks that specified options are enabled for this request.
    /// </summary>
    /// <param name="requiredOptions">The required options.</param>
    /// <returns><see langword="true"/> is specified options is suppored;
    /// otherwise, <see langword="false"/>.</returns>
    public bool CheckOptions(RequestOptions requiredOptions)
    {
      return (Options & requiredOptions)==requiredOptions;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="options">The options.</param>
    protected Request(ISqlCompileUnit statement, RequestOptions options)
    {
      Statement = statement;
      Options = options;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    protected Request(ISqlCompileUnit statement)
      : this(statement, RequestOptions.AllowBatching)
    {
    }
  }
}