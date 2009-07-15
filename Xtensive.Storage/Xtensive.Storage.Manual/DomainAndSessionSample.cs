// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.17

using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Manual
{
  public class DomainAndSessionSample
  {
    #region Connection URL examples
    public const string PostrgeSqlUrl = "postgresql://user:password@127.0.0.1:8032/myDatabase?Encoding=Unicode";
    public const string SqlServerUrl = "sqlserver://localhost/myDatabase";
    public const string InMemoryUrl = "memory://localhost/myDatabase";
    #endregion

    [HierarchyRoot]
    public class Person : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string Name { get; set; }
    }


    #region Domain sample
    public void Main()
    {
      var configuration = new DomainConfiguration("sqlserver://localhost/MyDatabase");

      // Register all types in specified assembly and namespace
      configuration.Types.Register(typeof (Person).Assembly, typeof(Person).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      var domain = Domain.Build(configuration);

      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {

          var person = new Person();
          person.Name = "Barack Obama";

          transactionScope.Complete();
        }
      }
    }
    #endregion

    #region Session sample
    public void SessionSample(Domain domain)
    {
      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {

          var person = new Person();
          person.Name = "Barack Obama";

          transactionScope.Complete();
        }
      }
    }
    #endregion
  }
}