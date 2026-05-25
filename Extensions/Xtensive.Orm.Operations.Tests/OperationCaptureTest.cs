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


namespace Xtensive.Orm.Operations.Tests.OperationCapturingTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class Line2d : Entity
  {
    [Field, Key]
    public int Id {  get; private set; }

    [Field]
    public Point A { get; set; }

    [Field]
    public Point B { get; set; }

    public Line2d(Session session, Point a, Point b)
      : base(session)
    {
      A = a;
      B = b;
    }
  }

  [Serializable]
  public class Point : Structure
  {
    [Field]
    public double X { get; set; }

    [Field]
    public double Y { get; set; }

    public Point(Session session, double x, double y)
      : base(session)
    {
      X = x;
      Y = y;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 120)]
    public string Title { get; set; }

    [Field]
    public int NumberOfPages { get; set; }

    [Field]
    [Association(PairTo = nameof(Author.Books))]
    public Author Author { get; set; }

    public override string ToString()
    {
      return Title;
    }

    public Book(Session session)
      : base(session)
    {
    }

    public Book(Session session, int numberOfPages)
      : base(session)
    {
      NumberOfPages = numberOfPages;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string FirstName { get; set; }

    [Field(Length = 50)]
    public string LastName { get; set; }

    [Field]
    public DateTime? DateOfBirth {  get; set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }

    public override string ToString()
    {
      return FirstName + " " + LastName;
    }

    public Author(Session session, string firstName, string lastName)
      : base(session)
    {
      FirstName = firstName;
      LastName = lastName;
    }
  }
}

