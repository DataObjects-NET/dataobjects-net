// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.17

using System;

namespace Xtensive.Messaging
{
  /// <summary>
  /// Assigns <see cref="IQueryMessage"/> to specified <see cref="IMessageProcessor"/>.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
  [Serializable]
  public sealed class MessageProcessorAttribute: Attribute
  {
    private readonly Type messageType;
    private ProcessorActivationMode processorActivationMode = ProcessorActivationMode.SingleCall;

    /// <summary>
    /// Gets <see cref="Type"/> of <see cref="IQueryMessage"/>.
    /// </summary>
    public Type MessageType
    {
      get { return messageType; }
    }

    /// <summary>
    /// Gets <see langword="true"/> if messaging processor is default for incoming messages.
    /// </summary>
    public bool IsDefaultProcessor
    {
      get { return messageType==null; }
    }
  

    /// <summary>
    /// Gets or sets <see cref="ProcessorActivationMode"/> describing <see cref="IMessageProcessor"/> creation behavior.
    /// </summary>
    public ProcessorActivationMode ProcessorActivationMode
    {
      get { return processorActivationMode; }
      set { processorActivationMode = value; }
    }


    /// <summary>
    /// Creates new instance of <see cref="MessageProcessorAttribute"/>. Affected <see cref="IMessageProcessor"/> will be default for incoming messages.
    /// </summary>
    public MessageProcessorAttribute()
      : this(null)
    {
    }

    /// <summary>
    /// Creates new instance of <see cref="MessageProcessorAttribute"/>.
    /// </summary>
    /// <param name="messageType"><see cref="Type"/> of <see cref="IQueryMessage"/> to process by affected class. If <see langword="null" /> than affected <see cref="IMessageProcessor"/> will be default for incoming messages.</param>
    public MessageProcessorAttribute(Type messageType)
    {
      if (messageType!=null) {
        this.messageType = messageType;
      }
    }
  }
}