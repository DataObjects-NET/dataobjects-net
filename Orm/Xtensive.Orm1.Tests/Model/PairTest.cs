// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.09

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Tests.PairModel;

namespace Xtensive.Orm.Tests.PairModel
{
  [Serializable]
  [HierarchyRoot]
  public class Master : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Slave Slave { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Slave : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field , Association(PairTo = "Slave")]
    public Master Master { get; set; }
  }

}

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture]
  public class PairTest : AutoBuildTest
  {
    protected override Xtensive.Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(Master).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Master m1 = new Master();
          m1.Slave = new Slave();
          Assert.AreEqual(m1, m1.Slave.Master);
          Master m2 = new Master();
          m2.Slave = new Slave();
          Assert.AreEqual(m2, m2.Slave.Master);
          Slave s1 = m1.Slave;
          Slave s2 = m2.Slave;
          m1.Slave = m2.Slave;
          Assert.IsNull(m2.Slave);
          Assert.IsNull(s1.Master);
          Assert.AreEqual(m1.Slave, s2);
          Assert.AreEqual(m1, s2.Master);
          t.Complete();
        }
      }
    }
  }
}