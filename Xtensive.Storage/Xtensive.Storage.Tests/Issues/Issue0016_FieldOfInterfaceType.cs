// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.12.09

using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Tests.Issues.Issue0016_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0016_Model
{
  public interface IMaster : IEntity
  {
    [Field]
    Slave Slave { get; set; }
  }

  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Master : Entity, IMaster
  {
    [Field]
    public int Id { get; private set; }

    public Slave Slave { get; set; }
  }

  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Slave : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field (PairTo = "Slave")]
    public IMaster Master { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0016_FieldOfInterfaceType : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config =  base.BuildConfiguration();
      config.Types.Register(typeof (Slave).Assembly, typeof (Slave).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {

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