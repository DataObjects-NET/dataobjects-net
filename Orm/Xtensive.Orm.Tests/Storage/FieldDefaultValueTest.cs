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

    [Field(DefaultValue = float.Epsilon)]
    public float FFloat { get; set; }

    [Field(DefaultValue = float.MaxValue)]
    public double FDouble { get; set; }

    [Field(DefaultValue = 12.12)]
    public decimal FDecimal { get; set; }

    [Field(DefaultValue = "2012.12.12")]
    public DateTime FDateTime { get; set; }

    [Field(DefaultValue = "2012.12.12")]
    public DateOnly FDateOnly { get; set; }

    [Field(DefaultValue = "00:35:53.35")]
    public TimeOnly FTimeOnly { get; set; }

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

    [Field(DefaultValue = float.Epsilon)]
    public float? FNFloat { get; set; }

    [Field(DefaultValue = float.MaxValue)]
    public double? FNDouble { get; set; }

    [Field(DefaultValue = 12.12)]
    public decimal? FNDecimal { get; set; }

    [Field(DefaultValue = "2012.12.12")]
    public DateTime? FNDateTime { get; set; }

    [Field(DefaultValue = "2012.12.12")]
    public DateOnly? FNDateOnly { get; set; }

    [Field(DefaultValue = "00:35:53.35")]
    public TimeOnly? FNTimeOnly { get; set; }

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
          Assert.That(x.FBool, Is.EqualTo(true));
          Assert.That(x.FByte, Is.EqualTo(byte.MaxValue));
          Assert.That(x.FDateTime, Is.EqualTo(DateTime.Parse("2012.12.12")));

          Assert.That(x.FDateOnly, Is.EqualTo(DateOnly.Parse("2012.12.12")));
          Assert.That(x.FTimeOnly, Is.EqualTo(TimeOnly.Parse("00:35:53.35")));

          Assert.That(x.FDecimal, Is.EqualTo(12.12M));
          Assert.That(x.FDouble, Is.EqualTo(float.MaxValue));
          Assert.That(x.FEByte, Is.EqualTo(EByte.Max));
          Assert.That(x.FEInt, Is.EqualTo(EInt.Max));
          Assert.That(x.FELong, Is.EqualTo(ELong.Max));
          Assert.That(x.FESByte, Is.EqualTo(ESByte.Max));
          Assert.That(x.FEShort, Is.EqualTo(EShort.Max));
          Assert.That(x.FEUInt, Is.EqualTo(EUInt.Max));
          Assert.That(x.FEULong, Is.EqualTo(EULong.Max));
          Assert.That(x.FEUShort, Is.EqualTo(EUShort.Max));
          Assert.That(x.FFloat, Is.EqualTo(float.Epsilon));
          Assert.That(x.FGuid, Is.EqualTo(new Guid(CodeRegistry.GuidDefaultValue)));
          Assert.That(x.FInt, Is.EqualTo(int.MaxValue));
          Assert.That(x.FLong, Is.EqualTo(long.MaxValue));

          Assert.That(x.FSByte, Is.EqualTo(sbyte.MaxValue));
          Assert.That(x.FShort, Is.EqualTo(short.MaxValue));
          Assert.That(x.FString, Is.EqualTo("default value"));
          Assert.That(x.FTimeSpan, Is.EqualTo(TimeSpan.FromTicks(1000)));
          Assert.That(x.FUInt, Is.EqualTo(uint.MaxValue));
          Assert.That(x.FULong, Is.EqualTo(long.MaxValue));
          Assert.That(x.FUShort, Is.EqualTo(ushort.MaxValue));

          Assert.That(x.FNBool, Is.EqualTo(true));
          Assert.That(x.FNByte, Is.EqualTo(byte.MaxValue));
          Assert.That(x.FNDateTime, Is.EqualTo(DateTime.Parse("2012.12.12")));
          Assert.That(x.FNDateOnly, Is.EqualTo(DateOnly.Parse("2012.12.12")));
          Assert.That(x.FTimeOnly, Is.EqualTo(TimeOnly.Parse("00:35:53.35")));

          Assert.That(x.FNDecimal, Is.EqualTo(12.12M));
          Assert.That(x.FNDouble, Is.EqualTo(float.MaxValue));
          Assert.That(x.FNEByte, Is.EqualTo(EByte.Max));
          Assert.That(x.FNEInt, Is.EqualTo(EInt.Max));
          Assert.That(x.FNELong, Is.EqualTo(ELong.Max));
          Assert.That(x.FNESByte, Is.EqualTo(ESByte.Max));
          Assert.That(x.FNEShort, Is.EqualTo(EShort.Max));
          Assert.That(x.FNEUInt, Is.EqualTo(EUInt.Max));
          Assert.That(x.FNEULong, Is.EqualTo(EULong.Max));
          Assert.That(x.FNEUShort, Is.EqualTo(EUShort.Max));
          Assert.That(x.FNFloat, Is.EqualTo(float.Epsilon));
          Assert.That(x.FNGuid, Is.EqualTo(new Guid(CodeRegistry.GuidDefaultValue)));
          Assert.That(x.FNInt, Is.EqualTo(int.MaxValue));
          Assert.That(x.FNLong, Is.EqualTo(long.MaxValue));
          Assert.That(x.FNSByte, Is.EqualTo(sbyte.MaxValue));
          Assert.That(x.FNShort, Is.EqualTo(short.MaxValue));
          Assert.That(x.FNTimeSpan, Is.EqualTo(TimeSpan.FromTicks(1000)));
          Assert.That(x.FNUInt, Is.EqualTo(uint.MaxValue));
          Assert.That(x.FNULong, Is.EqualTo(long.MaxValue));
          Assert.That(x.FNUShort, Is.EqualTo(ushort.MaxValue));
          Assert.That(x.Ref, Is.Not.Null);
          Assert.That(x.EnumKeyEntityRef, Is.Not.Null);

          if (SupportsDefaultForLongString()) {
            var y = session.Query.SingleOrDefault<Y>(keyY);
            Assert.That(y.FLongString, Is.EqualTo("default value"));
          }
          if (SupportsDefaultForArrays()) {
            var z = session.Query.SingleOrDefault<Z>(keyZ);
            Assert.That(z.FByteArray, Is.EqualTo(new byte[] { 10, 10, 10, 10 }));
            Assert.That(z.FLongByteArray, Is.EqualTo(new byte[] { 10, 10, 10, 10 }));
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