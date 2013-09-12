// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.23

using System;
using NUnit.Framework;
using Xtensive.Testing;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Manual.Transactions.NestedTransactions
{
  #region Model

  [Serializable]
  [HierarchyRoot]
  public class User : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Name { get;  set; }

    [Field]
    public EntitySet<WebPage> FavoritePages { get; private set; }

    [Field]
    [Association(
      OnOwnerRemove = OnRemoveAction.Cascade,
      OnTargetRemove = OnRemoveAction.Clear)]
    public WebPage PersonalPage { get; set; }

    [Field]
    [Association(PairTo = "Author")]
    public EntitySet<BlogPost> BlogPosts { get; private set; }

    [Field]
    [Association(PairTo = "Friends")]
    public EntitySet<User> Friends { get; private set; }

    public void RemoveAndCancel()
    {
      using (Session.OpenTransaction(TransactionOpenMode.New)) {
        Remove();
        throw new InvalidOperationException("Cancelled.");
      }
    }

    public User(Session session)
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class WebPage : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Title { get; set; }

    [Field(Length = 200)]
    public string Url { get; set; }

    public WebPage(Session session) 
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class BlogPost : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 100)]
    public string Title { get; set; }

    [Field(Length = 1000)]
    public string Content { get; set; }

    [Field]
    public User Author { get; set;}

    public BlogPost(Session session)
      : base(session)
    {}
  }

  #endregion

  [TestFixture]
  public class NestedTransactionsTest
  {
    [Test]
    public void MainTest()
    {
      // Creatign new Domain configuration
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests") {
        UpgradeMode = DomainUpgradeMode.Recreate
      };
      // Modifying it by registering the persistent types
      config.Types.Register(typeof(User).Assembly, typeof(User).Namespace);
      // And finally building the domain
      var domain = Domain.Build(config);

      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {

          // Creating user
          var dmitri = new User (session) {
            Name = "Dmitri"
          };

          // Modifying the entity
          dmitri.Name = "Dmitri Maximov";

          // Opening new nested transaction
          using (var nestedScope = session.OpenTransaction(TransactionOpenMode.New)) {
            // Removing the entity
            dmitri.Remove();
            Assert.IsTrue(dmitri.IsRemoved);
            AssertEx.Throws<InvalidOperationException>(() => {
              var dmitryName = dmitri.Name;
            });
            // No nestedScope.Complete(), so nested transaction will be rolled back
          }

          // Transparent Entity state update
          Assert.IsFalse(dmitri.IsRemoved);
          Assert.AreEqual("Dmitri Maximov", dmitri.Name);

          // Repeating the same, but using transactional method
          AssertEx.Throws<InvalidOperationException>(dmitri.RemoveAndCancel);

          // Transparent Entity state update
          Assert.IsFalse(dmitri.IsRemoved);
          Assert.AreEqual("Dmitri Maximov", dmitri.Name);
          
          // Marking the transaction scope as completed to commit it 
          transactionScope.Complete();
        }
      }
    }
  }
}