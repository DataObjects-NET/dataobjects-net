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
    /// Initializes a new instance of the <see cref="SqlCompilerException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public SqlCompilerException(string message)
      : base(message)
    {
    }

    // Serialization


    /// <summary>
    /// Initializes a new instance of the <see cref="SqlCompilerException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
    ///   
    /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
    protected SqlCompilerException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}