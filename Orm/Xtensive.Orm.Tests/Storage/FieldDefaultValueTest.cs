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
using Xtensive.Orm.Tests.Storage.FieldDefaultValueModel;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage.FieldDefaultValueModel
{
  public static class CodeRegistry
  {
    public const string GuidKeyValue = "b4fa0c56-be9a-4bd0-a50f-17c4c6b4af91";
    public const string GuidDefaultValue = "6C539ECE-E02A-42C1-B6D3-BEC03A0A25EA";
    public const OneTwo EnumKeyValue = OneTwo.Two;
  }

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
    Min = ulong.MinValue, Default = 0, Max = long.MaxValue
  }

  public enum OneTwo
  {
    One = 1,
    Two = 2,
  }

  #endregion

  [HierarchyRoot]
  public class XRef : Entity
  {
    [Field, Key]
    public Guid Id { get; private set;}

    public XRef(Guid key)
      : base(key)
    {}
  }

  [HierarchyRoot]
  public class EnumKeyEntity : Entity
  {
    [Key, Field]
    public OneTwo Id { get; private set; }

    [Field]
    public OneTwo OneTwoField { get; set; }

    public EnumKeyEntity(OneTwo id)
      : base(id)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class X : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(DefaultValue = true)]
    public bool FBool { get; set; }

    [Field(DefaultValue = byte.MaxValue)]
    public byte FByte { get; set; }

    [Field(DefaultValue = sbyte.MaxValue)]
    public sbyte FSByte { get; set; }

    [Field(DefaultValue = short.MaxValue)]
    public short FShort { get; set; }

    [Field(DefaultValue = ushort.MaxValue)]
    public ushort FUShort { get; set; }

    [Field(DefaultValue = int.MaxValue)]
    public int FInt { get; set; }

    [Field(DefaultValue = uint.MaxValue)]
    public uint FUInt { get; set; }

    [Field(DefaultValue = long.MaxValue)]
    public long FLong { get; set; }

    [Field(DefaultValue = long.MaxValue)] // SQLite provides only 8 byte signed integer
    public ulong FULong { get; set; }

    [Field(DefaultValue = CodeRegistry.GuidDefaultValue)]
    public Guid FGuid { get; set; }

    [Field(DefaultValue = float.MaxValue)]
    public float FFloat { get; set; }

    [Field(DefaultValue = float.MaxValue)]
    public double FDouble { get; set; }

    [Field(DefaultValue = 12.12)]
    public decimal FDecimal { get; set; }

    [Field(DefaultValue = "2012.12.12")]
    public DateTime FDateTime { get; set; }
#if NET6_0_OR_GREATER

    [Field(DefaultValue = "2012.12.12")]
    public DateOnly FDateOnly { get; set; }

    [Field(DefaultValue = "00:35:53.35")]
    public TimeOnly FTimeOnly { get; set; }
#endif

    [Field(DefaultValue = 1000)]
    public TimeSpan FTimeSpan { get; set; }

    [Field(Length = 1000, DefaultValue = "default value")]
    public string FString { get; set; }

    [Field(DefaultValue = EByte.Max)]
    public EByte FEByte { get; set; }

    [Field(DefaultValue = ESByte.Max)]
    public ESByte FESByte { get; set; }

    [Field(DefaultValue = EShort.Max)]
    public EShort FEShort { get; set; }

    [Field(DefaultValue = EUShort.Max)]
    public EUShort FEUShort { get; set; }

    [Field(DefaultValue = EInt.Max)]
    public EInt FEInt { get; set; }

    [Field(DefaultValue = EUInt.Max)]
    public EUInt FEUInt { get; set; }

    [Field(DefaultValue = ELong.Max)]
    public ELong FELong { get; set; }

    [Field(DefaultValue = EULong.Max)]
    public EULong FEULong { get; set; }

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

    [Field(DefaultValue = ushort.MaxValue)]
    public ushort? FNUShort { get; set; }

    [Field(DefaultValue = int.MaxValue)]
    public int? FNInt { get; set; }

    [Field(DefaultValue = uint.MaxValue)]
    public uint? FNUInt { get; set; }

    [Field(DefaultValue = long.MaxValue)]
    public long? FNLong { get; set; }

    [Field(DefaultValue = long.MaxValue)] // SQLite provides only 8 byte signed integer
    public ulong? FNULong { get; set; }

    [Field(DefaultValue = CodeRegistry.GuidDefaultValue)]
    public Guid? FNGuid { get; set; }

    [Field(DefaultValue = float.MaxValue)]
    public float? FNFloat { get; set; }

    [Field(DefaultValue = float.MaxValue)]
    public double? FNDouble { get; set; }

    [Field(DefaultValue = 12.12)]
    public decimal? FNDecimal { get; set; }

    [Field(DefaultValue = "2012.12.12")]
    public DateTime? FNDateTime { get; set; }

