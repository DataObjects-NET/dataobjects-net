// Copyright (C) 2021-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Linq.WhereByEnumTestModel;

namespace Xtensive.Orm.Tests.Linq.WhereByEnumTestModel
{
  public enum ByteEnum : byte
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = byte.MaxValue
  }

  public enum SByteEnum : sbyte
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = sbyte.MaxValue
  }

  public enum ShortEnum : short
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = short.MaxValue
  }

  public enum UShortEnum : ushort
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = ushort.MaxValue
  }

  public enum IntEnum : int
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = int.MaxValue
  }

  public enum UIntEnum : uint
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = uint.MaxValue
  }

  public enum LongEnum : long
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = long.MaxValue
  }

  public enum ULongEnum : ulong
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = ulong.MaxValue
  }

  [HierarchyRoot]
  public class EnumContainer : Entity
  {
    public const string Zero = "Zero";
    public const string One = "One";
    public const string Two = "Two";
    public const string Max = "Max";

    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public ByteEnum ByteEnumField { get; set; }

    [Field]
    public SByteEnum SByteEnumField { get; set; }

    [Field]
    public ShortEnum ShortEnumField { get; set; }

    [Field]
    public UShortEnum UShortEnumField { get; set; }

    [Field]
    public IntEnum IntEnumField { get; set; }

    [Field]
    public UIntEnum UIntEnumField { get; set; }

    [Field]
    public LongEnum LongEnumField { get; set; }

    [Field]
    public ULongEnum ULongEnumField { get; set; }

    [Field]
    public ByteEnum? NByteEnumField { get; set; }

    [Field]
    public SByteEnum? NSByteEnumField { get; set; }

    [Field]
    public ShortEnum? NShortEnumField { get; set; }

    [Field]
    public UShortEnum? NUShortEnumField { get; set; }

    [Field]
    public IntEnum? NIntEnumField { get; set; }

    [Field]
    public UIntEnum? NUIntEnumField { get; set; }

    [Field]
    public LongEnum? NLongEnumField { get; set; }

    [Field]
    public ULongEnum? NULongEnumField { get; set; }

    public EnumContainer(Session session, string enumValue)
      : base(session)
    {
      ByteEnumField = (ByteEnum) Enum.Parse(typeof(ByteEnum), enumValue);
      SByteEnumField = (SByteEnum) Enum.Parse(typeof(SByteEnum), enumValue);
      ShortEnumField = (ShortEnum) Enum.Parse(typeof(ShortEnum), enumValue);
      UShortEnumField = (UShortEnum) Enum.Parse(typeof(UShortEnum), enumValue);
      IntEnumField = (IntEnum) Enum.Parse(typeof(IntEnum), enumValue);
      UIntEnumField = (UIntEnum) Enum.Parse(typeof(UIntEnum), enumValue);
      LongEnumField = (LongEnum) Enum.Parse(typeof(LongEnum), enumValue);
      ULongEnumField = (ULongEnum) Enum.Parse(typeof(ULongEnum), enumValue);

      NByteEnumField = (ByteEnum) Enum.Parse(typeof(ByteEnum), enumValue);
      NSByteEnumField = (SByteEnum) Enum.Parse(typeof(SByteEnum), enumValue);
      NShortEnumField = (ShortEnum) Enum.Parse(typeof(ShortEnum), enumValue);
      NUShortEnumField = (UShortEnum) Enum.Parse(typeof(UShortEnum), enumValue);
      NIntEnumField = (IntEnum) Enum.Parse(typeof(IntEnum), enumValue);
      NUIntEnumField = (UIntEnum) Enum.Parse(typeof(UIntEnum), enumValue);
      NLongEnumField = (LongEnum) Enum.Parse(typeof(LongEnum), enumValue);
      NULongEnumField = (ULongEnum) Enum.Parse(typeof(ULongEnum), enumValue);
    }
  }
}

