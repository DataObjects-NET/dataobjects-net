// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.09

using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Model.EntitySetModel;
using Person=Xtensive.Storage.Tests.Model.EntitySetModel.Person;

namespace Xtensive.Storage.Tests.Model.EntitySetModel
{
  public class Passport : Structure
  {
    [Field]
    public int Number { get; set; }

  }

  [HierarchyRoot("Number", Generator = typeof(Generator))]
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
        if (Number != value.Number)
          Number = value.Number;
      }
    }

    [Field]
    public EntitySet<BookReview> Reviews { get; private set; }
  }

  [Index("PenName:DESC", MappingName = "IX_PENNAME")]
  public class Author : Person
  {
    [Field(Length = 64)]
    public string PenName { get; set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }
  }

  [HierarchyRoot(typeof(IsbnKeyProvider), "Isbn")]
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
  }

  [HierarchyRoot("Book", "Reviewer")]
  public class BookReview : Entity
  {
    [Field(MappingName = "Book", OnDelete = ReferentialAction.Cascade)]
    public Book Book { get; set; }

    [Field(OnDelete = ReferentialAction.SetNull)]
    public Person Reviewer { get; set; }

    [Field(Length = 4096)]
    public string Text { get; set; }

    public BookReview(Key book, Key reviewer)
      : base(TupleExtensions.CombineWith(book.Tuple, reviewer.Tuple))
    {
    }
  }

  public class IsbnKeyProvider : Generator
  {
    private int counter;

    protected override Tuple NextOne()
    {
      Tuple result = Tuple.Create(counter.ToString());
      counter++;
      return result;
    }

    protected override IEnumerable<Tuple> NextMany()
    {
      for (int i = 0; i < CacheSize; i++)
        yield return NextOne();
    }
  }
}

namespace Xtensive.Storage.Tests.Model
{

  [TestFixture]
  public class EntitySetTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof(Person).Assembly, "Xtensive.Storage.Tests.Model.EntitySetModel");
      // config.Builders.Add(typeof (LibraryDomainBuilder));
      return config;
    }


    [Test]
    public void TestReferenceEntity()
    {
      Domain.Model.Dump();
    }
  }
}