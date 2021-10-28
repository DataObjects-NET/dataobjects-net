// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueGithub0048_CoalesceReturnsWrongResultsModel;

namespace Xtensive.Orm.Tests.Issues.IssueGithub0048_CoalesceReturnsWrongResultsModel
{
  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public AuthorName FullName { get; set; }

    [Field]
    public AuthorName NoFullName { get; set; }

    [Field(Nullable = true, Length = 50)]
    public string ShortDescription { get; set; }

    [Field(Nullable = true, Length = 100, LazyLoad = true)]
    public string LongDescription { get; set; }

    [Field]
    [Association(PairTo = nameof(Book.Author))]
    public EntitySet<Book> Books { get; set; }
  }

  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 25)]
    public string Title { get; set; }

    [Field]
    public Author Author { get; set; }

    [Field]
    public Author NoAuthor { get; set; }

    [Field]
    public int? NumberOfPages { get; set; }

    [Field]
    public DateTime? PublishDate { get; set; }

    [Field]
    public EntitySet<Review> Reviews { get; private set; }
  }

  [HierarchyRoot]
  public class Review : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [Association(PairTo = nameof(Book.Reviews))]
    public Book ReviewedBook { get; set; }

    [Field]
    public ReviewAuthor ReviewAuthor { get; set; }

    [Field]
    public byte Rating { get; set; }
  }

  [HierarchyRoot]
  public class ReviewAuthor : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 20)]
    public string Nickname { get; set; }
  }

  public class AuthorName : Structure
  {
    [Field(Length = 20)]
    public string FirstName { get; set; }

    [Field(Length = 20)]
    public string LastName { get; set; }

    [Field(Length = 20)]
    public string MidName { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueGithub0048_CoalesceReturnsWrongResults : AutoBuildTest
  {
    private int nullAuthorId;
    private int nullReviewAuthorId;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(AuthorName).Assembly, typeof(AuthorName).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullAuthor = new Author() {
          FullName = new AuthorName() { FirstName = "Null", LastName = "Null" },
          NoFullName = new AuthorName() { FirstName = "No FirstName", LastName = "No LastName" }
        };
        var nullBook = new Book() { NoAuthor = nullAuthor };
        var nullReviewAuthor = new ReviewAuthor() { Nickname = "Null" };
        nullAuthorId = nullAuthor.Id;
        nullReviewAuthorId = nullReviewAuthor.Id;

        var tolstoy = new Author() {
          FullName = new AuthorName { FirstName = "Lev", LastName = "Tolstoy" },
          NoFullName = new AuthorName() { FirstName = "No FirstName", LastName = "No LastName" }
        };
        var bookInfo = new (string name, Review review)[] {
          ("War and Peace", new Review() { ReviewAuthor = new ReviewAuthor()}),
          ( "Anna Karenina", new Review() { ReviewAuthor = new ReviewAuthor() } ),
          ( "The Death of Ivan Ilyich", new Review() { ReviewAuthor = nullReviewAuthor } ) };
        foreach (var item in bookInfo) {
          var b = new Book() { Title = item.name, NoAuthor = nullAuthor };
          _ = b.Reviews.Add(item.review);
          _ = tolstoy.Books.Add(b);
        }

        var chekhov = new Author() {
          FullName = new AuthorName { FirstName = "Anton", LastName = "Chekhov" },
          NoFullName = new AuthorName() { FirstName = "No FirstName", LastName = "No LastName" }
        };

        bookInfo = new (string name, Review review)[] {
          ( "The Seagull", new Review() { ReviewAuthor = new ReviewAuthor()}),
          ( "Three Sisters", new Review() { ReviewAuthor = new ReviewAuthor() } ),
          ( "The Cherry Orchard", new Review() { ReviewAuthor = nullReviewAuthor } ) };
        foreach(var item in bookInfo) {
          var b = new Book() { Title = item.name, NoAuthor = nullAuthor };
          _ = b.Reviews.Add(item.review);
          _ = chekhov.Books.Add(b);
        }

        tx.Complete();
      }
    }

    #region Coalesce

    [Test]
    public void CoalesceWithNullObjectEntityTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullAuthorInstance = session.Query.All<Author>().FirstOrDefault(a => a.Id == nullAuthorId);

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(book => book.Author ?? nullAuthorInstance)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .ToList(7);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Book>()
          .Select(book => book.Author ?? nullAuthorInstance)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Coalesce expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    public void CoalesceWithNullObjectEntityTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullAuthorInstance = session.Query.All<Author>().FirstOrDefault(a => a.Id == nullAuthorId);

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(book => nullAuthorInstance ?? book.Author)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .ToList(7);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Book>()
          .Select(book => nullAuthorInstance ?? book.Author)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Coalesce expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    public void CoalesceWithTwoEntityFieldsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(book => book.Author ?? book.NoAuthor)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .ToList(7);

        var storageResult = session.Query.All<Book>()
          .Select(book => book.Author ?? book.NoAuthor)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .ToList(7);

        Assert.That(localResult.SequenceEqual(storageResult), Is.True);
      }
    }

    [Test]
    public void CoalesceComplexEntityTest1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullAuthorInstance = session.Query.All<Author>().FirstOrDefault(a => a.Id == nullAuthorId);

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(a => a.Author ?? a.NoAuthor ?? nullAuthorInstance)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .ToList(7);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Book>()
          .Select(a => a.Author ?? a.NoAuthor ?? nullAuthorInstance)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Coalesce expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    public void CoalesceComplexEntityTest2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullAuthorInstance = session.Query.All<Author>().FirstOrDefault(a => a.Id == nullAuthorId);

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(a => a.Author ?? nullAuthorInstance ?? a.NoAuthor)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .ToList(7);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Book>()
          .Select(a => a.Author ?? nullAuthorInstance ?? a.NoAuthor)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Coalesce expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    public void CoalesceWithNullConstantTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var nullAuthor = session.Query.All<Author>().First(a => a.Id == nullAuthorId);

        var storageResult = session.Query.All<Book>()
          .Select(book => book.Author ?? null)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .ToList(7);

        Assert.That(storageResult.Count, Is.EqualTo(6));
        Assert.That(storageResult.Contains(nullAuthor), Is.False);

        storageResult = session.Query.All<Book>()
          .Select(book => book.Author ?? null)
          .Where(a => a != null)
          .OrderBy(a => a.FullName.FirstName)
          .ToList(7);

        Assert.That(storageResult.Count, Is.EqualTo(6));
        Assert.That(storageResult.Contains(nullAuthor), Is.False);

        storageResult = session.Query.All<Book>()
          .Select(book => book.Author ?? null)
          .Where(a => a == null)
          .OrderBy(a => a.FullName.FirstName)
          .ToList(7);

        Assert.That(storageResult.Count, Is.EqualTo(1));
        Assert.That(storageResult.Contains(null), Is.True);
      }
    }

    [Test]
    public void CoalesceWithQueryAsSourceTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullReviewAuthor = session.Query.All<ReviewAuthor>().FirstOrDefault(a => a.Id == nullReviewAuthorId);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Book>()
          .Select(b => (b.Reviews.SingleOrDefault().ReviewAuthor ?? nullReviewAuthor))
          .Where(e => e != nullReviewAuthor).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Coalesce expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    public void CoalesceWithNullObjectStructureTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullFullNameInstance = new AuthorName();

        var localResult = session.Query.All<Author>()
          .AsEnumerable()
          .Select(a => a.FullName ?? nullFullNameInstance)
          .Where(a => a.FirstName.Length > 0)
          .ToList(7);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Author>()
          .Select(a => a.FullName ?? nullFullNameInstance)
          .Where(a => a.FirstName.Length > 0).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Coalesce expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    public void CoalesceWithNullObjectStructureTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullFullNameInstance = new AuthorName();

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Author>()
          .Select(a => nullFullNameInstance ?? a.FullName)
          .Where(a => a.FirstName.Length > 0).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Coalesce expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    public void CoalesceComplexStructureTest1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullFullNameInstance = new AuthorName();

        var localResult = session.Query.All<Author>()
          .AsEnumerable()
          .Select(a => a.FullName ?? a.NoFullName ?? nullFullNameInstance)
          .Where(a => a.FirstName.Length > 0)
          .ToList(7);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Author>()
          .Select(a => a.FullName ?? a.NoFullName ?? nullFullNameInstance)
          .Where(a => a.FirstName.Length > 0).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Coalesce expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    public void CoalesceComplexStructureTest2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullFullNameInstance = new AuthorName();

        var localResult = session.Query.All<Author>()
          .AsEnumerable()
          .Select(a => a.FullName ?? nullFullNameInstance ?? a.NoFullName)
          .Where(a => a.FirstName.Length > 0)
          .ToList(7);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Author>()
          .Select(a => a.FullName ?? nullFullNameInstance ?? a.NoFullName)
          .Where(a => a.FirstName.Length > 0).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Coalesce expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    public void CoalesceWithNumericFieldTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullNumberOfPages = 0;

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(b => b.NumberOfPages ?? nullNumberOfPages)
          .Where(pn => pn >= 0)
          .ToList(7)
          .OrderBy(pn => pn);

        var storageResult = session.Query.All<Book>()
          .Select(b => b.NumberOfPages ?? nullNumberOfPages)
          .Where(pn => pn >= 0)
          .ToList(7)
          .OrderBy(pn => pn);

        Assert.That(localResult.SequenceEqual(storageResult), Is.True);
      }
    }

    [Test]
    public void CoalesceWithNumericFieldTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Nullable<int> nullNumberOfPages = 0;

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(b => nullNumberOfPages ?? b.NumberOfPages)
          .Where(pn => pn >= 0)
          .ToList(7)
          .OrderBy(pn => pn);

        var storageResult = session.Query.All<Book>()
          .Select(b => nullNumberOfPages ?? b.NumberOfPages)
          .Where(pn => pn >= 0)
          .ToList(7)
          .OrderBy(pn => pn);

        Assert.That(localResult.SequenceEqual(storageResult), Is.True);
      }
    }

    [Test]
    public void CoalesceWithDateTimeFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullPublishDate = new DateTime(1900,1,1);

        var expectedSequence = session.Query.All<Book>()
          .AsEnumerable()
          .Select(a => a.PublishDate ?? nullPublishDate)
          .Where(d => d.Month > 2)
          .OrderBy(d => d)
          .ToList(7);

        var resultSequence = session.Query.All<Book>()
          .Select(a => a.PublishDate ?? nullPublishDate)
          .Where(d => d.Month > 2)
          .OrderBy(d => d)
          .ToList(7);

        Assert.That(expectedSequence.SequenceEqual(resultSequence), Is.True);
      }
    }

    [Test]
    public void CoalesceWithStringFieldTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullTitle = "No title";

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(b => b.Title ?? nullTitle)
          .Where(t => t.Contains("N") || t.Contains("n"))
          .ToList(7)
          .OrderBy(t => t);

        var storageResult = session.Query.All<Book>()
          .Select(b => b.Title ?? nullTitle)
          .Where(t => t.Contains("N") || t.Contains("n"))
          .ToList(7)
          .OrderBy(t => t);

        Assert.That(localResult.SequenceEqual(storageResult), Is.True);
      }
    }

    [Test]
    public void CoalesceWithStringFieldTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullTitle = "No title";

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(b => nullTitle ?? b.Title)
          .Where(t => t.Contains("N") || t.Contains("n"))
          .ToList(7)
          .OrderBy(t => t);

        var storageResult = session.Query.All<Book>()
          .Select(b => nullTitle ?? b.Title)
          .Where(t => t.Contains("N"))
          .ToList(7)
          .OrderBy(t => t);

        Assert.That(localResult.SequenceEqual(storageResult), Is.True);
      }
    }

    #endregion

    #region Ternary operator

    [Test]
    [SuppressMessage("Style", "IDE0029:Use coalesce expression", Justification = "Test suppose to have ternary operator, not coalesce one")]
    public void TernaryWithNullObjectEntityTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullAuthorInstance = session.Query.All<Author>().FirstOrDefault(a => a.Id == nullAuthorId);

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(book => book.Author != null ? book.Author : nullAuthorInstance)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .ToList(7);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Book>()
          .Select(book => book.Author != null ? book.Author : nullAuthorInstance)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Conditional expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    [SuppressMessage("Style", "IDE0029:Use coalesce expression", Justification = "Test suppose to have ternary operator, not coalesce one")]
    public void TernaryWithNullObjectEntityTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullAuthorInstance = session.Query.All<Author>().FirstOrDefault(a => a.Id == nullAuthorId);

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(book => book.Author == null ? nullAuthorInstance : book.Author)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .ToList(7);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Book>()
          .Select(book => book.Author == null ? nullAuthorInstance : book.Author)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Conditional expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    public void TernaryWithTwoEntityFieldsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(book => book.Author != null ? book.Author : book.NoAuthor)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .ToList(7);

        var storageResult = session.Query.All<Book>()
          .Select(book => book.Author != null ? book.Author : book.NoAuthor)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .ToList(7);

        Assert.That(localResult.SequenceEqual(storageResult), Is.True);
      }
    }

    [Test]
    public void TernaryComplexEntityTest1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullAuthorInstance = session.Query.All<Author>().FirstOrDefault(a => a.Id == nullAuthorId);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Book>()
          .Select(book => book.Author.Id > 0
                          ? (book.Author.Id > 10)
                            ? book.Author
                            : book.NoAuthor
                          : nullAuthorInstance)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Conditional expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    public void TernaryComplexEntityTest2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullAuthorInstance = session.Query.All<Author>().FirstOrDefault(a => a.Id == nullAuthorId);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Book>()
          .Select(book => book.Author.Id > 0
                          ? (book.Author.Id > 10)
                            ? book.Author
                            : nullAuthorInstance
                          : book.NoAuthor)
          .Where(a => a.FullName.FirstName != null)
          .OrderBy(a => a.FullName.FirstName)
          .Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Conditional expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    [SuppressMessage("Style", "IDE0029:Use coalesce expression", Justification = "Test suppose to have ternary operator, not coalesce one")]
    public void TernaryWithNullObjectStructureTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullFullNameInstance = new AuthorName();

        var localResult = session.Query.All<Author>()
          .AsEnumerable()
          .Select(a => a.FullName != null ? a.FullName : nullFullNameInstance)
          .Where(a => a.FirstName.Length > 0)
          .ToList(7);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Author>()
          .Select(a => a.FullName != null ? a.FullName : nullFullNameInstance)
          .Where(a => a.FirstName.Length > 0)
          .Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Conditional expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    [SuppressMessage("Style", "IDE0029:Use coalesce expression", Justification = "Test suppose to have ternary operator, not coalesce one")]
    public void TernaryWithNullObjectStructureTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullFullNameInstance = new AuthorName();

        var localResult = session.Query.All<Author>()
          .AsEnumerable()
          .Select(a => a.FullName == null ? nullFullNameInstance : a.FullName)
          .Where(a => a.FirstName.Length > 0)
          .ToList(7);

        var ex = Assert.Throws<QueryTranslationException>(() => session.Query.All<Author>()
          .Select(a => a.FullName == null ? nullFullNameInstance : a.FullName)
          .Where(a => a.FirstName.Length > 0)
          .Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Conditional expressions", StringComparison.Ordinal));
      }
    }

    [Test]
    public void TernaryWithTwoStructureFieldsTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullFullNameInstance = new AuthorName();

        var localResult = session.Query.All<Author>()
          .AsEnumerable()
          .Select(a => a.FullName != null ? a.FullName : a.NoFullName)
          .Where(a => a.FirstName.Length > 0)
          .ToList(7);

        var storageResult = session.Query.All<Author>()
          .Select(a => a.FullName != null ? a.FullName : a.NoFullName)
          .Where(a => a.FirstName.Length > 0)
          .ToList(7);
        Assert.That(localResult.SequenceEqual(storageResult), Is.True);
      }
    }

    [Test]
    public void TernaryWithTwoStructureFieldsTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullFullNameInstance = new AuthorName();

        var localResult = session.Query.All<Author>()
          .AsEnumerable()
          .Select(a => a.FullName == null ? a.NoFullName : a.FullName)
          .Where(a => a.FirstName.Length > 0)
          .ToList(7);

        var storageResult = session.Query.All<Author>()
          .Select(a => a.FullName == null ? a.NoFullName : a.FullName)
          .Where(a => a.FirstName.Length > 0)
          .ToList(7);
        Assert.That(localResult.SequenceEqual(storageResult), Is.True);
      }
    }

    [Test]
    public void TernaryWithNumericFieldTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullNumberOfPages = 0;

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(b => b.NumberOfPages != null ? b.NumberOfPages.Value : nullNumberOfPages)
          .Where(pn => pn >= 0)
          .ToList(7)
          .OrderBy(pn => pn);

        var storageResult = session.Query.All<Book>()
          .Select(b => b.NumberOfPages != null ? b.NumberOfPages.Value : nullNumberOfPages)
          .Where(pn => pn >= 0)
          .ToList(7)
          .OrderBy(pn => pn);

        Assert.That(localResult.SequenceEqual(storageResult), Is.True);
      }
    }

    [Test]
    public void TernaryWithNumericFieldTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullNumberOfPages = 0;

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(b => b.NumberOfPages == null ? nullNumberOfPages : b.NumberOfPages.Value)
          .Where(pn => pn >= 0)
          .ToList(7)
          .OrderBy(pn => pn);

        var storageResult = session.Query.All<Book>()
          .Select(b => b.NumberOfPages == null ? nullNumberOfPages : b.NumberOfPages.Value)
          .Where(pn => pn >= 0)
          .ToList(7)
          .OrderBy(pn => pn);

        Assert.That(localResult.SequenceEqual(storageResult), Is.True);
      }
    }

    [Test]
    public void TernaryWithDateTimeFieldTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullPublishDate = new DateTime(1900, 1, 1);

        var expectedSequence = session.Query.All<Book>()
          .AsEnumerable()
          .Select(a => a.PublishDate != null ? a.PublishDate.Value : nullPublishDate)
          .Where(d => d.Month > 2)
          .OrderBy(d => d)
          .ToList(7);

        var resultSequence = session.Query.All<Book>()
          .Select(a => a.PublishDate != null ? a.PublishDate.Value : nullPublishDate)
          .Where(d => d.Month > 2)
          .OrderBy(d => d)
          .ToList(7);

        Assert.That(expectedSequence.SequenceEqual(resultSequence), Is.True);//but it's false
      }
    }

    [Test]
    public void TernaryWithDateTimeFieldTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullPublishDate = new DateTime(1900, 1, 1);

        var expectedSequence = session.Query.All<Book>()
          .AsEnumerable()
          .Select(a => a.PublishDate == null ? nullPublishDate : a.PublishDate.Value)
          .Where(d => d.Month > 2)
          .OrderBy(d => d)
          .ToList(7);

        var resultSequence = session.Query.All<Book>()
          .Select(a => a.PublishDate == null ? nullPublishDate : a.PublishDate.Value)
          .Where(d => d.Month > 2)
          .OrderBy(d => d)
          .ToList(7);

        Assert.That(expectedSequence.SequenceEqual(resultSequence), Is.True);//but it's false
      }
    }

    [Test]
    [SuppressMessage("Style", "IDE0029:Use coalesce expression", Justification = "Test suppose to have ternary operator, not coalesce one")]
    public void TernaryWithStringFieldTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullTitle = "No title";

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(b => b.Title != null ? b.Title : nullTitle)
          .Where(t => t.Contains("N") || t.Contains("n"))
          .ToList(7)
          .OrderBy(t => t);

        var storageResult = session.Query.All<Book>()
          .Select(b => b.Title != null ? b.Title : nullTitle)
          .Where(t => t.Contains("N") || t.Contains("n"))
          .ToList(7)
          .OrderBy(t => t);

        Assert.That(localResult.SequenceEqual(storageResult), Is.True);
      }
    }

    [Test]
    [SuppressMessage("Style", "IDE0029:Use coalesce expression", Justification = "Test suppose to have ternary operator, not coalesce one")]
    public void TernaryWithStringFieldTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var nullTitle = "No title";

        var localResult = session.Query.All<Book>()
          .AsEnumerable()
          .Select(b => b.Title == null ? nullTitle : b.Title)
          .Where(t => t.Contains("N") || t.Contains("n"))
          .ToList(7)
          .OrderBy(t => t);

        var storageResult = session.Query.All<Book>()
          .Select(b => b.Title == null ? nullTitle : b.Title)
          .Where(t => t.Contains("N") || t.Contains("n"))
          .ToList(7)
          .OrderBy(t => t);

        Assert.That(localResult.SequenceEqual(storageResult), Is.True);
      }
    }

    #endregion
  }
}
