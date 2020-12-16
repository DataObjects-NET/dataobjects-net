// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.12.24

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.IssueJira0784_IncludedTypeIdReferenceBreaksTypedIndexCompilationModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0784_IncludedTypeIdReferenceBreaksTypedIndexCompilationModel
{
  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public abstract class Media : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public TypeIdIncludedEntity MediaRef { get; set; }
  }

  public class Film : Media
  {
    [Field]
    public int Sensitivity { get; set; }

    [Field]
    public TypeIdIncludedEntity FilmRef { get; set; }
  }

  public class DigitalSensor : Film
  {
    [Field]
    public TimeSpan TimeSpan { get; set; }

    [Field]
    public TypeIdIncludedEntity DigitalSensorRef { get; set; }
  }

  [HierarchyRoot(IncludeTypeId = true)]
  public class TypeIdIncludedEntity : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0784_IncludedTypeIdReferenceBreaksTypedIndexCompilation : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Media).Assembly, typeof(Media).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new Film();
        _ = new Film();
        _ = new Film();
        _ = new Film();

        _ = new DigitalSensor();
        _ = new DigitalSensor();
        _ = new DigitalSensor();
        _ = new DigitalSensor();
        _ = new DigitalSensor();
        _ = new DigitalSensor();

        transaction.Complete();
      }
    }

    [Test]
    public void BaseClassQueryTest()
    {
      var primaryIndex = FindTypedPrimaryIndex(typeof(Media));
      Assert.That(primaryIndex.Columns.Select(c => c.Field).Where(f => f.IsTypeId).Count(), Is.EqualTo(2));
      Assert.That(primaryIndex.Columns.Select(c => c.Field).Where(f => f.IsTypeId && f.IsSystem).Count(), Is.EqualTo(1));

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<Media>().ToArray();

        Assert.That(result.Length, Is.EqualTo(10));
        Assert.That(result.OfType<Film>().Count(), Is.EqualTo(10));
        Assert.That(result.OfType<DigitalSensor>().Count(), Is.EqualTo(6));

      }
    }

    [Test]
    public void MidClassQueryTest()
    {
      var primaryIndex = FindTypedPrimaryIndex(typeof(Film));
      Assert.That(primaryIndex.Columns.Select(c => c.Field).Where(f => f.IsTypeId).Count(), Is.EqualTo(3));
      Assert.That(primaryIndex.Columns.Select(c => c.Field).Where(f => f.IsTypeId && f.IsSystem).Count(), Is.EqualTo(1));

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<Film>().ToArray();

        Assert.That(result.Length, Is.EqualTo(10));
        Assert.That(result.OfType<Film>().Count(), Is.EqualTo(10));
        Assert.That(result.OfType<DigitalSensor>().Count(), Is.EqualTo(6));
      }
    }

    [Test]
    public void LeafClassQueryTest()
    {
      var primaryIndex = FindTypedPrimaryIndex(typeof(DigitalSensor));
      Assert.That(primaryIndex.Columns.Select(c => c.Field).Where(f => f.IsTypeId).Count(), Is.EqualTo(4));
      Assert.That(primaryIndex.Columns.Select(c => c.Field).Where(f => f.IsTypeId && f.IsSystem).Count(), Is.EqualTo(1));

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<DigitalSensor>().ToArray();

        Assert.That(result.Length, Is.EqualTo(6));
        Assert.That(result.OfType<DigitalSensor>().Count(), Is.EqualTo(6));
      }
    }

    private IndexInfo FindTypedPrimaryIndex(Type type)
    {
      var typeInfo = Domain.Model.Types[type];
      var pk = typeInfo.Indexes.PrimaryIndex;
      var allIndexes = pk.UnderlyingIndexes
        .Flatten(i => i.UnderlyingIndexes.Count > 0 ? i.UnderlyingIndexes : Enumerable.Empty<IndexInfo>(), i => { }, true)
        .Union(typeInfo.Indexes.Where(i => i.IsPrimary)).ToArray();
      return allIndexes.Where(i => i.ReflectedType == typeInfo).Single(i => i.IsPrimary && i.IsTyped);
    }
  }
}