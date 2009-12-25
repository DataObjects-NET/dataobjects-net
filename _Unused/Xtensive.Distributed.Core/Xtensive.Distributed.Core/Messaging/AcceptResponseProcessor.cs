// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Voronov
// Created:    2007.10.01

using System;
using Xtensive.Messaging;

namespace Xtensive.Distributed.Core
{
  [MessageProcessor(typeof(AcceptResponse))]
  internal class AcceptResponseProcessor: BaseProcessor
  {
    public override void ProcessMessage(object message, Sender replySender)
    {
      AcceptResponse m = (AcceptResponse)message;
      if (m.Accepted) {
        algorithm.ReceiveAcceptResponse(m.Act, m.Participant);
      }
    }
  }
}
