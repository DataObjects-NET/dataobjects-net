// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.19

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Tests.Rse
{
  [TestFixture]
  public class BehaviorTest
  {
    [Test]
    public void LeftJoinTest()
    {
      const int personCount = 2000;
      Tuple personTuple = Tuple.Create(new[] {typeof (int), typeof (string), typeof (string)});
      Tuple authorTuple = Tuple.Create(new[] {typeof (int), typeof (string)});
      var personColumns = new[]
        {
          new RecordColumn("ID", 0, typeof (int)),
          new RecordColumn("FirstName", 1, typeof (string)),
          new RecordColumn("LastName", 2, typeof (string)),
        };
      var authorColumns = new[]
        {
          new RecordColumn("ID", 0, typeof (int)),
          new RecordColumn("Title", 1, typeof (string)),
        };
      var personHeader = new RecordSetHeader(personTuple.Descriptor, personColumns, null, new[] { new RecordColumnGroupMapping(null, null ), }, null);
      var authorHeader = new RecordSetHeader(authorTuple.Descriptor, authorColumns, null, new[] { new RecordColumnGroupMapping(null, null ), }, null);

      var persons = new Tuple[personCount];
      var authors = new Tuple[personCount / 2];
      for (int i = 0; i < personCount; i++) {
        Tuple person = personTuple.CreateNew();
        person.SetValue(0, i);
        person.SetValue(1, "FirstName" + i % 5);
        person.SetValue(2, "LastName" + i / 5);
        persons[i] = person;
        if (i % 2==0) {
          Tuple author = authorTuple.CreateNew();
          author.SetValue(0, i);
          author.SetValue(1, "Title" + i);
          authors[i / 2] = author;
        }
      }

      RecordSet personsRS = new RawProvider(personHeader, persons).Result;
      RecordSet authorsRS = new RawProvider(authorHeader, authors).Result;

      RecordSet personIndexed = personsRS.IndexBy(OrderBy.Asc(0));
      RecordSet authorsIndexed = authorsRS.IndexBy(OrderBy.Asc(0)).Alias("Authors");

      RecordSet result = personIndexed.JoinLeft(authorsIndexed, new Pair<int>(0, 0));
      int count = result.Count();
      Assert.AreEqual(personCount, count);
    }

    [Test]
    public void Test()
    {
      const int authorCount = 1000;
      const int booksPerAuthor = 20;
      Tuple authorTuple = Tuple.Create(new[] {typeof (int), typeof (string), typeof (string)});
      Tuple bookTuple = Tuple.Create(new[] {typeof (int), typeof (int), typeof (string)});
      var authorColumns = new[]
        {
          new RecordColumn("ID", 0, typeof (int)),
          new RecordColumn("FirstName", 1, typeof (string)),
          new RecordColumn("LastName", 2, typeof (string)),
        };
      var bookColumns = new[]
        {
          new RecordColumn("ID", 0, typeof (int)),
          new RecordColumn("IDAuthor", 1, typeof (int)),
          new RecordColumn("Title", 2, typeof (string)),
        };
      var authorHeader = new RecordSetHeader(authorTuple.Descriptor, authorColumns, TupleDescriptor.Empty, ArrayUtils<RecordColumnGroupMapping>.EmptyArray, new DirectionCollection<int>());
      var bookHeader = new RecordSetHeader(bookTuple.Descriptor, bookColumns, null, null, null);

      var authors = new Tuple[authorCount];
      var books = new Tuple[authorCount * booksPerAuthor];
      for (int i = 0; i < authorCount; i++) {
        Tuple author = authorTuple.CreateNew();
        author.SetValue(0, i);
        author.SetValue(1, "FirstName" + i % 5);
        author.SetValue(2, "LastName" + i / 5);
        authors[i] = author;
        for (int j = 0; j < booksPerAuthor; j++) {
          Tuple book = bookTuple.CreateNew();
          book.SetValue(0, i * booksPerAuthor + j);
          book.SetValue(1, i);
          book.SetValue(2, "Title" + i * booksPerAuthor + j);
          books[i * booksPerAuthor + j] = book;
        }
      }

      RecordSet authorRS = new RawProvider(authorHeader, authors).Result;
      RecordSet bookRS = new RawProvider(bookHeader, books).Result;

      // trying to execute following query 
      // select books.Title from books
      // inner join authors on authors.Id = books.IDAuthor
      // where authors.LastName = 'LastName16'
      // order by books.Title desc

      RegularTuple authorFilterTuple = Tuple.Create("LastName16");

      RecordSet result = authorRS
        .Alias("Authors")
        .IndexBy(OrderBy.Asc(2, 0))
        .Range(authorFilterTuple, authorFilterTuple)
        .Join(bookRS.Alias("Books"), new Pair<int>(0, 1))
        .OrderBy(OrderBy.Desc(5));

      foreach (Tuple record in result)
        Console.Out.WriteLine(record.GetValue<string>(result.IndexOf("Books.Title")));
    }
  }
}
