// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.21

using System.IO;
using System.Text.Json;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture]
  public class SerializationTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof (Customer).Assembly, typeof (Customer).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      var model = Domain.Model.ToStoredModel();
      var serialized = model.Serialize();
      var result = StoredDomainModel.Deserialize(serialized);
      result.UpdateReferences();
    }
  }
}