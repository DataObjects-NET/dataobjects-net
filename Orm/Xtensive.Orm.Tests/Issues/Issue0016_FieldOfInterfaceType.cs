// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.12.09

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0016_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0016_Model
{
  public interface IMaster : IEntity
  {
    [Field]
    Slave Slave { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Master : Entity, IMaster
  {
    [Field, Key]
    public int Id { get; private set; }

    public Slave Slave { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Slave : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field , Association(PairTo = "Slave")]
    public IMaster Master { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0016_FieldOfInterfaceType : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config =  base.BuildConfiguration();
      config.Types.Register(typeof (Slave).Assembly, typeof (Slave).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          Master m = new Master();
          Slave s = new Slave();
          m.Slave = s;
          Assert.IsNotNull(m.Slave);
          Assert.AreSame(s, m.Slave);

          m.Slave = null;
          Assert.IsNull(m.Slave);

          s.Master = m;
          Assert.IsNotNull(s.Master);
          Assert.AreSame(m, s.Master);

          t.Complete();
        }
      }
    }
  }
}