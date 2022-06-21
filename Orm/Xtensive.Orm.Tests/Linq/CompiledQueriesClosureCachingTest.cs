// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.CompiledQueriesClosureCachingTestModel;

namespace Xtensive.Orm.Tests.Linq.CompiledQueriesClosureCachingTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class AllTypesNeverCreatedEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public bool FBool { get; set; }

    [Field]
    public char FChar { get; set; }

    [Field]
    public byte FByte { get; set; }

    [Field]
    public sbyte FSByte { get; set; }

    [Field]
    public short FShort { get; set; }

    [Field]
    public ushort FUShort { get; set; }

    [Field]
    public int FInt { get; set; }

    [Field]
    public uint FUInt { get; set; }

    [Field]
    public long FLong { get; set; }

    [Field]
    public ulong FULong { get; set; }

    [Field]
    public Guid FGuid { get; set; }

    [Field]
    public float FFloat { get; set; }

    [Field]
    public double FDouble { get; set; }

    [Field]
    public decimal FDecimal { get; set; }

    [Field]
    public DateTime FDateTime { get; set; }

    [Field]
    public TimeSpan FTimeSpan { get; set; }

    [Field]
    public string FString { get; set; }

    [Field]
    public string FLongString { get; set; }

    // Nullable fields

    [Field(DefaultValue = true)]
    public bool? FNBool { get; set; }

    [Field(DefaultValue = 'x')]
    public char? FNChar { get; set; }

    [Field(DefaultValue = byte.MaxValue)]
    public byte? FNByte { get; set; }

    [Field(DefaultValue = sbyte.MaxValue)]
    public sbyte? FNSByte { get; set; }

    [Field(DefaultValue = short.MaxValue)]
    public short? FNShort { get; set; }

    [Field]
    public ushort? FNUShort { get; set; }

    [Field]
    public int? FNInt { get; set; }

    [Field]
    public uint? FNUInt { get; set; }

    [Field]
    public long? FNLong { get; set; }

    [Field] // SQLite provides only 8 byte signed integer
    public ulong? FNULong { get; set; }

    [Field]
    public Guid? FNGuid { get; set; }

    [Field]
    public float? FNFloat { get; set; }

    [Field]
    public double? FNDouble { get; set; }

    [Field]
    public decimal? FNDecimal { get; set; }

    [Field]
    public DateTime? FNDateTime { get; set; }

    [Field]
    public TimeSpan? FNTimeSpan { get; set; }

    public AllTypesNeverCreatedEntity(Session session)
      : base(session)
    {
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class DateTimeOffsetNeverCreatedEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DateTimeOffset FDateTimeOffset { get; set; }

    [Field]
    public DateTimeOffset? FNDateTimeOffset { get; set; }

    public DateTimeOffsetNeverCreatedEntity(Session session)
      : base(session)
    {
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class PgSqlTypesNeverCreatedEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public NpgsqlTypes.NpgsqlPoint FPoint { get; set; }

    [Field]
    public NpgsqlTypes.NpgsqlPoint? FNPoint { get; set; }

    public PgSqlTypesNeverCreatedEntity(Session session)
      : base(session)
    {
    }
  }

  public struct PropsStruct
  {
    public int IntField { get; set; }

    public long LongField { get; set; }
  }

  public struct PropsWrongStruct
  {
    public int IntField { get; set; }

    public Tuple<int> TupleField { get; set; }
  }

  public struct FieldsStruct
  {
    public int IntField;

    public long LongField;
  }

  public struct FieldsWrongStruct
  {
    public int IntField;

    public Tuple<int> TupleField;
  }
}

