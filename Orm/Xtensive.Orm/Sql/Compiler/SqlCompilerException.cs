// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Runtime.Serialization;


namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// An <see cref="SqlCompiler"/> exception.
  /// </summary>
  [Serializable]
  public class SqlCompilerException : Exception
  {
    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="message">The message.</param>
    public SqlCompilerException(string message)
      : base(message)
    {
    }

    // Serialization

    /// <summary>
    /// Deserializes instance of this type.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
#if NET8_0_OR_GREATER
    [Obsolete(DiagnosticId = "SYSLIB0051")]
#endif
    protected SqlCompilerException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}