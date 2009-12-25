// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Voronov
// Created:    2007.10.01

using System;
using Xtensive.Messaging;

namespace Xtensive.Distributed.Core
{
  [Serializable]
  internal class NotifyMessage: IQueryMessage
  {
    private readonly ElectionAct act;
    private readonly NetworkEntity master;

    private string receiverUrl;
    private TimeSpan timeout;
    private long queryId;

    public ElectionAct Act
    {
      get { return act; }
    }

    public NetworkEntity Master
    {
      get { return master; }
    }

    #region IQueryMessage Members

    public TimeSpan? CalculateTimeout()
    {
      throw new NotImplementedException();
    }

    public string ReceiverUrl
    {
      get { return receiverUrl; }
      set { receiverUrl = value; }
    }

    public TimeSpan Timeout
    {
      get { return timeout; }
      set { timeout = value; }
    }

    #endregion

    #region IMessage Members

    public long QueryId
    {
      get { return queryId; }
      set { queryId = value; }
    }

    #endregion


    // Constructors

    public NotifyMessage(ElectionAct act, NetworkEntity master)
    {
      this.act = act;
      this.master = master;
    }
  }
}
