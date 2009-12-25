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
  internal class PrepareMessage
  {
    private readonly ElectionAct act;
    private readonly NetworkEntity pretender;

    public ElectionAct Act
    {
      get { return act; }
    }

    public NetworkEntity Pretender
    {
      get { return pretender; }
    }


    // Constructors

    public PrepareMessage(ElectionAct act, NetworkEntity pretender)
    {
      this.act = act;
      this.pretender = pretender;
    }
  }
}