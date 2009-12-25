// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Voronov
// Created:    2007.10.01

using System;
using Xtensive.Distributed.Core.Resources;
using Xtensive.Messaging;

namespace Xtensive.Distributed.Core
{
  [MessageProcessor(typeof(AcceptMessage))]
  internal class AcceptProcessor: BaseProcessor
  {
    public override void ProcessMessage(object message, Sender replySender)
    {
      AcceptMessage m = (AcceptMessage)message;
      try {
        replySender.Send(algorithm.Accept(m.Act, m.Pretender));
      }
      catch (Exception ex) {
        //Log.Error(ex, Strings.ExSendError);
      }
    }
  }
}
