// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;


namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// An <see cref="SqlCompiler"/> exception.
  /// </summary>
  /// <remarks>
  /// Initializes new instance of this type.
  /// </remarks>
  /// <param name="message">The message.</param>
  public class SqlCompilerException(string message) : Exception(message)
  {
  }
}