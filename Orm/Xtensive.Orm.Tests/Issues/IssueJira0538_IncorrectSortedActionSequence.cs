// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.06.05

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0538_IncorrectSortedActionSequenceModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0538_IncorrectSortedActionSequenceModel
{
  public abstract class DataFieldBase : FormElement
  {
    [Field(Length = 4000)]
    public string BindingExpression { get; set; }

    protected DataFieldBase(Guid id)
      : base(id)
    {
    }
  }
  
  [Serializable]
  public class ExternalDataField : DataFieldBase
  {
    [Field(Nullable = false)]
    public string SysName { get; set; }
    
    public ExternalDataField(Guid id)
      : base(id)
    {
    }
  }
  
  public interface IOwned<out TOwner> : IEntityBase
      where TOwner : IEntityBase
  {
    
    [Field(Nullable = false, Indexed = false)]
    [Association(OnTargetRemove = OnRemoveAction.Cascade, OnOwnerRemove = OnRemoveAction.Clear)]
    TOwner Owner { get; }
  }

  [HierarchyRoot(Clustered = false)]
  public abstract class FormElement : EntityBase
  {
    protected FormElement(Guid id)
      : base(id)
    {
    }

    [Association(PairTo = "Owner", OnTargetRemove = OnRemoveAction.Clear, OnOwnerRemove = OnRemoveAction.Cascade)]
    [Field]
    public EntitySet<TpVisibleEnable> Visibility { get; private set; }

    public abstract class TablePartBase<TOwner> : EntityBase, IOwned<TOwner> where TOwner : EntityBase
    {
      protected TablePartBase(Guid id, TOwner owner)
        : base(id)
      {
        Owner = owner;
      }

      public TOwner Owner { get; private set; }
    }

    [HierarchyRoot]
    [Serializable]
    public class TpVisibleEnable : TablePartBase<FormElement>
    {
      public TpVisibleEnable(Guid id, FormElement owner)
        : base(id, owner)
      {
      }

      [Field(Length = 4000)]
      public string DynamicLinqExpression { get; set; }

    }
  }

  /// <summary>
  /// Base Entity interface
  /// </summary>
  public interface IEntityBase : IEntity
  {
    [Field]
    [Key]
    new Guid Id { get; }
  }

  public abstract class EntityBase : Entity, IEntityBase
  {
    protected EntityBase(Guid id)
      : base(id)
    {
    }

    public Guid Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0538_IncorrectSortedActionSequence : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      var domainConfiguration = DomainConfigurationFactory.Create();
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      foreach (var type in typeof(FormElement).Assembly.GetTypes()) {
        if (typeof(IEntityBase).IsAssignableFrom(type))
          domainConfiguration.Types.Register(type);
      }
      using (var domain = Domain.Build(domainConfiguration)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var transaction = Session.Current.OpenTransaction()) {
          var externalDataField = new ExternalDataField(Guid.NewGuid()) { SysName = "ee" };
          var eee = new FormElement.TpVisibleEnable(Guid.NewGuid(), externalDataField);

          transaction.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var transaction = Session.Current.OpenTransaction()) {
          var e1 = Query.All<ExternalDataField>().Single();
          e1.Remove();
          transaction.Complete();
        }
      }
    }
  }
}
