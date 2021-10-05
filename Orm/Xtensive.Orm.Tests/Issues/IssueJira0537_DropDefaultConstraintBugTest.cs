// Copyright (C) 2013-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.12.30

using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;
using Xtensive.Sql.Drivers.SqlServer;
using Xtensive.Sql.Model;
using Model1 = Xtensive.Orm.Tests.Issues.IssueJira0537_DropDefaultConstraintBugTestModel1;
using Model2 = Xtensive.Orm.Tests.Issues.IssueJira0537_DropDefaultConstraintBugTestModel2;

namespace Xtensive.Orm.Tests.Issues.IssueJira0537_DropDefaultConstraintBugTestModel1
{
  [HierarchyRoot]
  public class Area : Entity
  {
    [Field, Key]
    public long ID { get; set; }

    [Field(Nullable = false)]
    public string NotEmpty { get; set; }

    [Field(Nullable = false)]
    public bool BoolField { get; set; }

    [Field(Nullable = false)]
    public char CharField { get; set; }

    [Field(Nullable = false)]
    public sbyte SbyteField { get; set; }

    [Field(Nullable = false)]
    public byte ByteField { get; set; }

    [Field(Nullable = false)]
    public short ShortField { get; set; }

    [Field(Nullable = false)]
    public ushort UshortField { get; set; }

    [Field(Nullable = false)]
    public int IntField { get; set; }

    [Field(Nullable = false)]
    public uint UIntField { get; set; }

    [Field(Nullable = false)]
    public long LongField { get; set; }

    [Field(Nullable = false)]
    public ulong UlongField { get; set; }

    [Field(Nullable = false)]
    public float FloatField { get; set; }

    [Field(Nullable = false)]
    public double Double { get; set; }
  }

  [HierarchyRoot]
  public class StoredObject : Entity
  {
    [Field, Key]
    public long ID { get; set; }
    [Field]
    public Area Area { get; set; }
  }

  [HierarchyRoot]
  public class AnotherArea : Entity
  {
    [Field, Key]
    public long ID { get; set; }

    [Field]
    public string NotEmpty { get; set; }

    [Field]
    public bool BoolField { get; set; }

    [Field]
    public char CharField { get; set; }

    [Field]
    public sbyte SbyteField { get; set; }

    [Field]
    public byte ByteField { get; set; }

    [Field]
    public short ShortField { get; set; }

    [Field]
    public ushort UshortField { get; set; }

    [Field]
    public int IntField { get; set; }

    [Field]
    public uint UIntField { get; set; }

    [Field]
    public long LongField { get; set; }

    [Field]
    public ulong UlongField { get; set; }

    [Field]
    public float FloatField { get; set; }

    [Field]
    public double Double { get; set; }
  }

