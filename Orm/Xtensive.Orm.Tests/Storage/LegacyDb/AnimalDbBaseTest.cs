// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.26

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.LegacyDb.AnimalDbTestModel;

namespace Xtensive.Orm.Tests.Storage.LegacyDb.AnimalDbTestModel
{
  [Serializable]
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.SingleTable)]
  [TypeDiscriminatorValue("Animal", Default = true)]
  public class Animal : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    [Field(Length = 50), TypeDiscriminator]
    [FieldMapping("Type")]
    public string ElementType { get; private set; }

    [Field(Length = 50)]
    public string Name { get; set; }

    [Field]
    public int Age { get; set; }

    [Field]
    [FieldMapping("Owner")]
    public Person Owner { get; set; }
  }

  [Serializable]
  [TypeDiscriminatorValue("Dog")]
  public class Dog : Animal
  {
  }

  [Serializable]
  [TypeDiscriminatorValue("Cat")]
  public class Cat : Animal
  {
  }

  [Serializable]
  [HierarchyRoot]
  public abstract class Person : Entity
  {
    [Field, Key]
    public Guid Id { get; private set;}

    [Field(Length = 50)]
    public string Name { get; set; }

    [Field(Length = 50), TypeDiscriminator]
    [FieldMapping("Type")]
    public string ElementType { get; private set; }
  }

  [Serializable]
  [TypeDiscriminatorValue("Cat")]
  public class CatLover : Person
  {
    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Clear)]
    public Cat Favorite { get; set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Clear)]
    public EntitySet<Cat> Pets { get; private set; }
  }

  [Serializable]
  [TypeDiscriminatorValue("Dog")]
  public class DogLover : Person
  {
    [Association(OnOwnerRemove = OnRemoveAction.Clear)]
    public Dog Favorite { get; set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Clear)]
    public EntitySet<Dog> Pets { get; private set; }
  }

  [Serializable]
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
  public abstract class VetClinic : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    [Field(Length = 50)]
    public string Name { get; set; }

    [Field(Length = 50)]
    public string Address { get; set; }

    public abstract void Heal(Animal patient);

    public abstract void Kill(Animal victim);
  }

  [Serializable]
  public class DogClinic : VetClinic
  {
    public override void Heal(Animal patient)
    {
      Console.WriteLine(string.Format("Healing the dog {0}", patient.Name));
    }

    public override void Kill(Animal victim)
    {
      Console.WriteLine(string.Format("Killing the dog {0}", victim.Name));
    }
  }

  [Serializable]
  public class CatClinic : VetClinic
  {
    public override void Heal(Animal patient)
    {
      Console.WriteLine(string.Format("Healing the cat {0}", patient.Name));
    }

    public override void Kill(Animal victim)
    {
      Console.WriteLine(string.Format("Killing the cat {0}", victim.Name));
    }
  }
}

namespace Xtensive.Orm.Tests.Storage.LegacyDb
{
  public abstract class AnimalDbBaseTest : LegacyDbAutoBuildTest
  {
    protected abstract DomainUpgradeMode GetUpgradeMode();
    
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Animal).Assembly, typeof (Animal).Namespace);
      config.UpgradeMode = GetUpgradeMode();
      return config;
    }

    protected override string GetCreateDbScript(DomainConfiguration config)
    {
      return @"CREATE TABLE [dbo].[Person](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Type] [nvarchar](50) NULL,
 CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[VetClinic](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Address] [nvarchar](50) NOT NULL
) ON [PRIMARY]