namespace Xtensive.Orm.Tests.Linq
{
  public class WhereByEnumTest : AutoBuildTest
  {
    private Session sharedSession;
    private string castSign;

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      CreateSessionAndTransaction();
      sharedSession = Session.Current;
      castSign = StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.PostgreSql)
        ? "::"
        : "CAST";
    }

    public override void TestFixtureTearDown()
    {
      sharedSession = null;
      base.TestFixtureTearDown();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(EnumContainer));
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new EnumContainer(session, EnumContainer.Zero);
        _ = new EnumContainer(session, EnumContainer.One);
        _ = new EnumContainer(session, EnumContainer.Two);
        _ = new EnumContainer(session, EnumContainer.Max);
        tx.Complete();
      }
    }

    [Test]
    public void ByteTest()
    {
      var queryFormatter = sharedSession.Services.Demand<QueryFormatter>();
      
      var query = sharedSession.Query.All<EnumContainer>().Where(e => e.ByteEnumField != ByteEnum.Zero);
      var queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ByteEnumField > ByteEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ByteEnumField >= ByteEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ByteEnumField < ByteEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ByteEnumField <= ByteEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ByteEnumField == ByteEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));


      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NByteEnumField != ByteEnum.Zero);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NByteEnumField > ByteEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NByteEnumField >= ByteEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NByteEnumField < ByteEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NByteEnumField <= ByteEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NByteEnumField == ByteEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));
    }

    [Test]
    public void SByteTest()
    {
      var queryFormatter = sharedSession.Services.Demand<QueryFormatter>();

      var query = sharedSession.Query.All<EnumContainer>().Where(e => e.SByteEnumField != SByteEnum.Zero);
      var queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.SByteEnumField > SByteEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.SByteEnumField >= SByteEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.SByteEnumField < SByteEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.SByteEnumField <= SByteEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.SByteEnumField == SByteEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));


      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NSByteEnumField != SByteEnum.Zero);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NSByteEnumField > SByteEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NSByteEnumField >= SByteEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NSByteEnumField < SByteEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NSByteEnumField <= SByteEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NSByteEnumField == SByteEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));
    }

    [Test]
    public void ShortTest()
    {
      var queryFormatter = sharedSession.Services.Demand<QueryFormatter>();

      var query = sharedSession.Query.All<EnumContainer>().Where(e => e.ShortEnumField == ShortEnum.Zero);
      var queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ShortEnumField == ShortEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ShortEnumField == ShortEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ShortEnumField == ShortEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));


      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NShortEnumField != ShortEnum.Zero);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NShortEnumField > ShortEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NShortEnumField >= ShortEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ShortEnumField < ShortEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ShortEnumField <= ShortEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ShortEnumField == ShortEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));
    }

    [Test]
    public void UShortTest()
    {
      var queryFormatter = sharedSession.Services.Demand<QueryFormatter>();

      var query = sharedSession.Query.All<EnumContainer>().Where(e => e.UShortEnumField != UShortEnum.Zero);
      var queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.UShortEnumField > UShortEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.UShortEnumField >= UShortEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.UShortEnumField < UShortEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.UShortEnumField <= UShortEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.UShortEnumField == UShortEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));


      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NUShortEnumField != UShortEnum.Zero);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NUShortEnumField > UShortEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NUShortEnumField >= UShortEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NUShortEnumField < UShortEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NUShortEnumField <= UShortEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.UShortEnumField == UShortEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));
    }

    [Test]
    public void IntTest()
    {
      var queryFormatter = sharedSession.Services.Demand<QueryFormatter>();

      var query = sharedSession.Query.All<EnumContainer>().Where(e => e.IntEnumField != IntEnum.Zero);
      var queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.IntEnumField > IntEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.IntEnumField >= IntEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.IntEnumField < IntEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.IntEnumField <= IntEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.IntEnumField == IntEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));


      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NIntEnumField != IntEnum.Zero);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NIntEnumField > IntEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NIntEnumField >= IntEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NIntEnumField < IntEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NIntEnumField <= IntEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NIntEnumField == IntEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));
    }

    [Test]
    public void UIntTest()
    {
      var queryFormatter = sharedSession.Services.Demand<QueryFormatter>();

      var query = sharedSession.Query.All<EnumContainer>().Where(e => e.UIntEnumField != UIntEnum.Zero);
      var queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.UIntEnumField > UIntEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.UIntEnumField >= UIntEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.UIntEnumField < UIntEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.UIntEnumField <= UIntEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.UIntEnumField == UIntEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));


      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NUIntEnumField != UIntEnum.Zero);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NUIntEnumField > UIntEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NUIntEnumField >= UIntEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NUIntEnumField < UIntEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NUIntEnumField <= UIntEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NUIntEnumField == UIntEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));
    }

    [Test]
    public void LongTest()
    {
      var queryFormatter = sharedSession.Services.Demand<QueryFormatter>();

      var substractValue = StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.SqlServer)
        ? castSign.Length
        : 0;

      var query = sharedSession.Query.All<EnumContainer>().Where(e => e.LongEnumField != LongEnum.Zero);
      var queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length - substractValue));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.LongEnumField > LongEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length - substractValue));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.LongEnumField >= LongEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length - substractValue));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.LongEnumField < LongEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length - substractValue));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.LongEnumField <= LongEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length - substractValue));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.LongEnumField == LongEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length - substractValue));
      Assert.That(query.Count(), Is.EqualTo(1));


      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NLongEnumField != LongEnum.Zero);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length - substractValue));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NLongEnumField > LongEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length - substractValue));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NLongEnumField >= LongEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length - substractValue));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NLongEnumField < LongEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length - substractValue));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NLongEnumField <= LongEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length - substractValue));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NLongEnumField == LongEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length - substractValue));
      Assert.That(query.Count(), Is.EqualTo(1));
    }

    [Test]
    public void ULongTest()
    {
      var queryFormatter = sharedSession.Services.Demand<QueryFormatter>();

      var query = sharedSession.Query.All<EnumContainer>().Where(e => e.ULongEnumField != ULongEnum.Zero);
      var queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ULongEnumField > ULongEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ULongEnumField >= ULongEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ULongEnumField < ULongEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ULongEnumField <= ULongEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.ULongEnumField == ULongEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));


      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NULongEnumField != ULongEnum.Zero);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NULongEnumField > ULongEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NULongEnumField >= ULongEnum.One);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NULongEnumField < ULongEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(2));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NULongEnumField <= ULongEnum.Two);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(3));

      query = sharedSession.Query.All<EnumContainer>().Where(e => e.NULongEnumField == ULongEnum.Max);
      queryString = queryFormatter.ToSqlString(query);
      Assert.That(queryString.Replace(castSign, "").Length, Is.EqualTo(queryString.Length));
      Assert.That(query.Count(), Is.EqualTo(1));
    }
  }
}