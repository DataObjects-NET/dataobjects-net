// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.09.26

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Distributed.Core.Resources;

namespace Xtensive.Distributed.Core
{
  [Serializable]
  public class ElectionResult
  {
    [NonSerialized]
    private readonly ElectionContext context;
    private readonly ElectionAct act;
    private readonly NetworkEntity master;
    private readonly ReadOnlySet<NetworkEntity> slaves;
    private readonly DateTime expireTime; // result is valid until this time
    
    public ElectionContext Context
    {
      get { return context; }
    }

    public ElectionAct Act
    {
      get { return act; }
    }
    
    public bool IsActual {
      get { return (context.Result == this) && (HighResolutionTime.Now < expireTime); }
    }

    public NetworkEntity Master
    {
      get
      {
        return master;
      }
    }

    public ReadOnlySet<NetworkEntity> Slaves
    {
      get
      {
        return slaves;
      }
    }

    public DateTime ExpireTime
    {
      get { return expireTime; }
    }


    // Constructors

    public ElectionResult(ElectionContext context, ElectionAct act, NetworkEntity master, DateTime expireTime)
    {
      if (act.Result!=null)
        throw new ArgumentException(Strings.ExSpecifiedElectionActIsAlreadyAssociatedWithResult);
      this.context = context;
      this.act = act;
      this.act.SetResult(this);
      this.master = master;
      this.expireTime = expireTime;
      Set<NetworkEntity> tmpSlaves = new Set<NetworkEntity>(context.Group.Participants);
      tmpSlaves.Remove(master);
      this.slaves = new ReadOnlySet<NetworkEntity>(tmpSlaves);
    }
  }
}