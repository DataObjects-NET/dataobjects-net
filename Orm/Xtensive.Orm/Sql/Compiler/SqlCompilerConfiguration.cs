// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.15

using Xtensive.Internals.DocTemplates;
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
  }
}