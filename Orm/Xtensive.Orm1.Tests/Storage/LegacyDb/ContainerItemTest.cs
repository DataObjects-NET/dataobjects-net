// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.26

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.LegacyDb.ContainerItemModel;

namespace Xtensive.Orm.Tests.Storage.LegacyDb.ContainerItemModel
{
  [Serializable]
  [HierarchyRoot]
  public class Container : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [Association(PairTo = "Container", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Item> Items { get; private set; }

    [Field]
    [Association(PairTo = "Container", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Option> Options { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Item : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Nullable = false)]
    public Container Container { get; set; }

    [Field]
    [Association(PairTo = "Item", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<ItemOption> Options { get; private set; }

    public Item(Container container)
    {
      using (Session.DisableSaveChanges(this))
        Container = container;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Option : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Nullable = false)]
    public Container Container { get; set; }

    public Option(Container container)
    {
      Container = container;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class ItemOption : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Item Item { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.LegacyDb
{
  public class ContainerItemTest : LegacyDbAutoBuildTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.ProviderVersionAtMost(StorageProviderVersion.SqlServer2008R2);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Container).Assembly, typeof (Container).Namespace);
      return config;
    }

    protected override string GetCreateDbScript(DomainConfiguration config)
    {
      return @"
CREATE TABLE [dbo].[Container](
	[Id] [int] NOT NULL,
 CONSTRAINT [PK_Container] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]
CREATE TABLE [dbo].[Item](
	[Id] [int] NOT NULL,
	[Container.Id] [int] NOT NULL,
 CONSTRAINT [PK_Item] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]
CREATE TABLE [dbo].[Option](
	[Id] [int] NOT NULL,
	[Container.Id] [int] NOT NULL,
 CONSTRAINT [PK_Option] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]
CREATE TABLE [dbo].[ItemOption](
	[Id] [int] NOT NULL,
	[Item.Id] [int] NOT NULL,
 CONSTRAINT [PK_ItemOption] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]
CREATE TABLE [dbo].[Int32-Generator](
	[ID] [int] IDENTITY(128,128) NOT NULL,
 CONSTRAINT [PK_Int32-Generator] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
) ON [PRIMARY]
ALTER TABLE [dbo].[Item]  WITH CHECK ADD  CONSTRAINT [FK_Item_Container] FOREIGN KEY([Container.Id])
REFERENCES [dbo].[Container] ([Id])

ALTER TABLE [dbo].[Item] CHECK CONSTRAINT [FK_Item_Container]

ALTER TABLE [dbo].[Option]  WITH CHECK ADD  CONSTRAINT [FK_Option_Container] FOREIGN KEY([Container.Id])
REFERENCES [dbo].[Container] ([Id])

ALTER TABLE [dbo].[Option] CHECK CONSTRAINT [FK_Option_Container]

ALTER TABLE [dbo].[ItemOption]  WITH CHECK ADD  CONSTRAINT [FK_ItemOption_Item] FOREIGN KEY([Item.Id])
REFERENCES [dbo].[Item] ([Id])

ALTER TABLE [dbo].[ItemOption] CHECK CONSTRAINT [FK_ItemOption_Item]
";
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var c = new Container();
        var item = new Item(c);
        c.Items.Add(item);
        var option = new Option(c);
        c.Options.Add(option);
        Session.Current.SaveChanges();
        c.Remove();
        t.Complete();
      }
    }

    [Test]
    public void SequentialTransactionsTest()
    {
      using (var session = Domain.OpenSession()) {
        Container c;
        using (var t = session.OpenTransaction()) {
          c = new Container();
          t.Complete();
        }
        Item i;
        using (var t = session.OpenTransaction()) {
          i = new Item(c);
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          i.Remove();
          t.Complete();
        }
      }
    }
  }
}