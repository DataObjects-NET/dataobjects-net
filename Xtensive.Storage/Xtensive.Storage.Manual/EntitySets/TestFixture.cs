// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.29

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Aspects;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Manual.EntitySets
{
  [TestFixture]
  public class TestFixture
  {
    [HierarchyRoot]
    public class User : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public Account Account { get; set; }

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

      [Field]
      [Association(PairTo = "Participants")]
      public EntitySet<Meeting> Meetings { get; private set; }

      [Transactional(TransactionOpenMode.New)]
      public void RemoveAndCancel()
      {
        Remove();
        throw new InvalidOperationException("Cancelled.");
      }
    }

    [HierarchyRoot]
    public class WebPage : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field(Length = 200)]
      public string Title { get; set; }

      [Field(Length = 200)]
      public string Url { get; set; }
    }

    [HierarchyRoot]
    public class Meeting : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public DateTime Date { get; set; }

      [Field(Length = 100)]
      public string Description { get; set; }

      [Field]
      public EntitySet<User> Participants { get; private set; }
    }

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
    }

    [HierarchyRoot]
    public class Account : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field, Association(PairTo = "Account", 
        OnOwnerRemove = OnRemoveAction.Deny, 
        OnTargetRemove = OnRemoveAction.Cascade)]
      public User User { get; set; }
    }

    [Test]
    public void OneToManyTest()
    {
      var domain = BuildDomain();
      using (Session.Open(domain)) {
        using (Transaction.Open()) {

          User user = new User();

          var firstPost = new BlogPost {Title = "First post"};
          user.BlogPosts.Add(firstPost);

          Assert.AreEqual(user, firstPost.Author);

          var secondPost = new BlogPost {Title = "Second post"};
          secondPost.Author = user;

          Assert.IsTrue(user.BlogPosts.Contains(secondPost));

          user.BlogPosts.Remove(secondPost);

          Assert.IsNull(secondPost.Author);
        }
      }
    }

    [Test]
    public void OneToOneTest()
    {
      var domain = BuildDomain();
      using (Session.Open(domain)) {
        using (Transaction.Open()) {

          User user = new User();
          Account account = new Account();
          user.Account = account;

          Assert.AreEqual(user, account.User);

          user.Account = null;
          Assert.IsNull(account.User);

          account.User = user;
          Assert.AreEqual(user, account.User);

          AssertEx.Throws<ReferentialIntegrityException>(account.Remove);

          user.Remove();
          Assert.IsTrue(account.IsRemoved);
        }
      }
    }

    [Test]
    public void EntitySetTest()
    {
      var domain = BuildDomain();
      using (Session.Open(domain)) {
        using (Transaction.Open()) {
          User user = new User {Name = "Alex"};

          WebPage xtensive = new WebPage {Title = "Xtensive company", Url = "http://www.x-tensive.com"};
          WebPage dataobjects = new WebPage {Title = "DataObjects.Net", Url = "http://www.dataobjects.net"};

          user.FavoritePages.Add(xtensive);
          user.FavoritePages.Add(dataobjects);

          foreach (var page in user.FavoritePages)
            Console.WriteLine("{0} ( {1} )", page.Title, page.Url);

          Assert.AreEqual(2, user.FavoritePages.Count);

          Assert.AreEqual(1, SelectPages(user).Count());

          user.FavoritePages.Add(xtensive);
          Assert.AreEqual(2, user.FavoritePages.Count);
        }
      }
    }

    [Test]
    public void BasicOperationsTest()
    {
      // Building the domain
      var domain = BuildDomain();

      Key dmitriKey;
      int dmitriId;
      string dmitriKeyString;

      // Opening Session
      using (Session.Open(domain)) {

        // Opening transaction
        using (var transactionScope = Transaction.Open()) {

          // Creating user
          var dmitri = new User {
            Name = "Dmitri"
          };
          
          // Storing entity key
          dmitriKey = dmitri.Key;
          dmitriKeyString = dmitriKey.Format();
          dmitriId = dmitri.Id;

          Console.WriteLine("Dmitri's Key (human readable): {0}", dmitriKey);
          Console.WriteLine("Dmitri's Key (serializable): {0}", dmitriKeyString);
          Console.WriteLine("Dmitri's Id: {0}", dmitriId);

          // Marking the transaction scope as completed to commit it 
          transactionScope.Complete();
        }

        // Opening another transaction
        using (var transactionScope = Transaction.Open()) {
          // Parses the serialized key
          var anotherDimtriKey = Key.Parse(Domain.Current, dmitriKeyString);
          // Keys are equal
          Assert.AreEqual(dmitriKey, anotherDimtriKey);

          // Materialization on fetch
          var dmitri = Query<User>.Single(dmitriKey);
          // Alternative way to do the same
          var anotherDmitri = Query<User>.SingleOrDefault(dmitriKey);
          Assert.AreSame(dmitri, anotherDmitri);
          // Fetching by key value(s)
          anotherDmitri = Query<User>.Single(dmitriId);
          Assert.AreSame(dmitri, anotherDmitri);

          // Querying the storage using regular LINQ query
          var query =
            from user in Query<User>.All
            where user.Name=="Dmitri"
            select user;
          Assert.AreSame(dmitri, query.First());

          // Querying the storage using compiled query
          anotherDmitri = Query.Execute(() => // Default caching key is methodof( () => ... )
            from user in Query<User>.All
            where user.Name=="Dmitri"
            select user).First();
          Assert.AreSame(dmitri, anotherDmitri);

          // Querying the storage using compiled future scalar query
          var delayedDmitry1 = Query.ExecuteFutureScalar(() => (
            from user in Query<User>.All
            where user.Name=="Dmitri"
            select user
            ).FirstOrDefault());
          var delayedDmitry2 = Query.ExecuteFutureScalar(() => (
            from user in Query<User>.All
            where user.Id==dmitriId
            select user
            ).First());
          Assert.AreSame(dmitri, delayedDmitry1.Value); // Both queries are executed at once here
          Assert.AreSame(dmitri, delayedDmitry2.Value);

          // Modifying the entity
          dmitri.Name = "Dmitri Maximov";

          // Opening new nested transaction
          using (var nestedScope = Transaction.Open(TransactionOpenMode.New)) {
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
          
          // Creating few more objects
          var xtensiveWebPage = new WebPage {
            Title = "Xtensive Web Site", 
            Url = "http://www.x-tensive.com"
          };
          var alexYakuninBlogPage = new WebPage {
            Title = "Alex Yakunin's Blog", 
            Url = "http://blog.alexyakunin.com"
          };
          var subsonicPage = new WebPage {
            Title = "SubSonic project page", 
            Url = "http://www.subsonicproject.com/"
          };

          // Adding the items to EntitySet
          dmitri.FavoritePages.Add(xtensiveWebPage);
          dmitri.FavoritePages.Add(alexYakuninBlogPage);
          dmitri.FavoritePages.Add(subsonicPage);

          // Removing the item from EntitySet
          dmitri.FavoritePages.Remove(subsonicPage);

          // Getting count of items in EntitySet
          Console.WriteLine("Dmitri's favorite page count: {0}", dmitri.FavoritePages.Count);
          Assert.AreEqual(2, dmitri.FavoritePages.Count);
          Assert.AreEqual(2, dmitri.FavoritePages.Count()); // The same, but by LINQ query

          // Enumerating EntitySet
          foreach (var page in dmitri.FavoritePages)
            Console.WriteLine("Dmitri's favorite page: {0} ({1})", page.Title, page.Url);

          // Checking for the containment
          Assert.IsTrue(dmitri.FavoritePages.Contains(xtensiveWebPage));
          Assert.IsTrue(dmitri.FavoritePages.Contains(alexYakuninBlogPage));
          Assert.IsFalse(dmitri.FavoritePages.Contains(subsonicPage));

          // Opening new nested transaction
          using (var nestedScope = Transaction.Open(TransactionOpenMode.New)) {
            // Clearing the EntitySet
            dmitri.FavoritePages.Clear();
            Assert.IsFalse(dmitri.FavoritePages.Contains(xtensiveWebPage));
            Assert.IsFalse(dmitri.FavoritePages.Contains(alexYakuninBlogPage));
            Assert.IsFalse(dmitri.FavoritePages.Contains(subsonicPage));
            Assert.AreEqual(0, dmitri.FavoritePages.Count);
            Assert.AreEqual(0, dmitri.FavoritePages.Count()); // By query
            // No nestedScope.Complete(), so nested transaction will be rolled back
          }

          // Transparent EntitySet state update
          Assert.IsTrue(dmitri.FavoritePages.Contains(xtensiveWebPage));
          Assert.IsTrue(dmitri.FavoritePages.Contains(alexYakuninBlogPage));
          Assert.IsFalse(dmitri.FavoritePages.Contains(subsonicPage));
          Assert.AreEqual(2, dmitri.FavoritePages.Count);
          Assert.AreEqual(2, dmitri.FavoritePages.Count()); // The same, but by LINQ query

          // Finally, let's query the EntitySet:
          
          // Query construction
          var dmitryFavoriteBlogs =
            from page in dmitri.FavoritePages
            where page.Url.ToLower().Contains("blog")
            select page;
          
          // Query execution
          var dmitryFavoriteBlogList = dmitryFavoriteBlogs.ToList();

          // Printing the results
          Console.WriteLine("Dmitri's favorite blog count: {0}", dmitryFavoriteBlogList.Count);
          foreach (var page in dmitryFavoriteBlogList)
            Console.WriteLine("Dmitri's favorite blog: {0} ({1})", page.Title, page.Url);

          Assert.IsTrue(dmitryFavoriteBlogList.Contains(alexYakuninBlogPage));
          Assert.IsFalse(dmitryFavoriteBlogList.Contains(xtensiveWebPage));
          Assert.AreEqual(1, dmitryFavoriteBlogList.Count);
          
          // Marking the transaction scope as completed to commit it 
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void NestedTransactionTest()
    {
      // Building the domain
      var domain = BuildDomain();

      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {

          // Creating user
          var dmitri = new User {
            Name = "Dmitri"
          };

          // Modifying the entity
          dmitri.Name = "Dmitri Maximov";

          // Opening new nested transaction
          using (var nestedScope = Transaction.Open(TransactionOpenMode.New)) {
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
          AssertEx.Throws<InvalidOperationException>(() => {
            dmitri.RemoveAndCancel();
          });

          // Transparent Entity state update
          Assert.IsFalse(dmitri.IsRemoved);
          Assert.AreEqual("Dmitri Maximov", dmitri.Name);
          
          // Marking the transaction scope as completed to commit it 
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public IEnumerable<string> SelectPages(User user)
    {
      return
        from page in user.FavoritePages
        where page.Url.StartsWith("http://www.x-tensive.com")
        select page.Title;
    }

    [Test]
    public void MainTest()
    {
      BuildDomain();
    }

    private Domain BuildDomain()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(User).Assembly, typeof(User).Namespace);
      // var domain = Domain.Build(config);
      return Domain.Build(config);
    }
  }
}