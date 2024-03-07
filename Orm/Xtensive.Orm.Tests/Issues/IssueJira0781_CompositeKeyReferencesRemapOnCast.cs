// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.12.16

using NUnit.Framework;
using System.Text;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0781_CompositeKeyReferencesRemapOnCastModel;
using System.Text.RegularExpressions;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests.Issues.IssueJira0781_CompositeKeyReferencesRemapOnCastModel
{
  public interface IProduct : IEntity
  {
    [Field]
    string Name { get; set; }

    [Field]
    CompositeKeyEntity CompositeKeyRef { get; set; }

    [Field]
    Guid UIndentifier { get; set; }

    [Field]
    TypeIdIncludedEntity TypeIdIncludedRef { get; set; }

    [Field]
    DateTime CreationDate { get; set; }
  }

  public interface IMedia : IEntity
  {
    [Field]
    string Name { get; set; }

    [Field]
    CompositeKeyEntity CompositeKeyRef { get; set; }

    [Field]
    Guid UIndentifier { get; set; }

    [Field]
    TypeIdIncludedEntity TypeIdIncludedRef { get; set; }

    [Field]
    DateTime CreationDate { get; set; }
  }

  public interface IUser : IEntity
  {
    [Field]
    string Name { get; set; }

    [Field]
    CompositeKeyEntity CompositeKeyRef { get; set; }

    [Field]
    Guid UIndentifier { get; set; }

    [Field]
    TypeIdIncludedEntity TypeIdIncludedRef { get; set; }

    [Field]
    DateTime CreationDate { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.ClassTable)]
  public class Product : Entity, IProduct
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public TypeIdIncludedEntity TypeIdIncludedRef { get; set; }

    [Field]
    public DateTime CreationDate { get; set; }

    [Field]
    public CompositeKeyEntity CompositeKeyRef { get; set; }

    [Field]
    public Guid UIndentifier { get; set; }
  }

  public class DerivedProduct : Product
  {
    [Field]
    public TimeSpan TimeSpan { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class Film : Entity, IMedia
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public CompositeKeyEntity CompositeKeyRef { get; set; }

    [Field]
    public Guid UIndentifier { get; set; }

    [Field]
    public TypeIdIncludedEntity TypeIdIncludedRef { get; set; }

    [Field]
    public DateTime CreationDate { get; set; }
  }

  public class DigitalSensor : Film
  {
    [Field]
    public TimeSpan TimeSpan { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.SingleTable)]
  public class User : Entity, IUser
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public CompositeKeyEntity CompositeKeyRef { get; set; }

    [Field]
    public Guid UIndentifier { get; set; }

    [Field]
    public TypeIdIncludedEntity TypeIdIncludedRef { get; set; }

    [Field]
    public DateTime CreationDate { get; set; }
  }

  public class AdminUser : User
  {
    [Field]
    public TimeSpan TimeSpan { get; set; }
  }


  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class CompositeKeyEntity : Entity
  {
    [Field(Length = 5), Key(0)]
    public string Id0 { get; private set; }

    [Field(Length = 5), Key(1)]
    public string Id1 { get; private set; }

    [Field(Length = 5), Key(2)]
    public string Id2 { get; private set; }

    [Field]
    public string Name { get; set; }

    public CompositeKeyEntity(string id0, string id1, string id2)
      : base(id0, id1, id2)
    {
    }
  }

  [HierarchyRoot(IncludeTypeId = true)]
  public class TypeIdIncludedEntity : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0781_CompositeKeyReferencesRemapOnCast : AutoBuildTest
  {
    private readonly Dictionary<Key, Key> productToComositeKeyMap = new Dictionary<Key, Key>();
    private readonly Dictionary<Key, Key> derivedProductToComositeKeyMap = new Dictionary<Key, Key>();

    private readonly Dictionary<Key, Key> productToIncludedTypeKeyMap = new Dictionary<Key, Key>();
    private readonly Dictionary<Key, Key> derivedProductToIncludedTypeKeyMap = new Dictionary<Key, Key>();

    private readonly Dictionary<Key, Key> filmToComositeKeyMap = new Dictionary<Key, Key>();
    private readonly Dictionary<Key, Key> dSensorToComositeKeyMap = new Dictionary<Key, Key>();

    private readonly Dictionary<Key, Key> filmToIncludedTypeKeyMap = new Dictionary<Key, Key>();
    private readonly Dictionary<Key, Key> dSensorToIncludedTypeKeyMap = new Dictionary<Key, Key>();

    private readonly Dictionary<Key, Key> userToComositeKeyMap = new Dictionary<Key, Key>();
    private readonly Dictionary<Key, Key> adminToComositeKeyMap = new Dictionary<Key, Key>();

    private readonly Dictionary<Key, Key> userToIncludedTypeKeyMap = new Dictionary<Key, Key>();
    private readonly Dictionary<Key, Key> adminToIncludedTypeKeyMap = new Dictionary<Key, Key>();

    private readonly Regex innerJoinFinder = new Regex("INNER JOIN");

    private string commandText;

    [TearDown]
    public void ClearTextForNextTest() => commandText = null;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Product).Assembly, typeof(Product).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var compositeKeyEntities = CreateCompositeKeyEntities();
        var typeIdIncludedEntities = CreateTypeIdIncludedEntities();

        CreateProducts(compositeKeyEntities, typeIdIncludedEntities);
        CreateMedia(compositeKeyEntities, typeIdIncludedEntities);
        CreateUsers(compositeKeyEntities, typeIdIncludedEntities);

        transaction.Complete();
      }
    }

    [Test]
    public void CastDirectImplementorToInterfaceTest1()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += CaptureCommand;
        var result = session.Query.All<Product>()
          .Cast<IProduct>()
          .Where(p => p.Name != null)
          .ToArray();
        session.Events.DbCommandExecuted -= CaptureCommand;

        Assert.That(result.Length, Is.EqualTo(10));

        foreach (var product in result.OrderBy(p => p.Name)) {
          Assert.That(product.CompositeKeyRef, Is.Not.Null);
          var compositeRefEntity = product.CompositeKeyRef;
          var typeIdIncludedEntity = product.TypeIdIncludedRef;

          if (product is DerivedProduct) {
            Assert.That(compositeRefEntity.Key, Is.EqualTo(derivedProductToComositeKeyMap[product.Key]));
            Assert.That(typeIdIncludedEntity.Key, Is.EqualTo(derivedProductToIncludedTypeKeyMap[product.Key]));
          }
          else {
            Assert.That(compositeRefEntity.Key, Is.EqualTo(productToComositeKeyMap[product.Key]));
            Assert.That(typeIdIncludedEntity.Key, Is.EqualTo(productToIncludedTypeKeyMap[product.Key]));
          }
        }

        Assert.That(innerJoinFinder.Matches(commandText).Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void CastDirectImplementorToInterfaceTest2()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += CaptureCommand;
        var result = session.Query.All<Film>()
          .Cast<IMedia>()
          .Where(p => p.Name != null)
          .ToArray();
        session.Events.DbCommandExecuted -= CaptureCommand;

        Assert.That(result.Length, Is.EqualTo(10));

        foreach (var media in result.OrderBy(p => p.Name)) {
          Assert.That(media.CompositeKeyRef, Is.Not.Null);
          var compositeRefEntity = media.CompositeKeyRef;
          var typeIdIncludedEntity = media.TypeIdIncludedRef;

          if (media is DigitalSensor) {
            Assert.That(compositeRefEntity.Key, Is.EqualTo(dSensorToComositeKeyMap[media.Key]));
            Assert.That(typeIdIncludedEntity.Key, Is.EqualTo(dSensorToIncludedTypeKeyMap[media.Key]));
          }
          else {
            Assert.That(compositeRefEntity.Key, Is.EqualTo(filmToComositeKeyMap[media.Key]));
            Assert.That(typeIdIncludedEntity.Key, Is.EqualTo(filmToIncludedTypeKeyMap[media.Key]));
          }
        }

        Assert.That(innerJoinFinder.Matches(commandText).Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void CastDirectImplementorToInterfaceTest3()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += CaptureCommand;
        var result = session.Query.All<User>()
          .Cast<IUser>()
          .Where(p => p.Name != null)
          .ToArray();
        session.Events.DbCommandExecuted -= CaptureCommand;

        Assert.That(result.Length, Is.EqualTo(10));

        foreach (var user in result.OrderBy(p => p.Name)) {
          Assert.That(user.CompositeKeyRef, Is.Not.Null);
          var compositeRefEntity = user.CompositeKeyRef;
          var typeIdIncludedEntity = user.TypeIdIncludedRef;

          if (user is AdminUser) {
            Assert.That(compositeRefEntity.Key, Is.EqualTo(adminToComositeKeyMap[user.Key]));
            Assert.That(typeIdIncludedEntity.Key, Is.EqualTo(adminToIncludedTypeKeyMap[user.Key]));
          }
          else {
            Assert.That(compositeRefEntity.Key, Is.EqualTo(userToComositeKeyMap[user.Key]));
            Assert.That(typeIdIncludedEntity.Key, Is.EqualTo(userToIncludedTypeKeyMap[user.Key]));
          }
        }

        Assert.That(innerJoinFinder.Matches(commandText).Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void CastIndirectImplementorToInterfaceTest1()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += CaptureCommand;
        var result = session.Query.All<DerivedProduct>()
          .Cast<IProduct>()
          .Where(p => p.Name != null)
          .ToArray();
        session.Events.DbCommandExecuted -= CaptureCommand;

        Assert.That(result.Length, Is.EqualTo(5));

        foreach (var product in result.OrderBy(p => p.Name)) {
          Assert.That(product.CompositeKeyRef, Is.Not.Null);
          var compositeRefEntity = product.CompositeKeyRef;
          var typeIdIncludedEntity = product.TypeIdIncludedRef;

          Assert.That(compositeRefEntity.Key, Is.EqualTo(derivedProductToComositeKeyMap[product.Key]));
          Assert.That(typeIdIncludedEntity.Key, Is.EqualTo(derivedProductToIncludedTypeKeyMap[product.Key]));
        }

        Assert.That(innerJoinFinder.Matches(commandText).Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void CastIndirectImplementorToInterfaceTest2()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += CaptureCommand;
        var result = session.Query.All<DigitalSensor>()
          .Cast<IMedia>()
          .Where(p => p.Name != null)
          .ToArray();
        session.Events.DbCommandExecuted -= CaptureCommand;

        Assert.That(result.Length, Is.EqualTo(5));

        foreach (var media in result.OrderBy(p => p.Name)) {
          Assert.That(media.CompositeKeyRef, Is.Not.Null);
          var compositeRefEntity = media.CompositeKeyRef;
          var typeIdIncludedEntity = media.TypeIdIncludedRef;

          Assert.That(compositeRefEntity.Key, Is.EqualTo(dSensorToComositeKeyMap[media.Key]));
          Assert.That(typeIdIncludedEntity.Key, Is.EqualTo(dSensorToIncludedTypeKeyMap[media.Key]));
        }

        Assert.That(innerJoinFinder.Matches(commandText).Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void CastIndirectImplementorToInterfaceTest3()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += CaptureCommand;
        var result = session.Query.All<AdminUser>()
          .Cast<IUser>()
          .Where(p => p.Name != null)
          .ToArray();
        session.Events.DbCommandExecuted -= CaptureCommand;

        Assert.That(result.Length, Is.EqualTo(5));

        foreach (var user in result.OrderBy(p => p.Name)) {
          Assert.That(user.CompositeKeyRef, Is.Not.Null);
          var compositeRefEntity = user.CompositeKeyRef;
          var typeIdIncludedEntity = user.TypeIdIncludedRef;

          Assert.That(compositeRefEntity.Key, Is.EqualTo(adminToComositeKeyMap[user.Key]));
          Assert.That(typeIdIncludedEntity.Key, Is.EqualTo(adminToIncludedTypeKeyMap[user.Key]));
        }

        Assert.That(innerJoinFinder.Matches(commandText).Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void CastToBaseClassTest1()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += CaptureCommand;
        var result = session.Query.All<DerivedProduct>()
          .Cast<Product>()
          .Where(p => p.Name != null)
          .ToArray();
        session.Events.DbCommandExecuted -= CaptureCommand;

        Assert.That(result.Length, Is.EqualTo(5));

        foreach (var product in result.OrderBy(p => p.Name)) {
          Assert.That(product.CompositeKeyRef, Is.Not.Null);
          var compositeRefEntity = product.CompositeKeyRef;
          var typeIdIncludedEntity = product.TypeIdIncludedRef;

          Assert.That(compositeRefEntity.Key, Is.EqualTo(derivedProductToComositeKeyMap[product.Key]));
          Assert.That(typeIdIncludedEntity.Key, Is.EqualTo(derivedProductToIncludedTypeKeyMap[product.Key]));
        }

        Assert.That(innerJoinFinder.Matches(commandText).Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void CastToBaseClassTest2()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += CaptureCommand;
        var simpleResult = session.Query.All<DigitalSensor>().Where(s => s.Name != null).ToArray();
        var result = session.Query.All<DigitalSensor>()
          .Cast<Film>()
          .Where(p => p.Name != null)
          .ToArray();
        session.Events.DbCommandExecuted -= CaptureCommand;

        Assert.That(result.Length, Is.EqualTo(5));

        foreach (var media in result.OrderBy(p => p.Name)) {
          Assert.That(media.CompositeKeyRef, Is.Not.Null);
          var compositeRefEntity = media.CompositeKeyRef;
          var typeIdIncludedEntity = media.TypeIdIncludedRef;

          Assert.That(compositeRefEntity.Key, Is.EqualTo(dSensorToComositeKeyMap[media.Key]));
          Assert.That(typeIdIncludedEntity.Key, Is.EqualTo(dSensorToIncludedTypeKeyMap[media.Key]));
        }

        Assert.That(innerJoinFinder.Matches(commandText).Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void CastToBaseClassTest3()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += CaptureCommand;
        var result = session.Query.All<AdminUser>()
          .Cast<User>()
          .Where(p => p.Name != null)
          .ToArray();
        session.Events.DbCommandExecuted -= CaptureCommand;

        Assert.That(result.Length, Is.EqualTo(5));

        foreach (var user in result.OrderBy(p => p.Name)) {
          Assert.That(user.CompositeKeyRef, Is.Not.Null);
          var compositeRefEntity = user.CompositeKeyRef;
          var typeIdIncludedEntity = user.TypeIdIncludedRef;

          Assert.That(compositeRefEntity.Key, Is.EqualTo(adminToComositeKeyMap[user.Key]));
          Assert.That(typeIdIncludedEntity.Key, Is.EqualTo(adminToIncludedTypeKeyMap[user.Key]));
        }

        Assert.That(innerJoinFinder.Matches(commandText).Count, Is.EqualTo(0));
      }
    }

    private void CaptureCommand(object sender, DbCommandEventArgs eventArgs)
      => commandText = eventArgs.Command.CommandText;

    private IList<CompositeKeyEntity> CreateCompositeKeyEntities()
    {
      return new List<CompositeKeyEntity> {
        new CompositeKeyEntity("1", "2", "3"),
        new CompositeKeyEntity("2", "3", "1"),
        new CompositeKeyEntity("3", "1", "2"),
        new CompositeKeyEntity("1", "3", "2"),
        new CompositeKeyEntity("1", "3", "3")
      };
    }

    private IList<TypeIdIncludedEntity> CreateTypeIdIncludedEntities()
    {
      return new List<TypeIdIncludedEntity> {
        new TypeIdIncludedEntity() { Name = "100" },
        new TypeIdIncludedEntity() { Name = "200" },
        new TypeIdIncludedEntity() { Name = "300" },
        new TypeIdIncludedEntity() { Name = "400" },
        new TypeIdIncludedEntity() { Name = "500" }
      };
    }

    private void CreateProducts(IList<CompositeKeyEntity> compositeKeyEntities, IList<TypeIdIncludedEntity> typeIdIncludedEntities)
    {
      var nameIndex = 0;
      var items = compositeKeyEntities.Zip(typeIdIncludedEntities, (c, t) => new { CompositeKeyEntity = c, TypeIdIncludedEntity = t });

      foreach (var item in items) {
        var product = new Product {
          Name = nameIndex.ToString("D2"),
          CreationDate = DateTime.UtcNow,
          UIndentifier = Guid.NewGuid(),
          CompositeKeyRef = item.CompositeKeyEntity,
          TypeIdIncludedRef = item.TypeIdIncludedEntity
        };
        productToComositeKeyMap.Add(product.Key, item.CompositeKeyEntity.Key);
        productToIncludedTypeKeyMap.Add(product.Key, item.TypeIdIncludedEntity.Key);
        nameIndex++;
      }

      foreach (var item in items.Reverse()) {
        var dProduct = new DerivedProduct {
          Name = nameIndex.ToString("D2"),
          CreationDate = DateTime.UtcNow,
          UIndentifier = Guid.NewGuid(),
          CompositeKeyRef = item.CompositeKeyEntity,
          TypeIdIncludedRef = item.TypeIdIncludedEntity
        };
        derivedProductToComositeKeyMap.Add(dProduct.Key, item.CompositeKeyEntity.Key);
        derivedProductToIncludedTypeKeyMap.Add(dProduct.Key, item.TypeIdIncludedEntity.Key);
      }
      nameIndex++;
    }

    private void CreateMedia(IList<CompositeKeyEntity> compositeKeyEntities, IList<TypeIdIncludedEntity> typeIdIncludedEntities)
    {
      var nameIndex = 0;
      var items = compositeKeyEntities.Zip(typeIdIncludedEntities, (c, t) => new { CompositeKeyEntity = c, TypeIdIncludedEntity = t });

      foreach (var item in items) {
        var film = new Film {
          Name = nameIndex.ToString("D2"),
          CreationDate = DateTime.UtcNow,
          UIndentifier = Guid.NewGuid(),
          CompositeKeyRef = item.CompositeKeyEntity,
          TypeIdIncludedRef = item.TypeIdIncludedEntity
        };
        filmToComositeKeyMap.Add(film.Key, item.CompositeKeyEntity.Key);
        filmToIncludedTypeKeyMap.Add(film.Key, item.TypeIdIncludedEntity.Key);
        nameIndex++;
      }

      foreach (var item in items.Reverse()) {
        var dSensor = new DigitalSensor {
          Name = nameIndex.ToString("D2"),
          CreationDate = DateTime.UtcNow,
          UIndentifier = Guid.NewGuid(),
          CompositeKeyRef = item.CompositeKeyEntity,
          TypeIdIncludedRef = item.TypeIdIncludedEntity
        };
        dSensorToComositeKeyMap.Add(dSensor.Key, item.CompositeKeyEntity.Key);
        dSensorToIncludedTypeKeyMap.Add(dSensor.Key, item.TypeIdIncludedEntity.Key);
        nameIndex++;
      }
    }

    private void CreateUsers(IList<CompositeKeyEntity> compositeKeyEntities, IList<TypeIdIncludedEntity> typeIdIncludedEntities)
    {
      var nameIndex = 0;
      var items = compositeKeyEntities.Zip(typeIdIncludedEntities, (c, t) => new { CompositeKeyEntity = c, TypeIdIncludedEntity = t });

      foreach (var item in items) {
        var user = new User {
          Name = nameIndex.ToString("D2"),
          CreationDate = DateTime.UtcNow,
          UIndentifier = Guid.NewGuid(),
          CompositeKeyRef = item.CompositeKeyEntity,
          TypeIdIncludedRef = item.TypeIdIncludedEntity
        };
        userToComositeKeyMap.Add(user.Key, item.CompositeKeyEntity.Key);
        userToIncludedTypeKeyMap.Add(user.Key, item.TypeIdIncludedEntity.Key);
        nameIndex++;
      }

      foreach (var item in items.Reverse()) {
        var admin = new AdminUser {
          Name = nameIndex.ToString("D2"),
          CreationDate = DateTime.UtcNow,
          UIndentifier = Guid.NewGuid(),
          CompositeKeyRef = item.CompositeKeyEntity,
          TypeIdIncludedRef = item.TypeIdIncludedEntity
        };
        adminToComositeKeyMap.Add(admin.Key, item.CompositeKeyEntity.Key);
        adminToIncludedTypeKeyMap.Add(admin.Key, item.TypeIdIncludedEntity.Key);
        nameIndex++;
      }
    }
  }
}
