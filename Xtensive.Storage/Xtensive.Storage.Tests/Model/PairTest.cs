// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.09

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Tests.PairModel;

namespace Xtensive.Storage.Tests.PairModel
{
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Master : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public Slave Slave { get; set; }
  }

  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Slave : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field (PairTo = "Slave")]
    public Master Master { get; set; }
  }

}

namespace Xtensive.Storage.Tests.Model
{
  [TestFixture]
  public class PairTest : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(Master).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using(Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
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