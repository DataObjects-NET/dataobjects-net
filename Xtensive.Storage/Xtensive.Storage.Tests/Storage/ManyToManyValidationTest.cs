// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.10

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Storage.ManyToManyValidationTest_Model;
using Xtensive.Integrity.Validation;

namespace Xtensive.Storage.Tests.Storage
{
  namespace ManyToManyValidationTest_Model
  {
    public interface IHasValidationCount
    {
      int ValidationCount { get; } 
    }

    [HierarchyRoot]
    public class Book : Entity, 
      IHasValidationCount
    {
      public int ValidationCount { get; set; }

      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Title { get; set; }

      [Field]
      public EntitySetValidatedByOwner<Author> Authors { get; private set; }

      protected override void OnValidate()
      {
        ValidationCount++;
      }
    }

    [HierarchyRoot]
    public class Author: Entity, 
      IHasValidationCount
    {
      public int ValidationCount { get; set; }

      [Field, Key]
      public int Id { get;  private set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public EntitySetValidatedByOwner<Book> Books { get; private set; }

      protected override void OnValidate()
      {
        ValidationCount++;
      }
    }

    public class EntitySetValidatedByOwner<T> : EntitySet<T>,
      IHasValidationCount
      where T: IEntity
    {
      public int ValidationCount { get; set; }

      protected override void OnValidate()
      {
        ValidationCount++;
        Owner.Validate();
      }

      protected EntitySetValidatedByOwner(Entity owner, FieldInfo field)
        : base(owner, field)
      {
      }

      protected EntitySetValidatedByOwner(SerializationInfo info, StreamingContext context)
        : base(info, context)
      {
      }
    }
  }

  [Serializable]
  public class ManyToManyValidationTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Author).Assembly, typeof (Author).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      Author author;
      Book book;
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        author = new Author {Name = "Vasya Pupkin"};
        book = new Book {Title = "Mathematics"};

        EnsureIsValidated(() => {
          book.Authors.Add(author);
        }, book, book.Authors);

        EnsureIsValidated(() => {
          book.Authors.Remove(author);
        }, book, book.Authors);

        EnsureIsValidated(() => {
          book.Authors.Clear();
        }, book, book.Authors);

        tx.Complete();
      }
    }

    private void EnsureIsValidated(Action action, params IHasValidationCount[] checkList)
    {
      var originalValidationCounts = (
        from o in checkList
        select o.ValidationCount
        ).ToArray();
      action.Invoke();
      for (int i = 0; i < originalValidationCounts.Length; i++) {
        var originalValidationCount = originalValidationCounts[i];
        Assert.AreNotEqual(originalValidationCounts[i], checkList[i].ValidationCount);
      }
    }
  }
}