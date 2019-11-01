// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.10.25

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.IssueJira0779_SetOperationsWithOrderAndTakeWrongQueryModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0779_SetOperationsWithOrderAndTakeWrongQueryModel
{
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class NamedValue : Entity
  {
    [Field, Key]
    public string Name { get; set; }

    [Field]
    public long Value { get; set; }

    public NamedValue(Session session, string name)
      : base(session, name)
    {

    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0779_SetOperationsWithOrderAndTakeWrongQuery : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.PagingRequiresOrderBy);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (NamedValue).Assembly, typeof (NamedValue).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()){
        new NamedValue(session, "A") {Value = 22};
        new NamedValue(session, "B") {Value = 21};
        new NamedValue(session, "C") {Value = 20};
        new NamedValue(session, "D") {Value = 19};
        new NamedValue(session, "E") {Value = 18};
        new NamedValue(session, "F") {Value = 17};
        new NamedValue(session, "G") {Value = 16};
        tx.Complete();
      }
    }

    [Test]
    public void ConcatTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query1 = session.Query.All<NamedValue>().OrderBy(c => c.Value).Take(3);
        var query2 = session.Query.All<NamedValue>().OrderByDescending(c => c.Value).Take(3);
        var result = query1.Concat(query2).ToList().Select(e => e.Name).ToArray();

        var expectedSequence = new [] {"G", "F", "E", "A", "B", "C"};
        Assert.That(result.Length, Is.EqualTo(expectedSequence.Length));
        for(int i =0; i < expectedSequence.Length; i++)
          Assert.That(result[i], Is.EqualTo(expectedSequence[i]));
      }
    }

    [Test]
    public void UnionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query1 = session.Query.All<NamedValue>().OrderBy(c => c.Value).Take(3);
        var query2 = session.Query.All<NamedValue>().OrderByDescending(c => c.Value).Take(3);
        var result = query1.Union(query2).ToList().Select(e => e.Name).ToArray();

        var expectedSequence = new[] { "A", "B", "C", "E", "F", "G" };
        Assert.That(result.Length, Is.EqualTo(expectedSequence.Length));
        for (int i = 0; i < expectedSequence.Length; i++)
          Assert.That(result[i], Is.EqualTo(expectedSequence[i]));
      }
    }

    [Test]
    public void IntersectTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query1 = session.Query.All<NamedValue>().OrderBy(c => c.Value).Take(5);
        var query2 = session.Query.All<NamedValue>().OrderByDescending(c => c.Value).Take(5);
        var result = query1.Intersect(query2).ToList().Select(e => e.Name).ToArray();

        // G F E D C
        //     E D C B A
        var expectedSequence = new[] { "E", "D", "C" };
        Assert.That(result.Length, Is.EqualTo(expectedSequence.Length));
        for (int i = 0; i < expectedSequence.Length; i++)
          Assert.That(result[i], Is.EqualTo(expectedSequence[i]));
      }
    }

    [Test]
    public void ExceptTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query1 = session.Query.All<NamedValue>().OrderBy(c => c.Value).Take(5);
        var query2 = session.Query.All<NamedValue>().OrderByDescending(c => c.Value).Take(5);
        var result = query1.Except(query2).ToList().Select(e => e.Name).ToArray();

        var expectedSequence = new[] { "G", "F"};
        Assert.That(result.Length, Is.EqualTo(expectedSequence.Length));
        for (int i = 0; i < expectedSequence.Length; i++)
          Assert.That(result[i], Is.EqualTo(expectedSequence[i]));
      }
    }
  }
}