namespace Xtensive.Orm.Tests.Linq
{
  public sealed class CompiledQueriesClosureCachingTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(AllTypesNeverCreatedEntity));

      if (StorageProviderInfo.Instance.CheckAllFeaturesSupported(Providers.ProviderFeatures.DateTimeOffset))
        config.Types.Register(typeof(DateTimeOffsetNeverCreatedEntity));
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.PostgreSql))
        config.Types.Register(typeof(PgSqlTypesNeverCreatedEntity));

      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected override void CheckRequirements() => Require.ProviderIsNot(StorageProvider.Sqlite);

    [Test]
    public void BooleanTest()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, true);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (bool?) true);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task BooleanTestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, true);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (bool?) true);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void ByteTest()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, (byte) 127);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (byte?) 127);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task ByteTestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, (byte) 127);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (byte?) 127);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void SByteTest()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, (sbyte) 127);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (sbyte?) 127);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task SByteTestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, (sbyte) 127);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (sbyte?) 127);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void Int16Test()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, (short) 256);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (short?) 256);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task Int16TestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = Domain.OpenSession();
      await using var tx = session.OpenTransaction();
      await QueryAsync(session, (short) 256);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (short?) 256);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void UInt16Test()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, (ushort) 256);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (ushort?) 256);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task UInt16TestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = Domain.OpenSession();
      await using var tx = session.OpenTransaction();
      await QueryAsync(session, (ushort) 256);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (ushort?) 256);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void Int32Test()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, 512);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (int?) 512);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task Int32TestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, 512);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (int?) 512);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void UInt32Test()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, (uint) 512);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (uint?) 512);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task UInt32TestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, (uint) 512);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (uint?) 512);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void Int64Test()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, (long) 1024);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (long?) 1024);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task Int64TestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, (long) 1024);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (long?) 1024);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void UInt64Test()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, (ulong) 1024);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (ulong?) 1024);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task UInt64TestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, (ulong) 1024);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (ulong?) 1024);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void SingleTest()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, 1024.1f);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (float?) 1024.1f);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task SingleTestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, 1024.1f);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (float?) 1024.1f);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void DoubleTest()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, 1024.2);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (double?) 1024.2);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task DoubleTestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, 1024.2);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (double?) 1024.2);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void DecimalTest()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, (decimal) 1024.3);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (decimal?) 1024.3);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task DecimalTestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, (decimal) 1024.3);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (decimal?) 1024.3);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void CharTest()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, 'c');

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (char?) 'c');

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task CharTestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, 'c');

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (char?) 'c');

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void StringTest()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, "string");

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));
    }

    [Test]
    public async Task StringTestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = Domain.OpenSession();
      await using var tx = session.OpenTransaction();
      await QueryAsync(session, "string");

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));
    }

    [Test]
    public void DateTimeTest()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, DateTime.Now);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (DateTime?) DateTime.Now);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task DateTimeAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, DateTime.Now);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (DateTime?) DateTime.Now);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void TimeSpanTest()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, TimeSpan.Zero);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (TimeSpan?) TimeSpan.Zero);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task TimeSpanTestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, TimeSpan.Zero);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (TimeSpan?) TimeSpan.Zero);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void GuidTest()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, Guid.NewGuid());

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (Guid?) Guid.NewGuid());

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task GuidTestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, Guid.NewGuid());

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (Guid?) Guid.NewGuid());

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void DateTimeOffsetTest()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.DateTimeOffset);

      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, DateTimeOffset.Now);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (DateTimeOffset?) DateTimeOffset.Now);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task DateTimeOffsetTestAsync()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.DateTimeOffset);

      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, DateTimeOffset.Now);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (DateTimeOffset?) DateTimeOffset.Now);

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }


    [Test]
    public void PgSqlTypesTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);

      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, new NpgsqlTypes.NpgsqlPoint(0, 0));

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, (NpgsqlTypes.NpgsqlPoint?) new NpgsqlTypes.NpgsqlPoint(0, 0));

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task PgSqlTypesTestAsync()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);

      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, new NpgsqlTypes.NpgsqlPoint(0, 0));

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, (NpgsqlTypes.NpgsqlPoint?) new NpgsqlTypes.NpgsqlPoint(0, 0));

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public void ValueTupleTest()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, (1, 1.2f));

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));
    }

    [Test]
    public async Task ValueTupleTestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, (1, 1.2f));

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));
    }

    [Test]
    public void CustomStructTest()
    {
      var countBefore = Domain.QueryCache.Count;

      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();
      Query(session, new PropsStruct() { IntField = 10, LongField = 100 });

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      Query(session, new FieldsStruct() { IntField = 10, LongField = 100 });

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    [Test]
    public async Task CustomStructTestAsync()
    {
      var countBefore = Domain.QueryCache.Count;

      await using var session = await Domain.OpenSessionAsync();
      await using var tx = await session.OpenTransactionAsync();
      await QueryAsync(session, new PropsStruct() { IntField = 10, LongField = 100 });

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 1));

      await QueryAsync(session, new FieldsStruct() { IntField = 10, LongField = 100 });

      Assert.That(Domain.QueryCache.Count, Is.EqualTo(countBefore + 2));
    }

    #region General types

    private void Query(Session session, bool value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FBool == value));

    private async Task QueryAsync(Session session, bool value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FBool == value));

    private void Query(Session session, bool? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNBool == value));

    private async Task QueryAsync(Session session, bool? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNBool == value));

    private void Query(Session session, byte value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FByte == value));

    private async Task QueryAsync(Session session, byte value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FByte == value));

    private void Query(Session session, byte? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNByte == value));

    private async Task QueryAsync(Session session, byte? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNByte == value));

    private void Query(Session session, sbyte value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FSByte == value));

    private async Task QueryAsync(Session session, sbyte value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FSByte == value));

    private void Query(Session session, sbyte? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNSByte == value));

    private async Task QueryAsync(Session session, sbyte? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNSByte == value));

    private void Query(Session session, short value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FShort == value));

    private async Task QueryAsync(Session session, short value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FShort == value));

    private void Query(Session session, short? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNShort == value));

    private async Task QueryAsync(Session session, short? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNShort == value));

    private void Query(Session session, ushort value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FUShort == value));

    private async Task QueryAsync(Session session, ushort value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FUShort == value));

    private void Query(Session session, ushort? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNUShort == value));

    private async Task QueryAsync(Session session, ushort? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNUShort == value));

    private void Query(Session session, int value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FInt == value));

    private async Task QueryAsync(Session session, int value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FInt == value));

    private void Query(Session session, int? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNInt == value));

    private async Task QueryAsync(Session session, int? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNInt == value));

    private void Query(Session session, uint value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FUInt == value));

    private async Task QueryAsync(Session session, uint value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FUInt == value));

    private void Query(Session session, uint? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNUInt == value));

    private async Task QueryAsync(Session session, uint? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNUInt == value));

    private void Query(Session session, long value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FLong == value));

    private async Task QueryAsync(Session session, long value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FLong == value));

    private void Query(Session session, long? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNLong == value));

    private async Task QueryAsync(Session session, long? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNLong == value));

    private void Query(Session session, ulong value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FULong == value));

    private static async Task QueryAsync(Session session, ulong value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FULong == value));

    private void Query(Session session, ulong? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNULong == value));

    private static async Task QueryAsync(Session session, ulong? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNULong == value));

    private void Query(Session session, float value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FFloat == value));

    private async Task QueryAsync(Session session, float value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FFloat == value));

    private void Query(Session session, float? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNFloat == value));

    private async Task QueryAsync(Session session, float? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNFloat == value));

    private void Query(Session session, double value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FDouble == value));

    private async Task QueryAsync(Session session, double value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FDouble == value));

    private void Query(Session session, double? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNDouble == value));

    private async Task QueryAsync(Session session, double? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNDouble == value));

    private void Query(Session session, decimal value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FDecimal == value));

    private async Task QueryAsync(Session session, decimal value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FDecimal == value));

    private void Query(Session session, decimal? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNDecimal == value));

    private async Task QueryAsync(Session session, decimal? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNDecimal == value));

    private void Query(Session session, char value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FChar == value));

    private async Task QueryAsync(Session session, char value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FChar == value));

    private void Query(Session session, char? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNChar == value));

    private async Task QueryAsync(Session session, char? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNChar == value));

    private void Query(Session session, string value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FString == value));

    private async Task QueryAsync(Session session, string value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FString == value));

    private void Query(Session session, DateTime value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FDateTime == value));

    private async Task QueryAsync(Session session, DateTime value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FDateTime == value));

    private void Query(Session session, DateTime? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNDateTime == value));

    private async Task QueryAsync(Session session, DateTime? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNDateTime == value));

    private void Query(Session session, TimeSpan value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FTimeSpan == value));

    private async Task QueryAsync(Session session, TimeSpan value) =>
      await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FTimeSpan == value));

    private void Query(Session session, TimeSpan? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNTimeSpan == value));

    private async Task QueryAsync(Session session, TimeSpan? value) =>
      await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNTimeSpan == value));

    private void Query(Session session, Guid value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FGuid == value));

    private async Task QueryAsync(Session session, Guid value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FGuid == value));

    private void Query(Session session, Guid? value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNGuid == value));

    private async Task QueryAsync(Session session, Guid? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FNGuid == value));

    private void Query(Session session, (int value1, float value2) value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FInt == value.value1 || e.FFloat == value.value2));

    private async Task QueryAsync(Session session, (int value1, float value2) value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FInt == value.value1 || e.FFloat == value.value2));

    #endregion

    #region Provider-specific types

    private void Query(Session session, DateTimeOffset value) =>
      _ = session.Query.Execute(q => q.All<DateTimeOffsetNeverCreatedEntity>().Where(e => e.FDateTimeOffset == value));

    private async Task QueryAsync(Session session, DateTimeOffset value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<DateTimeOffsetNeverCreatedEntity>().Where(e => e.FDateTimeOffset == value));

    private void Query(Session session, DateTimeOffset? value) =>
      _ = session.Query.Execute(q => q.All<DateTimeOffsetNeverCreatedEntity>().Where(e => e.FNDateTimeOffset == value));

    private async Task QueryAsync(Session session, DateTimeOffset? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<DateTimeOffsetNeverCreatedEntity>().Where(e => e.FNDateTimeOffset == value));

    private void Query(Session session, NpgsqlTypes.NpgsqlPoint value) =>
      _ = session.Query.Execute(q => q.All<PgSqlTypesNeverCreatedEntity>().Where(e => e.FPoint == value));

    private async Task QueryAsync(Session session, NpgsqlTypes.NpgsqlPoint value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<PgSqlTypesNeverCreatedEntity>().Where(e => e.FPoint == value));

    private void Query(Session session, NpgsqlTypes.NpgsqlPoint? value) =>
      _ = session.Query.Execute(q => q.All<PgSqlTypesNeverCreatedEntity>().Where(e => e.FNPoint == value));

    private async Task QueryAsync(Session session, NpgsqlTypes.NpgsqlPoint? value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<PgSqlTypesNeverCreatedEntity>().Where(e => e.FNPoint == value));


    private void Query(Session session, PropsStruct value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FInt == value.IntField || e.FLong == value.LongField));

    private async Task QueryAsync(Session session, PropsStruct value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FInt == value.IntField || e.FLong == value.LongField));

    private void Query(Session session, FieldsStruct value) =>
      _ = session.Query.Execute(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FInt == value.IntField || e.FLong == value.LongField));

    private async Task QueryAsync(Session session, FieldsStruct value) =>
      _ = await session.Query.ExecuteAsync(q => q.All<AllTypesNeverCreatedEntity>().Where(e => e.FInt == value.IntField || e.FLong == value.LongField));

    #endregion
  }
}
