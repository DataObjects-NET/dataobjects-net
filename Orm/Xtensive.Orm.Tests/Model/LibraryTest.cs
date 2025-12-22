// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.04

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Model.LibraryModel;
using FieldAttributes=Xtensive.Orm.Model.FieldAttributes;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Tests.Model.LibraryModel
{
  [Serializable]
  public class IdentityCard : Structure
  {
    [Field(Length = 64)]
    public string FirstName { get; set; }

    [Field(Length = 64)]
    public string SecondName { get; set; }

    [Field(Length = 64)]
    public string LastName { get; set; }
  }

  [Serializable]
  public class Passport : Structure
  {
    [Field]
    public int Number { get; set; }

    [Field]
    public IdentityCard Card { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, FieldMapping("PassportNumber"), Key]
    public int Number
    {
      get { return GetFieldValue<int>("Number"); }
    }

    [Field]
    public Passport Passport
    {
      get { return GetFieldValue<Passport>("Passport"); }
      set
      {
        SetFieldValue("Passport", value);
      }
    }

    [Field, Association(PairTo = "Reviewer")]
    public EntitySet<BookReview> Reviews { get; private set; }
  }

  [Serializable]
  [Index("PenName:DESC", Name = "IX_PENNAME")]
  public class Author : Person
  {
    [Field(Length = 64)]
    public string PenName { get; set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }
  }

  [Serializable]
  [KeyGenerator(KeyGeneratorKind.None)]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field(Length = 32), Key]
    public string Isbn { get; private set; }

    [Field(Length = 128, Indexed = true)]
    public string Title { get; set; }

    [Field, FieldMapping("BookAuthor")]
    [Association(OnTargetRemove = OnRemoveAction.Deny)]
    public Author Author { get; set; }

    public int Rating { get; set; }

    public Book(string isbn)
      : base(isbn)
    {
    }
  }

  [Serializable]
  [KeyGenerator(KeyGeneratorKind.None)]
  [HierarchyRoot]
  public class BookReview : Entity
  {
    [Field, Association(OnTargetRemove = OnRemoveAction.Clear), Key(1)]
    public Person Reviewer { get; private set; }

    [Field, FieldMapping("Book"), Association(OnTargetRemove = OnRemoveAction.Cascade), Key(0)]
    public Book Book { get; private set; }

    [Field(Length = 4096)]
    public string Text { get; set; }

    public BookReview(Book book, Person reviewer)
      : base(book, reviewer)
    {
    }
  }

  public class IsbnKeyGenerator : KeyGenerator
  {
    private int counter;

    public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
    {
    }

    public override Tuple GenerateKey(KeyInfo keyInfo, Session session)
    {
      var result = Tuple.Create(counter.ToString());
      counter++;
      return result;
    }
  }

  public class LibraryDomainBuilder : IModule
  {
    public static bool IsEnabled;

    private BuildingContext context;

    public void OnBuilt(Domain domain)
    {
    }

    private void VerifyTypeCollection()
    {
      var types = context.ModelDef.Types;
      Assert.That(types.FindAncestor(types[typeof (Entity)]), Is.Null);
      Assert.That(types.FindAncestor(types[typeof (IEntity)]), Is.Null);
      Assert.That(types.FindAncestor(types[typeof (Structure)]), Is.Null);
      Assert.That(types[typeof (Structure)], Is.EqualTo(types.FindAncestor(types[typeof (Passport)])));
      Assert.That(types[typeof (Structure)], Is.EqualTo(types.FindAncestor(types[typeof (IdentityCard)])));
      Assert.That(types[typeof (Entity)], Is.EqualTo(types.FindAncestor(types[typeof (Person)])));
      Assert.That(types[typeof (Entity)], Is.EqualTo(types.FindAncestor(types[typeof (Book)])));
      Assert.That(types[typeof (Entity)], Is.EqualTo(types.FindAncestor(types[typeof (BookReview)])));
      Assert.That(types[typeof (Person)], Is.EqualTo(types.FindAncestor(types[typeof (Author)])));
    }

    private void RedefineTypes()
    {
      context.ModelDef.Types.Clear();
      _ = context.ModelDef.DefineType(typeof (BookReview));
      _ = context.ModelDef.DefineType(typeof (Book));
      _ = context.ModelDef.DefineType(typeof (Person));
      _ = context.ModelDef.DefineType(typeof (Author));
      _ = context.ModelDef.DefineType(typeof (Structure));
      _ = context.ModelDef.DefineType(typeof (Passport));
      _ = context.ModelDef.DefineType(typeof (IdentityCard));
      _ = context.ModelDef.DefineType(typeof (Entity));
      _ = context.ModelDef.DefineType(typeof (IEntity));
    }

    private void RedefineFields()
    {
      var types = context.ModelDef.Types;
      foreach (TypeDef type in types) {
        type.Fields.Clear();
        type.Indexes.Clear();

        var properties =
          type.UnderlyingType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
            BindingFlags.DeclaredOnly);
        for (int index = 0, count = properties.Length; index < count; index++) {
          var attributes = properties[index].GetCustomAttributes(typeof (FieldAttribute), true);
          if (attributes==null || attributes.Length==0)
            continue;
          if (!properties[index].Name.Contains("."))
            _ = type.DefineField(properties[index]);
        }
      }

      types["Book"].Fields["Author"].OnTargetRemove = OnRemoveAction.Deny;
      types["BookReview"].Fields["Book"].OnTargetRemove = OnRemoveAction.Cascade;
      types["BookReview"].Fields["Reviewer"].OnTargetRemove = OnRemoveAction.Clear;


      IndexDef indexDef;

      indexDef = types["Author"].DefineIndex("IX_PENNAME");
      indexDef.MappingName = "IX_PENNAME";
      indexDef.KeyFields.Add("PenName", Direction.Negative);

      indexDef = types["Book"].DefineIndex("IX_Title");
      indexDef.KeyFields.Add("Title");
    }

    private void RedefineIndexes()
    {
    }

    private void VerifyDefinition()
    {
      Assert.That(context.ModelDef.Types[typeof (Entity)], Is.Not.Null);
      Assert.That(context.ModelDef.Types[typeof (IEntity)], Is.Not.Null);

      #region IdentityCard

      TypeDef typeDef = context.ModelDef.Types[typeof (IdentityCard)];
      Assert.That(typeDef, Is.Not.Null);
      Assert.That(typeDef.Fields["FirstName"], Is.Not.Null);
      Assert.That(typeDef.Fields["FirstName"].Name, Is.EqualTo("FirstName"));
      Assert.That(typeDef.Fields["FirstName"].Length, Is.EqualTo(64));
      Assert.That(typeDef.Fields["SecondName"], Is.Not.Null);
      Assert.That(typeDef.Fields["SecondName"].Name, Is.EqualTo("SecondName"));
      Assert.That(typeDef.Fields["SecondName"].Length, Is.EqualTo(64));
      Assert.That(typeDef.Fields["LastName"], Is.Not.Null);
      Assert.That(typeDef.Fields["LastName"].Name, Is.EqualTo("LastName"));
      Assert.That(typeDef.Fields["LastName"].Length, Is.EqualTo(64));

      #endregion

      #region Passport

      typeDef = context.ModelDef.Types[typeof (Passport)];
      Assert.That(typeDef.Fields["Number"], Is.Not.Null);
      Assert.That(typeDef.Fields["Number"].Name, Is.EqualTo("Number"));
      Assert.That(typeDef.Fields["Card"], Is.Not.Null);
      Assert.That(typeDef.Fields["Card"].Name, Is.EqualTo("Card"));

      #endregion

      #region Person

      typeDef = context.ModelDef.Types[typeof (Person)];
      Assert.That(typeDef, Is.Not.Null);
      Assert.That(context.ModelDef.Types["Person"], Is.Not.Null);
      Assert.That(context.ModelDef.Types["Person"], Is.EqualTo(typeDef));
      Assert.That(typeDef.Name, Is.EqualTo("Person"));

      // Fields
      Assert.That(typeDef.Fields["Passport"], Is.Not.Null);
      Assert.That(typeDef.Fields["Passport"].Name, Is.EqualTo("Passport"));
      Assert.That(typeDef.Fields["Passport"].IsStructure, Is.True);
      Assert.That(typeDef.Fields["Passport"].IsEntity, Is.False);
      Assert.That(typeDef.Fields["Passport"].IsEntitySet, Is.False);

      #endregion

      #region Author

      typeDef = context.ModelDef.Types[typeof (Author)];
      Assert.That(typeDef, Is.Not.Null);
      Assert.That(context.ModelDef.Types["Author"], Is.Not.Null);
      Assert.That(context.ModelDef.Types["Author"], Is.EqualTo(typeDef));
      Assert.That(typeDef.Name, Is.EqualTo("Author"));

      // Fields
      Assert.That(typeDef.Fields["PenName"], Is.Not.Null);
      Assert.That(typeDef.Fields["PenName"].Name, Is.EqualTo("PenName"));

      // Indexes
      Assert.That(typeDef.Indexes["IX_PENNAME"], Is.Not.Null);
      Assert.That(typeDef.Indexes[0], Is.Not.Null);
      Assert.That(typeDef.Indexes[0], Is.EqualTo(typeDef.Indexes["IX_PENNAME"]));
      Assert.That(typeDef.Indexes["IX_PENNAME"].IsPrimary, Is.False);
      Assert.That(typeDef.Indexes["IX_PENNAME"].IsUnique, Is.False);
      Assert.That(typeDef.Indexes[0].KeyFields.Count, Is.EqualTo(1));
      Assert.That(typeDef.Indexes[0].KeyFields[0].Key, Is.EqualTo("PenName"));
      Assert.That(typeDef.Indexes[0].KeyFields[0].Value, Is.EqualTo(Direction.Negative));

      #endregion

      #region Book

      typeDef = context.ModelDef.Types[typeof (Book)];
      Assert.That(typeDef, Is.Not.Null);
      Assert.That(context.ModelDef.Types["Book"], Is.Not.Null);
      Assert.That(context.ModelDef.Types["Book"], Is.EqualTo(typeDef));
      Assert.That(typeDef.Name, Is.EqualTo("Book"));

      // Fields
      Assert.That(typeDef.Fields["Isbn"], Is.Not.Null);
      Assert.That(typeDef.Fields["Isbn"].Name, Is.EqualTo("Isbn"));
      Assert.That(typeDef.Fields["Isbn"].Length, Is.EqualTo(32));

      Assert.That(typeDef.Fields["Title"], Is.Not.Null);
      Assert.That(typeDef.Fields["Title"].Name, Is.EqualTo("Title"));
      Assert.That(typeDef.Fields["Title"].Length, Is.EqualTo(128));

      Assert.That(typeDef.Fields["Author"], Is.Not.Null);
      Assert.That(typeDef.Fields["Author"].OnTargetRemove, Is.EqualTo(OnRemoveAction.Deny));
      Assert.That(typeDef.Fields["Author"].Name, Is.EqualTo("Author"));

      Assert.That(typeDef.Indexes["IX_Title"], Is.Not.Null);
      Assert.That(typeDef.Indexes["IX_Title"].IsPrimary, Is.False);
      Assert.That(typeDef.Indexes["IX_Title"].IsUnique, Is.False);
      Assert.That(typeDef.Indexes["IX_Title"].KeyFields.Count, Is.EqualTo(1));
      Assert.That(typeDef.Indexes["IX_Title"].KeyFields[0].Key, Is.EqualTo("Title"));
      Assert.That(typeDef.Indexes["IX_Title"].KeyFields[0].Value, Is.EqualTo(Direction.Positive));

      #endregion

      #region BookReview

      typeDef = context.ModelDef.Types[typeof (BookReview)];
      Assert.That(typeDef, Is.Not.Null);
      Assert.That(context.ModelDef.Types["BookReview"], Is.Not.Null);
      Assert.That(context.ModelDef.Types["BookReview"], Is.EqualTo(typeDef));
      Assert.That(typeDef.Name, Is.EqualTo("BookReview"));

      // Fields
      Assert.That(typeDef.Fields["Book"], Is.Not.Null);
      Assert.That(typeDef.Fields["Book"].Name, Is.EqualTo("Book"));
      Assert.That(typeDef.Fields["Book"].OnTargetRemove, Is.EqualTo(OnRemoveAction.Cascade));

      Assert.That(typeDef.Fields["Reviewer"], Is.Not.Null);
      Assert.That(typeDef.Fields["Reviewer"].Name, Is.EqualTo("Reviewer"));
      Assert.That(typeDef.Fields["Reviewer"].OnTargetRemove, Is.EqualTo(OnRemoveAction.Clear));

      Assert.That(typeDef.Fields["Text"], Is.Not.Null);
      Assert.That(typeDef.Fields["Text"].Name, Is.EqualTo("Text"));
      Assert.That(typeDef.Fields["Text"].Length, Is.EqualTo(4096));

      #endregion

      Console.WriteLine("-- Model verification is completed --");
    }

    #region IModule Members

    public void OnDefinitionsBuilt(BuildingContext buildingContext, DomainModelDef model)
    {
      if (!IsEnabled)
        return;

      context = buildingContext;

      try {
        Console.WriteLine("-- Verifying model --");
        VerifyDefinition();
        Console.WriteLine("-- Redefining types --");
        RedefineTypes();
        Console.WriteLine("-- Verifying model --");
        VerifyDefinition();
        Console.WriteLine("-- Redefining fields --");
        RedefineFields();
        Console.WriteLine("-- Redefining indexes --");
        RedefineIndexes();
        Console.WriteLine("-- Verifying model --");
        VerifyDefinition();
        VerifyTypeCollection();
      }
      finally {
        context = null;
      }
    }

    #endregion
  }
}

