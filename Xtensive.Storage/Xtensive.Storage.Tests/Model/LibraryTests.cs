// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.04

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Generators;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Model.Library;
using FieldAttributes=Xtensive.Storage.Model.FieldAttributes;

namespace Xtensive.Storage.Tests.Model.Library
{
  public class IdentityCard : Structure
  {
    [Field(Length = 64)]
    public string FirstName { get; set; }

    [Field(Length = 64)]
    public string SecondName { get; set; }

    [Field(Length = 64)]
    public string LastName { get; set; }
  }

  public class Passport : Structure
  {
    [Field]
    public int Number { get; set; }

    [Field]
    public IdentityCard Card { get; set; }
  }

  [HierarchyRoot(typeof (IncrementalGenerator), "Number")]
  public class Person : Entity
  {
    [Field]
    public int Number
    {
      get { return GetValue<int>("Number"); }
      set
      {
        SetValue("Number", value);
        Passport.Number = value;
      }
    }

    [Field]
    public Passport Passport
    {
      get { return GetValue<Passport>("Passport"); }
      set
      {
        SetValue("Passport", value);
        if (Number!=value.Number)
          Number = value.Number;
      }
    }

    [Field]
    public EntitySet<BookReview> Reviews { get; private set; }
  }

  [Index("PenName:DESC", MappingName = "IX_PENNAME")]
  public class Author : Person
  {
    [Field(IsNullable = true, Length = 64)]
    public string PenName { get; set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }
  }

  [HierarchyRoot(typeof (IsbnKeyProvider), "Isbn")]
  [Index("Title:ASC")]
  public class Book : Entity
  {
    [Field(Length = 32)]
    public string Isbn { get; private set; }

    [Field(Length = 128)]
    public string Title { get; set; }

    [Field(OnDelete = ReferentialAction.Restrict)]
    public Author Author { get; set; }

    public int Rating { get; set; }

    public Book(string isbn)
      : base(Tuple.Create(isbn))
    {
    }

    /* [Field]
    public EntitySet<BookReview> Reviews
    {
      get { throw new NotImplementedException(); }
    }*/
  }

  [HierarchyRoot(typeof (BookReviewProvider), "Book", "Reviewer")]
  public class BookReview : Entity
  {
    [Field(MappingName = "Book", OnDelete = ReferentialAction.Cascade)]
    public Book Book { get; set; }

    [Field(OnDelete = ReferentialAction.SetNull)]
    public Person Reviewer { get; set; }

    [Field(Length = 4096)]
    public string Text { get; set; }

//    public BookReview(Key book, Key reviewer)
//      : base()
//    {
//    }
  }

  [KeyProvider(typeof (Book), typeof (Person))]
  internal class BookReviewProvider
  {
    public Key GetNext()
    {
      throw new NotSupportedException();
    }
  }

  [KeyProvider(typeof (string))]
  public class IsbnKeyProvider : GeneratorBase
  {
    private int counter;

    public override Tuple Next()
    {
      Tuple result = Tuple.Create(counter.ToString());
      counter++;
      return result;
    }

    public IsbnKeyProvider(HierarchyInfo hierarchy)
      : base(hierarchy)
    {
    }
  }

  public class LibraryDefinitionBuilder : IDomainBuilder
  {
    private static void VerifyTypeCollection()
    {
      BuildingContext context = BuildingScope.Context;
      TypeDefCollection types = context.Definition.Types;
      Assert.IsNull(types.FindAncestor(types[typeof (Entity)]));
      Assert.IsNull(types.FindAncestor(types[typeof (IEntity)]));
      Assert.IsNull(types.FindAncestor(types[typeof (Structure)]));
      Assert.AreEqual(types.FindAncestor(types[typeof (Passport)]), types[typeof (Structure)]);
      Assert.AreEqual(types.FindAncestor(types[typeof (IdentityCard)]), types[typeof (Structure)]);
      Assert.AreEqual(types.FindAncestor(types[typeof (Person)]), types[typeof (Entity)]);
      Assert.AreEqual(types.FindAncestor(types[typeof (Book)]), types[typeof (Entity)]);
      Assert.AreEqual(types.FindAncestor(types[typeof (BookReview)]), types[typeof (Entity)]);
      Assert.AreEqual(types.FindAncestor(types[typeof (Author)]), types[typeof (Person)]);
    }

