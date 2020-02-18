// Copyright (C) 2020 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0793_FieldValidationTriggersLazyLoadFieldsFetch : AutoBuildTest
  {
    private class QueryCounter
    {
      public int Count { get; private set; }

      public Disposable Attach(Session session)
      {
        session.Events.DbCommandExecuted += Encount;
        return new Disposable((disposing) => session.Events.DbCommandExecuted -= Encount);
      }

      public void Reset()
      {
        Count = 0;
      }

      private void Encount(object sender, DbCommandEventArgs eventArgs)
      {
        Count++;
      }

      public QueryCounter()
      {
        Count = 0;
      }
    }

    private Key oblomovKey;
    private Key goncharovKey;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Book).Assembly, typeof (Book).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var author = new Author() { FirstName = "Ivan", LastName = "Goncharov", Biography = "Some biography of Ivan Alexandrovich" };
        var book = new Book() {
          Title = "Oblomov",
          Description = "A drama story of how human lazyness and absence of strenght within may cause his life.",
          BookFile = new byte[] { 3, 3, 3, 3, 3, 3, 3 },
          Author = author
        };

        new Chapter() { Title = "Chapter #1", Description = "Detailed description of Chapter #1", Owner = book };
        new Chapter() { Title = "Chapter #2", Description = "Detailed description of Chapter #2", Owner = book };
        new Chapter() { Title = "Chapter #3", Description = "Detailed description of Chapter #3", Owner = book };

        oblomovKey = book.Key;
        goncharovKey = author.Key;

        transaction.Complete();
      }
    }

    [Test]
    public void LazyFieldHasNoConstraintTest()
    {
      using (var session = Domain.OpenSession()) {
        var counter = new QueryCounter();
        using (counter.Attach(session)) {
          using (var transaction = session.OpenTransaction()) {
            var oblomov = session.Query.Single<Book>(oblomovKey);
            oblomov.Title = "Oblomov by Goncharov";
            transaction.Complete();
          }
        }
        Assert.That(counter.Count, Is.EqualTo(2));
        counter.Reset();
      }
    }

    [Test]
    public void LazyFieldHasCheckAlwaysConstraintTest()
    {
      using (var session = Domain.OpenSession()) {
        var counter = new QueryCounter();
        using (counter.Attach(session)) {
          using (var transaction = session.OpenTransaction()) {
            var goncharov = session.Query.Single<Author>(goncharovKey);
            goncharov.FirstName = goncharov.FirstName + "modified"; // should prefetch lazy load field
            transaction.Complete();
          }
        }
        Assert.That(counter.Count, Is.EqualTo(3));
        counter.Reset();
      }
    }

    [Test]
    public void LazyFieldHasCheckIfModifiedConstraintTest()
    {
      using (var session = Domain.OpenSession()) {
        var counter = new QueryCounter();
        using (counter.Attach(session)) {
          using (var transaction = session.OpenTransaction()) {
            foreach (var chapter in session.Query.All<Chapter>()) {
              chapter.Title = chapter.Title + " modified";
            }
            transaction.Complete();
          }
          Assert.That(counter.Count, Is.EqualTo(2));
          counter.Reset();
        }
      }

      using (var session = Domain.OpenSession()) {
        var counter = new QueryCounter();
        using (counter.Attach(session)) {
          int updatedItems = 0;
          using (var transaction = session.OpenTransaction()) {
            foreach(var chapter in session.Query.All<Chapter>()) {
              chapter.Description = chapter.Description + " modified";
              updatedItems++;
            }
            transaction.Complete();
          }
          Assert.That(counter.Count, Is.EqualTo(5)); // query + fetches + update
          counter.Reset();
        }
      }
    }
  }
}
