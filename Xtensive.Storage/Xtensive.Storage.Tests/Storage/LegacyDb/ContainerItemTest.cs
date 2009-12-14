// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.26

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.LegacyDb.ContainerItemModel;

namespace Xtensive.Storage.Tests.Storage.LegacyDb.ContainerItemModel
{
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
      using (Session.Pin(this))
        Container = container;
    }
  }

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

  [HierarchyRoot]
  public class ItemOption : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Item Item { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage.LegacyDb
{
  public class ContainerItemTest : LegacyDbAutoBuildTest
  {
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
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var c = new Container();
        var item = new Item(c);
        c.Items.Add(item);
        var option = new Option(c);
        c.Options.Add(option);
        Session.Current.Persist();
        c.Remove();
        t.Complete();
      }
    }

    [Test]
    public void SequentialTransactionsTest()
    {
      using (Session.Open(Domain)) {
        Container c;
        using (var t = Transaction.Open()) {
          c = new Container();
          t.Complete();
        }
        Item i;
        using (var t = Transaction.Open()) {
          i = new Item(c);
          t.Complete();
        }
        using (var t = Transaction.Open()) {
          i.Remove();
          t.Complete();
        }
      }
    }
  }
}