// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.26

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.States;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class LazyLoad
  {
    [Test]
    [Explicit]
    [Category("Sql-based")]
    public void States()
    {
      Domain domain = GetSqlStorage("Xtensive.Storage.Tests.Storage.States");
      Key key = null;
      using (domain.OpenSession()) {
        State state = new State();
        state.Name = "Name";
        state.Description = "Description";
        key = state.Key;
      }
      using (domain.OpenSession()) {
        State state = key.Resolve<State>();
        Tuple data = (Tuple) typeof (Persistent).GetField("data", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(state);
        int descriptionIndex = state.Type.Columns["Description"].Field.MappingInfo.Offset;
        Assert.IsFalse(data.IsAvailable(descriptionIndex));
        Assert.IsNull(data.GetValueOrDefault(descriptionIndex));
        Assert.AreEqual(state.Description, "Description");
        Assert.IsTrue(data.IsAvailable(descriptionIndex));
        Assert.IsNotNull(data.GetValueOrDefault(descriptionIndex));
      }
    }

    private Domain GetSqlStorage(string nameSpace)
    {
      DomainConfiguration configuration = new DomainConfiguration("mssql2005://localhost/Vehicles"); // TODO: real address.
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nameSpace);
      return Domain.Build(configuration);
    }
  }
}