// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Voronov
// Created:    2007.09.25

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Threading;
using Xtensive.Core.Diagnostics;
using Xtensive.Distributed.Core.Resources;
using Xtensive.Messaging;

namespace Xtensive.Distributed.Core
{
  public class SimpleElectionAlgorithm: IElectionAlgorithm, IHasSyncRoot
  {
    // Protocol constants & Timeouts
    private const string AlgorithmUrlKey = "SimpleElections";
    
    private readonly TimeSpan MasterLease = TimeSpan.FromSeconds(1);
    
    private readonly TimeSpan MasterReelectTime = TimeSpan.FromSeconds(10);
                              // master tries to reelect himself after this period.
                              // for this behavior, it should be less than MasterLease.
    
    private readonly TimeSpan PrepareLease = TimeSpan.FromSeconds(.600);
                              // prepared host promises not to accept other requests during this time.
    
    private readonly TimeSpan ElectionActTimeout = TimeSpan.FromSeconds(.600);
                              // during this time election act is valid (supposed to be equal to PrepareLease now).

    private readonly TimeSpan ReelectWaitTime = TimeSpan.FromSeconds(.100);
                              // time to wait between unsuccessful elections.

    private readonly TimeSpan ClockSync = TimeSpan.FromSeconds(0.020);
                              // possible system clocks mistiming during master lease

    private readonly TimeSpan NetworkSync = TimeSpan.FromSeconds(3);
                              // medium should guarantee that no message goes longer than this time.
                              // we use it to ensure that everybody has invalidated current master.
                              // it is CRITICAL for the algorithm correctness to have this value larger than
                              // the worst time of message delivery

    private readonly ElectionContext context;

    private ExpiringVariable<ElectionAct> currentAct;
    private readonly ExpiringVariable<NetworkEntity> master = null;
    private readonly ExpiringVariable<NetworkEntity> acceptedMaster = null;
    private readonly ExpiringVariable<NetworkEntity> masterPretender = null;

    private readonly Receiver receiver;
    private int retriesCount = 0;

    private readonly NetworkEntity me;
    private readonly ISet<NetworkEntity> partners;
    private ISet<NetworkEntity> preparedPartners;
    private ISet<NetworkEntity> acceptedPartners;

    private readonly AutoResetEvent majorityPreparedEvent = new AutoResetEvent(false);
    private readonly AutoResetEvent majorityAcceptedEvent = new AutoResetEvent(false);

    private readonly Thread monitorThread;
    private readonly Random rand = new Random();

    private int Majority
    {
      get { return context.Group.Participants.Count / 2 + 1; }
    }

    public object SyncRoot
    {
      get { return this; }
    }

    // RPC

    public PrepareResponse Prepare(ElectionAct act, NetworkEntity pretender)
    {
      using (LockType.Exclusive.LockRegion(SyncRoot)) {
        bool prepared = false;
        if ((master.Value != null) && (pretender == master.Value)) // master reelection  
        {
          prepared = true;
        }
        else if ((masterPretender.Value != null) || (acceptedMaster.Value != null) ||
                 (master.Value != null) || (currentAct.Value != null)) {
          prepared = false;
        }
        else // no master and no pretender
        {
          prepared = true;
        }
        if (prepared) {
          Debug.Assert(currentAct.Value == null);
          Debug.Assert(masterPretender.Value == null);
          currentAct.Value = act;
          masterPretender.Value = pretender;
        }
        // DEBUG:
        // if (prepared) Console.WriteLine(me.Id + " is prepared.");
        // DEBUG:
        // if (!prepared) Console.WriteLine(me.Id + " REFUSED prepare request from " + pretender.Id);

        return new PrepareResponse(act, me, prepared);
      }
    }

