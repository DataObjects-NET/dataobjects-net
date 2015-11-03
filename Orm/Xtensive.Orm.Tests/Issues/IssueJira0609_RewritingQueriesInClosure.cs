// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2009.11.02

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Xtensive.Orm.Tests.Issues.IssueJira0609_RewritingQueriesInClosureModel;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0609_RewritingQueriesInClosure : AutoBuildTest
  {
    private List<string> uniqueIdentifiers;

    [Test]
    public void DirectInParametersTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var q = Query.All<StoredMaterialLot>().Where(c => c.IsAvailable);
        var pallets = Query.All<BegroPallet>().Where(p => p.UniqueIdentifier.In("2","4","6","8","10"));
        var storedContainers = Query.All<StoredContainer>().Where(sc => sc.Container.Id.In(pallets.Select(p => p.Id)));
        q = q.Where(sml => sml.StoredContainer.Id.In(storedContainers.Select(sc => sc.Id)));
        Assert.DoesNotThrow(()=>q.Run());
        var expectedCountOfElemetns = uniqueIdentifiers.Where((s,i)=> i % 2==0).Count();
        Assert.That(q.Count(), Is.EqualTo(expectedCountOfElemetns));
      }
    }

    [Test]
    public void ArrayAsParameterForInTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var q = Query.All<StoredMaterialLot>().Where(c => c.IsAvailable);
        var requiredIdentifiers = uniqueIdentifiers.Where((s, i) => i % 2==0).ToArray();
        var pallets = Query.All<BegroPallet>().Where(p => p.UniqueIdentifier.In(requiredIdentifiers));
        var storedContainers = Query.All<StoredContainer>().Where(sc => sc.Container.Id.In(pallets.Select(p => p.Id)));
        q = q.Where(sml => sml.StoredContainer.Id.In(storedContainers.Select(sc => sc.Id)));
        Assert.DoesNotThrow(() => q.Run());
        Assert.That(q.Count(), Is.EqualTo(requiredIdentifiers.Length));
      }
    }

    [Test]
    public void EnumerableAsParameterForInTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var q = Query.All<StoredMaterialLot>().Where(c => c.IsAvailable);
        var requiredIdentifiers = uniqueIdentifiers.Where((s, i) => i % 2==0);
        var pallets = Query.All<BegroPallet>().Where(p => p.UniqueIdentifier.In(requiredIdentifiers));
        var storedContainers = Query.All<StoredContainer>().Where(sc => sc.Container.Id.In(pallets.Select(p => p.Id)));
        q = q.Where(sml => sml.StoredContainer.Id.In(storedContainers.Select(sc => sc.Id)));
        Assert.DoesNotThrow(() => q.Run());
        Assert.That(q.Count(), Is.EqualTo(requiredIdentifiers.Count()));
      }
    }

    protected override void PopulateData()
    {
      PopulateIdentifiers();
      PopulateEntities();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(StoredMaterialLot));
      return configuration;
    }

    private void PopulateIdentifiers()
    {
      uniqueIdentifiers = new List<string>(Enumerable.Range(1, 10).Select(el => el.ToString(CultureInfo.InvariantCulture)));
    }

    private void PopulateEntities()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        foreach (var uniqueIdentifier in uniqueIdentifiers) {
          new StoredMaterialLot {
            IsAvailable = true,
            StoredContainer = new StoredContainer {
              Container = new BegroPallet {UniqueIdentifier = uniqueIdentifier}
            }
          };
        }
        transaction.Complete();
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.Xtensive.Orm.Tests.Issues.IssueJira0609_RewritingQueriesInClosureModel
{
  [HierarchyRoot]
  public class StoredMaterialLot : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public bool IsAvailable { get; set; }

    [Field]
    public StoredContainer StoredContainer { get; set; }
  }

  [HierarchyRoot]
  public class BegroPallet : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string UniqueIdentifier { get; set; }
  }

  [HierarchyRoot]
  public class StoredContainer : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public BegroPallet Container { get; set; }
  }
}