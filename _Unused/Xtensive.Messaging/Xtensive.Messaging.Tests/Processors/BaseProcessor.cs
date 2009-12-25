// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.20

namespace Xtensive.Messaging.Tests
{
  public abstract class BaseProcessor : IMessageProcessor
  {
    private object context;

    #region IMessageProcessor Members

    /// <summary>
    /// Processes message and send reply to <paramref name="replySender"/>
    /// </summary>
    /// <param name="message">Message to process.</param>
    /// <param name="replySender"><see cref="Sender"/> to send reply to.</param>
    public virtual void ProcessMessage(object message, Sender replySender)
    {
      throw new System.NotImplementedException();
    }

    /// <summary>
    /// Sets execution context.
    /// </summary>
    public virtual void SetContext(object value)
    {
      context = value;
    }

    #endregion
  }
}