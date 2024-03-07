// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using NUnit.Framework;
using Xtensive.Orm.Localization.Tests.Model;

namespace Xtensive.Orm.Localization.Tests
{
  [TestFixture]
  public class ProjectionToCustomTypesTests : LocalizationBaseTest
  {
    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession()) 
      using (var ts = session.OpenTransaction()) {
          // populating database
          var m1 = new Country(session) {
            Identifier = "HUN",
            Name = "Magyarország"
          };
          var m2 = new Country(session) {
            Identifier = "RUS",
            Name = "Oroszország"
          };
          using (new LocalizationScope(EnglishCulture)) {
            m2.Name = "Russia";
          }
          using (new LocalizationScope(SpanishCulture)) {
            m2.Name = "Rusia";  
          }
        ts.Complete();
      }
    }

    [Test]
    public void EntityHierarchyWithAbstractPropertyTest()
    {
      Thread.CurrentThread.CurrentCulture = EnglishCulture;
      using (var session = Domain.OpenSession()) 
      using (var ts = session.OpenTransaction()) {
          var q = session.Query.All<Country>().OrderBy(e => e.Identifier).Select(e => new { e.Name });
          var l = q.ToList();
          // assertions
          Assert.AreEqual(2, l.Count);
          var propertyInfos = l.First().GetType().GetProperties();
          Assert.AreEqual(propertyInfos.Length, 1);
          Assert.AreEqual(propertyInfos.First().Name, nameof(Country.Name));
          Assert.AreEqual(l.First().Name, "Magyarország");
          Assert.AreEqual(l.Last().Name, "Russia");
      }
    }
  }
}