// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.24

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Tests.ObjectModel.Interfaces.Slavery
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

  [Serializable]
  [HierarchyRoot]
  public class Slave : Entity, ISlave<IMaster<Slave>>
  {
    [Field,Key]
    public long Id { get; private set; }

    public IMaster Master { get; set;}

    public IMaster<Slave> XMaster { get; set;}
   
  }

  [Serializable]
  [HierarchyRoot]
  public class Master : Entity, IMaster<Slave>
  {
    [Field, Key]
    public long Id { get; private set; }

    public EntitySet<ISlave> Slaves { get; private set;}

    public EntitySet<Slave> XSlaves { get; private set; }
  }
}