  [HierarchyRoot]
  public class AnotherStoredObject : Entity
  {
    [Field, Key]
    public long ID { get; set; }
    [Field]
    public AnotherArea Area { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0537_DropDefaultConstraintBugTestModel2
{
  namespace WMS
  {
    [HierarchyRoot]
    public class StoredObject : Entity
    {
      [Field, Key]
      public long ID { get; set; }

      [Field]
      public Core.Area Area { get; set; }
    }

    [HierarchyRoot]
    public class AnotherStoredObject : Entity
    {
      [Field, Key]
      public long ID { get; set; }

      [Field]
      public Core.AnotherArea Area { get; set; }
    }
  }

  namespace Core
  {
    [HierarchyRoot]
    public class Area : Entity
    {
      [Field, Key]
      public long ID { get; set; }

      [Field]
      public string NotEmpty { get; set; }

      [Field]
      public bool BoolField { get; set; }

      [Field]
      public char CharField { get; set; }

      [Field]
      public sbyte SbyteField { get; set; }

      [Field]
      public byte ByteField { get; set; }

      [Field]
      public short ShortField { get; set; }

      [Field]
      public ushort UshortField { get; set; }

      [Field]
      public int IntField { get; set; }

      [Field]
      public uint UIntField { get; set; }

      [Field]
      public long LongField { get; set; }

      [Field]
      public ulong UlongField { get; set; }

      [Field]
      public float FloatField { get; set; }

      [Field]
      public double Double { get; set; }
    }

    [HierarchyRoot]
    public class AnotherArea : Entity
    {
      [Field, Key]
      public long ID { get; set; }

      [Field(Nullable = false)]
      public string NotEmpty { get; set; }

      [Field(Nullable = false)]
      public bool BoolField { get; set; }

      [Field(Nullable = false)]
      public char CharField { get; set; }

      [Field(Nullable = false)]
      public sbyte SbyteField { get; set; }

      [Field(Nullable = false)]
      public byte ByteField { get; set; }

      [Field(Nullable = false)]
      public short ShortField { get; set; }

      [Field(Nullable = false)]
      public ushort UshortField { get; set; }

      [Field(Nullable = false)]
      public int IntField { get; set; }

      [Field(Nullable = false)]
      public uint UIntField { get; set; }

      [Field(Nullable = false)]
      public long LongField { get; set; }

      [Field(Nullable = false)]
      public ulong UlongField { get; set; }

      [Field(Nullable = false)]
      public float FloatField { get; set; }

      [Field(Nullable = false)]
      public double Double { get; set; }
    }
  }

  public class Upgrader : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void  AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      _ = hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0537_DropDefaultConstraintBugTestModel1.StoredObject", typeof(WMS.StoredObject)));
      _ = hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0537_DropDefaultConstraintBugTestModel1.Area", typeof(Core.Area)));
      _ = hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0537_DropDefaultConstraintBugTestModel1.AnotherStoredObject", typeof(WMS.AnotherStoredObject)));
      _ = hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0537_DropDefaultConstraintBugTestModel1.AnotherArea", typeof(Core.AnotherArea)));
      _ = hints.Add(new ChangeFieldTypeHint(typeof(Core.AnotherArea), "NotEmpty"));
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0537_DropDefaultConstraintBugTest : AutoBuildTest
  {
    private const string Database1Name = WellKnownDatabases.MultiDatabase.AdditionalDb1;
    private const string Database2Name = WellKnownDatabases.MultiDatabase.AdditionalDb2;
    private const string CoreAlias = "core";
    private const string WmsAlias = "wms";
    private const string SpecialSchemaAlias = WellKnownSchemas.SqlServerDefaultSchema;

    private ConnectionInfo connectionInfo;

    private static string multiDatabaseConnectionString;
    private static string singleDatabaseConnectionStringDatabase1;
    private static string singleDatabaseConnectionStringDatabase2;

    protected override void  CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }

    protected override void PopulateData() => BuildSingleDomain(Database1Name);

    [OneTimeSetUp]
    public override void TestFixtureSetUp()
    {
      CheckRequirements();
      InitializeConnectionStrings();
      CleanUp();
      PopulateData();
    }

    [Test]
    public void MainTest()
    {
      BuildMultipleDomain(Database2Name, Database1Name);
    }

    private static void BuildMultipleDomain(string coreDatabaseName, string wmsDatabaseName)
    {
      var domainConfiguration = new DomainConfiguration(multiDatabaseConnectionString) {
        DefaultDatabase = WmsAlias,
        DefaultSchema = SpecialSchemaAlias,
        UpgradeMode = DomainUpgradeMode.PerformSafely
      };

      var coreDatabase = new DatabaseConfiguration(CoreAlias) {
        RealName = coreDatabaseName
      };

      var wmsDatabase = new DatabaseConfiguration(WmsAlias) {
        RealName = wmsDatabaseName
      };

      domainConfiguration.Databases.Add(coreDatabase);
      domainConfiguration.Databases.Add(wmsDatabase);

      domainConfiguration.Types.Register(typeof(Model2.Upgrader).Assembly, typeof(Model2.Upgrader).Namespace);

      domainConfiguration.MappingRules.Map(typeof(Model2.Core.Area).Namespace).ToDatabase(CoreAlias);
      domainConfiguration.MappingRules.Map(typeof(Model2.WMS.StoredObject).Namespace).ToDatabase(WmsAlias);

      using (var domain = Domain.Build(domainConfiguration)) { }
    }

    private static void BuildSingleDomain(string wmsDatabaseName)
    {
      var domainConfiguration = new DomainConfiguration(singleDatabaseConnectionStringDatabase1) {
        UpgradeMode = DomainUpgradeMode.Recreate,
        DefaultSchema = SpecialSchemaAlias,
        DefaultDatabase = WmsAlias
      };

      var wmsDatabase = new DatabaseConfiguration(WmsAlias) {
        RealName = wmsDatabaseName
      };

      domainConfiguration.Databases.Add(wmsDatabase);

      domainConfiguration.MappingRules.Map(typeof (Model1.Area).Namespace).To(WmsAlias, SpecialSchemaAlias);

      domainConfiguration.Types.Register(typeof(Model1.Area));
      domainConfiguration.Types.Register(typeof(Model1.StoredObject));
      domainConfiguration.Types.Register(typeof(Model1.AnotherArea));
      domainConfiguration.Types.Register(typeof(Model1.AnotherStoredObject));

      _ = domainConfiguration.Databases.Add(wmsDatabaseName);

      using (var domain = Domain.Build(domainConfiguration)) { }
    }


    private void CleanUp()
    {
      var driverFactory = new DriverFactory();
      var driver = driverFactory.GetDriver(new ConnectionInfo(singleDatabaseConnectionStringDatabase1));
      ClearSchema(driver);
      driver = driverFactory.GetDriver(new ConnectionInfo(singleDatabaseConnectionStringDatabase2));
      ClearSchema(driver);
    }

    private void ClearSchema(SqlDriver driver)
    {
      using (var connection = driver.CreateConnection()) {
        connection.Open();
        try {
          var schema = driver.ExtractSchema(connection, SpecialSchemaAlias);
          var foreignKeys = schema.Tables
            .Select(t => new {
              Table = t,
              ForeignKeys = t.TableConstraints.OfType<ForeignKey>()
            });
          foreach (var dropConstraintText in from foreignKeyInfo in foreignKeys
            from foreignKey in foreignKeyInfo.ForeignKeys
            select driver.Compile(SqlDdl.Alter(foreignKeyInfo.Table, SqlDdl.DropConstraint(foreignKey))).GetCommandText()) {
            using (var command = connection.CreateCommand(dropConstraintText)) {
              _ = command.ExecuteNonQuery();
            }
          }

          foreach (var table in schema.Tables) {
            var dropTableText = driver.Compile(SqlDdl.Drop(table, true)).GetCommandText();
            using (var command = connection.CreateCommand(dropTableText)) {
              _ = command.ExecuteNonQuery();
            }
          }
        }
        finally {
          connection.Close();
        }
      }
    }

    private void InitializeConnectionStrings()
    {

      connectionInfo = TestConfiguration.Instance.GetConnectionInfo(TestConfiguration.Instance.Storage);

      var connectionUrl = connectionInfo.ConnectionUrl;

      var initialString = (!string.IsNullOrEmpty(connectionUrl.User) && !string.IsNullOrEmpty(connectionUrl.Password))
        ? string.Format("{0}://{1}:{2}@", connectionUrl.Protocol, connectionUrl.User, connectionUrl.Password)
        : string.Format("{0}://", connectionUrl.Protocol);

      var stringBuilder = new StringBuilder(initialString)
        .AppendFormat("{0}{1}", connectionUrl.Host, (connectionUrl.Port > 0) ? string.Format(":{0}", connectionUrl.Port) : string.Empty)
        .Append("/{0}{1}");

      var paramsString = string.Empty;
      foreach (var pair in connectionUrl.Params) {
        paramsString += string.IsNullOrEmpty(paramsString)
          ? string.Format("?{0}={1}", pair.Key, pair.Value)
          : string.Format("&{0}={1}", pair.Key, pair.Value);
      }

      singleDatabaseConnectionStringDatabase1 = string.Format(stringBuilder.ToString(), Database1Name, paramsString);
      singleDatabaseConnectionStringDatabase2 = string.Format(stringBuilder.ToString(), Database2Name, paramsString);
      multiDatabaseConnectionString = string.Format(stringBuilder.ToString(), string.Empty, paramsString);
    }
  }
}
