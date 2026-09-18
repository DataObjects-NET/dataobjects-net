// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Core;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Operations.Tests.OperationCapturingTestModel;

namespace Xtensive.Orm.Operations.Tests
{
  [TestFixture]
  public class OperationLogReplay : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof(Book).Assembly, typeof(Book).Namespace);
      configuration.Types.Register(typeof(OperationRegistry));
      configuration.Types.Register(typeof(OperationFactory));
      return configuration;
    }

    [Test]
    public void ComplexTest()
    {
      var operationLog = new OperationLog(OperationLogType.SystemOperationLog);
      using (var session = Domain.OpenSession())
      using (var capturer = OperationCapturer.Attach(session, operationLog)) {
        using (var tx = session.OpenTransaction()) {
          var author1 = new Author(session, "Unknown", "Author");
          author1.DateOfBirth = new DateTime(1985, 6, 12);

          var book11 = new Book(session);
          book11.Title = "100 pages about something";
          book11.NumberOfPages = 100;
          book11.Author = author1;

          var book12 = new Book(session);
          book12.Title = "Even more pages abount something";
          book12.NumberOfPages = 200;
          book12.Author = author1;

          var book13 = new Book(session);
          book13.Title = "The biggest book ever";
          book13.NumberOfPages = 300;
          book13.Author = author1;

          var author2 = new Author(session, "Even Less known", "Author");
          author2.DateOfBirth = new DateTime(1985, 6, 12);
          _ = author2.Books.Add(new Book(session) { Title = "100 ways to not live", NumberOfPages = 111 });
          _ = author2.Books.Add(new Book(session) { Title = "50 ways of eating pizza", NumberOfPages = 25 });
          _ = author2.Books.Add(new Book(session) { Title = "Single way to eat banana", NumberOfPages = 3 });

          _ = author1.Books.Remove(book12);
          book12.Remove();

          var book23 = author2.Books.AsEnumerable().Last();
          _ = author2.Books.Remove(book23);

          book23.Remove();

          author2.DateOfBirth = null;

          var line1 = new Line2d(session, new Point(session, 0, 1), new Point(session, 0, 10));
          var line2 = new Line2d(session, new Point(session, 0, 1), new Point(session, 0, 10));
          var line3 = new Line2d(session, new Point(session, 0, 1), new Point(session, 0, 10));

          line1.A.X = 1;
          line1.B.Y = 12;

          line2.A = new Point(session, 1, 2);

          //rollback
        }

      }

      using (var sessionForReplay = Domain.OpenSession())
      using (var tx = sessionForReplay.OpenTransaction()) {
        var booksCount = sessionForReplay.Query.CreateDelayedQuery(q => q.All<Book>().Count());
        var authorsCount = sessionForReplay.Query.CreateDelayedQuery(q => q.All<Author>().Count());
        var linesCount = sessionForReplay.Query.CreateDelayedQuery(q => q.All<Line2d>().Count());

        if (booksCount.Value + authorsCount.Value + linesCount.Value > 0) {
          throw new NUnit.Framework.InconclusiveException("database should be empty.");
        }

        var keys = operationLog.Replay(sessionForReplay);

        tx.Complete();
      }

      using (var sessionAfterReply = Domain.OpenSession())
      using (var tx = sessionAfterReply.OpenTransaction()) {
        var booksCount = sessionAfterReply.Query.CreateDelayedQuery(q => q.All<Book>().Count());
        var authorsCount = sessionAfterReply.Query.CreateDelayedQuery(q => q.All<Author>().Count());
        var linesCount = sessionAfterReply.Query.CreateDelayedQuery(q => q.All<Line2d>().Count());

        Assert.That(booksCount.Value, Is.EqualTo(4));
        Assert.That(authorsCount.Value, Is.EqualTo(2));
        Assert.That(linesCount.Value, Is.EqualTo(3));

        var books = sessionAfterReply.Query.All<Book>().ToList();

        var book11 = books.FirstOrDefault(b => b.Title == "100 pages about something");
        Assert.That(book11, Is.Not.Null);
        Assert.That(book11.NumberOfPages, Is.EqualTo(100));

        var book12 = books.FirstOrDefault(b => b.Title == "Even more pages abount something");
        Assert.That(book12, Is.Null);

        var book13 = books.FirstOrDefault(b => b.Title == "The biggest book ever");
        Assert.That(book13, Is.Not.Null);
        Assert.That(book13.NumberOfPages, Is.EqualTo(300));


        var book21 = books.FirstOrDefault(b => b.Title == "100 ways to not live");
        Assert.That(book21, Is.Null);

        var book22 = books.FirstOrDefault(b => b.Title == "50 ways of eating pizza");
        Assert.That(book22, Is.Not.Null);
        Assert.That(book22.NumberOfPages, Is.EqualTo(25));

        var book23 = books.FirstOrDefault(b => b.Title == "Single way to eat banana");
        Assert.That(book23, Is.Not.Null);
        Assert.That(book23.NumberOfPages, Is.EqualTo(3));

        var authors = sessionAfterReply.Query.All<Author>().ToList();

        var author1 = authors.FirstOrDefault(a => a.FirstName == "Unknown");
        Assert.That(author1, Is.Not.Null);
        Assert.That(author1.DateOfBirth, Is.EqualTo(new DateTime(1985, 6, 12)));
        Assert.That(author1.Books.Contains(book11), Is.True);
        Assert.That(author1.Books.Contains(book13), Is.True);


        var author2 = authors.FirstOrDefault(a => a.FirstName == "Even Less known");
        Assert.That(author2, Is.Not.Null);
        Assert.That(author2.DateOfBirth, Is.Null);
        Assert.That(author2.Books.Contains(book22), Is.True);
        Assert.That(author2.Books.Contains(book23), Is.True);

        //var lines = sessionAfterReply.Query.All<Line2d>().ToList();

        tx.Complete();
      }

    }
  }
}