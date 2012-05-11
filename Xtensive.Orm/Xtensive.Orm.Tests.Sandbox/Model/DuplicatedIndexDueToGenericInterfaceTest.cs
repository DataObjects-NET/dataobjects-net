// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.11

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Model.DuplicatedIndexDueToGenericInterfaceTestModel;

namespace Xtensive.Orm.Tests.Model
{
  namespace DuplicatedIndexDueToGenericInterfaceTestModel
  {
    public interface IHasReference<TReference> : IEntity
      where TReference : Entity
    {
      [Field]
      TReference Ref { get; }
    }

    [HierarchyRoot]
    public class ReferencedEntity : Entity
    {
      [Key, Field]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    public class IndexedEntity : Entity, IHasReference<ReferencedEntity>
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public ReferencedEntity Ref { get; set; }
    }
  }

  [TestFixture]
  public class DuplicatedIndexDueToGenericInterfaceTest : AutoBuildTest
  {
    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (ReferencedEntity).Assembly, typeof (ReferencedEntity).Namespace);
      return configuration;
    }

    [Test]
    public void Test()
    {
      var indexes = Domain.Model.Types[typeof(IndexedEntity)].Indexes;
      var refIndexes = indexes.Where(i => i.Columns.Any(c => c.Name=="Ref.Id") && i.IsSecondary && !i.IsVirtual).ToList();
      Assert.That(refIndexes.Count, Is.EqualTo(1));
    }
  }
}