    private static void RedefineTypes()
    {
      BuildingContext context = BuildingScope.Context;
      context.Definition.Types.Clear();
      context.Definition.DefineType(typeof (BookReview));
      context.Definition.DefineType(typeof (Book));
      context.Definition.DefineType(typeof (Person));
      context.Definition.DefineType(typeof (Author));
      context.Definition.DefineType(typeof (Structure));
      context.Definition.DefineType(typeof (Passport));
      context.Definition.DefineType(typeof (IdentityCard));
      context.Definition.DefineType(typeof (Entity));
      context.Definition.DefineType(typeof (IEntity));
    }

    private static void RedefineFields()
    {
      TypeDefCollection types = BuildingScope.Context.Definition.Types;
      foreach (TypeDef type in types) {
        type.Fields.Clear();
        type.Indexes.Clear();

        PropertyInfo[] properties =
          type.UnderlyingType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
            BindingFlags.DeclaredOnly);
        for (int index = 0, count = properties.Length; index < count; index++) {
          object[] attributes = properties[index].GetCustomAttributes(typeof (FieldAttribute), true);
          if (attributes==null || attributes.Length==0)
            continue;
          if (!properties[index].Name.Contains("."))
            type.DefineField(properties[index]);
        }
      }

      types["Book"].Fields["Author"].OnDelete = ReferentialAction.Restrict;
      types["BookReview"].Fields["Book"].OnDelete = ReferentialAction.Cascade;
      types["BookReview"].Fields["Reviewer"].OnDelete = ReferentialAction.SetNull;


      IndexDef indexDef;

      indexDef = types["Author"].DefineIndex("IX_PENNAME");
      indexDef.MappingName = "IX_PENNAME";
      indexDef.KeyFields.Add("PenName", Direction.Negative);

      indexDef = types["Book"].DefineIndex("IX_Title");
      indexDef.KeyFields.Add("Title");
    }

    private static void RedefineIndexes()
    {
    }

