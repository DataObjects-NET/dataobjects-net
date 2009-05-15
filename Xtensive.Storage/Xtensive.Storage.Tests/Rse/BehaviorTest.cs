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
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Parameters;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Tests.Rse
{
  [TestFixture]
  public class BehaviorTest
  {
    [Test]
    public void JoinTest()
    {
      const int personCount = 100;
      Tuple personTuple = Tuple.Create(new[] {typeof (int), typeof (string), typeof (string)});
      Tuple authorTuple = Tuple.Create(new[] {typeof (int), typeof (string)});
      var personColumns = new[]
                          {
                            new MappedColumn("ID", 0, typeof (int)),
                            new MappedColumn("FirstName", 1, typeof (string)),
                            new MappedColumn("LastName", 2, typeof (string)),
                          };
      var authorColumns = new[]
                          {
                            new MappedColumn("ID", 0, typeof (int)),
                            new MappedColumn("Title", 1, typeof (string)),
                          };
      var personHeader = new RecordSetHeader(personTuple.Descriptor, personColumns, new[]
                                                                                    {
                                                                                      new ColumnGroup(null, new[] {0}, new[] {0, 1, 2}),
                                                                                    }, null, null);
      var authorHeader = new RecordSetHeader(authorTuple.Descriptor, authorColumns, new[]
                                                                                    {
                                                                                      new ColumnGroup(null, new[] {0}, new[] {0, 1}),
                                                                                    }, null, null);

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

      RecordSet personIndexed = personsRS.OrderBy(OrderBy.Asc(0), true);
      RecordSet authorsIndexed = authorsRS.OrderBy(OrderBy.Asc(0), true).Alias("Authors");

      RecordSet resultLeft = personIndexed.JoinLeft(authorsIndexed, 0, 0);
      RecordSet resultLeft1 = personIndexed.JoinLeft(authorsIndexed, JoinAlgorithm.Hash, 0, 0);
      TestJoinCount(resultLeft, resultLeft1, personCount);

      resultLeft = personIndexed.Join(authorsIndexed, 0, 0);
      resultLeft1 = personIndexed.Join(authorsIndexed, JoinAlgorithm.Hash, 0, 0);
      TestJoinCount(resultLeft, resultLeft1, personCount / 2);

      resultLeft = authorsIndexed.JoinLeft(personIndexed, 0, 0);
      resultLeft1 = authorsIndexed.JoinLeft(personIndexed, JoinAlgorithm.Hash, 0, 0);
      TestJoinCount(resultLeft, resultLeft1, personCount / 2);

      resultLeft = authorsIndexed.Join(personIndexed, 0, 0);
      resultLeft1 = authorsIndexed.Join(personIndexed, JoinAlgorithm.Hash, 0, 0);
      TestJoinCount(resultLeft, resultLeft1, personCount / 2);
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
                            new MappedColumn("ID", 0, typeof (int)),
                            new MappedColumn("FirstName", 1, typeof (string)),
                            new MappedColumn("LastName", 2, typeof (string)),
                          };
      var bookColumns = new[]
                        {
                          new MappedColumn("ID", 0, typeof (int)),
                          new MappedColumn("IDAuthor", 1, typeof (int)),
                          new MappedColumn("Title", 2, typeof (string)),
                        };

      var authorHeader = new RecordSetHeader(authorTuple.Descriptor, authorColumns);
      var bookHeader = new RecordSetHeader(bookTuple.Descriptor, bookColumns);


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
        .OrderBy(OrderBy.Asc(2, 0), true)
        .Range(authorFilterTuple, authorFilterTuple)
        .Join(bookRS.Alias("Books"), new Pair<int>(0, 1))
        .OrderBy(OrderBy.Desc(5))
        .Select(0, 1, 3, 5);

      foreach (Tuple record in result)
        Console.Out.WriteLine(record /*.GetValue<string>(result.IndexOf("Books.Title"))*/);
    }

    [Test]
    public void ProviderTest()
    {
      #region Populate recordset.

      const int authorCount = 1000;
      Tuple authorTuple = Tuple.Create(new[] {typeof (int), typeof (string), typeof (string)});
      var authorColumns = new[]
                          {
                            new MappedColumn("ID", 0, typeof (int)),
                            new MappedColumn("FirstName", 1, typeof (string)),
                            new MappedColumn("LastName", 2, typeof (string)),
                          };
      var authorHeader = new RecordSetHeader(authorTuple.Descriptor, authorColumns);

      var authors = new Tuple[authorCount];
      for (int i = 0; i < authorCount; i++) {
        Tuple author = authorTuple.CreateNew();
        author.SetValue(0, i);
        author.SetValue(1, "FirstName" + i % 5);
        author.SetValue(2, "LastName" + i / 5);
        authors[i] = author;
      }

      RecordSet authorRS = authors
        .ToRecordSet(authorHeader)
        .OrderBy(new DirectionCollection<int>(0), true);

      #endregion

      authorRS.Save(TemporaryDataScope.Transaction, "authors");
    }

    private void TestJoinCount(RecordSet item1, RecordSet item2, int resultCount)
    {
      using (EnumerationScope.Open()) {
        var count1 = (int) item1.Count();
        var count2 = (int) item2.Count();
        Assert.AreEqual(count1, count2);
        Assert.AreEqual(resultCount, count1);
      }
    }

    [Test]
    public void DistinctTest()
    {
      const int authorCount = 1000;
      Tuple authorTuple = Tuple.Create(new[] {typeof (int), typeof (string), typeof (string)});
      var authorColumns = new[]
                          {
                            new MappedColumn("ID", 0, typeof (int)),
                            new MappedColumn("FirstName", 1, typeof (string)),
                            new MappedColumn("LastName", 2, typeof (string)),
                          };
      var authorHeader = new RecordSetHeader(authorTuple.Descriptor, authorColumns);

      var authors = new Tuple[authorCount];
      for (int i = 0; i < authorCount; i++) {
        Tuple author = authorTuple.CreateNew();
        author.SetValue(0, 1);
        author.SetValue(1, "FirstName");
        author.SetValue(2, "LastName");
        authors[i] = author;
      }

      using (new Measurement(authorCount)) {
        RecordSet authorRS = authors
          .ToRecordSet(authorHeader)
          .Distinct();

        Assert.AreEqual(1, authorRS.Count());
      }
    }

    [Test]
    public void SubqueryTest()
    {
      const int authorCount = 1000;
      const int booksPerAuthor = 20;
      Tuple authorTuple = Tuple.Create(new[] {typeof (int), typeof (string), typeof (string)});
      Tuple bookTuple = Tuple.Create(new[] {typeof (int), typeof (int), typeof (string)});
      var authorColumns = new[]
                          {
                            new MappedColumn("ID", 0, typeof (int)),
                            new MappedColumn("FirstName", 1, typeof (string)),
                            new MappedColumn("LastName", 2, typeof (string)),
                          };
      var bookColumns = new[]
                        {
                          new MappedColumn("ID", 0, typeof (int)),
                          new MappedColumn("IDAuthor", 1, typeof (int)),
                          new MappedColumn("Title", 2, typeof (string)),
                        };

      var authorHeader = new RecordSetHeader(authorTuple.Descriptor, authorColumns);
      var bookHeader = new RecordSetHeader(bookTuple.Descriptor, bookColumns);


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
      RecordSet bookRS = new RawProvider(bookHeader, books).Result.Alias("Book");

//      Assert.IsTrue(bookRS.All(t => t.GetValue(0) == t.GetValue(0)));

//      using (new Measurement("Select many on Enumerable")) {
//        var enumerable = authorRS.SelectMany((l) => bookRS.Where(r => r.GetValue<int>(1) == l.GetValue<int>(0)), (l, r) => new Pair<Tuple>(l, r));
//        var list0 = enumerable.ToList();
//      }

      using (new Measurement("Apply through Rse")) {
        var p = new ApplyParameter();
        var result = authorRS.Apply(p, bookRS.Filter(t => t.GetValue<int>(1)==p.Value.GetValue<int>(0)));
        var list = result.ToList();
        Assert.AreEqual(authorCount * booksPerAuthor, list.Count);
      }
    }

    [Test]
    public void RowNumberTest()
    {
      Random random = RandomManager.CreateRandom();
      const int authorCount = 1000;
      const int categoryCount = 20;
      Tuple authorTuple = Tuple.Create(new[] { typeof(int), typeof(string), typeof(string), typeof(int) });
      var authorColumns = new[] {
                            new MappedColumn("ID", 0, typeof (int)),
                            new MappedColumn("FirstName", 1, typeof (string)),
                            new MappedColumn("LastName", 2, typeof (string)),
                            new MappedColumn("Category", 3, typeof (int)),
                          };
      var authorHeader = new RecordSetHeader(authorTuple.Descriptor, authorColumns);

      var authors = new Tuple[authorCount];
      for (int i = 0; i < authorCount; i++) {
        Tuple author = authorTuple.CreateNew();
        author.SetValue(0, i);
        author.SetValue(1, string.Format("FirstName_{0}", i));
        author.SetValue(2, string.Format("LastName{0}", i));
        author.SetValue(3, random.Next(0, categoryCount - 1));
        authors[i] = author;
      }

      // One row number

      var rowNumberColumnName = "AuthorRowNumberColumn";
      RecordSet authorRS = authors
        .ToRecordSet(authorHeader)
        .OrderBy(new DirectionCollection<int>(0))
        .RowNumber(rowNumberColumnName);

      Assert.AreEqual(5, authorRS.Header.Length);
      int rowNumber = 1;
      foreach (var tuple in authorRS) {
        Assert.AreEqual(rowNumber++, tuple[authorRS.Header.Columns[rowNumberColumnName].Index]);
      }

      // Two row numbers

      var rowNumberColumnName2 = "AuthorRowNumberColumn2";
      RecordSet authorRS2 = authorRS.RowNumber(rowNumberColumnName2);

      Assert.AreEqual(6, authorRS2.Header.Length);
      int rowNumber2 = 1;
      foreach (var tuple in authorRS2)
      {
        Assert.AreEqual(rowNumber2, tuple[authorRS2.Header.Columns[rowNumberColumnName].Index]);
        Assert.AreEqual(rowNumber2++, tuple[authorRS2.Header.Columns[rowNumberColumnName2].Index]);
      }

      // Row numbers and Join
      Tuple categoryTuple = Tuple.Create(new[] { typeof(int), typeof(string) });
      var categoryColumns = new[] {
                            new MappedColumn("CategoryID", 0, typeof (int)),
                            new MappedColumn("CategoryName", 1, typeof (string)),
                          };
      var categoryHeader = new RecordSetHeader(categoryTuple.Descriptor, categoryColumns);
      var categories = new Tuple[categoryCount];
      for (int i = 0; i < categoryCount; i++)
      {
        Tuple category = categoryTuple.CreateNew();
        category.SetValue(0, i);
        category.SetValue(1, string.Format("CategoryName_{0}", i));
        categories[i] = category;
      }

      const string categoryRowNumberColumnName = "CategoryRowNumber";
      RecordSet categoryRS = categories
        .ToRecordSet(categoryHeader)
        .OrderBy(new DirectionCollection<int>(0))
        .RowNumber(categoryRowNumberColumnName);
      Assert.AreEqual(6, authorRS2.Header.Length);
      int categoryRowNumber = 1;
      foreach (var tuple in categoryRS)
      {
        Assert.AreEqual(categoryRowNumber++, tuple[categoryRS.Header.Columns[categoryRowNumberColumnName].Index]);
      }

      const string joinRowNumberColumnName = "JoinRowNumber";
      var joinRS = authorRS.Join(categoryRS, JoinAlgorithm.Default, 3, 0).RowNumber(joinRowNumberColumnName);
      Assert.AreEqual(authorRS.Count(), joinRS.Count());

      int joinRowNumber = 1;
      foreach (var tuple in joinRS) {
        Assert.AreEqual(joinRowNumber++, tuple[joinRS.Header.Columns[joinRowNumberColumnName].Index]);
        Assert.AreEqual((int)tuple[joinRS.Header.Columns["CategoryID"].Index] + 1, tuple[joinRS.Header.Columns[categoryRowNumberColumnName].Index]);
        Assert.AreEqual((int)tuple[joinRS.Header.Columns["ID"].Index] + 1, tuple[joinRS.Header.Columns[rowNumberColumnName].Index]);
        Log.Debug(tuple.ToString());
      }

    }
  }
}