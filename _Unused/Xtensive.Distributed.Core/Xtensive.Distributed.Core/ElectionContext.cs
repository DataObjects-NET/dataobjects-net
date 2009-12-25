// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Voronov
// Created:    2007.09.25

using System;
using Xtensive.Core;
using Xtensive.Core.Threading;
using Xtensive.Distributed.Core.Resources;

namespace Xtensive.Distributed.Core
{
  public class ElectionContext: ISynchronizable, IHasSyncRoot
  {
    private readonly string groupId;
    private readonly NetworkEntity me;
    private ElectionGroup group;
    private ElectionResult result;
    private IElectionAlgorithm algorithm = null;

    public ElectionGroup Group
    {
      get { return group; }
      set
      {
        using (LockType.Exclusive.LockRegion(SyncRoot)) {
          if (value == null)
            throw new ArgumentNullException("value");
          if (value.Id != groupId)
            throw new ArgumentException(Strings.ExNewElectionGroupShouldHaveTheSameId);
          if (!value.Participants.Contains(me))
            throw new ArgumentException(Strings.ExNewElectionGroupShouldContainMe);
          group = value;
        }
      }
    }

    public NetworkEntity Me
    {
      get { return me; }
    }

    public ElectionResult Result
    {
      get { return result; }
    }

    public void Invalidate()
    {
      using (LockType.Exclusive.LockRegion(SyncRoot))
        result = null;
    }

    internal void OnCompleteElectionAct(ElectionAct act)
    {
      using (LockType.Exclusive.LockRegion(SyncRoot))
        result = act.Result;
    }

    // ISynchronizable & IHasSyncRoot
    
    public bool IsSynchronized
    {
      get { return true; }
    }

    public object SyncRoot
    {
      get { return this; }
    }

    public IElectionAlgorithm Algorithm
    {
      get { return algorithm; }
      set
      {
        if (algorithm != null)
          throw new AlgorithmDefinedException(Strings.ExAlgorithmAlreadyDefined);
        algorithm = value;
      }
    }


    // Constructors

    public ElectionContext(ElectionGroup group, NetworkEntity me)
    {
      if (group == null)
        throw new ArgumentNullException("group");
      this.group = group;
      this.groupId = group.Id;
      this.me = me;
    }

    // Dispose & finalizer

    ~ElectionContext()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) 
    {
    }
  }
}
