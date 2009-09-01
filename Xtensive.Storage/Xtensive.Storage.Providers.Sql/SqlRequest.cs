// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.27

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Base class for any SQL request.
  /// </summary>
  public abstract class SqlRequest
  {
    private ThreadSafeCached<SqlCompilationResult> compilationResult = ThreadSafeCached<SqlCompilationResult>.Create(new object());
    
    /// <summary>
    /// Gets the statement.
    /// </summary>
    protected ISqlCompileUnit Statement { get; private set; }

    /// <summary>
    /// Compiles the request using <see cref="SqlDriver"/> from specified <see cref="DomainHandler"/>.
    /// </summary>
    /// <param name="domainHandler">The domain handler.</param>
    /// <returns></returns>
    public SqlCompilationResult Compile(DomainHandler domainHandler)
    {
      return compilationResult.GetValue(
        (driver, _this) => driver.Compile(_this.Statement, new SqlCompilerOptions {DelayParameterNameAssignment = true, ForcedAliasing = true}),
        domainHandler.Driver, this);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    protected SqlRequest(ISqlCompileUnit statement)
    {
      Statement = statement;
    }
  }
}