    private static void VerifyDefinition()
    {
      BuildingContext context = BuildingScope.Context;
      Assert.IsNotNull(context.Definition.Types[typeof (Entity)]);
      Assert.IsNotNull(context.Definition.Types[typeof (IEntity)]);

      #region IdentityCard

      TypeDef typeDef = context.Definition.Types[typeof (IdentityCard)];
      Assert.IsNotNull(typeDef);
      Assert.IsNotNull(typeDef.Fields["FirstName"]);
      Assert.AreEqual("FirstName", typeDef.Fields["FirstName"].Name);
      Assert.AreEqual(64, typeDef.Fields["FirstName"].Length);
      Assert.IsNotNull(typeDef.Fields["SecondName"]);
      Assert.AreEqual("SecondName", typeDef.Fields["SecondName"].Name);
      Assert.AreEqual(64, typeDef.Fields["SecondName"].Length);
      Assert.IsNotNull(typeDef.Fields["LastName"]);
      Assert.AreEqual("LastName", typeDef.Fields["LastName"].Name);
      Assert.AreEqual(64, typeDef.Fields["LastName"].Length);

      #endregion

      #region Passport

      typeDef = context.Definition.Types[typeof (Passport)];
      Assert.IsNotNull(typeDef.Fields["Number"]);
      Assert.AreEqual("Number", typeDef.Fields["Number"].Name);
      Assert.IsNotNull(typeDef.Fields["Card"]);
      Assert.AreEqual("Card", typeDef.Fields["Card"].Name);

      #endregion

      #region Person

      typeDef = context.Definition.Types[typeof (Person)];
      Assert.IsNotNull(typeDef);
      Assert.IsNotNull(context.Definition.Types["Person"]);
      Assert.AreEqual(typeDef, context.Definition.Types["Person"]);
      Assert.AreEqual("Person", typeDef.Name);

      // Fields
      Assert.IsNotNull(typeDef.Fields["Passport"]);
      Assert.AreEqual("Passport", typeDef.Fields["Passport"].Name);
      Assert.IsTrue(typeDef.Fields["Passport"].IsStructure);
      Assert.IsFalse(typeDef.Fields["Passport"].IsEntity);
      Assert.IsFalse(typeDef.Fields["Passport"].IsEntitySet);

      #endregion

      #region Author

      typeDef = context.Definition.Types[typeof (Author)];
      Assert.IsNotNull(typeDef);
      Assert.IsNotNull(context.Definition.Types["Author"]);
      Assert.AreEqual(typeDef, context.Definition.Types["Author"]);
      Assert.AreEqual("Author", typeDef.Name);

      // Fields
      Assert.IsNotNull(typeDef.Fields["PenName"]);
      Assert.AreEqual("PenName", typeDef.Fields["PenName"].Name);

      // Indexes
      Assert.IsNotNull(typeDef.Indexes["IX_PENNAME"]);
      Assert.IsNotNull(typeDef.Indexes[0]);
      Assert.AreEqual(typeDef.Indexes["IX_PENNAME"], typeDef.Indexes[0]);
      Assert.IsFalse(typeDef.Indexes["IX_PENNAME"].IsPrimary);
      Assert.IsFalse(typeDef.Indexes["IX_PENNAME"].IsUnique);
      Assert.AreEqual(1, typeDef.Indexes[0].KeyFields.Count);
      Assert.IsNotNull(typeDef.Indexes[0].KeyFields[0]);
      Assert.AreEqual("PenName", typeDef.Indexes[0].KeyFields[0].Key);
      Assert.AreEqual(Direction.Negative, typeDef.Indexes[0].KeyFields[0].Value);

      #endregion

      #region Book

      typeDef = context.Definition.Types[typeof (Book)];
      Assert.IsNotNull(typeDef);
      Assert.IsNotNull(context.Definition.Types["Book"]);
      Assert.AreEqual(typeDef, context.Definition.Types["Book"]);
      Assert.AreEqual("Book", typeDef.Name);

      // Fields
      Assert.IsNotNull(typeDef.Fields["Isbn"]);
      Assert.AreEqual("Isbn", typeDef.Fields["Isbn"].Name);
      Assert.AreEqual(32, typeDef.Fields["Isbn"].Length);

      Assert.IsNotNull(typeDef.Fields["Title"]);
      Assert.AreEqual("Title", typeDef.Fields["Title"].Name);
      Assert.AreEqual(128, typeDef.Fields["Title"].Length);

      Assert.IsNotNull(typeDef.Fields["Author"]);
      Assert.AreEqual(ReferentialAction.Restrict, typeDef.Fields["Author"].OnDelete);
      Assert.AreEqual("Author", typeDef.Fields["Author"].Name);

      Assert.IsNotNull(typeDef.Indexes["IX_Title"]);
      Assert.IsFalse(typeDef.Indexes["IX_Title"].IsPrimary);
      Assert.IsFalse(typeDef.Indexes["IX_Title"].IsUnique);
      Assert.AreEqual(1, typeDef.Indexes["IX_Title"].KeyFields.Count);
      Assert.AreEqual("Title", typeDef.Indexes["IX_Title"].KeyFields[0].Key);
      Assert.AreEqual(Direction.Positive, typeDef.Indexes["IX_Title"].KeyFields[0].Value);

      #endregion

      #region BookReview

      typeDef = context.Definition.Types[typeof (BookReview)];
      Assert.IsNotNull(typeDef);
      Assert.IsNotNull(context.Definition.Types["BookReview"]);
      Assert.AreEqual(typeDef, context.Definition.Types["BookReview"]);
      Assert.AreEqual("BookReview", typeDef.Name);

      // Fields
      Assert.IsNotNull(typeDef.Fields["Book"]);
      Assert.AreEqual("Book", typeDef.Fields["Book"].Name);
      Assert.AreEqual(ReferentialAction.Cascade, typeDef.Fields["Book"].OnDelete);

      Assert.IsNotNull(typeDef.Fields["Reviewer"]);
      Assert.AreEqual("Reviewer", typeDef.Fields["Reviewer"].Name);
      Assert.AreEqual(ReferentialAction.SetNull, typeDef.Fields["Reviewer"].OnDelete);

      Assert.IsNotNull(typeDef.Fields["Text"]);
      Assert.AreEqual("Text", typeDef.Fields["Text"].Name);
      Assert.AreEqual(4096, typeDef.Fields["Text"].Length);

      #endregion

      Console.WriteLine("-- Model verification is completed --");
    }

