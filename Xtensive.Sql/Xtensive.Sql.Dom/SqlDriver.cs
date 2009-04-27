// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Database.Extractor;

namespace Xtensive.Sql.Dom
{
  /// <summary>
  /// Declares a base functionality of any <see cref="SqlDriver"/>
  /// that can be instantiated by the <see cref="ConnectionProvider"/>.
  /// </summary>
  /// <remarks>
  /// <para>Any driver that is intended to be accessible via <see cref="ConnectionProvider"/>
  /// has to be <see cref="SqlDriver"/> descendant and in addition has to
  /// be marked with <see cref="ProtocolAttribute"/>.</para>
  /// </remarks>
  /// <seealso cref="ConnectionProvider"/>
  /// <seealso cref="ProtocolAttribute"/>
  public abstract class SqlDriver : Driver
  {
    private SqlExtractor extractor;
    private SqlTranslator translator;

    /// <summary>
    /// Gets the extractor.
    /// </summary>
    /// <value>The extractor.</value>
    public SqlExtractor Extractor
    {
      get {
        if (extractor==null)
          extractor = CreateExtractor();
        return extractor;
      }
    }

    /// <summary>
    /// Gets the translator.
    /// </summary>
    /// <value>The extractor.</value>
    public SqlTranslator Translator
    {
      get {
        if (translator==null)
          translator = CreateAndInitializeTranslator();
        return translator;
      }
    }
    
    /// <summary>
    /// Compiles the specified statement into SQL command representation.
    /// </summary>
    /// <param name="unit">The Sql.Dom statement.</param>
    /// <returns></returns>
    public SqlCompilationResult Compile(ISqlCompileUnit unit)
    {
      ArgumentValidator.EnsureArgumentNotNull(unit, "unit");
      return CreateCompiler().Compile(unit);
    }

    /// <summary>
    /// Creates the compiler.
    /// </summary>
    /// <returns></returns>
    protected virtual SqlCompiler CreateCompiler()
    {
      return new SqlCompiler(this);
    }

    /// <summary>
    /// Creates the Sql.Dom translator.
    /// </summary>
    protected abstract SqlTranslator CreateTranslator();

    /// <summary>
    /// Creates the extractor.
    /// </summary>
    protected abstract SqlExtractor CreateExtractor();

    private SqlTranslator CreateAndInitializeTranslator()
    {
      var result = CreateTranslator();
      result.Initialize();
      return result;
    }

    protected SqlDriver(VersionInfo versionInfo)
      : base(versionInfo)
    {
    }

    protected SqlDriver(ConnectionInfo connectionInfo)
      : base(connectionInfo)
    {
    }
  }
}