namespace Xtensive.Orm.Tests.Model
{
  public class LibraryTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof (Person).Assembly, "Xtensive.Orm.Tests.Model.LibraryModel");
      // config.Builders.Add(typeof (LibraryDomainBuilder));
      return config;
    }

    private static void VerifyModel(Domain domain)
    {
      TypeInfoCollection types = domain.Model.Types;
      Assert.That(types[typeof(Person)].Ancestor, Is.Null);
      Assert.That(types[typeof(Book)].Ancestor, Is.Null);
      Assert.That(types[typeof(BookReview)].Ancestor, Is.Null);
      Assert.That(types[typeof (Person)], Is.EqualTo(types[typeof (Author)].Ancestor));

      var collection = types.Structures;
      Assert.That(collection.Any(), Is.True);
      foreach (TypeInfo item in collection) {
        Assert.That(item.IsStructure, Is.True);
        Assert.That(item.IsInterface, Is.False);
        Assert.That(item.IsEntity, Is.False);
      }

      collection = types.Interfaces;
      Assert.That(collection.Any(), Is.False);
      foreach (TypeInfo item in collection) {
        Assert.That(item.IsInterface, Is.True);
        Assert.That(item.IsStructure, Is.False);
        Assert.That(item.IsEntity, Is.False);
      }

      collection = types.Entities;
      Assert.That(collection.Any(), Is.True);
      foreach (TypeInfo item in collection) {
        Assert.That(item.IsEntity, Is.True);
        Assert.That(item.IsInterface, Is.False);
        Assert.That(item.IsStructure, Is.False);
      }

      #region IdentityCard

      TypeInfo typeInfo = domain.Model.Types[typeof (IdentityCard)];

      // Fields
      Assert.That(typeInfo.Fields["FirstName"], Is.Not.Null);
      Assert.That(typeInfo.Fields["FirstName"].Name, Is.EqualTo("FirstName"));
      Assert.That(typeInfo.Fields["FirstName"].Length, Is.EqualTo(64));
      Assert.That(typeInfo.Fields["FirstName"].IsDeclared, Is.True);
      Assert.That(typeInfo.Fields["FirstName"].IsInherited, Is.False);
      Assert.That(typeInfo.Fields["SecondName"], Is.Not.Null);
      Assert.That(typeInfo.Fields["SecondName"].Name, Is.EqualTo("SecondName"));
      Assert.That(typeInfo.Fields["SecondName"].Length, Is.EqualTo(64));
      Assert.That(typeInfo.Fields["SecondName"].IsDeclared, Is.True);
      Assert.That(typeInfo.Fields["SecondName"].IsInherited, Is.False);
      Assert.That(typeInfo.Fields["LastName"], Is.Not.Null);
      Assert.That(typeInfo.Fields["LastName"].Name, Is.EqualTo("LastName"));
      Assert.That(typeInfo.Fields["LastName"].Length, Is.EqualTo(64));
      Assert.That(typeInfo.Fields["LastName"].IsDeclared, Is.True);
      Assert.That(typeInfo.Fields["LastName"].IsInherited, Is.False);

      #endregion

      #region Passport

      typeInfo = domain.Model.Types[typeof (Passport)];

      // Fields
      Assert.That(typeInfo.Fields["Number"], Is.Not.Null);
      Assert.That(typeInfo.Fields["Number"].Name, Is.EqualTo("Number"));
      Assert.That(typeInfo.Fields["Number"].IsDeclared, Is.True);
      Assert.That(typeInfo.Fields["Number"].IsInherited, Is.False);
      Assert.That(typeInfo.Fields["Number"].IsStructure, Is.False);
      Assert.That(typeInfo.Fields["Number"].IsEntity, Is.False);
      Assert.That(typeInfo.Fields["Number"].IsEntitySet, Is.False);

      Assert.That(typeInfo.Fields["Card"], Is.Not.Null);
      Assert.That(typeInfo.Fields["Card"].Name, Is.EqualTo("Card"));
      Assert.That(typeInfo.Fields["Card"].IsStructure, Is.True);
      Assert.That(typeInfo.Fields["Card"].IsEntity, Is.False);
      Assert.That(typeInfo.Fields["Card"].IsEntitySet, Is.False);
      Assert.That(typeInfo.Fields["Card"].Fields.Count, Is.EqualTo(3));
      Assert.That(typeInfo.Fields["Card"].IsDeclared, Is.True);
      Assert.That(typeInfo.Fields["Card"].IsInherited, Is.False);

      Assert.That(typeInfo.Columns["Card.FirstName"], Is.Not.Null);
      Assert.That(typeInfo.Columns["Card.FirstName"].Name, Is.EqualTo("Card.FirstName"));
      Assert.That(typeInfo.Columns["Card.FirstName"].Length, Is.EqualTo(64));
      Assert.That(typeInfo.Columns["Card.SecondName"], Is.Not.Null);
      Assert.That(typeInfo.Columns["Card.SecondName"].Name, Is.EqualTo("Card.SecondName"));
      Assert.That(typeInfo.Columns["Card.SecondName"].Length, Is.EqualTo(64));
      Assert.That(typeInfo.Columns["Card.LastName"], Is.Not.Null);
      Assert.That(typeInfo.Columns["Card.LastName"].Name, Is.EqualTo("Card.LastName"));
      Assert.That(typeInfo.Columns["Card.LastName"].Length, Is.EqualTo(64));

      #endregion

      #region Person

      typeInfo = domain.Model.Types[typeof (Person)];
      Assert.That(typeInfo, Is.Not.Null);
      Assert.That(domain.Model.Types["Person"], Is.Not.Null);
      Assert.That(domain.Model.Types["Person"], Is.EqualTo(typeInfo));
      Assert.That(typeInfo.Name, Is.EqualTo("Person"));

      // Fields
      Assert.That(typeInfo.Fields["Passport"], Is.Not.Null);
      Assert.That(typeInfo.Fields["Passport"].Name, Is.EqualTo("Passport"));
      Assert.That(typeInfo.Fields["Passport"].IsStructure, Is.True);
      Assert.That(typeInfo.Fields["Passport"].IsEntity, Is.False);
      Assert.That(typeInfo.Fields["Passport"].IsEntitySet, Is.False);
      //      Assert.AreEqual(typeInfo.Fields["Passport"].Columns.Count, 4);

      Assert.That(typeInfo.Fields["Reviews"], Is.Not.Null);
      Assert.That(typeInfo.Fields["Reviews"].Name, Is.EqualTo("Reviews"));
      Assert.That(typeInfo.Fields["Reviews"].IsStructure, Is.False);
      Assert.That(typeInfo.Fields["Reviews"].IsEntity, Is.False);
      Assert.That(typeInfo.Fields["Reviews"].IsEntitySet, Is.True);

      // KeyColumns
      Assert.That(typeInfo.Columns["Passport.Number"], Is.Not.Null);
      Assert.That(typeInfo.Columns["Passport.Number"].Name, Is.EqualTo("Passport.Number"));
      //      Assert.AreEqual(person.Fields["Passport"].Indexes[0], person.Key);
      Assert.That(typeInfo.Columns["Passport.Card.FirstName"], Is.Not.Null);
      Assert.That(typeInfo.Columns["Passport.Card.FirstName"].Name, Is.EqualTo("Passport.Card.FirstName"));
      Assert.That(typeInfo.Columns["Passport.Card.FirstName"].Length, Is.EqualTo(64));
      Assert.That(typeInfo.Columns["Passport.Card.SecondName"], Is.Not.Null);
      Assert.That(typeInfo.Columns["Passport.Card.SecondName"].Name, Is.EqualTo("Passport.Card.SecondName"));
      Assert.That(typeInfo.Columns["Passport.Card.SecondName"].Length, Is.EqualTo(64));
      Assert.That(typeInfo.Columns["Passport.Card.LastName"], Is.Not.Null);
      Assert.That(typeInfo.Columns["Passport.Card.LastName"].Name, Is.EqualTo("Passport.Card.LastName"));
      Assert.That(typeInfo.Columns["Passport.Card.LastName"].Length, Is.EqualTo(64));

      // Indexes
      if (IsSingleTable(typeInfo) || IsConcreteTable(typeInfo))
        Assert.That(typeInfo.Indexes.Count, Is.EqualTo(2));
      else
        Assert.That(typeInfo.Indexes.Count, Is.EqualTo(1));
      Assert.That(typeInfo.Indexes["PK_Person"], Is.Not.Null);
      Assert.That(typeInfo.Indexes["PK_Person"].IsPrimary, Is.True);
      Assert.That(typeInfo.Indexes["PK_Person"].IsUnique, Is.True);
      Assert.That(typeInfo.Indexes["PK_Person"].KeyColumns.Count, Is.EqualTo(typeInfo.Hierarchy.Key.TupleDescriptor.Count));
      Assert.That(typeInfo.Indexes["PK_Person"].KeyColumns[0].Key, Is.EqualTo(typeInfo.Columns["PassportNumber"]));

      #endregion

      #region Author

      typeInfo = domain.Model.Types[typeof (Author)];
      Assert.That(typeInfo, Is.Not.Null);
      Assert.That(domain.Model.Types["Author"], Is.Not.Null);
      Assert.That(domain.Model.Types["Author"], Is.EqualTo(typeInfo));
      Assert.That(typeInfo.Name, Is.EqualTo("Author"));

      // Fields
      Assert.That(typeInfo.Fields["Passport"], Is.Not.Null);
      Assert.That(typeInfo.Fields["Passport"].Name, Is.EqualTo("Passport"));
      Assert.That(typeInfo.Fields["Passport"].IsStructure, Is.True);
      Assert.That(typeInfo.Fields["Passport"].IsInherited, Is.True);
      Assert.That(typeInfo.Fields["Passport"].Fields.Count, Is.EqualTo(5));
      Assert.That(typeInfo.Fields["PenName"], Is.Not.Null);
      Assert.That(typeInfo.Fields["PenName"].IsNullable, Is.EqualTo(true));
      Assert.That(typeInfo.Fields["PenName"].Name, Is.EqualTo("PenName"));
      Assert.That(typeInfo.Fields["PenName"].Length, Is.EqualTo(64));
      Assert.That(typeInfo.Fields["Books"], Is.Not.Null);
      Assert.That(typeInfo.Fields["Books"].IsNullable, Is.EqualTo(true));
      Assert.That(typeInfo.Fields["Books"].Name, Is.EqualTo("Books"));

      Assert.That(typeInfo.Fields.Find(FieldAttributes.Declared).Count(), Is.EqualTo(2));
      Assert.That(typeInfo.Fields.Find(FieldAttributes.Inherited).Count(), Is.EqualTo(9));

      // KeyColumns
      Assert.That(typeInfo.Columns["PassportNumber"], Is.Not.Null);
      Assert.That(typeInfo.Columns["PassportNumber"].Name, Is.EqualTo("PassportNumber"));
      //      Assert.AreEqual(person.Fields["Passport"].Indexes[0], person.Key);
      Assert.That(typeInfo.Columns["Passport.Card.FirstName"], Is.Not.Null);
      Assert.That(typeInfo.Columns["Passport.Card.FirstName"].Name, Is.EqualTo("Passport.Card.FirstName"));
      Assert.That(typeInfo.Columns["Passport.Card.FirstName"].Length, Is.EqualTo(64));
      Assert.That(typeInfo.Columns["Passport.Card.SecondName"], Is.Not.Null);
      Assert.That(typeInfo.Columns["Passport.Card.SecondName"].Name, Is.EqualTo("Passport.Card.SecondName"));
      Assert.That(typeInfo.Columns["Passport.Card.SecondName"].Length, Is.EqualTo(64));
      Assert.That(typeInfo.Columns["Passport.Card.LastName"], Is.Not.Null);
      Assert.That(typeInfo.Columns["Passport.Card.LastName"].Name, Is.EqualTo("Passport.Card.LastName"));
      Assert.That(typeInfo.Columns["Passport.Card.LastName"].Length, Is.EqualTo(64));
      Assert.That(typeInfo.Columns["PenName"], Is.Not.Null);
      Assert.That(typeInfo.Columns["PenName"].IsNullable, Is.EqualTo(true));
      Assert.That(typeInfo.Columns["PenName"].Name, Is.EqualTo("PenName"));
      Assert.That(typeInfo.Columns["PenName"].Length, Is.EqualTo(64));
      Assert.That(typeInfo.Columns.Contains("Reviews"), Is.False);

      // Indexes
      if (!IsSingleTable(typeInfo)) {
        if (IsConcreteTable(typeInfo))
          Assert.That(typeInfo.Indexes.Count, Is.EqualTo(2));
        else
          Assert.That(typeInfo.Indexes.Count, Is.EqualTo(3));
        Assert.That(typeInfo.Indexes["Author.IX_PENNAME"], Is.Not.Null);
        Assert.That(typeInfo.Indexes[0], Is.Not.Null);
        Assert.That(typeInfo.Indexes["Author.IX_PENNAME"], Is.EqualTo(typeInfo.Indexes[0]));
        Assert.That(typeInfo.Indexes[0].KeyColumns.Count, Is.EqualTo(1));
        Assert.That(typeInfo.Indexes[0].KeyColumns[0], Is.Not.EqualTo(default(KeyValuePair<ColumnInfo,Direction>)));
        Assert.That(typeInfo.Indexes[0].KeyColumns[0].Key, Is.EqualTo(typeInfo.Columns["PenName"]));
      }

      #endregion

      #region Book

      typeInfo = domain.Model.Types[typeof (Book)];
      Assert.That(typeInfo, Is.Not.Null);
      Assert.That(domain.Model.Types["Book"], Is.Not.Null);
      Assert.That(domain.Model.Types["Book"], Is.EqualTo(typeInfo));
      Assert.That("Book", Is.EqualTo(typeInfo.Name));

      // Fields
      Assert.That(typeInfo.Fields["Isbn"], Is.Not.Null);
      Assert.That("Isbn", Is.EqualTo(typeInfo.Fields["Isbn"].Name));
      Assert.That(32, Is.EqualTo(typeInfo.Fields["Isbn"].Length));
      //      Assert.AreEqual(book.Fields["Isbn"].Indexes[0], book.Key);

      Assert.That(typeInfo.Fields["Title"], Is.Not.Null);
      Assert.That("Title", Is.EqualTo(typeInfo.Fields["Title"].Name));
      Assert.That(128, Is.EqualTo(typeInfo.Fields["Title"].Length));

      Assert.That(typeInfo.Fields["Author"], Is.Not.Null);
      Assert.That("Author", Is.EqualTo(typeInfo.Fields["Author"].Name));
      Assert.That(typeInfo.Fields["Author"].IsStructure, Is.False);
      Assert.That(typeInfo.Fields["Author"].IsEntity, Is.True);
      Assert.That(typeInfo.Fields["Author"].IsEntitySet, Is.False);
      Assert.That(typeInfo.Fields["Author"].Associations.Last().OnTargetRemove, Is.EqualTo(OnRemoveAction.Deny));

      // Indexes
      Assert.That(typeInfo.Indexes.Count, Is.EqualTo(6));
      Assert.That(typeInfo.Indexes["PK_Book"], Is.Not.Null);
      Assert.That(typeInfo.Indexes["PK_Book"].IsPrimary, Is.True);
      Assert.That(typeInfo.Indexes["PK_Book"].IsUnique, Is.True);
      Assert.That(typeInfo.Indexes["PK_Book"].KeyColumns.Count, Is.EqualTo(typeInfo.Hierarchy.Key.TupleDescriptor.Count));
      Assert.That(typeInfo.Indexes["PK_Book"].KeyColumns[0].Key.Name, Is.EqualTo("Isbn"));
      Assert.That(typeInfo.Indexes["PK_Book"].KeyColumns[0].Value, Is.EqualTo(Direction.Positive));

      Assert.That(typeInfo.Indexes["Book.FK_Author"], Is.Not.Null);
      Assert.That(typeInfo.Indexes["Book.FK_Author"].IsPrimary, Is.False);
      Assert.That(typeInfo.Indexes["Book.FK_Author"].IsUnique, Is.False);
      Assert.That(typeInfo.Indexes["Book.FK_Author"].KeyColumns.Count, Is.EqualTo(domain.Model.Types[typeof(Author)].Hierarchy.Key.TupleDescriptor.Count));
      Assert.That(typeInfo.Indexes["Book.FK_Author"].KeyColumns[0].Key.Name, Is.EqualTo("BookAuthor"));
      Assert.That(typeInfo.Indexes["Book.FK_Author"].KeyColumns[0].Value, Is.EqualTo(Direction.Positive));

      Assert.That(typeInfo.Indexes["Book.IX_Title"], Is.Not.Null);
      Assert.That(typeInfo.Indexes["Book.IX_Title"].IsPrimary, Is.False);
      Assert.That(typeInfo.Indexes["Book.IX_Title"].IsUnique, Is.False);
      Assert.That(typeInfo.Indexes["Book.IX_Title"].KeyColumns.Count, Is.EqualTo(1));
      Assert.That(typeInfo.Indexes["Book.IX_Title"].KeyColumns[0].Key.Name, Is.EqualTo("Title"));
      Assert.That(typeInfo.Indexes["Book.IX_Title"].KeyColumns[0].Value, Is.EqualTo(Direction.Positive));

      #endregion

      #region BookReview

      typeInfo = domain.Model.Types[typeof (BookReview)];
      Assert.That(typeInfo, Is.Not.Null);
      Assert.That(domain.Model.Types["BookReview"], Is.Not.Null);
      Assert.That(domain.Model.Types["BookReview"], Is.EqualTo(typeInfo));
      Assert.That(typeInfo.Name, Is.EqualTo("BookReview"));

      // Fields
      Assert.That(typeInfo.Fields["Book"], Is.Not.Null);
      Assert.That(typeInfo.Fields["Book"].Name, Is.EqualTo("Book"));
      Assert.That(typeInfo.Fields["Book"].Associations.Last().OnTargetRemove, Is.EqualTo(OnRemoveAction.Cascade));

      Assert.That(typeInfo.Fields["Reviewer"], Is.Not.Null);
      Assert.That(typeInfo.Fields["Reviewer"].Name, Is.EqualTo("Reviewer"));
      Assert.That(typeInfo.Fields["Reviewer"].Associations.Last().OnTargetRemove, Is.EqualTo(OnRemoveAction.Clear));

      Assert.That(typeInfo.Fields["Text"], Is.Not.Null);
      Assert.That(typeInfo.Fields["Text"].Name, Is.EqualTo("Text"));
      Assert.That(typeInfo.Fields["Text"].Length, Is.EqualTo(4096));

      // Indexes
      Assert.That(typeInfo.Indexes.Count, Is.EqualTo(6));
      Assert.That(typeInfo.Indexes["PK_BookReview"], Is.Not.Null);
      Assert.That(typeInfo.Indexes["PK_BookReview"].IsPrimary, Is.True);
      Assert.That(typeInfo.Indexes["PK_BookReview"].IsUnique, Is.True);
      Assert.That(typeInfo.Indexes["PK_BookReview"].KeyColumns.Count, Is.EqualTo(typeInfo.Hierarchy.Key.TupleDescriptor.Count));
      Assert.That(typeInfo.Indexes["PK_BookReview"].KeyColumns[0].Key.Name, Is.EqualTo("Book"));
      Assert.That(typeInfo.Indexes["PK_BookReview"].KeyColumns[0].Value, Is.EqualTo(Direction.Positive));
//      Assert.AreEqual("Reviewer.PassportNumber", typeInfo.Indexes["PK_BookReview"].KeyColumns[1].Key.Name);
//      Assert.AreEqual(Direction.Positive, typeInfo.Indexes["PK_BookReview"].KeyColumns[1].Value);

      #endregion
    }

    [Test]
    public void DuplicatedKeyTest()
    {
      using (var session = Domain.OpenSession()) {
        Book book1;
        using (session.OpenTransaction()) {
          book1 = new Book("0976470705");
          book1.Remove();
          Session.Current.SaveChanges();
          Book book2 = new Book("0976470705");          
          book2.Remove();

          var k = Key.Create<Book>(Domain, "0976470705").Format();
          var key = Key.Parse(Domain, k);
          Assert.That(Key.Create<Book>(Domain, "0976470705"), Is.EqualTo(key));

          Assert.That(session.Query.SingleOrDefault(Key.Create<Book>(Domain, "0976470705")), Is.Null);
          Assert.That(session.Query.SingleOrDefault(Key.Create<Book>(Domain, "0976470705")), Is.EqualTo(null));
        }
      }
    }

    [Test]
    public void ModelVerificationTest()
    {
//      Domain.Model.Dump();
      VerifyModel(Domain);
    }

    [Test]
    public void ComplexKeyTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          Book book = new Book("5-272-00040-4");
          book.Title = "Assembler";
          book.Author = new Author();
          book.Author.Passport.Card.LastName = "Jurov";
          Person reviewer = new Person();
          reviewer.Passport.Card.LastName = "Kochetov";
          reviewer.Passport.Card.FirstName = "Alexius";
          BookReview review = new BookReview(book, reviewer);
          Assert.That(review.Book, Is.EqualTo((object) book));
          Assert.That(review.Reviewer, Is.EqualTo((object) reviewer));
        }
      }
    }

    private static bool IsConcreteTable(TypeInfo typeInfo)
    {
      return typeInfo.Hierarchy.InheritanceSchema==InheritanceSchema.ConcreteTable;
    }

    private static bool IsSingleTable(TypeInfo typeInfo)
    {
      return typeInfo.Hierarchy.InheritanceSchema==InheritanceSchema.SingleTable;
    }
  }
}
