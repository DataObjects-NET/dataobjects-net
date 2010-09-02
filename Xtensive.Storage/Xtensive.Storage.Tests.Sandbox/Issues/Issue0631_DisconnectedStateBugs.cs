// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0631.Model;
using Xtensive.Core;

namespace Xtensive.Storage.Tests.Issues.Issue0631.Model
{
  [Serializable]
  [HierarchyRoot]
  public class Unit : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    [Association(PairTo = "Unit")]
    public EntitySet<AppointmentUnitRelationship> Relationships { get; private set; }

    public override string ToString()
    {
      return Title;
    }
  }

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class AppointmentUnitRelationship : Entity
  {
    [Key(2), Field]
    public Guid Id { get; private set; }

    [Key(0), Field]
    public Appointment Appointment { get; private set; }

    [Key(1), Field]
    public Unit Unit { get; private set; }

    public AppointmentUnitRelationship(Appointment appointment, Unit unit)
      : base(appointment, unit, Guid.NewGuid())
    {
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Appointment : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    [Association(PairTo = "Appointment")]
    public EntitySet<AppointmentUnitRelationship> Relationships { get; private set; }

    public override string ToString()
    {
      return Title;
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0631_DisconnectedStateBugs2 : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Unit).Assembly, typeof(Unit).Namespace);
      return configuration;
    }

    protected override Domain  BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      using (var session = Session.Open(domain))
      using (var tx = Transaction.Open()) {
        new Unit() { Title = "Unit" };
        tx.Complete();
      }
      return domain;
    }

    [Test]
    public void CombinedTest()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (ds.Attach(session)) {
        using (var tx = Transaction.Open()) {
          using (ds.Connect()) {
            var appointment = new Appointment() {Title = "Appointment"};
            var unit = Query.All<Unit>().FirstOrDefault();
            appointment.Relationships.Add(
              new AppointmentUnitRelationship(appointment, unit));
          }
          tx.Complete(); // Local transaction completed.
        }
        Dump(ds.Operations);
        ds.ApplyChanges();
        
        Assert.IsTrue(ds.IsAttached);
        Assert.AreEqual(0, ds.Operations.Count);
      }
    }

    private void Dump(OperationLog operations)
    {
      var sb = new StringBuilder();
      foreach (var o in operations)
        sb.AppendLine(o.ToString());
      Log.Info("Operations:\r\n{0}", sb.ToString().Indent(2));
    }
  }
}