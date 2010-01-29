// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.28

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Core.ObjectMapping;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Disconnected;
using Xtensive.Storage.ObjectMapping;

namespace Xtensive.Storage.Manual.ObjectMapper
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
  }

  [Serializable]
  [HierarchyRoot]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Author", OnOwnerRemove = OnRemoveAction.Clear,
      OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Book> Books { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Author Author { get; set; }

    [Field]
    public string Title { get; set; }
  }

  #endregion

  #region DTO

  [Serializable]
  public abstract class IdentifiableDto
  {
    public string Key {get; set; }
  }

  [Serializable]
  public class UserDto : IdentifiableDto
  {
    public string Name { get;  set; }

    public List<WebPageDto> FavoritePages { get; set; }

    public WebPageDto PersonalPage { get; set; }

    public BlogPostDto[] BlogPosts { get; private set; }

    public HashSet<UserDto> Friends { get; private set; }
  }

  [Serializable]
  public class WebPageDto : IdentifiableDto
  {
    public string Title { get; set; }

    public string Url { get; set; }
  }

  [Serializable]
  public class BlogPostDto : IdentifiableDto
  {
    public string Title { get; set; }

    public string Content { get; set; }

    public UserDto Author { get; set;}
  }

  [Serializable]
  public class VersionDto : IdentifiableDto,
    IHasVersion
  {
    public byte[] Version { get; set; }
  }

  [Serializable]
  public class AuthorDto : VersionDto
  {
    public string Name { get; set; }

    public List<BookDto> Books { get; set; }
  }

  [Serializable]
  public class BookDto : VersionDto
  {
    public AuthorDto Author { get; set; }

    public string Title { get; set; }
  }

  #endregion

  [TestFixture]
  public sealed class ObjectMapperTest
  {
    private readonly BinaryFormatter formatter = new BinaryFormatter();

    [Test]
    public void MainTest()
    {
      // The key type must be string
      var mapping = new MappingBuilder()
        .MapType<Entity, IdentifiableDto, string>(m => m.Key.Format(), d => d.Key)
          .Inherit<IdentifiableDto, User, UserDto>()
          .Inherit<IdentifiableDto, WebPage, WebPageDto>()
          .Inherit<IdentifiableDto, BlogPost, BlogPostDto>()
        .Build();

      var domain = BuildDomain();

      UserDto userDto;
      using (var session = Session.Open(domain))
      using (var tx = Transaction.Open()) {
        var user = new User();
        var firstPost = new BlogPost {Title = "First post"};
        user.BlogPosts.Add(firstPost);
        var secondPost = new BlogPost {Title = "Second post", Author = user};
        user.BlogPosts.Add(secondPost);
        user.PersonalPage = new WebPage {Title = "Xtensive company", Url = "http://www.x-tensive.com"};

        // The mapping decription is immutable and can be used multiple times
        var mapper = new Mapper(mapping);
        userDto = (UserDto) mapper.Transform(user);
        tx.Complete();
      }

      // The original DTO graph have to be preserved to calculate changes
      var originalUserDto = Clone(userDto);

      // Modify the DTO graph

      // For a new object we must provide the key that is unique among all object keys 
      // in both of original and modified graphs
      var newFreindDto = new UserDto {Key = Guid.NewGuid().ToString()};
      newFreindDto.Name = "John Smith";
      newFreindDto.PersonalPage = new WebPageDto {
        Key = Guid.NewGuid().ToString(), Title = "DataObjects.Net", Url = "http://www.dataobjects.net"
      };
      userDto.Friends.Add(newFreindDto);
      userDto.PersonalPage.Title = "New title";

      // Replace the object
      var index = Array.FindIndex(userDto.BlogPosts, 0, userDto.BlogPosts.Length,
        post => post.Title=="First post");
      userDto.BlogPosts[index] = new BlogPostDto {
        Key = Guid.NewGuid().ToString(), Title = "Third post", Author = userDto
      };

      using (var session = Session.Open(domain))
      using (var tx = Transaction.Open()) {
        var mapper = new Mapper(mapping);
        // Compare original and modified graphs
        using (var comparisonResult = mapper.Compare(originalUserDto, userDto)) {
          // Apply found changes to domain model objects
          comparisonResult.Operations.Apply();
        }
        tx.Complete();
      }

      // Validation
      ValidateMainTest(domain, userDto, newFreindDto);
    }

    [Test]
    public void OptimisticOfflineLockTest()
    {
      var mapping = new MappingBuilder()
        .MapType<Entity, VersionDto, string>(m => m.Key.Format(), d => d.Key)
          // Specify custom converter for the property Version
          .MapProperty(e => SerializeVersionInfo(e.VersionInfo), v => v.Version)
          .Inherit<VersionDto, Author, AuthorDto>()
          .Inherit<VersionDto, Book, BookDto>()
        .Build();

      var domain = BuildDomain();

      var authorDto = GetAuthorDto(domain, mapping);
      var originalAuthorDto = Clone(authorDto);
      authorDto.Books[0] = new BookDto {
        Key = Guid.NewGuid().ToString(), Title = "The new greatest book.", Author = authorDto
      };
      authorDto.Name = "John";

      var mapper = new Mapper(mapping);

      // Validate version before applying changes
      using (var session = Session.Open(domain))
      using (var comparisonResult = mapper.Compare(originalAuthorDto, authorDto))
      using (VersionValidator.Attach(session, comparisonResult.VersionInfoProvider))
      using (var tx = Transaction.Open()) {
        comparisonResult.Operations.Apply();
        tx.Complete();
      }

      var staleAuthorDto = Clone(authorDto);
      staleAuthorDto.Books[0]=originalAuthorDto.Books[0];
      staleAuthorDto.Name = "Eugene";
      
      // Trying to use the stale object
      using (var session = Session.Open(domain))
      using (var comparisonResult = mapper.Compare(originalAuthorDto, staleAuthorDto))
      using (VersionValidator.Attach(session, comparisonResult.VersionInfoProvider)) {
        var tx = Transaction.Open();
        comparisonResult.Operations.Apply();
        tx.Complete();
        var wasThrown = false;
        try {
          tx.Dispose();
        }
        catch(InvalidOperationException) {
          wasThrown = true;
        }
        Assert.IsTrue(wasThrown);
      }

      ValidateOptimisticOfflineLockTest(domain, authorDto);
    }

    private byte[] SerializeVersionInfo(VersionInfo versionInfo)
    {
      // This isn't the best practice, it's just an example
      using (var stream = new MemoryStream()) {
        formatter.Serialize(stream, versionInfo);
        return stream.GetBuffer();
      }
    }

    private static AuthorDto GetAuthorDto(Domain domain, MappingDescription mapping)
    {
      AuthorDto result;
      using (var session = Session.Open(domain))
      using (var tx = Transaction.Open()) {
        var author = new Author {Name = "Jack"};
        author.Books.Add(new Book {Title = "The greatest book"});
        author.Books.Add(new Book {Title = "The another greatest book"});

        // The mapping decription is immutable and can be used multiple times
        var mapper = new Mapper(mapping);
        result = (AuthorDto) mapper.Transform(author);
        tx.Complete();
      }
      return result;
    }

    private static void ValidateMainTest(Domain domain, UserDto userDto, UserDto newFreindDto)
    {
      using (var session = Session.Open(domain))
      using (var tx = Transaction.Open()) {
        var user = Query.Single<User>(Key.Parse(domain, userDto.Key));
        Assert.AreEqual(userDto.PersonalPage.Title, user.PersonalPage.Title);
        var friend = user.Friends.Single();
        Assert.AreEqual(newFreindDto.Name, friend.Name);
        Assert.AreEqual(newFreindDto.PersonalPage.Title, friend.PersonalPage.Title);
        Assert.AreEqual(newFreindDto.PersonalPage.Url, friend.PersonalPage.Url);
        Assert.AreEqual(2, user.BlogPosts.Count);
        var secondPost = user.BlogPosts.Where(post => post.Title=="Second post").Single();
        Assert.AreEqual(secondPost.Author, user);
        var thirdPost = user.BlogPosts.Where(post => post.Title=="Third post").Single();
        Assert.AreEqual(thirdPost.Author, user);
      }
    }

    private static void ValidateOptimisticOfflineLockTest(Domain domain, AuthorDto authorDto)
    {
      using (var session = Session.Open(domain))
      using (var tx = Transaction.Open()) {
        var author = Query.Single<Author>(Key.Parse(authorDto.Key));
        Assert.AreEqual(authorDto.Name, author.Name);
        Assert.AreEqual(2, author.Books.Count);
        var firstBookDto = authorDto.Books[0];
        var firstBook = author.Books.Where(book => book.Title==firstBookDto.Title).Single();
        var secondBookDto = authorDto.Books[1];
        var secondBook = author.Books.Where(book => book.Title==secondBookDto.Title).Single();
        Assert.AreEqual(firstBookDto.Title, firstBook.Title);
        Assert.AreEqual(firstBookDto.Author.Key, author.Key.Format());
        Assert.AreEqual(secondBookDto.Title, secondBook.Title);
        Assert.AreEqual(secondBookDto.Author.Key, author.Key.Format());
      }
    }

    private static Domain BuildDomain()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests") {
        UpgradeMode = DomainUpgradeMode.Recreate
      };
      config.Types.Register(typeof(User).Assembly, typeof(User).Namespace);
      return Domain.Build(config);
    }

    private T Clone<T>(T original)
    {
      // This isn't the best practice, it's just an example
      using (var stream = new MemoryStream()) {
        formatter.Serialize(stream, original);
        stream.Seek(0, SeekOrigin.Begin);
        return (T) formatter.Deserialize(stream);
      }
    }
  }
}