#if NET6_0_OR_GREATER
    [Field(DefaultValue = "2012.12.12")]
    public DateOnly? FNDateOnly { get; set; }

    [Field(DefaultValue = "00:35:53.35")]
    public TimeOnly? FNTimeOnly { get; set; }

#endif
    [Field(DefaultValue = 1000)]
    public TimeSpan? FNTimeSpan { get; set; }

    [Field(DefaultValue = EByte.Max)]
    public EByte? FNEByte { get; set; }

    [Field(DefaultValue = ESByte.Max)]
    public ESByte? FNESByte { get; set; }

    [Field(DefaultValue = EShort.Max)]
    public EShort? FNEShort { get; set; }

    [Field(DefaultValue = EUShort.Max)]
    public EUShort? FNEUShort { get; set; }

    [Field(DefaultValue = EInt.Max)]
    public EInt? FNEInt { get; set; }

    [Field(DefaultValue = EUInt.Max)]
    public EUInt? FNEUInt { get; set; }

    [Field(DefaultValue = ELong.Max)]
    public ELong? FNELong { get; set; }

    [Field(DefaultValue = EULong.Max)]
    public EULong? FNEULong { get; set; }

    [Field(DefaultValue = CodeRegistry.GuidKeyValue)]
    public XRef Ref { get; set; }

    [Field(DefaultValue = CodeRegistry.EnumKeyValue)]
    public EnumKeyEntity EnumKeyEntityRef { get; set; }
  }

  [HierarchyRoot]
  [Serializable]
  public class Y : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = int.MaxValue, DefaultValue = "default value")]
    public string FLongString { get; set; }
  }

  [HierarchyRoot]
  [Serializable]
  public class Z : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 1000, DefaultValue = new byte[] { 10, 10, 10, 10 })]
    public byte[] FByteArray { get; set; }

    [Field(Length = int.MaxValue, DefaultValue = new byte[] { 10, 10, 10, 10 })]
    public byte[] FLongByteArray { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class FieldDefaultValueTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config =  base.BuildConfiguration();
      config.Types.Register(typeof(XRef));
      config.Types.Register(typeof(EnumKeyEntity));
      config.Types.Register(typeof(X));
      if (SupportsDefaultForLongString()) {
        config.Types.Register(typeof(Y));
      }
      if (SupportsDefaultForArrays()) {
        config.Types.Register(typeof(Z));
      }
      return config;
    }

    [Test]
    public void DefaultValuesTest()
    {
      using (var session = Domain.OpenSession()) {
        Key keyX;
        Key keyY = null;
        Key keyZ = null;
        using (var t = session.OpenTransaction()) {
          // To be sure that the reference field (X.Ref) would have meaning
          _ = new XRef(new Guid(CodeRegistry.GuidKeyValue));
          _ = new EnumKeyEntity(CodeRegistry.EnumKeyValue);
          keyX = new X().Key;

          if (SupportsDefaultForLongString()) {
            keyY = new Y().Key;
          }

          if(SupportsDefaultForArrays()) {
            keyZ = new Z().Key;
          }
          t.Complete();
        }

        using (var t = session.OpenTransaction()) {
          var x = session.Query.SingleOrDefault<X>(keyX);
          Assert.AreEqual(true, x.FBool);
          Assert.AreEqual(byte.MaxValue, x.FByte);
          Assert.AreEqual(DateTime.Parse("2012.12.12"), x.FDateTime);

#if NET6_0_OR_GREATER
          Assert.AreEqual(DateOnly.Parse("2012.12.12"), x.FDateOnly);
          Assert.AreEqual(TimeOnly.Parse("00:35:53.35"), x.FTimeOnly);

#endif
          Assert.AreEqual(12.12M, x.FDecimal);
          Assert.AreEqual(float.MaxValue, x.FDouble);
          Assert.AreEqual(EByte.Max, x.FEByte);
          Assert.AreEqual(EInt.Max, x.FEInt);
          Assert.AreEqual(ELong.Max, x.FELong);
          Assert.AreEqual(ESByte.Max, x.FESByte);
          Assert.AreEqual(EShort.Max, x.FEShort);
          Assert.AreEqual(EUInt.Max, x.FEUInt);
          Assert.AreEqual(EULong.Max, x.FEULong);
          Assert.AreEqual(EUShort.Max, x.FEUShort);
          Assert.AreEqual(float.MaxValue, x.FFloat);
          Assert.AreEqual(new Guid(CodeRegistry.GuidDefaultValue), x.FGuid);
          Assert.AreEqual(int.MaxValue, x.FInt);
          Assert.AreEqual(long.MaxValue, x.FLong);

          Assert.AreEqual(sbyte.MaxValue, x.FSByte);
          Assert.AreEqual(short.MaxValue, x.FShort);
          Assert.AreEqual("default value", x.FString);
          Assert.AreEqual(TimeSpan.FromTicks(1000), x.FTimeSpan);
          Assert.AreEqual(uint.MaxValue, x.FUInt);
          Assert.AreEqual(long.MaxValue, x.FULong);
          Assert.AreEqual(ushort.MaxValue, x.FUShort);

          Assert.AreEqual(true, x.FNBool);
          Assert.AreEqual(byte.MaxValue, x.FNByte);
          Assert.AreEqual(DateTime.Parse("2012.12.12"), x.FNDateTime);
#if NET6_0_OR_GREATER //DO_DATEONLY
          Assert.AreEqual(DateOnly.Parse("2012.12.12"), x.FNDateOnly);
          Assert.AreEqual(TimeOnly.Parse("00:35:53.35"), x.FNTimeOnly);
#endif
          Assert.AreEqual(12.12M, x.FNDecimal);
          Assert.AreEqual(float.MaxValue, x.FNDouble);
          Assert.AreEqual(EByte.Max, x.FNEByte);
          Assert.AreEqual(EInt.Max, x.FNEInt);
          Assert.AreEqual(ELong.Max, x.FNELong);
          Assert.AreEqual(ESByte.Max, x.FNESByte);
          Assert.AreEqual(EShort.Max, x.FNEShort);
          Assert.AreEqual(EUInt.Max, x.FNEUInt);
          Assert.AreEqual(EULong.Max, x.FNEULong);
          Assert.AreEqual(EUShort.Max, x.FNEUShort);
          Assert.AreEqual(float.MaxValue, x.FNFloat);
          Assert.AreEqual(new Guid(CodeRegistry.GuidDefaultValue), x.FNGuid);
          Assert.AreEqual(int.MaxValue, x.FNInt);
          Assert.AreEqual(long.MaxValue, x.FNLong);
          Assert.AreEqual(sbyte.MaxValue, x.FNSByte);
          Assert.AreEqual(short.MaxValue, x.FNShort);
          Assert.AreEqual(TimeSpan.FromTicks(1000), x.FNTimeSpan);
          Assert.AreEqual(uint.MaxValue, x.FNUInt);
          Assert.AreEqual(long.MaxValue, x.FNULong);
          Assert.AreEqual(ushort.MaxValue, x.FNUShort);
          Assert.IsNotNull(x.Ref);
          Assert.IsNotNull(x.EnumKeyEntityRef);

          if (SupportsDefaultForLongString()) {
            var y = session.Query.SingleOrDefault<Y>(keyY);
            Assert.AreEqual("default value", y.FLongString);
          }
          if (SupportsDefaultForArrays()) {
            var z = session.Query.SingleOrDefault<Z>(keyZ);
            Assert.AreEqual(new byte[] { 10, 10, 10, 10 }, z.FByteArray);
            Assert.AreEqual(new byte[] { 10, 10, 10, 10 }, z.FLongByteArray);
          }

          t.Complete();
        }
      }
    }

    [Test]
    public void ValidateTest()
    {
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Validate;
      configuration.Types.Register(typeof(X));
      if (SupportsDefaultForLongString()) {
        configuration.Types.Register(typeof(Y));
      }
      if (SupportsDefaultForArrays()) {
        configuration.Types.Register(typeof(Z));
      }
      var domain = Domain.Build(configuration);
      domain.Dispose();
    }

    private bool SupportsDefaultForArrays()
    {
      switch (StorageProviderInfo.Instance.Provider) {
        case StorageProvider.Firebird:
        case StorageProvider.MySql:
          return false;
        default:
          return true;
      }
    }

    private bool SupportsDefaultForLongString()
    {
      return StorageProviderInfo.Instance.Provider != StorageProvider.MySql;
    }
  }
}