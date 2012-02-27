// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.19

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Info;
using Xtensive.Orm.Tests.Storage.DbTypeSupportModel;
using Xtensive.Orm.Providers.Sql;
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
      config.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
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
        var domainHandler = Domain.Handlers.DomainHandler as DomainHandler;
        var minValue = new DateTime();
        if (domainHandler != null) {
          var field = typeof (StorageDriver).GetField("underlyingDriver", BindingFlags.Instance | BindingFlags.NonPublic);
          var sqlDriver = (SqlDriver) field.GetValue(domainHandler.Driver);
          var dataTypeInfo = sqlDriver.ServerInfo.DataTypes.DateTime;
          minValue = ((ValueRange<DateTime>) dataTypeInfo.ValueRange).MinValue;
        }
        using (var t = session.OpenTransaction()) {
          X x = session.Query.SingleOrDefault<X>(key);
          Assert.AreEqual(false, x.FBool);
          Assert.AreEqual(0, x.FByte);
          Assert.AreEqual(null, x.FByteArray);
          Assert.AreEqual(minValue, x.FDateTime);
          Assert.AreEqual(0, x.FDecimal);
          Assert.AreEqual(0, x.FDouble);
          Assert.AreEqual(EByte.Default, x.FEByte);
          Assert.AreEqual(EInt.Default, x.FEInt);
          Assert.AreEqual(ELong.Default, x.FELong);
          Assert.AreEqual(ESByte.Default, x.FESByte);
          Assert.AreEqual(EShort.Default, x.FEShort);
          Assert.AreEqual(EUInt.Default, x.FEUInt);
          Assert.AreEqual(EULong.Default, x.FEULong);
          Assert.AreEqual(EUShort.Default, x.FEUShort);
          Assert.AreEqual(0, x.FFloat);
          Assert.AreEqual(Guid.Empty, x.FGuid);
          Assert.AreEqual(0, x.FInt);
          Assert.AreEqual(0L, x.FLong);
          Assert.AreEqual(null, x.FLongByteArray);
          Assert.AreEqual(null, x.FLongString);
          Assert.AreEqual(0, x.FSByte);
          Assert.AreEqual(0, x.FShort);
          Assert.AreEqual(null, x.FString);
          Assert.AreEqual(TimeSpan.Zero, x.FTimeSpan);
          Assert.AreEqual(0, x.FUInt);
          Assert.AreEqual(0, x.FULong);
          Assert.AreEqual(0, x.FUShort);

          Assert.AreEqual(null, x.FNBool);
          Assert.AreEqual(null, x.FNByte);
          Assert.AreEqual(null, x.FNDateTime);
          Assert.AreEqual(null, x.FNDecimal);
          Assert.AreEqual(null, x.FNDouble);
          Assert.AreEqual(null, x.FNEByte);
          Assert.AreEqual(null, x.FNEInt);
          Assert.AreEqual(null, x.FNELong);
          Assert.AreEqual(null, x.FNESByte);
          Assert.AreEqual(null, x.FNEShort);
          Assert.AreEqual(null, x.FNEUInt);
          Assert.AreEqual(null, x.FNEULong);
          Assert.AreEqual(null, x.FNEUShort);
          Assert.AreEqual(null, x.FNFloat);
          Assert.AreEqual(null, x.FNGuid);
          Assert.AreEqual(null, x.FNInt);
          Assert.AreEqual(null, x.FNLong);
          Assert.AreEqual(null, x.FNSByte);
          Assert.AreEqual(null, x.FNShort);
          Assert.AreEqual(null, x.FNTimeSpan);
          Assert.AreEqual(null, x.FNUInt);
          Assert.AreEqual(null, x.FNULong);
          Assert.AreEqual(null, x.FNUShort);

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
          var container = new DecimalContainer() {
            d18_9 = d18_9,
            d18_0 = d18_0
          };
          key = container.Key;
          transactionScope.Complete();
        }
        using (var transactionScope = session.OpenTransaction()) {
          var container = session.Query.Single<DecimalContainer>(key);
          Assert.AreEqual(d18_9, container.d18_9);
          Assert.AreEqual(d18_0, container.d18_0);
          transactionScope.Complete();
        }
      }
    }
  }
}