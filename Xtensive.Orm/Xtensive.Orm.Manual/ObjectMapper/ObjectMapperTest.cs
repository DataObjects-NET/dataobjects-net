// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.28

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.ObjectMapping;
using Xtensive.ObjectMapping.Model;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Disconnected;
using Xtensive.Orm.ObjectMapping;

namespace Xtensive.Orm.Manual.ObjectMapper
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

    public Author(Session session)
      : base(session)
    {}
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

    public Book(Session session)
      : base(session)
    {}
  }

  public enum OrderPriority
  {
    Normal,
    High,
    Low
  }

  [Serializable]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public OrderPriority Priority { get; set; }
    
    [Field]
    public Customer Customer { get; set; }

    [Field]
    [Association(PairTo = "Order", OnOwnerRemove = OnRemoveAction.Cascade,
      OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<OrderItem> Items { get; set; }

    public Order(Session session)
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class OrderItem : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Order Order { get; set; }

    [Field]
    public string ProductName { get; set; }

    [Field]
    public double Price { get; set; }

    public OrderItem(Session session)
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public Address Address { get; set; }

    public Customer(Session session)
      : base(session)
    {}
  }

  public class Address : Structure
  {
    [Field]
    public string State { get; set; }

    [Field]
    public string City { get; set; }

    [Field]
    public string Street { get; set; }

    [Field]
    public int Building { get; set; }

    [Field]
    public int Office { get; set; }

    public Address(Session session)
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class Trunk : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string ProjectName { get; set; }

    public Trunk(Session session) 
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class Branch : Entity
  {
    [Key(0), Field]
    public int Id { get; private set; }

    [Key(1), Field]
    public Trunk Trunk { get; private set; }

    [Field]
    public DateTime CreationDate { get; set; }


    // Constructors

    public Branch(Session session, int id, Trunk trunk)
      : base(session, id, trunk)
    {}
  }

  #endregion

  #region DTO

  [Serializable]
  public class IdentifiableDto
  {
    public string Key {get; set; }
  }

  [Serializable]
  public class UserDto : IdentifiableDto
  {
    public string Name { get;  set; }

    public List<WebPageDto> FavoritePages { get; set; }

    public WebPageDto PersonalPage { get; set; }

    public BlogPostDto[] BlogPosts { get; set; }

    public HashSet<UserDto> Friends { get; set; }
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
    IHasBinaryVersion
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

  [Serializable]
  public class OrderDto : IdentifiableDto
  {
    public CustomerDto Customer { get; set; }

    public OrderPriority Priority { get; set; }

    public List<OrderItemDto> Items { get; set; }
  }

  [Serializable]
  public class OrderItemDto : IdentifiableDto
  {
    public OrderDto Order { get; set; }

    public string ProductName { get; set; }

    public double Price { get; set; }
  }

  [Serializable]
  public class CustomerDto : IdentifiableDto
  {
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public AddressDto Address { get; set; }
  }

  [Serializable]
  public struct AddressDto
  {
    public string State { get; set; }

    public string City { get; set; }

    public string Street { get; set; }

    public int Building { get; set; }

    public int Office { get; set; }
  }

  [Serializable]
  public class ExtendedOrderDto : IdentifiableDto
  {
    public double AvgItemPrice { get; set; }

    public OrderPriority Priority { get; set; }

    public CustomerDto Customer { get; set; }

    public List<OrderItemDto> Items { get; set; }
  }

  [Serializable]
  public class TrunkDto : IdentifiableDto
  {
    public string ProjectName { get; set; }
  }

  [Serializable]
  public class BranchDto : IdentifiableDto
  {
    public int Id { get; set; }

    public TrunkDto Trunk { get; set; }

    public DateTime CreationDate { get; set; }
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
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var user = new User(session);
        var firstPost = new BlogPost(session) {Title = "First post"};
        user.BlogPosts.Add(firstPost);
        var secondPost = new BlogPost(session) {Title = "Second post", Author = user};
        user.BlogPosts.Add(secondPost);
        user.PersonalPage = new WebPage(session) {Title = "Xtensive company", Url = "http://www.x-tensive.com"};

        // The mapping decription is immutable and can be used multiple times
        var mapper = new Mapper(session, mapping);
        userDto = (UserDto) mapper.Transform(user);
        tx.Complete();
      }

      // The original DTO graph have to be preserved to calculate changes
      var originalUserDto = Clone(userDto);

      // Modify the DTO graph

      // For a new object we must provide the key that is unique among all object keys 
      // in both original and modified graphs
      var newFriendDto = new UserDto {Key = Guid.NewGuid().ToString()};
      newFriendDto.Name = "John Smith";
      newFriendDto.PersonalPage = new WebPageDto {
        Key = Guid.NewGuid().ToString(), Title = "DataObjects.Net", Url = "http://www.dataobjects.net"
      };
      userDto.Friends.Add(newFriendDto);
      userDto.PersonalPage.Title = "New title";

      // Replace the object
      var index = Array.FindIndex(userDto.BlogPosts, 0, userDto.BlogPosts.Length,
        post => post.Title=="First post");
      userDto.BlogPosts[index] = new BlogPostDto {
        Key = Guid.NewGuid().ToString(), Title = "Third post", Author = userDto
      };

      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var mapper = new Mapper(session, mapping);
        // Compare original and modified graphs
        using (var comparisonResult = mapper.Compare(originalUserDto, userDto)) {
          // Apply found changes to domain model objects
          comparisonResult.Operations.Replay(session);
          // The property KeyMapping provides the mapping from simulated keys of 
          // new objects in the graph to real keys of these objects in the storage
          var newFreind = session.Query.Single<User>(Key.Parse(domain, (string) comparisonResult.KeyMapping[newFriendDto.Key]));
        }
        tx.Complete();
      }

      ValidateMainTest(domain, userDto, newFriendDto);
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

      // Validate version before applying changes
      using (var session = domain.OpenSession()) {
        var mapper = new Mapper(session, mapping);
        using (var comparisonResult = mapper.Compare(originalAuthorDto, authorDto))
        using (VersionValidator.Attach(session, comparisonResult.VersionInfoProvider))
        using (var tx = session.OpenTransaction()) {
          comparisonResult.Operations.Replay(session);
          tx.Complete();
        }
      }

      var staleAuthorDto = Clone(authorDto);
      staleAuthorDto.Books[0]=originalAuthorDto.Books[0];
      staleAuthorDto.Name = "Eugene";
      
      // Trying to use the stale object
      using (var session = domain.OpenSession()) {
        var mapper = new Mapper(session, mapping);
        using (var comparisonResult = mapper.Compare(originalAuthorDto, staleAuthorDto))
        using (VersionValidator.Attach(session, comparisonResult.VersionInfoProvider)) {
          var wasThrown = false;
          try {
            using (var tx = session.OpenTransaction()) {
              comparisonResult.Operations.Replay(session);
              tx.Complete();
            }
          }
          catch (VersionConflictException) {
            wasThrown = true;
          }
          Assert.IsTrue(wasThrown);
        }
      }

      ValidateOptimisticOfflineLockTest(domain, authorDto);
    }

    [Test]
    public void StructureTest()
    {
      var mapping = new MappingBuilder()
        .MapType<Entity, IdentifiableDto, string>(e => e.Key.Format(), dto => dto.Key)
          .Inherit<IdentifiableDto, Customer, CustomerDto>()
          .Inherit<IdentifiableDto, Order, OrderDto>()
          .Inherit<IdentifiableDto, OrderItem, OrderItemDto>()
        // The mapping for structure requires the target must be a structure,
        // but it doesn't require a key extractor
        .MapStructure<Address, AddressDto>()
        .Build();

      var domain = BuildDomain();

      var orderDto = GetOrderDto(mapping, domain);
      var originalOrderDto = Clone(orderDto);

      orderDto.Priority = OrderPriority.High;
      // Modify the structure
      orderDto.Customer.Address = new AddressDto {
        State = orderDto.Customer.Address.State, City = "Moscow", Street = orderDto.Customer.Address.Street,
        Building = 10, Office = orderDto.Customer.Address.Office
      };

      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var mapper = new Mapper(session, mapping);
        using (var comparisonResult = mapper.Compare(originalOrderDto, orderDto))
          comparisonResult.Operations.Replay(session);
        tx.Complete();
      }

      ValidateStructureTest(domain, orderDto);
    }

    [Test]
    public void AdvancedMappingTest()
    {
      var mapping = new MappingBuilder()
        .MapType<Entity, IdentifiableDto, string>(e => e.Key.Format(), dto => dto.Key)
          .Inherit<IdentifiableDto, Order, ExtendedOrderDto>()
            // Specify custom converter, the change tracking will be disabled for this property
            .MapProperty(o => o.Items.Average(item => item.Price), o => o.AvgItemPrice)
            // Exclude the property of DTO from mapping
            .IgnoreProperty(o => o.Items)
          .Inherit<IdentifiableDto, Customer, CustomerDto>()
            // Disable change tracking for the property
            .TrackChanges(c => c.FirstName, false)
            .TrackChanges(c => c.Address, false)
        .MapStructure<Address, AddressDto>()
        .Build();

      var domain = BuildDomain();

      List<object> orderDtos;
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var customer = new Customer(session) {
          FirstName = "First", LastName = "Customer", Address = new Address (session) {
            State = "Russia", City = "Yekaterinburg", Street = "Nagornaya", Building = 12, Office = 316
          }
        };
        var order0 = new Order(session) {Priority = OrderPriority.High, Customer = customer};
        order0.Items.Add(new OrderItem(session) {Price = 1.5, ProductName = "Product0"});
        order0.Items.Add(new OrderItem(session) {Price = 3, ProductName = "Product1"});
        var order1 = new Order(session) {Priority = OrderPriority.Normal, Customer = customer};
        order1.Items.Add(new OrderItem(session) {Price = 1.5, ProductName = "Product0"});
        order1.Items.Add(new OrderItem(session) {Price = 10, ProductName = "Product2"});
        var orders = new HashSet<Order> {order0, order1};
        var mapper = new Mapper(session, mapping);
        // The graph root can be a descendant of ICollection<T>
        orderDtos = (List<object>) mapper.Transform(orders);
        tx.Complete();
      }

      var originalOrderDtos = Clone(orderDtos);
      var orderDto0 = (ExtendedOrderDto) orderDtos[0];
      orderDto0.Priority += 1;
      orderDto0.Customer.FirstName = "New";
      orderDto0.Customer.LastName = "Smith";
      Assert.IsNull(orderDto0.Items);
      var orderDto1 = (ExtendedOrderDto) orderDtos[1];
      orderDto1.Priority += 1;
      orderDto1.AvgItemPrice += 1;
      orderDto1.Customer.Address = new AddressDto {
        State = "State", City = "City", Street = "Street", Building = 10, Office = 20
      };
      Assert.AreSame(orderDto0.Customer, orderDto1.Customer);

      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var mapper = new Mapper(session, mapping);
        using (var comparisonResult = mapper.Compare(originalOrderDtos, orderDtos)) {
          comparisonResult.Operations.Replay(session);
          var order0 = session.Query.Single<Order>(Key.Parse(domain, orderDto0.Key));
          Assert.AreEqual(orderDto0.Priority, order0.Priority);
          Assert.AreNotEqual(orderDto0.Customer.FirstName, order0.Customer.FirstName);
          Assert.AreEqual("First", order0.Customer.FirstName);
          Assert.AreEqual(orderDto0.Customer.LastName, order0.Customer.LastName);
          Assert.AreEqual(2, order0.Items.Count);
          var order1 = session.Query.Single<Order>(Key.Parse(domain, orderDto1.Key));
          Assert.AreEqual(orderDto1.Priority, order1.Priority);
          Assert.AreEqual(orderDto1.Customer.Key, order0.Customer.Key.Format());
          Assert.AreEqual(2, order0.Items.Count);
          Assert.AreNotEqual(orderDto1.Customer.Address.State, order1.Customer.Address.State);
          Assert.AreNotEqual(orderDto1.Customer.Address.Building, order1.Customer.Address.Building);
          Assert.AreEqual("Russia", order1.Customer.Address.State);
          Assert.AreEqual(12, order1.Customer.Address.Building);
          tx.Complete();
        }
      }
    }

    [Test]
    public void CustomKeyGenerationTest()
    {
      var mapping = new MappingBuilder()
        .MapType<Entity, IdentifiableDto, string>(e => e.Key.Format(), i => i.Key)
          .Inherit<IdentifiableDto, Trunk, TrunkDto>()
          // Specify the delegate providing key field values
          .Inherit<IdentifiableDto, Branch, BranchDto>(b => new object[] {b.Id, b.Trunk})
            // Change tracking for properties included in the key has to be disabled
            .TrackChanges(b => b.Id, false)
            .TrackChanges(b => b.Trunk, false)
        .Build();

      var domain = BuildDomain();

      var trunkDto = new TrunkDto {Key = Guid.NewGuid().ToString(), ProjectName = "DataObjects.Net"};
      var branchDto = new BranchDto {
        Key = Guid.NewGuid().ToString(), Id = 1, Trunk = trunkDto, CreationDate = DateTime.Now.AddDays(-30)
      };

      IDictionary<object,object> newObjectKeys;
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var mapper = new Mapper(session, mapping);
        using (var comparisonResult = mapper.Compare(null, branchDto)) {
          comparisonResult.Operations.Replay(session);
          newObjectKeys = comparisonResult.KeyMapping;
        }
        tx.Complete();
      }

      ValidateCustomKeyGenerationTest(domain, branchDto, newObjectKeys);
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
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author = new Author(session) {Name = "Jack"};
        author.Books.Add(new Book(session) {Title = "The greatest book"});
        author.Books.Add(new Book(session) {Title = "The another greatest book"});

        // The mapping decription is immutable and can be used multiple times
        var mapper = new Mapper(session, mapping);
        result = (AuthorDto) mapper.Transform(author);
        tx.Complete();
      }
      return result;
    }

    private static OrderDto GetOrderDto(MappingDescription mapping, Domain domain)
    {
      OrderDto orderDto;
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var customer = new Customer(session) {
          FirstName = "John", LastName = "Smith", Address = new Address (session) {
            State = "Russia", City = "Yekaterinburg", Street = "Nagornaya", Building = 12, Office = 316
          }
        };
        var orderItem = new OrderItem(session) {Price = 99.99, ProductName = "Product"};
        var order = new Order(session) {Customer = customer, Priority = OrderPriority.Normal};
        order.Items.Add(orderItem);
        var mapper = new Mapper(session, mapping);
        orderDto = (OrderDto) mapper.Transform(order);
        tx.Complete();
      }
      return orderDto;
    }

    private static void ValidateMainTest(Domain domain, UserDto userDto, UserDto newFreindDto)
    {
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var user = session.Query.Single<User>(Key.Parse(domain, userDto.Key));
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
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author = session.Query.Single<Author>(Key.Parse(domain, authorDto.Key));
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

    private static void ValidateStructureTest(Domain domain, OrderDto orderDto)
    {
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var order = session.Query.Single<Order>(Key.Parse(domain, orderDto.Key));
        Assert.AreEqual(orderDto.Priority, order.Priority);
        var orderItemDto = orderDto.Items.Single();
        var orderItem = order.Items.Single();
        Assert.AreEqual(orderItemDto.Price, orderItem.Price);
        Assert.AreEqual(orderItemDto.ProductName, orderItem.ProductName);
        Assert.AreSame(orderItemDto.Order, orderDto);
        Assert.AreEqual(orderItemDto.Order.Key, orderItem.Order.Key.Format());
        var customer = order.Customer;
        var customerDto = orderDto.Customer;
        Assert.AreEqual(customerDto.Key, customer.Key.Format());
        Assert.AreEqual(customerDto.FirstName, customer.FirstName);
        Assert.AreEqual(customerDto.LastName, customer.LastName);
        Assert.AreEqual(customerDto.Address.Building, customer.Address.Building);
        Assert.AreEqual(customerDto.Address.City, customer.Address.City);
        Assert.AreEqual(customerDto.Address.Office, customer.Address.Office);
        Assert.AreEqual(customerDto.Address.State, customer.Address.State);
        Assert.AreEqual(customerDto.Address.Street, customer.Address.Street);
      }
    }

    private static void ValidateCustomKeyGenerationTest(Domain domain, BranchDto branchDto, IDictionary<object, object> newObjectKeys)
    {
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var branch = session.Query.Single<Branch>(Key.Parse(domain, (string) newObjectKeys[branchDto.Key]));
        Assert.AreEqual(Key.Parse(domain, (string) newObjectKeys[branchDto.Trunk.Key]), branch.Trunk.Key);
        Assert.AreEqual(branchDto.Id, branch.Id);
        Assert.AreEqual(branchDto.CreationDate.ToString(), branch.CreationDate.ToString());
        Assert.AreEqual(branchDto.Trunk.ProjectName, branch.Trunk.ProjectName);
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