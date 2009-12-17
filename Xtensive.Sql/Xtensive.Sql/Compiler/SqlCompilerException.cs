// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// A <see cref="SqlCompiler"/> exception
  /// </summary>
  public class SqlCompilerException : Exception
  {
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    public SqlCompilerException(string message)
      : base(message)
    {
    }
  }
}