    public AcceptResponse Accept(ElectionAct act, NetworkEntity pretender)
    {
      using (LockType.Exclusive.LockRegion(SyncRoot)) {
        bool accepted = false;
        if ((master.Value != null) && (pretender == master.Value) && (act == currentAct.Value)) {
          // master reelection
          accepted = true;
        }
        else if ((masterPretender.Value != null) && (pretender == masterPretender.Value) && (act == currentAct.Value)) {
          // new master accepted (but it is not known if he was accepted by majority)
          accepted = true;
        }
        if (accepted) {
          Debug.Assert((master.Value == null) || (me == master.Value));
          Debug.Assert((acceptedMaster.Value == null) || (me == acceptedMaster.Value));
          acceptedMaster.Value = pretender;
          retriesCount = 0;
        }
        // DEBUG:
        // if (accepted) Console.WriteLine(me.Id + " has accepted: " + pretender.Id);

        return new AcceptResponse(act, me, accepted);
      }
    }

    public void ReceivePrepareResponse(ElectionAct act, NetworkEntity participant)
    {
      using (LockType.Exclusive.LockRegion(SyncRoot)) {

        if (act != currentAct.Value) {
          // DEBUG:
          //Console.WriteLine(me.Id + " received prepare response for old election act.");
          //Console.WriteLine(act.Id);

          return;
        }
      }

      preparedPartners.Add(participant);

      if (preparedPartners.Count + 1 >= Majority) {
        majorityPreparedEvent.Set();
        Thread.Sleep(0);
      }
    }

    public void ReceiveAcceptResponse(ElectionAct act, NetworkEntity participant)
    {
      using (LockType.Exclusive.LockRegion(SyncRoot)) {
        if (act != currentAct.Value)
          return;

        acceptedPartners.Add(participant);

        if (acceptedPartners.Count + 1 >= Majority) {
          majorityAcceptedEvent.Set();
          Thread.Sleep(0);
        }
      }
    }

    public void OnMasterElected(ElectionAct act, NetworkEntity newMaster)
    {
      using (LockType.Exclusive.LockRegion(SyncRoot)) {
        if ((master.Value != null))
          throw new MultipleMastersException(Strings.ExNewMasterLearnedInValidMasterPresence);
        if (newMaster == null)
          throw new ArgumentNullException();
        // DEBUG:
        DateTime now = HighResolutionTime.Now;
        Console.WriteLine(me.Id + " learned that master is: " + newMaster.Id + " Last act: " + act.Id + " " + now.Second +
                          "." + now.Millisecond);

        retriesCount = 0;
        DateTime electionTime = HighResolutionTime.Now;
        master.Value = newMaster;
        acceptedMaster.Set(newMaster, electionTime + MasterLease + ClockSync);
        ElectionResult result = new ElectionResult(context, act, newMaster, electionTime + MasterLease);
        context.OnCompleteElectionAct(act);
      }
    }

    private void TryElectMe()
    {
      // DEBUG:
      //DateTime now = HighResolutionTimer.Now;
      //Console.WriteLine(me.Id + " STARTED elections; RetryCount = " + retriesCount + " " + now.Second + "." + now.Millisecond);
      Console.WriteLine(me.Id + " STARTED elections; RetryCount = " + retriesCount);

      ElectionAct actValue = new ElectionAct(Guid.NewGuid());
      ExpiringVariable<ElectionAct> act = new ExpiringVariable<ElectionAct>(actValue, ElectionActTimeout);

      majorityPreparedEvent.Reset();

      using (Locker.WriteRegion(SyncRoot)) {
        if ((master.Value != null) && (me != master.Value))
          return;
        if ((acceptedMaster.Value != null) && (me != acceptedMaster.Value))
          return;
        retriesCount++;
        //currentAct = act;
        preparedPartners = new Set<NetworkEntity>();
        acceptedPartners = new Set<NetworkEntity>();
      }

      // Prepare myself first
      // Sets prepareLeaseStarted on success
      if (!Prepare(actValue, me).Accepted)
        return;

      // broadcast "prepare" message
      foreach (NetworkEntity participant in partners)
      {
        using (Sender s = new Sender(participant.EndPointUrls[AlgorithmUrlKey])) {
          s.ResponseReceiver = this.receiver;
          try {
            s.Send(new PrepareMessage(actValue, me));
          }
          catch (Exception ex) {
            //Log.Error(ex, Strings.ExSendError);
          }
        }
      }

      bool prepared = majorityPreparedEvent.WaitOne(act.ValidTimeLeft, false);

      if (!prepared)
        return;   // prepare lease expired

      // small optimization: no need to continue if we overslept
      if (actValue != currentAct.Value)
        return;

      // Accept myself.
      if (!Accept(actValue, me).Accepted)
        return;

      // broadcast "accept" message
      foreach (NetworkEntity participant in partners)
      {
        using (Sender s = new Sender(participant.EndPointUrls[AlgorithmUrlKey]))
        {
          s.ResponseReceiver = this.receiver;
          try {
            s.Send(new AcceptMessage(actValue, me));
          }
          catch (Exception ex) {
            //Log.Error(ex, Strings.ExSendError);
          }
        }
      }

      bool accepted = majorityAcceptedEvent.WaitOne(act.ValidTimeLeft, false);

      if (!accepted)
        return;   // prepare lease expired

      // small optimization: no need to continue if we overslept
      if (actValue != currentAct.Value)
        return;

      // if number of accepted >= number of participants / 2 + 1
      // you win! you are the Master!
      // notify myself
      OnMasterElected(actValue.Clone(), me);

      // notify other parties that you've become Master.
      foreach (NetworkEntity participant in partners)
      {
        using (Sender s = new Sender(participant.EndPointUrls[AlgorithmUrlKey]))
        {
          s.Send(new NotifyMessage(actValue.Clone(), me));
        }
      }
    }

