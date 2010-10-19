// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.01.30

using System;
using System.Runtime.Serialization;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;

namespace Xtensive
{
  /// <summary>
  /// Thrown by <see cref="ServiceContainerBase"/> on activation errors.
  /// </summary>
  [Serializable]
  public class ActivationException : Exception
  {
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ActivationException()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    public ActivationException(string message) 
      : base(message)
    {
    }

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ActivationException(string message, Exception innerException) 
      : base(message, innerException)
    {
    }

    /// <summary>
    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected ActivationException(SerializationInfo info, StreamingContext context) 
      : base(info, context)
    {
    }
  }
}