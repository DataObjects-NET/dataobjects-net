// Copyright (C) 2008-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.09.19

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Info;
using Xtensive.Orm.Tests.Storage.DbTypeSupportModel;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage.DbTypeSupportModel
{

  #region Various enums

  public enum EByte : byte
  {
    Min = byte.MinValue, Default = 0, Max = byte.MaxValue
  }

  public enum ESByte : sbyte
  {
    Min = sbyte.MinValue, Default = 0, Max = sbyte.MaxValue
  }

  public enum EShort : short 
  {
    Min = short.MinValue, Default = 0, Max = short.MaxValue
  }

  public enum EUShort : ushort 
  {
    Min = ushort.MinValue, Default = 0, Max = ushort.MaxValue
  }

  public enum EInt : int 
  {
    Min = int.MinValue, Default = 0, Max = int.MaxValue
  }

  public enum EUInt : uint 
  {
    Min = uint.MinValue, Default = 0, Max = uint.MaxValue
  }

  public enum ELong : long 
  {
    Min = long.MinValue, Default = 0, Max = long.MaxValue
  }

  public enum EULong : ulong
  {
    Min = ulong.MinValue, Default = 0, Max = ulong.MaxValue
  }

  #endregion

  [Serializable]
  [HierarchyRoot]
  public class X : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public bool FBool { get; set; }

//  Note: temporary disabled, do not remove ]:->
//  [Field]
//  public char FChar { get; set; }

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
    public DateOnly FDateOnly { get; set; }

    [Field]
    public TimeOnly FTimeOnly { get; set; }

    [Field]
    public TimeSpan FTimeSpan { get; set; }

    [Field(Length = 1000)]
    public byte[] FByteArray { get; set; }

    [Field(Length = int.MaxValue)]
    public byte[] FLongByteArray { get; set; }

    [Field(Length = 1000)]
    public string FString { get; set; }

    [Field(Length = int.MaxValue)]
    public string FLongString { get; set; }

    [Field]
    public EByte FEByte { get; set; }

    [Field]
    public ESByte FESByte { get; set; }

    [Field]
    public EShort FEShort { get; set; }

    [Field]
    public EUShort FEUShort { get; set; }

    [Field]
    public EInt FEInt { get; set; }

    [Field]
    public EUInt FEUInt { get; set; }

    [Field]
    public ELong FELong { get; set; }

    [Field]
    public EULong FEULong { get; set; }


    [Field]
    public bool? FNBool { get; set; }

    [Field]
    public char? FNChar { get; set; }

    [Field]
    public byte? FNByte { get; set; }

    [Field]
    public sbyte? FNSByte { get; set; }

    [Field]
    public short? FNShort { get; set; }

    [Field]
    public ushort? FNUShort { get; set; }

    [Field]
    public int? FNInt { get; set; }

    [Field]
    public uint? FNUInt { get; set; }

    [Field]
    public long? FNLong { get; set; }

    [Field]
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
    public DateOnly? FNDateOnly { get; set; }

    [Field]
    public TimeOnly? FNTimeOnly { get; set; }

    [Field]
    public TimeSpan? FNTimeSpan { get; set; }

    [Field]
    public EByte? FNEByte { get; set; }

    [Field]
    public ESByte? FNESByte { get; set; }

    [Field]
    public EShort? FNEShort { get; set; }

    [Field]
    public EUShort? FNEUShort { get; set; }

    [Field]
    public EInt? FNEInt { get; set; }

    [Field]
    public EUInt? FNEUInt { get; set; }

    [Field]
    public ELong? FNELong { get; set; }

    [Field]
    public EULong? FNEULong { get; set; }

    public X(Session session)
      : base(session)
    {
    }

    public X()
      : base()
    {
    }
  }

  [HierarchyRoot]
  public class DecimalContainer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Precision = 18, Scale = 9)]
    public decimal d18_9 { get; set; }

    [Field(Precision = 18, Scale = 0)]
    public decimal d18_0 { get; set; }

    public DecimalContainer(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class DoubleContainer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public double FDouble { get; set; }

    public DoubleContainer(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class TypeCompatibilityTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config =  base.BuildConfiguration();
      config.Types.RegisterCaching(typeof (X).Assembly, typeof (X).Namespace);
      return config;
    }

    [Test]
    public void DefaultValuesTest()
    {
      using (var session = Domain.OpenSession()) {
        Key key;
        using (var t = session.OpenTransaction()) {
          X x = new X();
          key = x.Key;
          t.Complete();
        }

        var sqlDriver = TestSqlDriver.Create(session.Domain.Configuration.ConnectionInfo);
        var dataTypeInfo = sqlDriver.ServerInfo.DataTypes.DateTime;
        var dateTimeMinValue = ((ValueRange<DateTime>) dataTypeInfo.ValueRange).MinValue;

        dataTypeInfo = sqlDriver.ServerInfo.DataTypes.DateOnly;
        var dateOnlyMinValue = ((ValueRange<DateOnly>) dataTypeInfo.ValueRange).MinValue;

        dataTypeInfo = sqlDriver.ServerInfo.DataTypes.TimeOnly;
        var timeOnlyMinValue = ((ValueRange<TimeOnly>) dataTypeInfo.ValueRange).MinValue;

        using (var t = session.OpenTransaction()) {
          X x = session.Query.SingleOrDefault<X>(key);
          Assert.That(x.FBool, Is.EqualTo(false));
          Assert.That(x.FByte, Is.EqualTo(0));
          Assert.That(x.FByteArray, Is.EqualTo(null));
          if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.MySql)) {
            Assert.That(x.FDateTime, Is.EqualTo(DateTime.MinValue));
            Assert.That(x.FDateOnly, Is.EqualTo(DateOnly.MinValue));
            Assert.That(x.FTimeOnly, Is.EqualTo(TimeOnly.MinValue));
          }
          else {
            Assert.That(x.FDateTime, Is.EqualTo(dateTimeMinValue));
            Assert.That(x.FDateOnly, Is.EqualTo(dateOnlyMinValue));
            Assert.That(x.FTimeOnly, Is.EqualTo(timeOnlyMinValue));
          }
          Assert.That(x.FDecimal, Is.EqualTo(0));
          Assert.That(x.FDouble, Is.EqualTo(0));
          Assert.That(x.FEByte, Is.EqualTo(EByte.Default));
          Assert.That(x.FEInt, Is.EqualTo(EInt.Default));
          Assert.That(x.FELong, Is.EqualTo(ELong.Default));
          Assert.That(x.FESByte, Is.EqualTo(ESByte.Default));
          Assert.That(x.FEShort, Is.EqualTo(EShort.Default));
          Assert.That(x.FEUInt, Is.EqualTo(EUInt.Default));
          Assert.That(x.FEULong, Is.EqualTo(EULong.Default));
          Assert.That(x.FEUShort, Is.EqualTo(EUShort.Default));
          Assert.That(x.FFloat, Is.EqualTo(0));
          Assert.That(x.FGuid, Is.EqualTo(Guid.Empty));
          Assert.That(x.FInt, Is.EqualTo(0));
          Assert.That(x.FLong, Is.EqualTo(0L));
          Assert.That(x.FLongByteArray, Is.EqualTo(null));
          Assert.That(x.FLongString, Is.EqualTo(null));
          Assert.That(x.FSByte, Is.EqualTo(0));
          Assert.That(x.FShort, Is.EqualTo(0));
          Assert.That(x.FString, Is.EqualTo(null));
          Assert.That(x.FTimeSpan, Is.EqualTo(TimeSpan.Zero));
          Assert.That(x.FUInt, Is.EqualTo(0));
          Assert.That(x.FULong, Is.EqualTo(0));
          Assert.That(x.FUShort, Is.EqualTo(0));

          Assert.That(x.FNBool, Is.EqualTo(null));
          Assert.That(x.FNByte, Is.EqualTo(null));
          Assert.That(x.FNDateTime, Is.EqualTo(null));
          Assert.That(x.FNDateOnly, Is.EqualTo(null));
          Assert.That(x.FNTimeOnly, Is.EqualTo(null));
          Assert.That(x.FNDecimal, Is.EqualTo(null));
          Assert.That(x.FNDouble, Is.EqualTo(null));
          Assert.That(x.FNEByte, Is.EqualTo(null));
          Assert.That(x.FNEInt, Is.EqualTo(null));
          Assert.That(x.FNELong, Is.EqualTo(null));
          Assert.That(x.FNESByte, Is.EqualTo(null));
          Assert.That(x.FNEShort, Is.EqualTo(null));
          Assert.That(x.FNEUInt, Is.EqualTo(null));
          Assert.That(x.FNEULong, Is.EqualTo(null));
          Assert.That(x.FNEUShort, Is.EqualTo(null));
          Assert.That(x.FNFloat, Is.EqualTo(null));
          Assert.That(x.FNGuid, Is.EqualTo(null));
          Assert.That(x.FNInt, Is.EqualTo(null));
          Assert.That(x.FNLong, Is.EqualTo(null));
          Assert.That(x.FNSByte, Is.EqualTo(null));
          Assert.That(x.FNShort, Is.EqualTo(null));
          Assert.That(x.FNTimeSpan, Is.EqualTo(null));
          Assert.That(x.FNUInt, Is.EqualTo(null));
          Assert.That(x.FNULong, Is.EqualTo(null));
          Assert.That(x.FNUShort, Is.EqualTo(null));

          t.Complete();
        }
      }
    }

    [Test]
    public void ValidateTest()
    {
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Validate;
      configuration.Types.Register(typeof (X));
      var domain = Domain.Build(configuration);
      domain.Dispose();
    }

    [Test]
    public void DecimalTest()
    {
      // TODO: Expand the test (precision: 1-38, scale: 0-28)
      const decimal d18_9 = 304861306.000000000m;
      const decimal d18_0 = 720020000000000000m;
      using (var session = Domain.OpenSession()) {
        Key key;
        using (var transactionScope = session.OpenTransaction()) {
          var container = new DecimalContainer(session) {
            d18_9 = d18_9,
            d18_0 = d18_0
          };
          key = container.Key;
          transactionScope.Complete();
        }
        using (var transactionScope = session.OpenTransaction()) {
          var container = session.Query.Single<DecimalContainer>(key);
          Assert.That(container.d18_9, Is.EqualTo(d18_9));
          Assert.That(container.d18_0, Is.EqualTo(d18_0));
          transactionScope.Complete();
        }
      }
    }
  }
}