    private void Monitor(object state)
    {
      while (true) {
        if (master.Value != null)
        {
          if ((me == master.Value) && (HighResolutionTime.Now > ( master.ValidUntil - MasterLease + MasterReelectTime)))
          {
            TryElectMe();   // reelect me as master
          }
        }
        else if ((acceptedMaster.Value == null) && (masterPretender.Value == null)) {
          // no valid master, neither pretender is known
          int participantCount = context.Group.Participants.Count;
          int myChance = (participantCount*(int)Math.Pow(2, Math.Min(10, retriesCount)));
          // DEBUG:
          // Console.WriteLine(me.Id.ToString() + " is ready to propose himself; myChance = " + myChance.ToString());
          if (rand.Next(myChance) == 0)
          {
            // my chance to have zero value
            TryElectMe();
          }
        }
        Thread.Sleep(ReelectWaitTime);
      }
    }


    // Constructors

    public SimpleElectionAlgorithm(ElectionContext context)
    {
      this.context = context;
      context.Algorithm = this;
      this.me = context.Me;
      currentAct = new ExpiringVariable<ElectionAct>(new ElectionAct(Guid.NewGuid()), ElectionActTimeout);
      partners = new Set<NetworkEntity>(context.Group.Participants);
      partners.Remove(me);
      preparedPartners = new Set<NetworkEntity>();
      acceptedPartners = new Set<NetworkEntity>();

      masterPretender = new ExpiringVariable<NetworkEntity>(null, PrepareLease);
      acceptedMaster = new ExpiringVariable<NetworkEntity>(null, MasterLease + NetworkSync + ClockSync);
      master = new ExpiringVariable<NetworkEntity>(null, MasterLease + ClockSync);

      this.receiver = new Receiver(context.Me.EndPointUrls[AlgorithmUrlKey]);
      this.receiver.ProcessorContext = this;
      this.receiver.AddProcessor(typeof(AcceptProcessor));
      this.receiver.AddProcessor(typeof(PrepareProcessor));
      this.receiver.AddProcessor(typeof(NotifyProcessor));
      this.receiver.AddProcessor(typeof(AcceptResponseProcessor));
      this.receiver.AddProcessor(typeof(PrepareResponseProcessor));
      // DEBUG:
      // this.receiver.MessageReceived += ReceiverMessageReceived;
      this.receiver.StartReceive();

      monitorThread = new Thread(Monitor);
      monitorThread.Start();
    }

    // Dispose & finalizer

    ~SimpleElectionAlgorithm()
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
      monitorThread.Abort();
    }
  }
}
