// Copyright (C) 2020-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2020.02.18

using System;
using System.Collections.Generic;
using System.Text;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Validation;
using Xtensive.Orm.Tests.Issues.IssueJira0793_FieldValidationTriggersLazyLoadFieldsFetchModel;
using NUnit.Framework;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Issues.IssueJira0793_FieldValidationTriggersLazyLoadFieldsFetchModel
{
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Length = 50), NotNullOrEmptyConstraint]
    public string Title { get; set; }

    [Field(Length = int.MaxValue, LazyLoad = true)]
    public string Description { get; set; }

    [Field(LazyLoad = true)]
    public byte[] BookFile { get; set; }

    [Field(Length = 5)]
    public string FileExtension { get; set; }

    [Field]
    public Author Author { get; set; }
  }

  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field()]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field(Length = int.MaxValue, LazyLoad = true)]
    [NotNullConstraint]// should always be triggered
    public string Biography { get; set; }
  }

  [HierarchyRoot]
  public class Chapter : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Book Owner { get; set; }

    [Field]
    public string Title { get; set; }

    [Field(Length = int.MaxValue, LazyLoad = true)]
    [NotNullOrEmptyConstraint(ValidateOnlyIfModified = true)]// should validate only if changed
    public string Description { get; set; }
  }

  public class QueryCounter
  {
    public int SelectCount { get; private set; }

    public int UpdateCount { get; private set; }

    public int OverallCount => SelectCount + UpdateCount;

    public Disposable Attach(Session session)
    {
      session.Events.DbCommandExecuted += Encount;
      return new Disposable((disposing) => session.Events.DbCommandExecuted -= Encount);
    }

    public void Reset() => SelectCount = UpdateCount = 0;

    private void Encount(object sender, DbCommandEventArgs eventArgs)
    {
      var text = eventArgs.Command.CommandText.Substring(0, 30);
      if (text.Contains("SELECT", StringComparison.OrdinalIgnoreCase)) {
        SelectCount++;
      }
      else if (text.Contains("UPDATE", StringComparison.OrdinalIgnoreCase)) {
        UpdateCount++;
      }
      else {
        throw new ArgumentOutOfRangeException("eventArgs.Command.CommandText");
      }
    }

    public QueryCounter()
    {
      Reset();
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0793_FieldValidationTriggersLazyLoadFieldsFetch : AutoBuildTest
  {
    private Key oblomovKey;
    private Key goncharovKey;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Sessions[WellKnown.Sessions.Default].BatchSize = 1;
      return configuration;
    }

    protected override void PopulateData()
    {
      var sessionConfig = new SessionConfiguration(SessionOptions.Default | SessionOptions.AutoActivation);

      using (var session = Domain.OpenSession(sessionConfig))
      using (var transaction = session.OpenTransaction()) {
        var author = new Author() {
          FirstName = "Ivan",
          LastName = "Goncharov",
          Biography = "Some biography of Ivan Alexandrovich" };
        var book = new Book() {
          Title = "Oblomov",
          Description = "A drama about how human's lazyness and absence of strenght may affect his life.",
          BookFile = new byte[] { 3, 3, 3, 3, 3, 3, 3 },
          Author = author
        };

        _ = new Chapter() { Title = "Chapter #1", Description = "Detailed description of Chapter #1", Owner = book };
        _ = new Chapter() { Title = "Chapter #2", Description = "Detailed description of Chapter #2", Owner = book };
        _ = new Chapter() { Title = "Chapter #3", Description = "Detailed description of Chapter #3", Owner = book };

        oblomovKey = book.Key;
        goncharovKey = author.Key;

        transaction.Complete();
      }
    }

    [Test]
    public void LazyFieldHasNoConstraintTest()
    {
      var counter = new QueryCounter();
      using (var session = Domain.OpenSession()) {
        using (counter.Attach(session))
        using (var transaction = session.OpenTransaction()) {
          var oblomov = session.Query.Single<Book>(oblomovKey);
          oblomov.Title = "Oblomov by Goncharov";
          transaction.Complete();
        }
        Assert.That(counter.OverallCount, Is.EqualTo(2));
        Assert.That(counter.SelectCount, Is.EqualTo(1));
        Assert.That(counter.UpdateCount, Is.EqualTo(1));
        counter.Reset();
      }
    }

    [Test]
    public void LazyFieldHasCheckAlwaysConstraintTest()
    {
      var counter = new QueryCounter();
      using (var session = Domain.OpenSession()) {
        using (counter.Attach(session))
        using (var transaction = session.OpenTransaction()) {
          var goncharov = session.Query.Single<Author>(goncharovKey);
          goncharov.FirstName = goncharov.FirstName + "modified"; // should prefetch lazy load field
          transaction.Complete();
        }
        Assert.That(counter.OverallCount, Is.EqualTo(3));
        Assert.That(counter.SelectCount, Is.EqualTo(2)); // 1 select + 1 fetch
        Assert.That(counter.UpdateCount, Is.EqualTo(1));
        counter.Reset();
      }
    }

    [Test]
    public void LazyFieldHasCheckIfModifiedConstraintTest()
    {
      var counter = new QueryCounter();
      using (var session = Domain.OpenSession()) {
        using (counter.Attach(session))
        using (var transaction = session.OpenTransaction()) {
          foreach (var chapter in session.Query.All<Chapter>()) {
            chapter.Title = chapter.Title + " modified";
          }
          transaction.Complete();
        }

        Assert.That(counter.OverallCount, Is.EqualTo(4));
        Assert.That(counter.SelectCount, Is.EqualTo(1)); // select items only, no fetch
        Assert.That(counter.UpdateCount, Is.EqualTo(3));

        counter.Reset();
      }

      using (var session = Domain.OpenSession()) {
        var updatedItems = 0;
        using (counter.Attach(session))
        using (var transaction = session.OpenTransaction()) {
          foreach(var chapter in session.Query.All<Chapter>()) {
            chapter.Description = chapter.Description + " modified";
            updatedItems++;
          }
          transaction.Complete();
        }

        Assert.That(counter.OverallCount, Is.EqualTo(7));
        Assert.That(counter.SelectCount, Is.EqualTo(4)); // query + fetches
        Assert.That(counter.UpdateCount, Is.EqualTo(3)); // updates

        counter.Reset();
      }
    }
  }
}
