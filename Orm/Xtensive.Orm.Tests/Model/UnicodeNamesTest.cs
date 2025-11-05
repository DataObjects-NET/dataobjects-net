// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2010.02.15

using NUnit.Framework;
using Xtensive.Orm.Tests.Model.UnicodeNamesTestModel;

namespace Xtensive.Orm.Tests.Model.UnicodeNamesTestModel
{
  [HierarchyRoot]
  public class Энимал : Entity
  {
    [Field, Key]
    public int Ид { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture]
  public class UnicodeNamesTest
  {
    [Test]
    public void MainTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql | StorageProvider.Firebird);
      var config = DomainConfigurationFactory.Create();
      config.Types.RegisterCaching(typeof(Энимал).Assembly, typeof(Энимал).Namespace);
      var domain = Domain.Build(config);
      domain.Dispose();
    }
  }
}