// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.15

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// A various options for <see cref="SqlCompiler"/>.
  /// </summary>
  public sealed class SqlCompilerConfiguration
  {
    /// <summary>
    /// Gets or sets the parameter prefix.
    /// </summary>
    public string ParameterNamePrefix { get; set; }

    /// <summary>
    /// Always use database-qualified objects in generated SQL.
    /// This option could be enabled if and only if
    /// server supports <see cref="QueryFeatures.MultidatabaseQueries"/>.
    /// </summary>
    public bool DatabaseQualifiedObjects { get; set; }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>Clone of this instance.</returns>
    public SqlCompilerConfiguration Clone()
    {
      return (SqlCompilerConfiguration) MemberwiseClone();
    }
  }
}