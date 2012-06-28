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

  public class IsbnKeyGenerator : IKeyGenerator
  {
    private int counter;

    public void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
    {
    }

    public Tuple GenerateKey(KeyInfo keyInfo, Session session)
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

    private void RedefineTypes()
    {
      context.ModelDef.Types.Clear();
      context.ModelDef.DefineType(typeof (BookReview));
      context.ModelDef.DefineType(typeof (Book));
      context.ModelDef.DefineType(typeof (Person));
      context.ModelDef.DefineType(typeof (Author));
      context.ModelDef.DefineType(typeof (Structure));
      context.ModelDef.DefineType(typeof (Passport));
      context.ModelDef.DefineType(typeof (IdentityCard));
      context.ModelDef.DefineType(typeof (Entity));
      context.ModelDef.DefineType(typeof (IEntity));
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
            type.DefineField(properties[index]);
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
      Assert.IsNotNull(context.ModelDef.Types[typeof (Entity)]);
      Assert.IsNotNull(context.ModelDef.Types[typeof (IEntity)]);

      #region IdentityCard

      TypeDef typeDef = context.ModelDef.Types[typeof (IdentityCard)];
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

      typeDef = context.ModelDef.Types[typeof (Passport)];
      Assert.IsNotNull(typeDef.Fields["Number"]);
      Assert.AreEqual("Number", typeDef.Fields["Number"].Name);
      Assert.IsNotNull(typeDef.Fields["Card"]);
      Assert.AreEqual("Card", typeDef.Fields["Card"].Name);

      #endregion

      #region Person

      typeDef = context.ModelDef.Types[typeof (Person)];
      Assert.IsNotNull(typeDef);
      Assert.IsNotNull(context.ModelDef.Types["Person"]);
      Assert.AreEqual(typeDef, context.ModelDef.Types["Person"]);
      Assert.AreEqual("Person", typeDef.Name);

      // Fields
      Assert.IsNotNull(typeDef.Fields["Passport"]);
      Assert.AreEqual("Passport", typeDef.Fields["Passport"].Name);
      Assert.IsTrue(typeDef.Fields["Passport"].IsStructure);
      Assert.IsFalse(typeDef.Fields["Passport"].IsEntity);
      Assert.IsFalse(typeDef.Fields["Passport"].IsEntitySet);

      #endregion

      #region Author

      typeDef = context.ModelDef.Types[typeof (Author)];
      Assert.IsNotNull(typeDef);
      Assert.IsNotNull(context.ModelDef.Types["Author"]);
      Assert.AreEqual(typeDef, context.ModelDef.Types["Author"]);
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

      typeDef = context.ModelDef.Types[typeof (Book)];
      Assert.IsNotNull(typeDef);
      Assert.IsNotNull(context.ModelDef.Types["Book"]);
      Assert.AreEqual(typeDef, context.ModelDef.Types["Book"]);
      Assert.AreEqual("Book", typeDef.Name);

      // Fields
      Assert.IsNotNull(typeDef.Fields["Isbn"]);
      Assert.AreEqual("Isbn", typeDef.Fields["Isbn"].Name);
      Assert.AreEqual(32, typeDef.Fields["Isbn"].Length);

      Assert.IsNotNull(typeDef.Fields["Title"]);
      Assert.AreEqual("Title", typeDef.Fields["Title"].Name);
      Assert.AreEqual(128, typeDef.Fields["Title"].Length);

      Assert.IsNotNull(typeDef.Fields["Author"]);
      Assert.AreEqual(OnRemoveAction.Deny, typeDef.Fields["Author"].OnTargetRemove);
      Assert.AreEqual("Author", typeDef.Fields["Author"].Name);

      Assert.IsNotNull(typeDef.Indexes["IX_Title"]);
      Assert.IsFalse(typeDef.Indexes["IX_Title"].IsPrimary);
      Assert.IsFalse(typeDef.Indexes["IX_Title"].IsUnique);
      Assert.AreEqual(1, typeDef.Indexes["IX_Title"].KeyFields.Count);
      Assert.AreEqual("Title", typeDef.Indexes["IX_Title"].KeyFields[0].Key);
      Assert.AreEqual(Direction.Positive, typeDef.Indexes["IX_Title"].KeyFields[0].Value);

      #endregion

      #region BookReview

      typeDef = context.ModelDef.Types[typeof (BookReview)];
      Assert.IsNotNull(typeDef);
      Assert.IsNotNull(context.ModelDef.Types["BookReview"]);
      Assert.AreEqual(typeDef, context.ModelDef.Types["BookReview"]);
      Assert.AreEqual("BookReview", typeDef.Name);

      // Fields
      Assert.IsNotNull(typeDef.Fields["Book"]);
      Assert.AreEqual("Book", typeDef.Fields["Book"].Name);
      Assert.AreEqual(OnRemoveAction.Cascade, typeDef.Fields["Book"].OnTargetRemove);

      Assert.IsNotNull(typeDef.Fields["Reviewer"]);
      Assert.AreEqual("Reviewer", typeDef.Fields["Reviewer"].Name);
      Assert.AreEqual(OnRemoveAction.Clear, typeDef.Fields["Reviewer"].OnTargetRemove);

      Assert.IsNotNull(typeDef.Fields["Text"]);
      Assert.AreEqual("Text", typeDef.Fields["Text"].Name);
      Assert.AreEqual(4096, typeDef.Fields["Text"].Length);

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
      config.Types.Register(typeof (Person).Assembly, "Xtensive.Orm.Tests.Model.LibraryModel");
      // config.Builders.Add(typeof (LibraryDomainBuilder));
      return config;
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

      ICollection<TypeInfo> collection = types.Structures;
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
      Assert.AreEqual("FirstName", typeInfo.Fields["FirstName"].Name);
      Assert.AreEqual(64, typeInfo.Fields["FirstName"].Length);
      Assert.IsTrue(typeInfo.Fields["FirstName"].IsDeclared);
      Assert.IsFalse(typeInfo.Fields["FirstName"].IsInherited);
      Assert.IsNotNull(typeInfo.Fields["SecondName"]);
      Assert.AreEqual("SecondName", typeInfo.Fields["SecondName"].Name);
      Assert.AreEqual(64, typeInfo.Fields["SecondName"].Length);
      Assert.IsTrue(typeInfo.Fields["SecondName"].IsDeclared);
      Assert.IsFalse(typeInfo.Fields["SecondName"].IsInherited);
      Assert.IsNotNull(typeInfo.Fields["LastName"]);
      Assert.AreEqual("LastName", typeInfo.Fields["LastName"].Name);
      Assert.AreEqual(64, typeInfo.Fields["LastName"].Length);
      Assert.IsTrue(typeInfo.Fields["LastName"].IsDeclared);
      Assert.IsFalse(typeInfo.Fields["LastName"].IsInherited);

      #endregion

      #region Passport

      typeInfo = domain.Model.Types[typeof (Passport)];

      // Fields
      Assert.IsNotNull(typeInfo.Fields["Number"]);
      Assert.AreEqual("Number", typeInfo.Fields["Number"].Name);
      Assert.IsTrue(typeInfo.Fields["Number"].IsDeclared);
      Assert.IsFalse(typeInfo.Fields["Number"].IsInherited);
      Assert.IsFalse(typeInfo.Fields["Number"].IsStructure);
      Assert.IsFalse(typeInfo.Fields["Number"].IsEntity);
      Assert.IsFalse(typeInfo.Fields["Number"].IsEntitySet);

      Assert.IsNotNull(typeInfo.Fields["Card"]);
      Assert.AreEqual("Card", typeInfo.Fields["Card"].Name);
      Assert.IsTrue(typeInfo.Fields["Card"].IsStructure);
      Assert.IsFalse(typeInfo.Fields["Card"].IsEntity);
      Assert.IsFalse(typeInfo.Fields["Card"].IsEntitySet);
      Assert.AreEqual(3, typeInfo.Fields["Card"].Fields.Count);
      Assert.IsTrue(typeInfo.Fields["Card"].IsDeclared);
      Assert.IsFalse(typeInfo.Fields["Card"].IsInherited);

      Assert.IsNotNull(typeInfo.Columns["Card.FirstName"]);
      Assert.AreEqual("Card.FirstName", typeInfo.Columns["Card.FirstName"].Name);
      Assert.AreEqual(64, typeInfo.Columns["Card.FirstName"].Length);
      Assert.IsNotNull(typeInfo.Columns["Card.SecondName"]);
      Assert.AreEqual("Card.SecondName", typeInfo.Columns["Card.SecondName"].Name);
      Assert.AreEqual(64, typeInfo.Columns["Card.SecondName"].Length);
      Assert.IsNotNull(typeInfo.Columns["Card.LastName"]);
      Assert.AreEqual("Card.LastName", typeInfo.Columns["Card.LastName"].Name);
      Assert.AreEqual(64, typeInfo.Columns["Card.LastName"].Length);

      #endregion

      #region Person

      typeInfo = domain.Model.Types[typeof (Person)];
      Assert.IsNotNull(typeInfo);
      Assert.IsNotNull(domain.Model.Types["Person"]);
      Assert.AreEqual(typeInfo, domain.Model.Types["Person"]);
      Assert.AreEqual("Person", typeInfo.Name);

      // Fields
      Assert.IsNotNull(typeInfo.Fields["Passport"]);
      Assert.AreEqual("Passport", typeInfo.Fields["Passport"].Name);
      Assert.IsTrue(typeInfo.Fields["Passport"].IsStructure);
      Assert.IsFalse(typeInfo.Fields["Passport"].IsEntity);
      Assert.IsFalse(typeInfo.Fields["Passport"].IsEntitySet);
      //      Assert.AreEqual(typeInfo.Fields["Passport"].Columns.Count, 4);

      Assert.IsNotNull(typeInfo.Fields["Reviews"]);
      Assert.AreEqual("Reviews", typeInfo.Fields["Reviews"].Name);
      Assert.IsFalse(typeInfo.Fields["Reviews"].IsStructure);
      Assert.IsFalse(typeInfo.Fields["Reviews"].IsEntity);
      Assert.IsTrue(typeInfo.Fields["Reviews"].IsEntitySet);

      // KeyColumns
      Assert.IsNotNull(typeInfo.Columns["Passport.Number"]);
      Assert.AreEqual("Passport.Number", typeInfo.Columns["Passport.Number"].Name);
      //      Assert.AreEqual(person.Fields["Passport"].Indexes[0], person.Key);
      Assert.IsNotNull(typeInfo.Columns["Passport.Card.FirstName"]);
      Assert.AreEqual("Passport.Card.FirstName", typeInfo.Columns["Passport.Card.FirstName"].Name);
      Assert.AreEqual(64, typeInfo.Columns["Passport.Card.FirstName"].Length);
      Assert.IsNotNull(typeInfo.Columns["Passport.Card.SecondName"]);
      Assert.AreEqual("Passport.Card.SecondName", typeInfo.Columns["Passport.Card.SecondName"].Name);
      Assert.AreEqual(64, typeInfo.Columns["Passport.Card.SecondName"].Length);
      Assert.IsNotNull(typeInfo.Columns["Passport.Card.LastName"]);
      Assert.AreEqual("Passport.Card.LastName", typeInfo.Columns["Passport.Card.LastName"].Name);
      Assert.AreEqual(64, typeInfo.Columns["Passport.Card.LastName"].Length);

      // Indexes
      if (IsSingleTable(typeInfo) || IsConcreteTable(typeInfo))
        Assert.AreEqual(2, typeInfo.Indexes.Count);
      else
        Assert.AreEqual(1, typeInfo.Indexes.Count);
      Assert.IsNotNull(typeInfo.Indexes["PK_Person"]);
      Assert.IsTrue(typeInfo.Indexes["PK_Person"].IsPrimary);
      Assert.IsTrue(typeInfo.Indexes["PK_Person"].IsUnique);
      Assert.AreEqual(typeInfo.Hierarchy.Key.TupleDescriptor.Count, typeInfo.Indexes["PK_Person"].KeyColumns.Count);
      Assert.AreEqual(typeInfo.Columns["PassportNumber"], typeInfo.Indexes["PK_Person"].KeyColumns[0].Key);

      #endregion

      #region Author

      typeInfo = domain.Model.Types[typeof (Author)];
      Assert.IsNotNull(typeInfo);
      Assert.IsNotNull(domain.Model.Types["Author"]);
      Assert.AreEqual(typeInfo, domain.Model.Types["Author"]);
      Assert.AreEqual("Author", typeInfo.Name);

      // Fields
      Assert.IsNotNull(typeInfo.Fields["Passport"]);
      Assert.AreEqual("Passport", typeInfo.Fields["Passport"].Name);
      Assert.IsTrue(typeInfo.Fields["Passport"].IsStructure);
      Assert.IsTrue(typeInfo.Fields["Passport"].IsInherited);
      Assert.AreEqual(5, typeInfo.Fields["Passport"].Fields.Count);
      Assert.IsNotNull(typeInfo.Fields["PenName"]);
      Assert.AreEqual(true, typeInfo.Fields["PenName"].IsNullable);
      Assert.AreEqual("PenName", typeInfo.Fields["PenName"].Name);
      Assert.AreEqual(64, typeInfo.Fields["PenName"].Length);
      Assert.IsNotNull(typeInfo.Fields["Books"]);
      Assert.AreEqual(true, typeInfo.Fields["Books"].IsNullable);
      Assert.AreEqual("Books", typeInfo.Fields["Books"].Name);

      Assert.AreEqual(2, typeInfo.Fields.Find(FieldAttributes.Declared).Count);
      Assert.AreEqual(9, typeInfo.Fields.Find(FieldAttributes.Inherited).Count);

      // KeyColumns
      Assert.IsNotNull(typeInfo.Columns["PassportNumber"]);
      Assert.AreEqual("PassportNumber", typeInfo.Columns["PassportNumber"].Name);
      //      Assert.AreEqual(person.Fields["Passport"].Indexes[0], person.Key);
      Assert.IsNotNull(typeInfo.Columns["Passport.Card.FirstName"]);
      Assert.AreEqual("Passport.Card.FirstName", typeInfo.Columns["Passport.Card.FirstName"].Name);
      Assert.AreEqual(64, typeInfo.Columns["Passport.Card.FirstName"].Length);
      Assert.IsNotNull(typeInfo.Columns["Passport.Card.SecondName"]);
      Assert.AreEqual("Passport.Card.SecondName", typeInfo.Columns["Passport.Card.SecondName"].Name);
      Assert.AreEqual(64, typeInfo.Columns["Passport.Card.SecondName"].Length);
      Assert.IsNotNull(typeInfo.Columns["Passport.Card.LastName"]);
      Assert.AreEqual("Passport.Card.LastName", typeInfo.Columns["Passport.Card.LastName"].Name);
      Assert.AreEqual(64, typeInfo.Columns["Passport.Card.LastName"].Length);
      Assert.IsNotNull(typeInfo.Columns["PenName"]);
      Assert.AreEqual(true, typeInfo.Columns["PenName"].IsNullable);
      Assert.AreEqual("PenName", typeInfo.Columns["PenName"].Name);
      Assert.AreEqual(64, typeInfo.Columns["PenName"].Length);
      Assert.IsFalse(typeInfo.Columns.Contains("Reviews"));

      // Indexes
      if (!IsSingleTable(typeInfo)) {
        if (IsConcreteTable(typeInfo))
          Assert.AreEqual(2, typeInfo.Indexes.Count);
        else
          Assert.AreEqual(3, typeInfo.Indexes.Count);
        Assert.IsNotNull(typeInfo.Indexes["Author.IX_PENNAME"]);
        Assert.IsNotNull(typeInfo.Indexes[0]);
        Assert.AreEqual(typeInfo.Indexes[0], typeInfo.Indexes["Author.IX_PENNAME"]);
        Assert.AreEqual(1, typeInfo.Indexes[0].KeyColumns.Count);
        Assert.IsNotNull(typeInfo.Indexes[0].KeyColumns[0]);
        Assert.AreEqual(typeInfo.Columns["PenName"], typeInfo.Indexes[0].KeyColumns[0].Key);
      }

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
      //      Assert.AreEqual(book.Fields["Isbn"].Indexes[0], book.Key);

      Assert.IsNotNull(typeInfo.Fields["Title"]);
      Assert.AreEqual(typeInfo.Fields["Title"].Name, "Title");
      Assert.AreEqual(typeInfo.Fields["Title"].Length, 128);

      Assert.IsNotNull(typeInfo.Fields["Author"]);
      Assert.AreEqual(typeInfo.Fields["Author"].Name, "Author");
      Assert.IsFalse(typeInfo.Fields["Author"].IsStructure);
      Assert.IsTrue(typeInfo.Fields["Author"].IsEntity);
      Assert.IsFalse(typeInfo.Fields["Author"].IsEntitySet);
      Assert.AreEqual(OnRemoveAction.Deny, typeInfo.Fields["Author"].Associations.Last().OnTargetRemove);

      // Indexes
      Assert.AreEqual(6, typeInfo.Indexes.Count);
      Assert.IsNotNull(typeInfo.Indexes["PK_Book"]);
      Assert.IsTrue(typeInfo.Indexes["PK_Book"].IsPrimary);
      Assert.IsTrue(typeInfo.Indexes["PK_Book"].IsUnique);
      Assert.AreEqual(typeInfo.Hierarchy.Key.TupleDescriptor.Count, typeInfo.Indexes["PK_Book"].KeyColumns.Count);
      Assert.AreEqual("Isbn", typeInfo.Indexes["PK_Book"].KeyColumns[0].Key.Name);
      Assert.AreEqual(Direction.Positive, typeInfo.Indexes["PK_Book"].KeyColumns[0].Value);

      Assert.IsNotNull(typeInfo.Indexes["Book.FK_Author"]);
      Assert.IsFalse(typeInfo.Indexes["Book.FK_Author"].IsPrimary);
      Assert.IsFalse(typeInfo.Indexes["Book.FK_Author"].IsUnique);
      Assert.AreEqual(domain.Model.Types[typeof(Author)].Hierarchy.Key.TupleDescriptor.Count, typeInfo.Indexes["Book.FK_Author"].KeyColumns.Count);
      Assert.AreEqual("BookAuthor", typeInfo.Indexes["Book.FK_Author"].KeyColumns[0].Key.Name);
      Assert.AreEqual(Direction.Positive, typeInfo.Indexes["Book.FK_Author"].KeyColumns[0].Value);

      Assert.IsNotNull(typeInfo.Indexes["Book.IX_Title"]);
      Assert.IsFalse(typeInfo.Indexes["Book.IX_Title"].IsPrimary);
      Assert.IsFalse(typeInfo.Indexes["Book.IX_Title"].IsUnique);
      Assert.AreEqual(1, typeInfo.Indexes["Book.IX_Title"].KeyColumns.Count);
      Assert.AreEqual("Title", typeInfo.Indexes["Book.IX_Title"].KeyColumns[0].Key.Name);
      Assert.AreEqual(Direction.Positive, typeInfo.Indexes["Book.IX_Title"].KeyColumns[0].Value);

      #endregion

      #region BookReview

      typeInfo = domain.Model.Types[typeof (BookReview)];
      Assert.IsNotNull(typeInfo);
      Assert.IsNotNull(domain.Model.Types["BookReview"]);
      Assert.AreEqual(typeInfo, domain.Model.Types["BookReview"]);
      Assert.AreEqual("BookReview", typeInfo.Name);

      // Fields
      Assert.IsNotNull(typeInfo.Fields["Book"]);
      Assert.AreEqual("Book", typeInfo.Fields["Book"].Name);
      Assert.AreEqual(OnRemoveAction.Cascade, typeInfo.Fields["Book"].Associations.Last().OnTargetRemove);

      Assert.IsNotNull(typeInfo.Fields["Reviewer"]);
      Assert.AreEqual("Reviewer", typeInfo.Fields["Reviewer"].Name);
      Assert.AreEqual(OnRemoveAction.Clear, typeInfo.Fields["Reviewer"].Associations.Last().OnTargetRemove);

      Assert.IsNotNull(typeInfo.Fields["Text"]);
      Assert.AreEqual("Text", typeInfo.Fields["Text"].Name);
      Assert.AreEqual(4096, typeInfo.Fields["Text"].Length);

      // Indexes
      Assert.AreEqual(6, typeInfo.Indexes.Count);
      Assert.IsNotNull(typeInfo.Indexes["PK_BookReview"]);
      Assert.IsTrue(typeInfo.Indexes["PK_BookReview"].IsPrimary);
      Assert.IsTrue(typeInfo.Indexes["PK_BookReview"].IsUnique);
      Assert.AreEqual(typeInfo.Hierarchy.Key.TupleDescriptor.Count, typeInfo.Indexes["PK_BookReview"].KeyColumns.Count);
      Assert.AreEqual("Book", typeInfo.Indexes["PK_BookReview"].KeyColumns[0].Key.Name);
      Assert.AreEqual(Direction.Positive, typeInfo.Indexes["PK_BookReview"].KeyColumns[0].Value);
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
          Assert.AreEqual(key, Key.Create<Book>(Domain, "0976470705"));

          Assert.IsNull(session.Query.SingleOrDefault(Key.Create<Book>(Domain, "0976470705")));
          Assert.AreEqual(null, session.Query.SingleOrDefault(Key.Create<Book>(Domain, "0976470705")));
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
          Assert.AreEqual((object) book, review.Book);
          Assert.AreEqual((object) reviewer, review.Reviewer);
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
