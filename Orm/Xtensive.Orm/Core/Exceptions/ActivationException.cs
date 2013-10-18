// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.01.30

using System;
using System.Runtime.Serialization;

using Xtensive.IoC;

namespace Xtensive.Core
{
  /// <summary>
  /// Thrown by <see cref="ServiceContainerBase"/> on activation errors.
  /// </summary>
  [Serializable]
  public class ActivationException : Exception
  {
    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public ActivationException()
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="message">The message.</param>
    public ActivationException(string message) 
      : base(message)
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ActivationException(string message, Exception innerException) 
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Deserializes instance of this type.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected ActivationException(SerializationInfo info, StreamingContext context) 
      : base(info, context)
    {
    }
  }
}