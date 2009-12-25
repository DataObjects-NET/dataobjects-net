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
  [MessageProcessor(typeof(PrepareMessage))]
  internal class PrepareProcessor: BaseProcessor
  {
    public override void ProcessMessage(object message, Sender replySender)
    {
      PrepareMessage m = (PrepareMessage)message;
      try {
        replySender.Send(algorithm.Prepare(m.Act, m.Pretender));
      }
      catch (Exception ex) {
        //Log.Error(ex, Strings.ExSendError);
      }
    }
  }
}