CREATE TABLE [dbo].[DogClinic](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Address] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_DogClinic] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[CatClinic](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Address] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_CatClinic] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[CatLover](
	[Id] [uniqueidentifier] NOT NULL,
	[Favorite.Id] [uniqueidentifier] NULL,
 CONSTRAINT [PK_CatLover.Person] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [CatLover.FK_Favorite] ON [dbo].[CatLover] 
(
	[Favorite.Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE TABLE [dbo].[Animal](
	[Id] [uniqueidentifier] NOT NULL,
	[Type] [nvarchar](50) NULL,
	[Name] [nvarchar](50) NULL,
	[Age] [int] NOT NULL,
	[Owner] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Animal] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [Animal.FK_Owner] ON [dbo].[Animal] 
(
	[Owner] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE TABLE [dbo].[DogLover](
	[Id] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_DogLover.Person] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[CatLover-Pets-Cat](
	[CatLover] [uniqueidentifier] NOT NULL,
	[Cat] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_CatLover-Pets-Cat] PRIMARY KEY CLUSTERED 
(
	[CatLover] ASC,
	[Cat] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [CatLover-Pets-Cat.FK_Master] ON [dbo].[CatLover-Pets-Cat] 
(
	[CatLover] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [CatLover-Pets-Cat.FK_Slave] ON [dbo].[CatLover-Pets-Cat] 
(
	[Cat] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE TABLE [dbo].[DogLover-Pets-Dog](
	[DogLover] [uniqueidentifier] NOT NULL,
	[Dog] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_DogLover-Pets-Dog] PRIMARY KEY CLUSTERED 
(
	[DogLover] ASC,
	[Dog] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [DogLover-Pets-Dog.FK_Master] ON [dbo].[DogLover-Pets-Dog] 
(
	[DogLover] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [DogLover-Pets-Dog.FK_Slave] ON [dbo].[DogLover-Pets-Dog] 
(
	[Dog] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

ALTER TABLE [dbo].[Person] ADD  DEFAULT (NULL) FOR [Name]

ALTER TABLE [dbo].[Person] ADD  DEFAULT (NULL) FOR [Type]

ALTER TABLE [dbo].[CatLover] ADD  DEFAULT (NULL) FOR [Favorite.Id]

ALTER TABLE [dbo].[Animal] ADD  DEFAULT (NULL) FOR [Type]

ALTER TABLE [dbo].[Animal] ADD  DEFAULT (NULL) FOR [Name]

ALTER TABLE [dbo].[Animal] ADD  DEFAULT ((0)) FOR [Age]

ALTER TABLE [dbo].[Animal] ADD  DEFAULT (NULL) FOR [Owner]

ALTER TABLE [dbo].[CatLover]  WITH CHECK ADD  CONSTRAINT [FK_CatLover_Person] FOREIGN KEY([Id])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[CatLover] CHECK CONSTRAINT [FK_CatLover_Person]

ALTER TABLE [dbo].[CatLover]  WITH CHECK ADD  CONSTRAINT [FK_CatLover-Favorite-Cat_Favorite] FOREIGN KEY([Favorite.Id])
REFERENCES [dbo].[Animal] ([Id])

ALTER TABLE [dbo].[CatLover] CHECK CONSTRAINT [FK_CatLover-Favorite-Cat_Favorite]

ALTER TABLE [dbo].[Animal]  WITH CHECK ADD  CONSTRAINT [FK_Animal-Owner-Person_Owner] FOREIGN KEY([Owner])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[Animal] CHECK CONSTRAINT [FK_Animal-Owner-Person_Owner]

ALTER TABLE [dbo].[DogLover]  WITH CHECK ADD  CONSTRAINT [FK_DogLover_Person] FOREIGN KEY([Id])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[DogLover] CHECK CONSTRAINT [FK_DogLover_Person]

ALTER TABLE [dbo].[CatLover-Pets-Cat]  WITH CHECK ADD  CONSTRAINT [FK_CatLover-Pets-Cat_Master] FOREIGN KEY([CatLover])
REFERENCES [dbo].[CatLover] ([Id])

ALTER TABLE [dbo].[CatLover-Pets-Cat] CHECK CONSTRAINT [FK_CatLover-Pets-Cat_Master]

ALTER TABLE [dbo].[CatLover-Pets-Cat]  WITH CHECK ADD  CONSTRAINT [FK_CatLover-Pets-Cat_Slave] FOREIGN KEY([Cat])
REFERENCES [dbo].[Animal] ([Id])

ALTER TABLE [dbo].[CatLover-Pets-Cat] CHECK CONSTRAINT [FK_CatLover-Pets-Cat_Slave]

ALTER TABLE [dbo].[DogLover-Pets-Dog]  WITH CHECK ADD  CONSTRAINT [FK_DogLover-Pets-Dog_Master] FOREIGN KEY([DogLover])
REFERENCES [dbo].[DogLover] ([Id])

ALTER TABLE [dbo].[DogLover-Pets-Dog] CHECK CONSTRAINT [FK_DogLover-Pets-Dog_Master]

ALTER TABLE [dbo].[DogLover-Pets-Dog]  WITH CHECK ADD  CONSTRAINT [FK_DogLover-Pets-Dog_Slave] FOREIGN KEY([Dog])
REFERENCES [dbo].[Animal] ([Id])

ALTER TABLE [dbo].[DogLover-Pets-Dog] CHECK CONSTRAINT [FK_DogLover-Pets-Dog_Slave]";
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        Animal animal = new Animal();
        animal.Name = "Elephant";
        Dog dog = new Dog();
        dog.Name = "Rex";
        Cat cat = new Cat();
        cat.Name = "Pussycat";
        var catLover = new CatLover {Name = "CatLover", Favorite = cat};
        catLover.Pets.Add(cat);
        var dogLover = new DogLover { Name = "CatLover", Favorite = dog };
        dogLover.Pets.Add(dog);
        var dogClinic = new DogClinic {Name = "Dog healer.", Address = "Address 1"};
        var catClinic = new CatClinic {Name = "Cat healer.", Address = "Address 2"};
        t.Complete();
      }
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var animals = session.Query.All<Animal>().ToList();
        var dogs = session.Query.All<Dog>().ToList();
        var cats = session.Query.All<Cat>().ToList();
        var persons = session.Query.All<Person>().ToList();
        var dogLovers = session.Query.All<DogLover>().ToList();
        var catLovers = session.Query.All<CatLover>().ToList();
        var clinics = session.Query.All<VetClinic>().ToList();
        var dogClinics = session.Query.All<DogClinic>().ToList();
        var catClinics = session.Query.All<CatClinic>().ToList();

        Assert.AreEqual(3, animals.Count);
        Assert.AreEqual(1, dogs.Count);
        Assert.AreEqual(1, cats.Count);

        Assert.AreEqual(2, persons.Count);
        Assert.AreEqual(1, dogLovers.Count);
        Assert.AreEqual(1, catLovers.Count);

        Assert.AreEqual(2, clinics.Count);
        Assert.AreEqual(1, dogClinics.Count);
        Assert.AreEqual(1, catClinics.Count);
        t.Complete();
      }
    }
  }
}