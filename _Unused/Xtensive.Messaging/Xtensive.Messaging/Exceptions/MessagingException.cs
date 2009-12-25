// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.11

using System;
using System.Runtime.Serialization;
using Xtensive.Messaging.Resources;

namespace Xtensive.Messaging
{
  /// <summary>
  /// Represents errors that occur during <see cref="Xtensive.Messaging"/> execution.
  /// </summary>
  [Serializable]
  public class MessagingException: Exception
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingException"/> class. 
    /// </summary>
    public MessagingException()
      : this(Strings.ExMessagingException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingException"/> class with a specified error message. 
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public MessagingException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception. 
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> reference (<see langword="Nothing"/> in Visual Basic) if no inner exception is specified.</param>
    public MessagingException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingException"/> class with serialized data. 
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
    protected MessagingException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}