using System;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;

namespace Xtensive.Tuples;

[TestFixture]
public class NullableTokenCollisionTest
{
  [Test]
  public void EnumWithNullableMetadataTokenIsNotTreatedAsNullable()
  {
    var nullableToken = typeof(Nullable<>).MetadataToken;
    var enumType = EmitEnumWithMetadataToken(nullableToken);
    
    Assert.That(enumType.IsEnum, Is.True);
    Assert.That(enumType.MetadataToken, Is.EqualTo(nullableToken));
    Assert.That(enumType.Module, Is.Not.EqualTo(typeof(Nullable<>).Module));
    Assert.DoesNotThrow(() =>TupleDescriptor.Create(new[] { enumType}));
  }

  [Test]
  public void GenuineNullableEnumStillResolves()
  {
    var nullableToken = typeof(Nullable<>).MetadataToken;
    var enumType = EmitEnumWithMetadataToken(nullableToken);
    var nullableEnum = typeof(Nullable<>).MakeGenericType(enumType);
    
    Assert.DoesNotThrow(() => TupleDescriptor.Create(new[] { nullableEnum }));
  }

  private static Type EmitEnumWithMetadataToken(int targetToken)
  {
    var rowId = targetToken & 0X00FFFFFF;
    var assembly = AssemblyBuilder.DefineDynamicAssembly(
      new AssemblyName("NullableTokenCollisionAsm"), AssemblyBuilderAccess.Run);
    var module = assembly.DefineDynamicModule("NullableTokenCollisionModule");

    //Starting at 2 because module is at 1 and then we add fake test until we reach the desired value
    for (var i = 2; i < rowId; i++) {
      module.DefineType("Filler" + i, TypeAttributes.Public).CreateType();
    }

    var enumBuilder = module.DefineEnum("CollidingStatus", TypeAttributes.Public, typeof(int));
    enumBuilder.DefineLiteral("FirstOption", 0);
    enumBuilder.DefineLiteral("SecondOption", 1);
    return enumBuilder.CreateType();
  }
}