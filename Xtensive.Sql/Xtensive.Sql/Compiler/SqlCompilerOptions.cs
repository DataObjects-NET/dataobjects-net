// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.15

using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// </summary>
  public sealed class SqlCompilerOptions
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

    /// <summary>
    /// Gets or sets a value indicating whether parameter name assignment is delayed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if parameter name assignment is delayed; otherwise, <see langword="false"/>.
    /// </value>
    public bool DelayParameterNameAssignment { get; set; }

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SqlCompilerOptions()
    {
      ForcedAliasing = true;
    }
  }
}