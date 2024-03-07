// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.MaterializationMethodTestsModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace MaterializationMethodTestsModel
  {
    [HierarchyRoot]
    public class Person : Entity
    {
      public Person(Session session) : base(session) { }

      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    public class PersonModel
    {
      public int Id { get; set; }
      public string Name { get; set; }
    }

    public abstract class ServiceBase<TEntity, TModel> where TModel : class, new() where TEntity : class, IEntity
    {
      public IList<TModel> GetAllViaPrivate(Session session) =>
        session.Query.All<TEntity>().Select(i => PrivateSelect(i)).ToList();

      public IList<TModel> GetAllViaPublic(Session session) =>
        session.Query.All<TEntity>().Select(i => PublicSelect(i)).ToList();

      public IList<TModel> GetAllViaProtected(Session session) =>
        session.Query.All<TEntity>().Select(i => ProtectedSelect(i)).ToList();

      public IList<TModel> GetAllViaInternal(Session session) =>
        session.Query.All<TEntity>().Select(i => InternalSelect(i)).ToList();

      public IList<TModel> GetAllViaProtectedInternal(Session session) =>
        session.Query.All<TEntity>().Select(i => ProtectedInternalSelect(i)).ToList();

      private TModel PrivateSelect(TEntity entity) => ProtectedSelect(entity);
      public TModel PublicSelect(TEntity entity) => ProtectedSelect(entity);
      internal TModel InternalSelect(TEntity entity) => ProtectedSelect(entity);
      protected internal TModel ProtectedInternalSelect(TEntity entity) => ProtectedSelect(entity);

      protected abstract TModel ProtectedSelect(TEntity entity);
    }

    public class PersonService : ServiceBase<Person, PersonModel>
    {
      protected override PersonModel ProtectedSelect(Person entity) =>
        new PersonModel { Id = entity.Id, Name = entity.Name };
    }
  }

  public class MaterializationMethodTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      var personType = typeof(Person);
      configuration.Types.Register(personType.Assembly, personType.Namespace);
      return configuration;
    }

    private static IEnumerable<(string, Func<PersonService, Session, IList<PersonModel>>)> DataExtractorMethods()
    {
      yield return (nameof(PersonService.GetAllViaPrivate), (service, session) => service.GetAllViaPrivate(session));
      yield return (nameof(PersonService.GetAllViaPublic), (service, session) => service.GetAllViaPublic(session));
      yield return (nameof(PersonService.GetAllViaInternal), (service, session) => service.GetAllViaInternal(session));
      yield return
        (nameof(PersonService.GetAllViaProtected), (service, session) => service.GetAllViaProtected(session));
      yield return
        (nameof(PersonService.GetAllViaProtectedInternal),
          (service, session) => service.GetAllViaProtectedInternal(session));
    }

    [Test]
    [TestCaseSource(nameof(DataExtractorMethods))]
    public void MaterializationByMethodOfBaseService(
      (string name, Func<PersonService, Session, IList<PersonModel>> getPeopleMethod) testCase)
    {
      using var session = Domain.OpenSession();
      using var _ = session.OpenTransaction();
      var personService = new PersonService();
      var people = testCase.getPeopleMethod(personService, session);
      Assert.AreEqual(0, people.Count);
    }
  }
}