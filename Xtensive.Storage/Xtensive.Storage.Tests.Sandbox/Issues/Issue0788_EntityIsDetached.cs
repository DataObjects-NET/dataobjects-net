// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2010.08.27

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0788_EntityIsDetached_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0788_EntityIsDetached_Model
{
  internal class Event : SyncObject
  {
    public Event(string token, Guid requestorId, bool signalled)
      : base(token, requestorId, signalled)
    {}
  }

  [KeyGenerator(KeyGeneratorKind.None)]
  [HierarchyRoot /*(InheritanceSchema = InheritanceSchema.SingleTable)*/]
  internal abstract class SyncObject : Entity
  {
    [Field(Length = 128)]
    [Key]
    public string Token { get; private set; }

    [Field]
    public Lock LockObject { get; private set; }

    public bool Signalled
    {
      get { return (null==LockObject); }
    }

    protected SyncObject(string token, Guid requestorId, bool signalled)
      : base(token)
    {
      if (!signalled) {
        //  Подпорка
        //LockObject = new Lock(token, requestorId, this);
      }
    }
  }

  [KeyGenerator(KeyGeneratorKind.None)]
  [HierarchyRoot]
  internal class Lock : Entity
  {
    [Field(Length = 128)]
    [Key]
    public string Token { get; private set; }

    [Field(Nullable = false)]
    public Guid RequestorId { get; private set; }

    [Field]
    public DateTime TimeStamp { get; private set; }

    [Field]
    //[Association(
      //    PairTo = "LockObject"//,                  //  linked to LockObject property of SyncObject
      //OnOwnerRemove = OnRemoveAction.Deny,    //  can't remove SyncObject while its LockObject exists
      //OnTargetRemove = OnRemoveAction.Clear     //  upon Lock's removal, clear corresponding LockObject property of SyncObject
      //  )  
      //]
      public SyncObject OwnerObject { get; set; }

    /// <summary>
    /// Constructs independent Lock object
    /// </summary>
    /// <param name="token">Token</param>
    /// <param name="requestorId">Requestor Id</param>
    public Lock(string token, Guid requestorId)
      : base(token)
    {
      RequestorId = requestorId;
      TimeStamp = DateTime.Now;
    }

    /// <summary>
    /// Constructs Lock object as part of owning <see cref="SyncObject"/>
    /// </summary>
    /// <param name="token">Token</param>
    /// <param name="requestorId">Requestor Id</param>
    /// <param name="owner">Owning <see cref="SyncObject"/></param>
    public Lock(string token, Guid requestorId, SyncObject owner)
      : base(token)
    {
      //using(Session.Pin(this))
      //{
      RequestorId = requestorId;
      TimeStamp = DateTime.Now;

      OwnerObject = owner;
      //}
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0788_EntityIsDetached : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Lock).Assembly, typeof (Lock).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        Guid key = new Guid("{0AF02FA4-F6C6-4A78-A569-9E5225281E27}");
        Event evt = null;
        Lock evtLock;

        using (var transactionScope = Transaction.Open()) {
          evt = new Event("dep", key, false);
          evtLock = new Lock("dep", key, null);
          evtLock.OwnerObject = evt;
          transactionScope.Complete();
        }
      }
    }
  }
}