// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.23

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Manual.Intro.CheatSheet
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
  public class CheatSheetTest
  {
    [Test]
    public void MainTest()
    {
      // Creatign new Domain configuration
      var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      // Modifying it by registering the persistent types
      config.Types.Register(typeof(User).Assembly, typeof(User).Namespace);
      // And finally building the domain
      var domain = Domain.Build(config);

      Key dmitriKey;
      int dmitriId;
      string dmitriKeyString;

      // Opening Session
      using (var session = domain.OpenSession()) {

        // Opening transaction
        using (var transactionScope = session.OpenTransaction()) {

          // Creating user
          var dmitri = new User(session) {
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
        using (var transactionScope = session.OpenTransaction()) {
          // Parses the serialized key
          var anotherDimtriKey = Key.Parse(session.Domain, dmitriKeyString);
          // Keys are equal
          Assert.AreEqual(dmitriKey, anotherDimtriKey);

          // Materialization on fetch
          var dmitri = session.Query.Single<User>(dmitriKey);
          // Alternative way to do the same
          var anotherDmitri = session.Query.SingleOrDefault<User>(dmitriKey);
          Assert.AreSame(dmitri, anotherDmitri);
          // Fetching by key value(s)
          anotherDmitri = session.Query.Single<User>(dmitriId);
          Assert.AreSame(dmitri, anotherDmitri);

          // Querying the storage using regular LINQ query
          var query =
            from user in session.Query.All<User>()
            where user.Name=="Dmitri"
            select user;
          Assert.AreSame(dmitri, query.First());

          // Querying the storage using compiled query
          anotherDmitri = session.Query.Execute(qe => // Default caching key is methodof( () => ... )
            from user in session.Query.All<User>()
            where user.Name=="Dmitri"
            select user).First();
          Assert.AreSame(dmitri, anotherDmitri);

          // Querying the storage using compiled future scalar query
          var delayedDmitry1 = session.Query.ExecuteDelayed(qe => (
            from user in qe.All<User>()
            where user.Name=="Dmitri"
            select user
            ).FirstOrDefault());
          var delayedDmitry2 = session.Query.ExecuteDelayed(qe => (
            from user in qe.All<User>()
            where user.Id==dmitriId
            select user
            ).First());
          Assert.AreSame(dmitri, delayedDmitry1.Value); // Both queries are executed at once here
          Assert.AreSame(dmitri, delayedDmitry2.Value);

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
          
          // Creating few more objects
          var xtensiveWebPage = new WebPage (session) {
            Title = "Xtensive Web Site", 
            Url = "http://www.x-tensive.com"
          };
          var alexYakuninBlogPage = new WebPage (session) {
            Title = "Alex Yakunin's Blog", 
            Url = "http://blog.alexyakunin.com"
          };
          var subsonicPage = new WebPage (session) {
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
          using (var nestedScope = session.OpenTransaction(TransactionOpenMode.New)) {
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
  }
}