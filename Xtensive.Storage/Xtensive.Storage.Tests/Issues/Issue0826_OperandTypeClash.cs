﻿using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Issues_Issue0826_OperandTypeClash;

namespace Xtensive.Storage.Tests.Issues_Issue0826_OperandTypeClash
{
  [HierarchyRoot(InheritanceSchema.ClassTable)]
  public abstract class IdentifiableEntity : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    [Field]
    public bool Active { get; set; }
  }

  [HierarchyRoot]
  public class Status : Entity
  {
    [Field, Key]
    public Guid ID { get; private set; }
  }

  public abstract class ConcreteMedia : IdentifiableEntity
  {
    [Field]
    public VirtualMedia VirtualMedia { get; set; }

    [Field]
    public EntitySet<Status> Statuses { get; set; }
  }

  public abstract class VirtualMedia : IdentifiableEntity
  {
    [Field]
    [Association(PairTo = "VirtualMedia")]
    public EntitySet<ConcreteMedia> Concretes { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0826_OperandTypeClash : AutoBuildTest
  {

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.ProviderIsNot(StorageProvider.Oracle);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (VirtualMedia).Assembly, typeof (VirtualMedia).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var transaction = Transaction.Open(session)) {
          var titles =
            from vm in Query.All<VirtualMedia>()
            join cm in Query.All<ConcreteMedia>() on vm equals cm.VirtualMedia
            where cm.Statuses.Any()
            select vm;

          var error = titles.ToList();
        }
      }
    }
  }
}