namespace Xtensive.Orm.Operations.Tests
{
  [TestFixture]
  public class OperationCaptureTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof(Book).Assembly, typeof(Book).Namespace);
      configuration.Types.Register(typeof(OperationRegistry));
      configuration.Types.Register(typeof(OperationFactory));
      return configuration;
    }

    [TearDown]
    public void TearDown()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        using (session.Operations.DisableSystemOperationRegistration()) {
          var lines = session.Query.All<Line2d>().ToList();
          session.Remove(lines);

          var books = session.Query.All<Book>().ToList();
          var authors = session.Query.All<Author>().ToList();
          foreach (var book in books)
            book.Author = null;
          session.Remove(books);
          session.Remove(authors);
        }
        tx.Complete();
      }
    }

    [Test]
    [TestCase(OperationLogType.SystemOperationLog)]
    [TestCase(OperationLogType.OutermostOperationLog)]
    public void EntityCreationCaptureTest(OperationLogType logType)
    {
      var operationLog = new OperationLog(logType);
      using(var session = Domain.OpenSession())
      using (var capturer = OperationCapturer.Attach(session,operationLog)) {
        using(var tx = session.OpenTransaction()) {
          var book = new Book(session);
          tx.Complete();
        }
      }

      Assert.That(operationLog.Count, Is.EqualTo(2));
      var operations = operationLog.ToList();
      Assert.That(operations[0], Is.InstanceOf<KeyGenerateOperation>());
      var keyGenerateOperation = (KeyGenerateOperation)operations[0];
      Assert.That(keyGenerateOperation.Key.IsTemporary(Domain), Is.False);

      Assert.That(operations[1], Is.InstanceOf<EntityCreateOperation>());
      var createEntityOperation = (EntityCreateOperation) operations[1];
      Assert.That(createEntityOperation.TypeName, Is.EqualTo(nameof(Book)));
      Assert.That(createEntityOperation.Key, Is.EqualTo(keyGenerateOperation.Key));


      operationLog = new OperationLog(logType);
      using (var session = Domain.OpenSession())
      using (var capturer = OperationCapturer.Attach(session, operationLog)) {
        using (var tx = session.OpenTransaction()) {
          var book = new Book(session, 111);
          tx.Complete();
        }
      }

      Assert.That(operationLog.Count, Is.EqualTo(3));
      operations = operationLog.ToList();
      Assert.That(operations[0], Is.InstanceOf<KeyGenerateOperation>());
      keyGenerateOperation = (KeyGenerateOperation) operations[0];
      Assert.That(keyGenerateOperation.Key.IsTemporary(Domain), Is.False);

      Assert.That(operations[1], Is.InstanceOf<EntityCreateOperation>());
      createEntityOperation = (EntityCreateOperation) operations[1];
      Assert.That(createEntityOperation.TypeName, Is.EqualTo(nameof(Book)));
      Assert.That(createEntityOperation.Key, Is.EqualTo(keyGenerateOperation.Key));

      Assert.That(operations[2], Is.InstanceOf<EntityFieldSetOperation>());
      var numberOfPagesSet = (EntityFieldSetOperation) operations[2];
      Assert.That(numberOfPagesSet.Key, Is.EqualTo(keyGenerateOperation.Key));
      Assert.That(numberOfPagesSet.Field, Is.EqualTo(Domain.Model.Types[nameof(Book)].Fields[nameof(Book.NumberOfPages)]));
      Assert.That(numberOfPagesSet.Value, Is.EqualTo(111));
    }

    [Test]
    [TestCase(OperationLogType.SystemOperationLog)]
    [TestCase(OperationLogType.OutermostOperationLog)]
    public void EntityCreationCaptureInClientProfileTest(OperationLogType logType)
    {
      var operationLog = new OperationLog(logType);
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile)))
      using (var capturer = OperationCapturer.Attach(session, operationLog)) {
        using (var tx = session.OpenTransaction()) {
          var book = new Book(session);
          tx.Complete();
        }
      }

      Assert.That(operationLog.Count, Is.EqualTo(2));
      var operations = operationLog.ToList();
      Assert.That(operations[0], Is.InstanceOf<KeyGenerateOperation>());
      var keyGenerateOperation = (KeyGenerateOperation) operations[0];
      Assert.That(keyGenerateOperation.Key.IsTemporary(Domain), Is.True);

      Assert.That(operations[1], Is.InstanceOf<EntityCreateOperation>());
      var createEntityOperation = (EntityCreateOperation) operations[1];
      Assert.That(createEntityOperation.TypeName, Is.EqualTo(nameof(Book)));
      Assert.That(createEntityOperation.Key, Is.EqualTo(keyGenerateOperation.Key));

      operationLog = new OperationLog(logType);
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile)))
      using (var capturer = OperationCapturer.Attach(session, operationLog)) {
        using (var tx = session.OpenTransaction()) {
          var book = new Book(session, 111);
          tx.Complete();
        }
      }

      Assert.That(operationLog.Count, Is.EqualTo(3));
      operations = operationLog.ToList();
      Assert.That(operations[0], Is.InstanceOf<KeyGenerateOperation>());
      keyGenerateOperation = (KeyGenerateOperation) operations[0];
      Assert.That(keyGenerateOperation.Key.IsTemporary(Domain), Is.True);

      Assert.That(operations[1], Is.InstanceOf<EntityCreateOperation>());
      createEntityOperation = (EntityCreateOperation) operations[1];
      Assert.That(createEntityOperation.TypeName, Is.EqualTo(nameof(Book)));
      Assert.That(createEntityOperation.Key, Is.EqualTo(keyGenerateOperation.Key));

      Assert.That(operations[2], Is.InstanceOf<EntityFieldSetOperation>());
      var numberOfPagesSet = (EntityFieldSetOperation) operations[2];
      Assert.That(numberOfPagesSet.Key, Is.EqualTo(keyGenerateOperation.Key));
      Assert.That(numberOfPagesSet.Field, Is.EqualTo(Domain.Model.Types[nameof(Book)].Fields[nameof(Book.NumberOfPages)]));
      Assert.That(numberOfPagesSet.Value, Is.EqualTo(111));
    }

    [Test]
    [TestCase(OperationLogType.SystemOperationLog)]
    [TestCase(OperationLogType.OutermostOperationLog)]
    public void ChangePrimitiveFieldTest(OperationLogType logType)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Book book;
        Author timelessAuthor;
        Author justAuthor;
        using (session.Operations.DisableSystemOperationRegistration()) {
          book = new Book(session, 100) { Title = "100 white pages" };
          timelessAuthor = new Author(session, "The timeless", "Author") { DateOfBirth = null };
          justAuthor = new Author(session, "The 60s", "Author") { DateOfBirth = new DateTime(1952, 8, 22) };
        }

        var booksOperationLog = new OperationLog(logType);
        using (var capturer = OperationCapturer.Attach(session, booksOperationLog)) {
          book.NumberOfPages = book.NumberOfPages;
          book.Title = book.Title;
        }

        Assert.That(booksOperationLog.Count, Is.EqualTo(2));
        var operations = booksOperationLog.ToList();

        Assert.That(operations[0], Is.InstanceOf<EntityFieldSetOperation>());
        var numberOfPagesSet = (EntityFieldSetOperation) operations[0];
        Assert.That(numberOfPagesSet.Key, Is.EqualTo(book.Key));
        Assert.That(numberOfPagesSet.Field, Is.EqualTo(book.TypeInfo.Fields[nameof(Book.NumberOfPages)]));
        Assert.That(numberOfPagesSet.Value, Is.EqualTo(100));

        Assert.That(operations[1], Is.InstanceOf<EntityFieldSetOperation>());
        var titleSet = (EntityFieldSetOperation) operations[1];
        Assert.That(titleSet.Key, Is.EqualTo(book.Key));
        Assert.That(titleSet.Field, Is.EqualTo(book.TypeInfo.Fields[nameof(Book.Title)]));
        Assert.That(titleSet.Value, Is.EqualTo("100 white pages"));

        booksOperationLog = new OperationLog(logType);
        using(var capturer = OperationCapturer.Attach(session, booksOperationLog)) {
          book.NumberOfPages = 999;
          book.Title = "Not only 100 white pages";
        }

        Assert.That(booksOperationLog.Count, Is.EqualTo(2));
        operations = booksOperationLog.ToList();

        Assert.That(operations[0], Is.InstanceOf<EntityFieldSetOperation>());
        numberOfPagesSet = (EntityFieldSetOperation) operations[0];
        Assert.That(numberOfPagesSet.Key, Is.EqualTo(book.Key));
        Assert.That(numberOfPagesSet.Field, Is.EqualTo(book.TypeInfo.Fields[nameof(Book.NumberOfPages)]));
        Assert.That(numberOfPagesSet.Value, Is.EqualTo(999));

        Assert.That(operations[1], Is.InstanceOf<EntityFieldSetOperation>());
        titleSet = (EntityFieldSetOperation) operations[1];
        Assert.That(titleSet.Key, Is.EqualTo(book.Key));
        Assert.That(titleSet.Field, Is.EqualTo(book.TypeInfo.Fields[nameof(Book.Title)]));
        Assert.That(titleSet.Value, Is.EqualTo("Not only 100 white pages"));


        var authorsOperationLog = new OperationLog(logType);
        using (var capturer = OperationCapturer.Attach(session, authorsOperationLog)) {
          timelessAuthor.DateOfBirth = null;
        }

        Assert.That(authorsOperationLog.Count, Is.EqualTo(1));
        operations = authorsOperationLog.ToList();

        Assert.That(operations[0], Is.InstanceOf<EntityFieldSetOperation>());
        var dateOfBirthSet = (EntityFieldSetOperation) operations[0];
        Assert.That(dateOfBirthSet.Key, Is.EqualTo(timelessAuthor.Key));
        Assert.That(dateOfBirthSet.Field, Is.EqualTo(timelessAuthor.TypeInfo.Fields[nameof(Author.DateOfBirth)]));
        Assert.That(dateOfBirthSet.Value, Is.Null);

        authorsOperationLog = new OperationLog(logType);
        using (var capturer = OperationCapturer.Attach(session, authorsOperationLog)) {
          timelessAuthor.DateOfBirth = new DateTime(1961,5,12);
        }

        Assert.That(authorsOperationLog.Count, Is.EqualTo(1));
        operations = authorsOperationLog.ToList();

        Assert.That(operations[0], Is.InstanceOf<EntityFieldSetOperation>());
        dateOfBirthSet = (EntityFieldSetOperation) operations[0];
        Assert.That(dateOfBirthSet.Key, Is.EqualTo(timelessAuthor.Key));
        Assert.That(dateOfBirthSet.Field, Is.EqualTo(timelessAuthor.TypeInfo.Fields[nameof(Author.DateOfBirth)]));
        Assert.That(dateOfBirthSet.Value, Is.EqualTo(new DateTime(1961, 5, 12)));


        authorsOperationLog = new OperationLog(logType);
        using (var capturer = OperationCapturer.Attach(session, authorsOperationLog)) {
          timelessAuthor.DateOfBirth = null;
        }

        Assert.That(authorsOperationLog.Count, Is.EqualTo(1));
        operations = authorsOperationLog.ToList();

        Assert.That(operations[0], Is.InstanceOf<EntityFieldSetOperation>());
        dateOfBirthSet = (EntityFieldSetOperation) operations[0];
        Assert.That(dateOfBirthSet.Key, Is.EqualTo(timelessAuthor.Key));
        Assert.That(dateOfBirthSet.Field, Is.EqualTo(timelessAuthor.TypeInfo.Fields[nameof(Author.DateOfBirth)]));
        Assert.That(dateOfBirthSet.Value, Is.Null);

        tx.Complete();
      }
    }

    [Test]
    [TestCase(OperationLogType.SystemOperationLog)]
    [TestCase(OperationLogType.OutermostOperationLog)]
    public void ChangePrimitiveFieldComplexTest(OperationLogType logType)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Book book;
        Author timelessAuthor;
        using (session.Operations.DisableSystemOperationRegistration()) {
          book = new Book(session, 100) { Title = "100 white pages" };
          timelessAuthor = new Author(session, "The timeless", "Author") { DateOfBirth = null };
        }

        var complexOperationLog = new OperationLog(logType);
        using (var capturer = OperationCapturer.Attach(session, complexOperationLog)) {
          book.NumberOfPages = book.NumberOfPages;
          book.Title = book.Title;

          timelessAuthor.DateOfBirth = null;
          timelessAuthor.DateOfBirth = new DateTime(1961, 5, 12);

          book.NumberOfPages = 999;
          book.Title = "Not only 100 white pages";
        }

        Assert.That(complexOperationLog.Count, Is.EqualTo(6));
        var operations = complexOperationLog.ToList();

        Assert.That(operations[0], Is.InstanceOf<EntityFieldSetOperation>());
        var numberOfPagesSet = (EntityFieldSetOperation) operations[0];
        Assert.That(numberOfPagesSet.Key, Is.EqualTo(book.Key));
        Assert.That(numberOfPagesSet.Field, Is.EqualTo(book.TypeInfo.Fields[nameof(Book.NumberOfPages)]));
        Assert.That(numberOfPagesSet.Value, Is.EqualTo(100));

        Assert.That(operations[1], Is.InstanceOf<EntityFieldSetOperation>());
        var titleSet = (EntityFieldSetOperation) operations[1];
        Assert.That(titleSet.Key, Is.EqualTo(book.Key));
        Assert.That(titleSet.Field, Is.EqualTo(book.TypeInfo.Fields[nameof(Book.Title)]));
        Assert.That(titleSet.Value, Is.EqualTo("100 white pages"));

        Assert.That(operations[2], Is.InstanceOf<EntityFieldSetOperation>());
        var dateOfBirthSet = (EntityFieldSetOperation) operations[2];
        Assert.That(dateOfBirthSet.Key, Is.EqualTo(timelessAuthor.Key));
        Assert.That(dateOfBirthSet.Field, Is.EqualTo(timelessAuthor.TypeInfo.Fields[nameof(Author.DateOfBirth)]));
        Assert.That(dateOfBirthSet.Value, Is.Null);

        Assert.That(operations[3], Is.InstanceOf<EntityFieldSetOperation>());
        dateOfBirthSet = (EntityFieldSetOperation) operations[3];
        Assert.That(dateOfBirthSet.Key, Is.EqualTo(timelessAuthor.Key));
        Assert.That(dateOfBirthSet.Field, Is.EqualTo(timelessAuthor.TypeInfo.Fields[nameof(Author.DateOfBirth)]));
        Assert.That(dateOfBirthSet.Value, Is.EqualTo(new DateTime(1961, 5, 12)));

        Assert.That(operations[4], Is.InstanceOf<EntityFieldSetOperation>());
        numberOfPagesSet = (EntityFieldSetOperation) operations[4];
        Assert.That(numberOfPagesSet.Key, Is.EqualTo(book.Key));
        Assert.That(numberOfPagesSet.Field, Is.EqualTo(book.TypeInfo.Fields[nameof(Book.NumberOfPages)]));
        Assert.That(numberOfPagesSet.Value, Is.EqualTo(999));

        Assert.That(operations[5], Is.InstanceOf<EntityFieldSetOperation>());
        titleSet = (EntityFieldSetOperation) operations[5];
        Assert.That(titleSet.Key, Is.EqualTo(book.Key));
        Assert.That(titleSet.Field, Is.EqualTo(book.TypeInfo.Fields[nameof(Book.Title)]));
        Assert.That(titleSet.Value, Is.EqualTo("Not only 100 white pages"));

        tx.Complete();
      }
    }

    [Test]
    [TestCase(OperationLogType.SystemOperationLog)]
    [TestCase(OperationLogType.OutermostOperationLog)]
    public void ChangeReferenceFieldTest(OperationLogType logType)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Book book;
        Author timelessAuthor;
        Author justAuthor;
        using (session.Operations.DisableSystemOperationRegistration()) {
          book = new Book(session, 100) { Title = "100 white pages" };
          timelessAuthor = new Author(session, "The timeless", "Author") { DateOfBirth = null };
          justAuthor = new Author(session, "The 60s", "Author") { DateOfBirth = new DateTime(1952, 8, 22) };
        }

        var referenceSetLog = new OperationLog(logType);
        using(var capturer = OperationCapturer.Attach(session, referenceSetLog)) {
          book.Author = null;
        }

        Assert.That(referenceSetLog.Count, Is.EqualTo(1));
        var operation = referenceSetLog.First();
        Assert.That(operation, Is.InstanceOf<EntityFieldSetOperation>());
        var refSet = (EntityFieldSetOperation) operation;
        Assert.That(refSet.Key, Is.EqualTo(book.Key));
        Assert.That(refSet.Field, Is.EqualTo(book.TypeInfo.Fields[nameof(Book.Author)]));
        Assert.That(refSet.ValueKey, Is.Null);

        referenceSetLog = new OperationLog(logType);
        using (var capturer = OperationCapturer.Attach(session, referenceSetLog)) {
          book.Author = timelessAuthor;
        }

        Assert.That(referenceSetLog.Count, Is.EqualTo(1));
        operation = referenceSetLog.First();
        Assert.That(operation, Is.InstanceOf<EntityFieldSetOperation>());
        refSet = (EntityFieldSetOperation) operation;
        Assert.That(refSet.Key, Is.EqualTo(book.Key));
        Assert.That(refSet.Field, Is.EqualTo(book.TypeInfo.Fields[nameof(Book.Author)]));
        Assert.That(refSet.ValueKey, Is.EqualTo(timelessAuthor.Key));

        referenceSetLog = new OperationLog(logType);
        using (var capturer = OperationCapturer.Attach(session, referenceSetLog)) {
          book.Author = justAuthor;
        }

        Assert.That(referenceSetLog.Count, Is.EqualTo(1));
        operation = referenceSetLog.First();
        Assert.That(operation, Is.InstanceOf<EntityFieldSetOperation>());
        refSet = (EntityFieldSetOperation) operation;
        Assert.That(refSet.Key, Is.EqualTo(book.Key));
        Assert.That(refSet.Field, Is.EqualTo(book.TypeInfo.Fields[nameof(Book.Author)]));
        Assert.That(refSet.ValueKey, Is.EqualTo(justAuthor.Key));

        referenceSetLog = new OperationLog(logType);
        using (var capturer = OperationCapturer.Attach(session, referenceSetLog)) {
          book.Author = null;
        }

        Assert.That(referenceSetLog.Count, Is.EqualTo(1));
        operation = referenceSetLog.First();
        Assert.That(operation, Is.InstanceOf<EntityFieldSetOperation>());
        refSet = (EntityFieldSetOperation) operation;
        Assert.That(refSet.Key, Is.EqualTo(book.Key));
        Assert.That(refSet.Field, Is.EqualTo(book.TypeInfo.Fields[nameof(Book.Author)]));
        Assert.That(refSet.ValueKey, Is.Null);

        tx.Complete();
      }
    }

    [Test]
    [TestCase(OperationLogType.SystemOperationLog)]
    [TestCase(OperationLogType.OutermostOperationLog)]
    public void ChangeStructureFieldTest(OperationLogType logType)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Line2d line;
        using (session.Operations.DisableSystemOperationRegistration()) {
          line = new Line2d(session, new Point(session, 1.0, 2.0), new Point(session, 3.0, 6.0));
        }

        var changeStructureObjectLog = new OperationLog(logType);
        using(var capturer = OperationCapturer.Attach(session, changeStructureObjectLog)) {
          var newA = new Point(session, 1.5, 1.6);// structure owner is null
          line.A = newA;
        }

        Assert.That(changeStructureObjectLog.Count, Is.EqualTo(1));
        var operation = changeStructureObjectLog.First();
        Assert.That(operation, Is.InstanceOf<EntityFieldSetOperation>());

        var followingOperations = operation.FollowingOperations;
        Assert.That(followingOperations.Count, Is.EqualTo(2));
        Assert.That(followingOperations[0], Is.InstanceOf<EntityFieldSetOperation>());
        var x = (EntityFieldSetOperation)followingOperations[0];
        Assert.That(x.Value, Is.EqualTo(1.5));

        Assert.That(followingOperations[1], Is.InstanceOf<EntityFieldSetOperation>());
        var y = (EntityFieldSetOperation) followingOperations[1];
        Assert.That(y.Value, Is.EqualTo(1.6));

        tx.Complete();
      }
    }

    [Test]
    [TestCase(OperationLogType.SystemOperationLog)]
    [TestCase(OperationLogType.OutermostOperationLog)]
    public void ChangeFieldOfStructureTest(OperationLogType logType)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Line2d line;
        using (session.Operations.DisableSystemOperationRegistration()) {
          line = new Line2d(session, new Point(session, 1.0, 2.0), new Point(session, 3.0, 6.0));
        }

        var changeStructureObjectLog = new OperationLog(logType);
        using (var capturer = OperationCapturer.Attach(session, changeStructureObjectLog)) {
          var newA = new Point(session, 1.0, 1.0);// structure owner is null
          line.A.X = newA.X;
        }

        Assert.That(changeStructureObjectLog.Count, Is.EqualTo(1));
        var operation = changeStructureObjectLog.First();
        Assert.That(operation, Is.InstanceOf<EntityFieldSetOperation>());

        tx.Complete();
      }
    }

    [Test]
    [TestCase(OperationLogType.SystemOperationLog)]
    [TestCase(OperationLogType.OutermostOperationLog)]
    public void EntitySetAddItemsTest(OperationLogType logType)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Book book1, book2;
        Author author;
        using (session.Operations.DisableSystemOperationRegistration()) {
          book1 = new Book(session, 100) { Title = "100 white pages" };
          book2 = new Book(session, 200) { Title = "200 white pages" };
          author = new Author(session, "The 60s", "Author") { DateOfBirth = new DateTime(1952, 8, 22) };
        }

        var entitySetUpdateLog = new OperationLog(logType);
        using(var capturer = OperationCapturer.Attach(session, entitySetUpdateLog)) {
          _ = author.Books.Add(book1);
          _ = author.Books.Add(book2);
        }

        Assert.That(entitySetUpdateLog.Count, Is.EqualTo(2));
        var operations = entitySetUpdateLog.ToList();

        Assert.That(operations[0], Is.InstanceOf<EntitySetItemAddOperation>());
        var addFirstBook = (EntitySetItemAddOperation) operations[0];
        Assert.That(addFirstBook.Key, Is.EqualTo(author.Key));
        Assert.That(addFirstBook.Field, Is.EqualTo(author.TypeInfo.Fields[nameof(Author.Books)]));
        Assert.That(addFirstBook.ItemKey, Is.EqualTo(book1.Key));

        Assert.That(operations[1], Is.InstanceOf<EntitySetItemAddOperation>());
        var addSecondBook = (EntitySetItemAddOperation) operations[1];
        Assert.That(addSecondBook.Key, Is.EqualTo(author.Key));
        Assert.That(addSecondBook.Field, Is.EqualTo(author.TypeInfo.Fields[nameof(Author.Books)]));
        Assert.That(addSecondBook.ItemKey, Is.EqualTo(book2.Key));

        tx.Complete();
      }
    }

    [Test]
    [TestCase(OperationLogType.SystemOperationLog)]
    [TestCase(OperationLogType.OutermostOperationLog)]
    public void EntitySetDeleteItemsTest(OperationLogType logType)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Book book1, book2;
        Author author;
        using (session.Operations.DisableSystemOperationRegistration()) {
          book1 = new Book(session, 100) { Title = "100 white pages" };
          book2 = new Book(session, 200) { Title = "200 white pages" };
          author = new Author(session, "The 60s", "Author") { DateOfBirth = new DateTime(1952, 8, 22) };
          _ = author.Books.Add(book1);
          _ = author.Books.Add(book2);
        }

        var entitySetUpdateLog = new OperationLog(logType);
        using (var capturer = OperationCapturer.Attach(session, entitySetUpdateLog)) {
          _ = author.Books.Remove(book1);
          _ = author.Books.Remove(book2);
        }

        Assert.That(entitySetUpdateLog.Count, Is.EqualTo(2));
        var operations = entitySetUpdateLog.ToList();

        Assert.That(operations[0], Is.InstanceOf<EntitySetItemRemoveOperation>());
        var delFirstBook = (EntitySetItemRemoveOperation) operations[0];
        Assert.That(delFirstBook.Key, Is.EqualTo(author.Key));
        Assert.That(delFirstBook.Field, Is.EqualTo(author.TypeInfo.Fields[nameof(Author.Books)]));
        Assert.That(delFirstBook.ItemKey, Is.EqualTo(book1.Key));

        Assert.That(operations[1], Is.InstanceOf<EntitySetItemRemoveOperation>());
        var delSecondBook = (EntitySetItemRemoveOperation) operations[1];
        Assert.That(delSecondBook.Key, Is.EqualTo(author.Key));
        Assert.That(delSecondBook.Field, Is.EqualTo(author.TypeInfo.Fields[nameof(Author.Books)]));
        Assert.That(delSecondBook.ItemKey, Is.EqualTo(book2.Key));

        tx.Complete();
      }
    }

    [Test]
    [TestCase(OperationLogType.SystemOperationLog)]
    [TestCase(OperationLogType.OutermostOperationLog)]
    public void EntitySetClearTest(OperationLogType logType)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Book book1, book2;
        Author author;
        using (session.Operations.DisableSystemOperationRegistration()) {
          book1 = new Book(session, 100) { Title = "100 white pages" };
          book2 = new Book(session, 200) { Title = "200 white pages" };
          author = new Author(session, "The 60s", "Author") { DateOfBirth = new DateTime(1952, 8, 22) };
          _ = author.Books.Add(book1);
          _ = author.Books.Add(book2);
        }

        var entitySetUpdateLog = new OperationLog(logType);
        using (var capturer = OperationCapturer.Attach(session, entitySetUpdateLog)) {
          author.Books.Clear();
        }

        Assert.That(entitySetUpdateLog.Count, Is.EqualTo(1));
        var first = entitySetUpdateLog.First();

        Assert.That(first, Is.InstanceOf<EntitySetClearOperation>());
        var clear = (EntitySetClearOperation) first;
        Assert.That(clear.Key, Is.EqualTo(author.Key));
        Assert.That(clear.Field, Is.EqualTo(author.TypeInfo.Fields[nameof(Author.Books)]));

        tx.Complete();
      }
    }
  }
}