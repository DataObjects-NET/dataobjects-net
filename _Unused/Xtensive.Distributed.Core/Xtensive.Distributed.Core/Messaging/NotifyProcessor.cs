// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Voronov
// Created:    2007.10.01

using System;
using Xtensive.Messaging;

namespace Xtensive.Distributed.Core
{
  [MessageProcessor(typeof(NotifyMessage))]
  internal class NotifyProcessor: BaseProcessor
  {
    public override void ProcessMessage(object message, Sender replySender)
    {
      NotifyMessage m = (NotifyMessage)message;
      algorithm.OnMasterElected(m.Act, m.Master);
    }
  }
}
