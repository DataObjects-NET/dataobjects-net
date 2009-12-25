// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Voronov
// Created:    2007.09.28

using System;
using Xtensive.Messaging;

namespace Xtensive.Distributed.Core
{
  [Serializable]
  public class PrepareResponse
  {
    private readonly ElectionAct act;
    private readonly NetworkEntity participant;
    private readonly bool accepted;

    public ElectionAct Act
    {
      get { return act; }
    }

    public NetworkEntity Participant
    {
      get { return participant; }
    }

    public bool Accepted
    {
      get { return accepted; }
    }


    // Constructors

    public PrepareResponse(ElectionAct act, NetworkEntity participant, bool accepted)
    {
      this.act = act;
      this.participant = participant;
      this.accepted = accepted;
    }
  }
}