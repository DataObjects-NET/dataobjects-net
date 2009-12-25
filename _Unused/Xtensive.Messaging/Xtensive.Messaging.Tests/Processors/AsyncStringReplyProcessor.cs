// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.08

using System;

namespace Xtensive.Messaging.Tests
{
  [MessageProcessor(typeof (string))]
  public class AsyncStringReplyProcessor: AsyncBaseProcessor
  {
    public override void ProcessMessage(object message, Sender replySender)
    {
      Console.WriteLine("AsyncStringReplyProcessor");
      replySender.Send(message);
    }
  }
}