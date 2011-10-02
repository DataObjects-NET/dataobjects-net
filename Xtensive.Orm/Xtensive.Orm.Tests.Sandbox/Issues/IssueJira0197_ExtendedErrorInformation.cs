// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.30

using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;
using Xtensive.Storage.Tests.Issues.IssueJira0197_ExtendedErrorInformationModel;

namespace Xtensive.Storage.Tests.Issues.IssueJira0197_ExtendedErrorInformationModel
{
  [HierarchyRoot, KeyGenerator(KeyGeneratorKind.None), Index("Unique", Unique = true)]
  public class ErrorProvider : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Unique { get; set; }

    [Field(Nullable = false)]
    public string NotNull { get; set; }

    public ErrorProvider(int id)
      : base(id)
    {
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class IssueJira0197_ExtendedErrorInformation : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (ErrorProvider).Assembly, typeof (ErrorProvider).Namespace);
      return config;
    }

    [Test]
    public void InsertNullTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          try {
            new ErrorProvider(1);
            session.Persist();
          }
          catch (CheckConstraintViolationException exception) {
            var expected = Domain.Model.Types[typeof (ErrorProvider)];
            Assert.AreEqual(expected, exception.Info.Type);
            Assert.AreEqual(expected.Fields["NotNull"], exception.Info.Field);
          }
        }
      }
    }

    [Test]
    public void DuplicateUniqueIndexTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          try {
            new ErrorProvider(1) {NotNull = string.Empty, Unique = 1};
            session.Persist();
            new ErrorProvider(2) {NotNull = string.Empty, Unique = 1};
            session.Persist();
            Assert.Fail();
          }
          catch (UniqueConstraintViolationException exception) {
            Assert.AreEqual(Domain.Model.Types[typeof (ErrorProvider)], exception.Info.Type);
          }
        }
      }
    }

    [Test]
    public void DuplicatePrimaryKeyTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          try {
            new ErrorProvider(1) {NotNull = string.Empty, Unique = 1};
            session.Persist();
            new ErrorProvider(1) {NotNull = string.Empty, Unique = 2};
            session.Persist();
            Assert.Fail();
          }
          catch (UniqueConstraintViolationException exception) {
            Assert.AreEqual(Domain.Model.Types[typeof(ErrorProvider)], exception.Info.Type);
          }
        }
      }
    }
  }
}