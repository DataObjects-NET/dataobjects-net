// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.19

using System;
using NUnit.Framework;
using Xtensive.Sql.Common;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Tests.Storage.DbTypeSupportModel;
using Xtensive.Storage.Providers.Sql;

namespace Xtensive.Storage.Tests.Storage.DbTypeSupportModel
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

  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class X : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public bool FBool { get; set; }

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
    public byte[] FByteArray { get; set; }

    [Field(Length = 8001)]
    public byte[] FLongByteArray { get; set; }

    [Field]
    public string FString { get; set; }

    [Field(Length = 4001)]
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
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class TypeCompatibilityTest : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config =  base.BuildConfiguration();
      config.Types.Register(typeof(X).Assembly, typeof(X).Namespace);
      return config;
    }

    [Test]
    public void DefaultValuesTest()
    {
      using(Domain.OpenSession()) {
        Key key;
        using (var t = Session.Current.OpenTransaction()) {
          X x = new X();
          key = x.Key;
          t.Complete();
        }
        DomainHandler dh =Domain.Handlers.DomainHandler as DomainHandler;
        DateTime dt = new DateTime();
        if (dh != null) {
          RangeDataTypeInfo<DateTime> dti = dh.SqlDriver.ServerInfo.DataTypes.DateTime;
          dt = dti.Value.MinValue;
        }
        using (var t = Session.Current.OpenTransaction()) {
          X x = key.Resolve<X>();
          Assert.AreEqual(false, x.FBool);
          Assert.AreEqual(0, x.FByte);
          Assert.AreEqual(null, x.FByteArray);
          Assert.AreEqual(dt, x.FDateTime);
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
          Assert.AreEqual(0l, x.FLong);
          Assert.AreEqual(null, x.FLongByteArray);
          Assert.AreEqual(null, x.FLongString);
          Assert.AreEqual(0, x.FSByte);
          Assert.AreEqual(0, x.FShort);
          Assert.AreEqual(null, x.FString);
          Assert.AreEqual(TimeSpan.Zero, x.FTimeSpan);
          Assert.AreEqual(0, x.FUInt);
          Assert.AreEqual(0, x.FULong);
          Assert.AreEqual(0, x.FUShort);
          t.Complete();
        }
      }
    }
  }
}