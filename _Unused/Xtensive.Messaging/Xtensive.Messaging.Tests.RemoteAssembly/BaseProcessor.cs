// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.16

namespace Xtensive.Messaging.Tests.RemoteAssembly
{
  [MessageProcessor(typeof(string), ProcessorActivationMode = ProcessorActivationMode.Singleton)]
  public class BaseProcessor: IMessageProcessor
  {
    private Statistics statistics;


    public void ProcessMessage(object message, Sender replySender)
    {
      replySender.Send("OK");
      statistics.IncreaseMessageCount();
    }

    public void SetContext(object value)
    {
      statistics = (Statistics)value;
    }
  }
}