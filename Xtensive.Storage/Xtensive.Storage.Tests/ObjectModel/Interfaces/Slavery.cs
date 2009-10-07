// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.24

using System;
using System.Diagnostics;

namespace Xtensive.Storage.Tests.ObjectModel.Interfaces.Slavery
{
  public interface ISlave : IEntity
  {
    [Field]
    IMaster Master { get; set; }
  }

  public interface IMaster : IEntity
  {
    [Field, Association(PairTo = "Master")]
    EntitySet<ISlave> Slaves { get; }
  }

  public interface ISlave<TMaster> : ISlave
    where TMaster : IMaster
  {
    [Field]
    TMaster XMaster { get; set; }
  }

  public interface IMaster<TSlave> : IMaster
    where TSlave : ISlave
  {
    [Field, Association(PairTo = "XMaster")]
    EntitySet<TSlave> XSlaves { get; }
  }

  [HierarchyRoot]
  public class Slave : Entity, ISlave<Master>
  {
    [Field,Key]
    public long Id { get; private set; }

    IMaster ISlave.Master
    {
      get { return GetFieldValue<IMaster>("ISlave.Master"); }
      set
      {
        XMaster = (Master) value;
      }
    }

    public Master XMaster
    {
      get { return (Master) ((ISlave)this).Master;}
      set
      {
        SetFieldValue("ISlave.Master", (IMaster)value);
        SetFieldValue("TMaster", value);
      }
    }
  }

  [HierarchyRoot]
  public class Master : Entity, IMaster<Slave>
  {
    [Field, Key]
    public long Id { get; private set; }

    EntitySet<ISlave> IMaster.Slaves
    {
      get { return GetFieldValue<EntitySet<ISlave>>("IMaster.Slaves"); }
    }

    public EntitySet<Slave> XSlaves { get; private set; }
  }
}