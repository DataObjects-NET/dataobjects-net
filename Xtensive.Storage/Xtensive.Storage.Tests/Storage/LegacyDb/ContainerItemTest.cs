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
    [Association(PairTo = "Container", OnOwnerRemove = OnRemoveAction.Cascade)]
    public EntitySet<Item> Items { get; private set; }

    [Field]
    [Association(PairTo = "Container", OnOwnerRemove = OnRemoveAction.Cascade)]
    public EntitySet<Option> Options { get; private set; }
  }

  [HierarchyRoot]
  public class Item : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Nullable = false)]
    public Container Container { get; set; }

    public Item(Container container)
    {
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
CREATE TABLE [dbo].[Int32-Generator](
	[ID] [int] IDENTITY(128,128) NOT NULL,
 CONSTRAINT [PK_Int32-Generator] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
) ON [PRIMARY]
";
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var c = new Container();
        c.Items.Add(new Item(c));
        c.Options.Add(new Option(c));
        Session.Current.Persist();
        c.Remove();
        t.Complete();
      }
    }
  }
}