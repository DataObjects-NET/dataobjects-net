// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.24

using System;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests.Upgrade.NewSkip.Model
{
  namespace Users
  {
    public interface IHasCreationDate
    {
      DateTime CreationDate { get; }
    }

    public abstract class EntityBase<TKey> : Entity, IHasCreationDate
    {
      [Field, Key]
      public TKey Id { get; private set; }

      [Field]
      public DateTime CreationDate { get; private set; }

      protected EntityBase()
      {
        CreationDate = DateTime.UtcNow;
      }
    }

    public abstract class EntityBase<TKey1, TKey2> : Entity, IHasCreationDate
    {
      [Field, Key(0)]
      public TKey1 Id1 { get; set; }

      [Field, Key(1)]
      public TKey2 Id2 { get; set; }

      public DateTime CreationDate { get; private set; }

      protected EntityBase(TKey1 key1, TKey2 key2)
        : base(key1, key2)
      {
        CreationDate = DateTime.UtcNow;
      }
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public abstract class Lookup : EntityBase<int>
    {
      [Field(Nullable = false)]
      public string Value { get; set; }
    }

    public class Country : Lookup
    {
    }

    public class Position : Lookup
    {
    }

    [HierarchyRoot]
    [Index("Email", Unique = true, IncludedFields = new []{"Person.Id"})]
    public class User : EntityBase<long>
    {
      [Field]
      public string Email { get; set; }

      [Field]
      public EntitySet<AuthorizationInfo> AuthorizationInfos { get; private set; }

      [Field]
      public Person Person { get; set; }
    }

    [HierarchyRoot]
    [Index("LastName", Clustered = true)]
    [Index("FirstName", Clustered = false, FillFactor = 0.5)]
    public sealed class Person : EntityBase<long>
    {
      [Field]
      public string LastName { get; set; }

      [Field]
      public string FirstName { get; set; }

      [Field]
      [Association(PairTo = "Person")]
      public User User { get; private set; }
    }

    [HierarchyRoot]
    [Index("TokenHash")]
    public sealed class AuthorizationInfo : EntityBase<long>
    {
      [Field]
      public ProviderInfo Provider { get; set; }

      [Field]
      public string TokenHash { get; set; }

      [Field]
      [Association(PairTo = "AuthorizationInfos")]
      public User User { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    [Index("Name", Unique = true)]
    public abstract class HashAlgorithm : EntityBase<int>
    {
      [Field(Length = 100)]
      public string Name { get; private set; }

      public abstract string ComputeHash(string value);

      protected HashAlgorithm(string name)
      {
        Name = name;
      }
    }

    public class MD5Hash : HashAlgorithm
    {
      public override string ComputeHash(string value)
      {
        using (var hasher = MD5.Create()) {
          var data = hasher.ComputeHash(Encoding.UTF8.GetBytes(value));
          return Encoding.UTF8.GetString(data);
        }
      }

      public MD5Hash()
        : base(typeof(MD5Hash).Name)
      {
      }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    [Index("Name", Unique = true)]
    public abstract class ProviderInfo : EntityBase<int>
    {
      [Field]
      public string Name { get; private set; }

      [Field]
      public HashAlgorithm HashAlgorithm { get; private set; }

      protected ProviderInfo(string name, HashAlgorithm algorithm)
      {
        Name = name;
        HashAlgorithm = algorithm;
      }
    }

    public sealed class BuildInProviderInfo : ProviderInfo
    {
      private const string BuildInProviderName = "SimpleBuildInProvider";

      public BuildInProviderInfo(HashAlgorithm algorithm)
        : base(BuildInProviderName, algorithm)
      {
      }
    }

    public abstract class OAuthProviderInfo : ProviderInfo
    {
      [Field(Nullable = false)]
      public string Url { get; set; }

      [Field]
      public string SecretKey { get; set; }

      public OAuthProviderInfo(string name, HashAlgorithm algorithm)
        : base(name, algorithm)
      {
      }
    }

    public sealed class GoogleOAuthProvider : OAuthProviderInfo
    {
      private const string GoogleProviderName = "OAuthGoogle";

      public GoogleOAuthProvider(HashAlgorithm algorithm)
        : base(GoogleProviderName, algorithm)
      {

      }
    }

    public abstract class OpenIdProviderInfo : ProviderInfo
    {
      [Field(Nullable = false)]
      public string Url { get; set; }

      [Field]
      public string SecretKey { get; set; }

      public OpenIdProviderInfo(string name, HashAlgorithm algorithm)
        : base(name, algorithm)
      {
      }
    }

    public sealed class AolOpenIdProviderInfo : OpenIdProviderInfo
    {
      private const string AolProviderName = "AolOpenId";

      public AolOpenIdProviderInfo(HashAlgorithm algorithm)
        : base(AolProviderName, algorithm)
      {
      }
    }

    [HierarchyRoot, Index("TestField", Filter = "Index")]
    public class SimpleFilterWithProperty : EntityBase<int>
    {
      public static Expression<Func<Storage.PartialIndexTestModel.SimpleFilterWithProperty, bool>> Index
      {
        get { return test => test.TestField.GreaterThan("hello world"); }
      }

      [Field]
      public string TestField { get; set; }
    }

    [HierarchyRoot]
    public class XRef : Entity
    {
      [Field, Key]
      public Guid Id { get; private set; }

      public XRef(Guid key)
        : base(key)
      { }
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

      [Field(DefaultValue = long.MaxValue)]
      public ulong FULong { get; set; }

      [Field(DefaultValue = CodeRegistry.GuidDefaultValue)]
      public Guid FGuid { get; set; }

      [Field(DefaultValue = float.MaxValue)]
      public float FFloat { get; set; }

      [Field(DefaultValue = float.MaxValue)]
      public double FDouble { get; set; }

      [Field(DefaultValue = 12.12, Precision = 18, Scale = 9)]
      public decimal FDecimal { get; set; }

      [Field(DefaultValue = "2012.12.12")]
      public DateTime FDateTime { get; set; }

      [Field(DefaultValue = 1000)]
      public TimeSpan FTimeSpan { get; set; }

      [Field(Length = 1000, DefaultValue = "default value")]
      public string FString { get; set; }

      [Field(Length = int.MaxValue, DefaultValue = "default value")]
      public string FLongString { get; set; }

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
    }

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

    public static class CodeRegistry
    {
      public const string GuidKeyValue = "b4fa0c56-be9a-4bd0-a50f-17c4c6b4af91";
      public const string GuidDefaultValue = "6C539ECE-E02A-42C1-B6D3-BEC03A0A25EA";
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class ComplexKeyEntity : EntityBase<int, int>
    {
      [Field(Length = 100)]
      public string SomeTextField { get; set; }

      public ComplexKeyEntity(int key1, int key2)
        : base(key1, key2)
      {
      }
    }
  }

  namespace Laptops
  {
    [HierarchyRoot]
    [Index("SerialNumber", Unique = true)]
    public class Laptop : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public Manufacturer Manufacturer { get; set; }

      [Field]
      public string SerialNumber { get; set; }

      [Field]
      public DisplayInfo DisplayInfo { get; set; }

      [Field]
      public CPUInfo CpuInfo { get; set; }

      [Field]
      public GraphicsCardInfo GraphicsCardInfo { get; set; }

      [Field]
      public StorageInfo StorageInfo { get; set; }

      [Field]
      public KeyboardInfo KeyboardInfo { get; set; }

      [Field]
      public IOPortsInfo IOPortsInfo { get; set; }
    }

    [HierarchyRoot]
    [Index("Name", Unique = true)]
    public class Manufacturer : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    public class IOPortsInfo : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public int UsbPortsCount { get; set; }

      [Field]
      public bool HasHDMIPort { get; set; }

      [Field]
      public int Usb30PortsCount { get; set; }

      [Field]
      public bool HasWiFiModule { get; set; }

      [Field]
      public bool HasEthernetPort { get; set; }

      [Field]
      public bool HasMicroInput { get; set; }

      [Field]
      public bool HasHeadphonesOutput { get; set; }

      [Field]
      [Association(PairTo = "IOPortsInfo")]
      public EntitySet<Laptop> Laptops { get; set; }
    }

    [HierarchyRoot]
    public class DisplayInfo : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public Manufacturer Manufacturer { get; set; }

      [Field]
      public int Dpi { get; set; }

      [Field]
      public Resolution Resolution { get; set; }

      [Field]
      public Formats Format { get; set; }

      [Field]
      [Association(PairTo = "DisplayInfo")]
      public EntitySet<Laptop> Laptops { get; set; }
    }

    [HierarchyRoot]
    [Index("Name", Unique = true)]
    public class CPUInfo : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public Manufacturer Manufacturer { get; set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public float BaseClockSpeed { get; set; }

      [Field]
      public int Multiplier { get; set; }

      [Field(Indexed = true)]
      public string ArchitectureName { get; set; }

      [Field]
      [Association(PairTo = "CpuInfo")]
      public EntitySet<Laptop> Laptops { get; set; }
    }

    [HierarchyRoot]
    [Index("Name", Unique = true)]
    public class GraphicsCardInfo : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public ChipProducers ChipProducer { get; set; }

      [Field]
      public Manufacturer Vendor { get; set; }

      [Field]
      public string Name { get; set; }

      [Field]
      [Association(PairTo = "GraphicsCardInfo")]
      public EntitySet<Laptop> Laptops { get; set; }
    }

    [HierarchyRoot]
    public class StorageInfo : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public Manufacturer Manufacturer { get; set; }

      [Field]
      public Capacity Capacity { get; set; }

      [Field]
      public bool IsSsd { get; set; }

      [Field(Indexed = true)]
      public int MaxWriteSpeed { get; set; }

      [Field(Indexed = true)]
      public int MaxReadSpeed { get; set; }

      [Field]
      [Association(PairTo = "StorageInfo")]
      public EntitySet<Laptop> Laptops { get; set; }
    }

    [HierarchyRoot]
    public class KeyboardInfo : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public int KeysCount { get; set; }

      [Field]
      public KeyboardType Type { get; set; }

      [Field]
      [Association(PairTo = "KeyboardInfo")]
      public EntitySet<Laptop> Laptops { get; set; }
    }

    public class Resolution : Structure
    {
      [Field]
      public int Wide { get; set; }

      [Field]
      public int Hight { get; set; }
    }

    public class Capacity : Structure
    {
      [Field]
      public int Value { get; set; }

      public StorageCapacityMeasure Measure { get; set; }
    }

    public enum Formats
    {
      Normal,
      Wide,
      UltraWide,
    }

    public enum ChipProducers
    {
      Amd,
      Nvidia,
      Intel
    }

    public enum KeyboardType
    {
      First,
      Second,
      Third,
    }

    public enum StorageCapacityMeasure
    {
      Kilobytes,
      Megabytes,
      Gigabytes,
      Terabytes
    }
  }
}
