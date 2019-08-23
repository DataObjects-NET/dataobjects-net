// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.06.21

using System.Linq;
using System.Runtime.Serialization;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.IssueJira0767_OfTypeForNonGenericSourcesModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0767_OfTypeForNonGenericSourcesModel
{
  [HierarchyRoot]
  public class EntitySetItem : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class HasEntitySetEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySet<EntitySetItem> PureEntitySet { get; private set; }

    [Field]
    public EntitySetOfTestEntity CustomEntitySet { get; private set; }
  }

  public class EntitySetOfTestEntity : EntitySet<EntitySetItem>
  {
    protected EntitySetOfTestEntity(Entity owner, FieldInfo field) : base(owner, field)
    {
    }

    protected EntitySetOfTestEntity(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0767_OfTypeForNonGenericSources : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (HasEntitySetEntity).Assembly, typeof (HasEntitySetEntity).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entitySetContainer = new HasEntitySetEntity();
        entitySetContainer.PureEntitySet.Add(new EntitySetItem());
        entitySetContainer.PureEntitySet.Add(new EntitySetItem());

        entitySetContainer.CustomEntitySet.Add(new EntitySetItem());
        entitySetContainer.CustomEntitySet.Add(new EntitySetItem());
        transaction.Complete();
      }
    }

    [Test]
    public void PureEntitySetTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => session.Query.All<HasEntitySetEntity>().Where(e=> e.PureEntitySet.OfType<EntitySetItem>().Any()).Run());
      }
    }

    [Test]
    public void CustomEntitySetTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => session.Query.All<HasEntitySetEntity>().Where(e => e.CustomEntitySet.OfType<EntitySetItem>().Any()).Run());
      }
    }
  }
}
