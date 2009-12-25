// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.07.11

using System;
using System.Runtime.Serialization;
using Xtensive.Messaging.Providers.Resources;

namespace Xtensive.Messaging.Providers
{
  /// <summary>
  /// Represents errors that occur during <see langword="Messaging Provider"/> execution.
  /// </summary>
  [Serializable]
  public class MessagingProviderException: Exception
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingProviderException"/> class. 
    /// </summary>
    public MessagingProviderException()
      : this(Strings.ExMessagingProviderException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingProviderException"/> class with a specified error message. 
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public MessagingProviderException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingProviderException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception. 
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> reference (<see langword="Nothing"/> in Visual Basic) if no inner exception is specified.</param>
    public MessagingProviderException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingProviderException"/> class with serialized data. 
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
    protected MessagingProviderException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}