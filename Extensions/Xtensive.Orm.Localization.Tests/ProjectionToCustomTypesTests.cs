using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Orm.Localization.Tests.Model;

namespace Xtensive.Orm.Localization.Tests
{
  [TestFixture]
  public class ProjectionToCustomTypesTests : AutoBuildTest
  {
    protected override void PopulateDatabase()
    {
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          // populating database

          Country c1 = new Country(session) {
            Enabled = true,
            Name = "Magyarország",
            OrderValue = 3
          };
          Country c2 = new Country(session) {
            Enabled = true,
            Name = "Anglia",
            OrderValue = 6
          };
          Country c3 = new Country(session) {
            Enabled = false,
            Name = "Spain",
            OrderValue = 2
          };
          using (new LocalizationScope(EnglishCulture)) {
            c1.Name = "Hungary";
            c2.Name = "England";
          }

          CommunicationPlatform cp1 = new CommunicationPlatform(session) {
            Identifier = "cp1",
            Name = "cp1",
            Description = "dcp1",
            ProtocolPrefix = "abc",
            Enabled = true
          };
          CommunicationPlatform cp2 = new CommunicationPlatform(session) {
            Identifier = "cp2",
            Name = "cp2",
            Description = "dcp2",
            ProtocolPrefix = "def"
          };

          BuiltinMessage m1 = new BuiltinMessage(session) {
            Identifier = "bm1",
            Name = "bm1",
            Description = "dbm1",
            Enabled = true
          };
          BuiltinMessage m2 = new BuiltinMessage(session) {
            Identifier = "bm2",
            Name = "bm2",
            Description = "dbm2",
            Enabled = true
          };
          using (new LocalizationScope(EnglishCulture))
            m2.Name = "eng-bm2";
          using (new LocalizationScope(SpanishCulture))
            m2.Name = "de-bm2";

          ts.Complete();
        }
      }
    }

    [Test]
    public void NonLocalizableTest()
    {
      Thread.CurrentThread.CurrentCulture = EnglishCulture;
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {
          var q = session.Query.All<CommunicationPlatform>().OrderBy(e => e.Identifier).Select(e => new { e.Identifier, e.Name, e.Enabled, e.Description });
          var l = q.ToList();
          // assertions
          var propertyInfos = l.First().GetType().GetProperties();
          Assert.AreEqual(propertyInfos.Length, 4);
          Assert.AreEqual(propertyInfos[0].Name, nameof(CommunicationPlatform.Identifier));
          Assert.AreEqual(propertyInfos[1].Name, nameof(CommunicationPlatform.Name));
          Assert.AreEqual(propertyInfos[2].Name, nameof(CommunicationPlatform.Enabled));
          Assert.AreEqual(propertyInfos[3].Name, nameof(CommunicationPlatform.Description));

          ts.Complete();
        }
      }
    }

    [Test]
    public void SimpleClassHierarchyTest()
    {
      Thread.CurrentThread.CurrentCulture = EnglishCulture;
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {
          var q = session.Query.All<Country>().OrderBy(e => e.OrderValue).Select(e => new { e.Name, e.Enabled });
          var l = q.ToList();
          // assertions
          var propertyInfos = l.First().GetType().GetProperties();
          Assert.AreEqual(propertyInfos.Length, 2);
          Assert.AreEqual(propertyInfos.First().Name, nameof(Country.Name));
          Assert.AreEqual(propertyInfos.Last().Name, nameof(Country.Enabled));

          ts.Complete();
        }
      }
    }

    [Test]
    public void ComplexClassHierarchyTest()
    {
      Thread.CurrentThread.CurrentCulture = EnglishCulture;
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {
          var q = session.Query.All<BuiltinMessage>().OrderBy(e => e.Identifier).Select(e => new { e.Identifier, e.Name, e.Enabled, e.Description });
          var l = q.ToList();
          // assertions
          Assert.AreEqual(2, l.Count);

          var propertyInfos = l.First().GetType().GetProperties();
          Assert.AreEqual(propertyInfos.Length, 4);
          Assert.AreEqual(propertyInfos[0].Name, nameof(BuiltinMessage.Identifier));
          Assert.AreEqual(propertyInfos[1].Name, nameof(BuiltinMessage.Name));
          Assert.AreEqual(propertyInfos[2].Name, nameof(BuiltinMessage.Enabled));
          Assert.AreEqual(propertyInfos[3].Name, nameof(BuiltinMessage.Description));

          var f = l.First();
          Assert.AreEqual(f.Identifier, "bm1");
          Assert.AreEqual(f.Name, "bm1");
          Assert.AreEqual(f.Description, "dbm1");
          Assert.AreEqual(f.Enabled, true);

          var s = l.Last();
          Assert.AreEqual(s.Identifier, "bm2");
          Assert.AreEqual(s.Name, "eng-bm2");
          Assert.AreEqual(s.Description, "dbm2");
          Assert.AreEqual(s.Enabled, true);
        }
      }
    }

  }
}