    #region IDomainBuilder Members

    public void Build(BuildingContext context, DomainDef domain)
    {
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

    #endregion
  }
}

namespace Xtensive.Storage.Tests.Model
{
  [TestFixture]
  public class LibraryTests
  {
    private static DomainConfiguration CreateStorageConfiguration()
    {
      DomainConfiguration configuration =
        new DomainConfiguration(@"memory://localhost\sql2005/Library");
      configuration.Types.Register(typeof (Person).Assembly, "Xtensive.Storage.Tests.Model.Library");
      return configuration;
    }

    private static void VerifyModel(Domain domain)
    {
      TypeInfoCollection types = domain.Model.Types;
      Assert.AreEqual(types.FindAncestor(types[typeof (Person)]), null);
      Assert.AreEqual(types.FindAncestor(types[typeof (Book)]), null);
      Assert.AreEqual(types.FindAncestor(types[typeof (BookReview)]), null);
      Assert.AreEqual(types.FindAncestor(types[typeof (Author)]), types[typeof (Person)]);

      Assert.AreEqual(types[typeof (Person)].GetAncestor(), null);
      Assert.AreEqual(types[typeof (Book)].GetAncestor(), null);
      Assert.AreEqual(types[typeof (BookReview)].GetAncestor(), null);
      Assert.AreEqual(types[typeof (Author)].GetAncestor(), types[typeof (Person)]);

      ICountable<TypeInfo> collection = types.Structures;
      Assert.IsTrue(collection.Count > 0);
      foreach (TypeInfo item in collection) {
        Assert.IsTrue(item.IsStructure);
        Assert.IsFalse(item.IsInterface);
        Assert.IsFalse(item.IsEntity);
      }

      collection = types.Interfaces;
      Assert.IsFalse(collection.Count > 0);
      foreach (TypeInfo item in collection) {
        Assert.IsTrue(item.IsInterface);
        Assert.IsFalse(item.IsStructure);
        Assert.IsFalse(item.IsEntity);
      }

      collection = types.Entities;
      Assert.IsTrue(collection.Count > 0);
      foreach (TypeInfo item in collection) {
        Assert.IsTrue(item.IsEntity);
        Assert.IsFalse(item.IsInterface);
        Assert.IsFalse(item.IsStructure);
      }

      #region IdentityCard

      TypeInfo typeInfo = domain.Model.Types[typeof (IdentityCard)];

      // Fields
      Assert.IsNotNull(typeInfo.Fields["FirstName"]);
      Assert.AreEqual(typeInfo.Fields["FirstName"].Name, "FirstName");
      Assert.AreEqual(typeInfo.Fields["FirstName"].Length, 64);
      Assert.IsTrue(typeInfo.Fields["FirstName"].IsDeclared);
      Assert.IsFalse(typeInfo.Fields["FirstName"].IsInherited);
      Assert.IsNotNull(typeInfo.Fields["SecondName"]);
      Assert.AreEqual(typeInfo.Fields["SecondName"].Name, "SecondName");
      Assert.AreEqual(typeInfo.Fields["SecondName"].Length, 64);
      Assert.IsTrue(typeInfo.Fields["SecondName"].IsDeclared);
      Assert.IsFalse(typeInfo.Fields["SecondName"].IsInherited);
      Assert.IsNotNull(typeInfo.Fields["LastName"]);
      Assert.AreEqual(typeInfo.Fields["LastName"].Name, "LastName");
      Assert.AreEqual(typeInfo.Fields["LastName"].Length, 64);
      Assert.IsTrue(typeInfo.Fields["LastName"].IsDeclared);
      Assert.IsFalse(typeInfo.Fields["LastName"].IsInherited);

      #endregion

      #region Passport

      typeInfo = domain.Model.Types[typeof (Passport)];

      // Fields
      Assert.IsNotNull(typeInfo.Fields["Number"]);
      Assert.AreEqual(typeInfo.Fields["Number"].Name, "Number");
      Assert.IsTrue(typeInfo.Fields["Number"].IsDeclared);
      Assert.IsFalse(typeInfo.Fields["Number"].IsInherited);
      Assert.IsFalse(typeInfo.Fields["Number"].IsStructure);
      Assert.IsFalse(typeInfo.Fields["Number"].IsEntity);
      Assert.IsFalse(typeInfo.Fields["Number"].IsEntitySet);

      Assert.IsNotNull(typeInfo.Fields["Card"]);
      Assert.AreEqual(typeInfo.Fields["Card"].Name, "Card");
      Assert.IsTrue(typeInfo.Fields["Card"].IsStructure);
      Assert.IsFalse(typeInfo.Fields["Card"].IsEntity);
      Assert.IsFalse(typeInfo.Fields["Card"].IsEntitySet);
      Assert.AreEqual(typeInfo.Fields["Card"].Fields.Count, 3);
      Assert.IsTrue(typeInfo.Fields["Card"].IsDeclared);
      Assert.IsFalse(typeInfo.Fields["Card"].IsInherited);

      Assert.IsNotNull(typeInfo.Columns["Card.FirstName"]);
      Assert.AreEqual(typeInfo.Columns["Card.FirstName"].Name, "Card.FirstName");
      Assert.AreEqual(typeInfo.Columns["Card.FirstName"].Length, 64);
      Assert.IsNotNull(typeInfo.Columns["Card.SecondName"]);
      Assert.AreEqual(typeInfo.Columns["Card.SecondName"].Name, "Card.SecondName");
      Assert.AreEqual(typeInfo.Columns["Card.SecondName"].Length, 64);
      Assert.IsNotNull(typeInfo.Columns["Card.LastName"]);
      Assert.AreEqual(typeInfo.Columns["Card.LastName"].Name, "Card.LastName");
      Assert.AreEqual(typeInfo.Columns["Card.LastName"].Length, 64);

      #endregion

      #region Person

      typeInfo = domain.Model.Types[typeof (Person)];
      Assert.IsNotNull(typeInfo);
      Assert.IsNotNull(domain.Model.Types["Person"]);
      Assert.AreEqual(typeInfo, domain.Model.Types["Person"]);
      Assert.AreEqual(typeInfo.Name, "Person");

      // Fields
      Assert.IsNotNull(typeInfo.Fields["Passport"]);
      Assert.AreEqual(typeInfo.Fields["Passport"].Name, "Passport");
      Assert.IsTrue(typeInfo.Fields["Passport"].IsStructure);
      Assert.IsFalse(typeInfo.Fields["Passport"].IsEntity);
      Assert.IsFalse(typeInfo.Fields["Passport"].IsEntitySet);
      //      Assert.AreEqual(typeInfo.Fields["Passport"].Columns.Count, 4);

      Assert.IsNotNull(typeInfo.Fields["Reviews"]);
      Assert.AreEqual(typeInfo.Fields["Reviews"].Name, "Reviews");
      Assert.IsFalse(typeInfo.Fields["Reviews"].IsStructure);
      Assert.IsFalse(typeInfo.Fields["Reviews"].IsEntity);
      Assert.IsTrue(typeInfo.Fields["Reviews"].IsEntitySet);

      // KeyColumns
      Assert.IsNotNull(typeInfo.Columns["Passport.Number"]);
      Assert.AreEqual(typeInfo.Columns["Passport.Number"].Name, "Passport.Number");
      //      Assert.AreEqual(person.Fields["Passport"].Indexes[0], person.KeyInfo);
      Assert.IsNotNull(typeInfo.Columns["Passport.Card.FirstName"]);
      Assert.AreEqual(typeInfo.Columns["Passport.Card.FirstName"].Name, "Passport.Card.FirstName");
      Assert.AreEqual(typeInfo.Columns["Passport.Card.FirstName"].Length, 64);
      Assert.IsNotNull(typeInfo.Columns["Passport.Card.SecondName"]);
      Assert.AreEqual(typeInfo.Columns["Passport.Card.SecondName"].Name, "Passport.Card.SecondName");
      Assert.AreEqual(typeInfo.Columns["Passport.Card.SecondName"].Length, 64);
      Assert.IsNotNull(typeInfo.Columns["Passport.Card.LastName"]);
      Assert.AreEqual(typeInfo.Columns["Passport.Card.LastName"].Name, "Passport.Card.LastName");
      Assert.AreEqual(typeInfo.Columns["Passport.Card.LastName"].Length, 64);

      // Indexes
      Assert.AreEqual(typeInfo.Indexes.Count, 2);
      Assert.IsNotNull(typeInfo.Indexes["PK_Person"]);
      Assert.IsTrue(typeInfo.Indexes["PK_Person"].IsPrimary);
      Assert.IsTrue(typeInfo.Indexes["PK_Person"].IsUnique);
      Assert.AreEqual(1, typeInfo.Indexes["PK_Person"].KeyColumns.Count);
      Assert.AreEqual(typeInfo.Columns["Number"], typeInfo.Indexes["PK_Person"].KeyColumns[0].Key);

      #endregion

      #region Author

      typeInfo = domain.Model.Types[typeof (Author)];
      Assert.IsNotNull(typeInfo);
      Assert.IsNotNull(domain.Model.Types["Author"]);
      Assert.AreEqual(typeInfo, domain.Model.Types["Author"]);
      Assert.AreEqual(typeInfo.Name, "Author");

      // Fields
      Assert.IsNotNull(typeInfo.Fields["Passport"]);
      Assert.AreEqual(typeInfo.Fields["Passport"].Name, "Passport");
      Assert.IsTrue(typeInfo.Fields["Passport"].IsStructure);
      Assert.IsTrue(typeInfo.Fields["Passport"].IsInherited);
      Assert.AreEqual(typeInfo.Fields["Passport"].Fields.Count, 4);
      Assert.IsNotNull(typeInfo.Fields["PenName"]);
      Assert.AreEqual(typeInfo.Fields["PenName"].IsNullable, true);
      Assert.AreEqual(typeInfo.Fields["PenName"].Name, "PenName");
      Assert.AreEqual(typeInfo.Fields["PenName"].Length, 64);
      Assert.IsNotNull(typeInfo.Fields["Books"]);
      Assert.AreEqual(typeInfo.Fields["Books"].IsNullable, false);
      Assert.AreEqual(typeInfo.Fields["Books"].Name, "Books");

      Assert.AreEqual(2, typeInfo.Fields.Find(FieldAttributes.Declared).Count);
      Assert.AreEqual(8, typeInfo.Fields.Find(FieldAttributes.Inherited).Count);

      // KeyColumns
      Assert.IsNotNull(typeInfo.Columns["Number"]);
      Assert.AreEqual(typeInfo.Columns["Number"].Name, "Number");
      //      Assert.AreEqual(person.Fields["Passport"].Indexes[0], person.KeyInfo);
      Assert.IsNotNull(typeInfo.Columns["Passport.Card.FirstName"]);
      Assert.AreEqual(typeInfo.Columns["Passport.Card.FirstName"].Name, "Passport.Card.FirstName");
      Assert.AreEqual(typeInfo.Columns["Passport.Card.FirstName"].Length, 64);
      Assert.IsNotNull(typeInfo.Columns["Passport.Card.SecondName"]);
      Assert.AreEqual(typeInfo.Columns["Passport.Card.SecondName"].Name, "Passport.Card.SecondName");
      Assert.AreEqual(typeInfo.Columns["Passport.Card.SecondName"].Length, 64);
      Assert.IsNotNull(typeInfo.Columns["Passport.Card.LastName"]);
      Assert.AreEqual(typeInfo.Columns["Passport.Card.LastName"].Name, "Passport.Card.LastName");
      Assert.AreEqual(typeInfo.Columns["Passport.Card.LastName"].Length, 64);
      Assert.IsNotNull(typeInfo.Columns["PenName"]);
      Assert.AreEqual(typeInfo.Columns["PenName"].IsNullable, true);
      Assert.AreEqual(typeInfo.Columns["PenName"].Name, "PenName");
      Assert.AreEqual(typeInfo.Columns["PenName"].Length, 64);
      Assert.IsFalse(typeInfo.Columns.Contains("Reviews"));

      // Indexes
      Assert.AreEqual(3, typeInfo.Indexes.Count);
      Assert.IsNotNull(typeInfo.Indexes["Author.IX_PENNAME"]);
      Assert.IsNotNull(typeInfo.Indexes[0]);
      Assert.AreEqual(typeInfo.Indexes[0], typeInfo.Indexes["Author.IX_PENNAME"]);
      Assert.AreEqual(1, typeInfo.Indexes[0].KeyColumns.Count);
      Assert.IsNotNull(typeInfo.Indexes[0].KeyColumns[0]);
      Assert.AreEqual(typeInfo.Columns["PenName"], typeInfo.Indexes[0].KeyColumns[0].Key);

      #endregion

      #region Book

      typeInfo = domain.Model.Types[typeof (Book)];
      Assert.IsNotNull(typeInfo);
      Assert.IsNotNull(domain.Model.Types["Book"]);
      Assert.AreEqual(typeInfo, domain.Model.Types["Book"]);
      Assert.AreEqual(typeInfo.Name, "Book");

      // Fields
      Assert.IsNotNull(typeInfo.Fields["Isbn"]);
      Assert.AreEqual(typeInfo.Fields["Isbn"].Name, "Isbn");
      Assert.AreEqual(typeInfo.Fields["Isbn"].Length, 32);
      //      Assert.AreEqual(book.Fields["Isbn"].Indexes[0], book.KeyInfo);

      Assert.IsNotNull(typeInfo.Fields["Title"]);
      Assert.AreEqual(typeInfo.Fields["Title"].Name, "Title");
      Assert.AreEqual(typeInfo.Fields["Title"].Length, 128);

      Assert.IsNotNull(typeInfo.Fields["Author"]);
      Assert.AreEqual(typeInfo.Fields["Author"].Name, "Author");
      Assert.IsFalse(typeInfo.Fields["Author"].IsStructure);
      Assert.IsTrue(typeInfo.Fields["Author"].IsEntity);
      Assert.IsFalse(typeInfo.Fields["Author"].IsEntitySet);
      Assert.AreEqual(ReferentialAction.Restrict, typeInfo.Fields["Author"].Association.OnDelete);

      // Indexes
      Assert.AreEqual(3, typeInfo.Indexes.Count);
      Assert.IsNotNull(typeInfo.Indexes["PK_Book"]);
      Assert.IsTrue(typeInfo.Indexes["PK_Book"].IsPrimary);
      Assert.IsTrue(typeInfo.Indexes["PK_Book"].IsUnique);
      Assert.AreEqual(typeInfo.Indexes["PK_Book"].KeyColumns.Count, 1);
      Assert.AreEqual(typeInfo.Indexes["PK_Book"].KeyColumns[0].Key.Name, "Isbn");
      Assert.AreEqual(typeInfo.Indexes["PK_Book"].KeyColumns[0].Value, Direction.Positive);

      Assert.IsNotNull(typeInfo.Indexes["Book.FK_Author"]);
      Assert.IsFalse(typeInfo.Indexes["Book.FK_Author"].IsPrimary);
      Assert.IsFalse(typeInfo.Indexes["Book.FK_Author"].IsUnique);
      Assert.AreEqual(typeInfo.Indexes["Book.FK_Author"].KeyColumns.Count, 1);
      Assert.AreEqual(typeInfo.Indexes["Book.FK_Author"].KeyColumns[0].Key.Name, "Author.Number");
      Assert.AreEqual(typeInfo.Indexes["Book.FK_Author"].KeyColumns[0].Value, Direction.Positive);

      Assert.IsNotNull(typeInfo.Indexes["Book.IX_Title"]);
      Assert.IsFalse(typeInfo.Indexes["Book.IX_Title"].IsPrimary);
      Assert.IsFalse(typeInfo.Indexes["Book.IX_Title"].IsUnique);
      Assert.AreEqual(typeInfo.Indexes["Book.IX_Title"].KeyColumns.Count, 1);
      Assert.AreEqual(typeInfo.Indexes["Book.IX_Title"].KeyColumns[0].Key.Name, "Title");
      Assert.AreEqual(typeInfo.Indexes["Book.IX_Title"].KeyColumns[0].Value, Direction.Positive);

      #endregion

      #region BookReview

      typeInfo = domain.Model.Types[typeof (BookReview)];
      Assert.IsNotNull(typeInfo);
      Assert.IsNotNull(domain.Model.Types["BookReview"]);
      Assert.AreEqual(typeInfo, domain.Model.Types["BookReview"]);
      Assert.AreEqual(typeInfo.Name, "BookReview");

      // Fields
      Assert.IsNotNull(typeInfo.Fields["Book"]);
      Assert.AreEqual(typeInfo.Fields["Book"].Name, "Book");
      Assert.AreEqual(ReferentialAction.Cascade, typeInfo.Fields["Book"].Association.OnDelete);

      Assert.IsNotNull(typeInfo.Fields["Reviewer"]);
      Assert.AreEqual(typeInfo.Fields["Reviewer"].Name, "Reviewer");
      Assert.AreEqual(ReferentialAction.SetNull, typeInfo.Fields["Reviewer"].Association.OnDelete);

      Assert.IsNotNull(typeInfo.Fields["Text"]);
      Assert.AreEqual(typeInfo.Fields["Text"].Name, "Text");
      Assert.AreEqual(typeInfo.Fields["Text"].Length, 4096);

      // Indexes
      Assert.AreEqual(3, typeInfo.Indexes.Count);
      Assert.IsNotNull(typeInfo.Indexes["PK_BookReview"]);
      Assert.IsTrue(typeInfo.Indexes["PK_BookReview"].IsPrimary);
      Assert.IsTrue(typeInfo.Indexes["PK_BookReview"].IsUnique);
      Assert.AreEqual(typeInfo.Indexes["PK_BookReview"].KeyColumns.Count, 2);
      Assert.AreEqual(typeInfo.Indexes["PK_BookReview"].KeyColumns[0].Key.Name, "Book.Isbn");
      Assert.AreEqual(typeInfo.Indexes["PK_BookReview"].KeyColumns[0].Value, Direction.Positive);
      Assert.AreEqual(typeInfo.Indexes["PK_BookReview"].KeyColumns[1].Key.Name, "Reviewer.Number");
      Assert.AreEqual(typeInfo.Indexes["PK_BookReview"].KeyColumns[1].Value, Direction.Positive);

      #endregion
    }

    [Test]
    public void ModelVerificationTest()
    {
      DomainConfiguration configuration = CreateStorageConfiguration();
      configuration.Builders.Add(typeof (LibraryDefinitionBuilder));
      Domain domain = Domain.Build(configuration);
      domain.Model.Dump();
      VerifyModel(domain);
    }

    [Test]
    public void ComplexKeyTest()
    {
      Domain domain = Domain.Build(CreateStorageConfiguration());
      domain.Model.Dump();
      using (domain.OpenSession()) {
        Book book = new Book("5-272-00040-4");
        book.Title = "Assembler";
        book.Author = new Author();
        book.Author.Passport.Card.LastName = "Jurov";
        Person reviewer = new Person();
        reviewer.Passport.Card.LastName = "Kochetov";
        reviewer.Passport.Card.FirstName = "Alexius";
//        BookReview review = new BookReview(book, reviewer);
//        Assert.AreEqual((object) book, review.Book);
//        Assert.AreEqual((object) reviewer, review.Reviewer);
      }
    }
  }
}