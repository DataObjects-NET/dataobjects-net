// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.10

using System;
using System.Linq;
using System.Runtime.Serialization;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.ManyToManyValidationTest_Model;

namespace Xtensive.Orm.Tests.Storage
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
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author = new Author {Name = "Vasya Pupkin"};
        var book = new Book {Title = "Mathematics"};

        EnsureIsValidated(session, () => book.Authors.Add(author), book, book.Authors);
        EnsureIsValidated(session, () => book.Authors.Remove(author), book, book.Authors);
        EnsureIsValidated(session, () => book.Authors.Clear(), book, book.Authors);

        tx.Complete();
      }
    }

    private void EnsureIsValidated(Session session, Action action, params IHasValidationCount[] checkList)
    {
      var originalValidationCounts = checkList.Select(o => o.ValidationCount).ToArray();
      action.Invoke();
      session.Validate();
      for (int i = 0; i < originalValidationCounts.Length; i++) {
        var originalValidationCount = originalValidationCounts[i];
        Assert.AreNotEqual(originalValidationCounts[i], checkList[i].ValidationCount);
      }
    }
  }
}