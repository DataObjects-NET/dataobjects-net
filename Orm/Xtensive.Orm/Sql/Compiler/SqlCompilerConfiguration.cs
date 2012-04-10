// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.15



namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// A various options for <see cref="SqlCompiler"/>.
  /// </summary>
  public sealed class SqlCompilerConfiguration
  {
    /// <summary>
    /// Gets or sets a value indicating whether full automatic aliasing is enforced.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if full automatic aliasing is enforced; otherwise, <see langword="false"/>.
    /// </value>
    public bool ForcedAliasing { get; set; }

    /// <summary>
    /// Gets or sets the parameter prefix.
    /// </summary>
    public string ParameterNamePrefix { get; set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlCompilerConfiguration"/> class.
    /// </summary>
    public SqlCompilerConfiguration()
    {
      ForcedAliasing = true;
    }
  }
}