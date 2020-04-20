// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.03.02

using System;
using System.Text;

namespace Xtensive.Orm.Tests.Storage.VersioningConventionTestModel
{
  [HierarchyRoot]
  public class SimpleTypesFieldsEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, Version]
    public long Version { get; private set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public bool BooleanField { get; set; }

    [Field]
    public byte ByteField { get; set; }

    [Field]
    public sbyte SByteField { get; set; }

    [Field]
    public short Int16Field { get; set; }

    [Field]
    public ushort UInt16Field { get; set; }

    [Field]
    public int Int32Field { get; set; }

    [Field]
    public uint UInt32Field { get; set; }

    [Field]
    public long Int64Field { get; set; }

    [Field]
    public ulong UInt64Field { get; set; }

    [Field]
    public DateTime DateTimeField { get; set; }

    [Field]
    public TimeSpan TimeSpanField { get; set; }

    [Field]
    public float SingleField { get; set; }

    [Field]
    public double DoubleField { get; set; }

    [Field]
    public decimal DecimalField { get; set; }

    [Field]
    public Guid GuidField { get; set; }

    [Field]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class ReferenceFieldsEntity : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public int Version { get; private set; }

    [Field]
    public ReferencedEntity ReferencedEntityField { get; set; }
  }

  [HierarchyRoot]
  public class StructureFieldsEntity : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field, Version]
    public int Version { get; set; }

    [Field]
    public TestStructure TestStructureField { get; set; }
  }

  [HierarchyRoot]
  public class ReferencedEntity : Entity
  {
    [Field, Key]
    public long Id { get; set; }
  }

  public class TestStructure : Structure
  {
    [Field]
    public int X { get; set; }

    [Field]
    public int Y { get; set; }
  }

  #region Zero-to-Many

  [HierarchyRoot]
  public class User : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field, Version]
    public long Version { get; private set; }

    [Field]
    public EntitySet<AuthenticationToken> Authentications { get; set; }
  }

  [HierarchyRoot]
  public class AuthenticationToken : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public long Version { get; private set; }

    [Field]
    public string Provider { get; set; }

    [Field]
    public string TokenValue { get; set; }
  }

  [HierarchyRoot]
  public class Category : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public long Version { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public EntitySet<Product> Products { get; private set; }
  }

  [HierarchyRoot]
  public abstract class Product : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public long Version { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  public class PackedProduct : Product
  {
    public int TypeOfPackage { get; set; }
  }

  public class UnpackedProduct : Product
  {
    // :)
    public int TypeOfUnpackage { get; set; }
  }

  [HierarchyRoot]
  public class Chat : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public long Version { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public EntitySet<IMessage> Messages { get; set; }
  }

  public interface IMessage : IEntity
  {
    [Field]
    long Id { get; }

    [Field]
    new long Version { get; }

    [Field]
    DateTime Date { get; set; }

    byte[] Content { get; set; }

  }

  [HierarchyRoot]
  public class PlainTextMessage : Entity, IMessage
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public long Version { get; private set; }

    [Field]
    public DateTime Date { get; set; }

    [Field]
    public string Message { get; set; }

    byte[] IMessage.Content
    {
      get { return MakeItByteArray(Message); }
      set { Message = MakeItMessage(value); }
    }

    private string MakeItMessage(byte[] bytes)
    {
      return Encoding.UTF8.GetString(bytes);
    }

    private byte[] MakeItByteArray(string message)
    {
      return Encoding.UTF8.GetBytes(message);
    }
  }

  [HierarchyRoot]
  public class UrlMessage : Entity, IMessage
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public long Version { get; private set; }

    [Field]
    public DateTime Date { get; set; }

    [Field]
    public string Url { get; set; }

    byte[] IMessage.Content
    {
      get { return MakeItByteArray(Url); }
      set { Url = MakeItMessage(value); }
    }

    private string MakeItMessage(byte[] bytes)
    {
      return Encoding.UTF8.GetString(bytes);
    }

    private byte[] MakeItByteArray(string message)
    {
      return Encoding.UTF8.GetBytes(message);
    }
  }

  [HierarchyRoot]
  public class MultimediaMessage : Entity, IMessage
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public long Version { get; private set; }

    [Field]
    public DateTime Date { get; set; }

    [Field]
    public byte[] Content { get; set; }
  }

  #endregion

  #region One-to-Many

  [HierarchyRoot]
  public class ArtWork : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public long Version { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Museum Museum { get; set; }
  }

  [HierarchyRoot]
  public class Museum : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public long Version { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Museum")]
    public EntitySet<ArtWork> Works { get; set; }
  }

  [HierarchyRoot]
  public class Organization : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field, Version]
    public int Version { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Employer")]
    public EntitySet<Emplpoyee> Emplpoyees { get; private set; }
  }

  [HierarchyRoot]
  public abstract class Emplpoyee : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public int Version { get; private set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public Organization Employer { get; set; }
  }

  public class PartTimeEmployee : Emplpoyee
  {
    [Field]
    public int HoursPerDay { get; set; }
  }

  public class FullTimeEmployee : Emplpoyee
  {
    [Field]
    public int StartHourOfWork { get; set; }
  }

  [HierarchyRoot]
  public class Bag : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public int Version { get; private set; }

    [Field]
    public int Space { get; set; }

    [Field]
    public int FreeSpace { get; set; }

    [Field]
    [Association(PairTo = "Bag")]
    public EntitySet<IBagItem> Items { get; private set; }
  }

  public interface IBagItem : IEntity
  {
    [Field]
    long Id { get; }

    [Field, Version]
    new int Version { get; }

    [Field]
    long SpaceNeeded { get; set; }

    [Field]
    Bag Bag { get; set; }
  }

  [HierarchyRoot]
  public class HealBottle : Entity, IBagItem
  {
    [Key]
    public long Id { get; private set; }

    public int Version { get; private set; }

    public long SpaceNeeded { get; set; }

    [Field]
    public double Value { get; set; }

    public Bag Bag { get; set; }
  }

  [HierarchyRoot]
  public class Armor : Entity, IBagItem
  {
    [Key]
    public long Id { get; private set; }

    public int Version { get; private set; }

    public long SpaceNeeded { get; set; }

    [Field]
    public double Value { get; set; }

    public Bag Bag { get; set; }
  }

  [HierarchyRoot]
  public class KnowledgeScroll : Entity, IBagItem
  {
    [Key]
    public long Id { get; private set; }

    public int Version { get; private set; }

    public long SpaceNeeded { get; set; }

    [Field]
    public double Value { get; set; }

    public Bag Bag { get; set; }
  }

  #endregion

  #region Many-to-One

  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public long Version { get; private set; }

    [Field]
    public DateTime Date { get; set; }

    [Field]
    [Association(PairTo = "Orders")]
    public Customer Customer { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public long Version { get; private set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public EntitySet<Order> Orders { get; private set; }
  }

  #endregion

  #region Many-to-Many

  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public long Version { get; private set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }
  }

  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public long Version { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    [Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; set; }
  }

  [HierarchyRoot]
  public class Subject : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field, Version]
    public int Version { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public EntitySet<Trainee> Trainees { get; private set; }
  }

  [HierarchyRoot]
  public abstract class Trainee : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public int Version { get; set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    [Association(PairTo = "Trainees")]
    public EntitySet<Subject> Subjects { get; set; }
  }

  public class Undergraduate : Trainee
  {
    [Field]
    public DateTime EnterDate { get; set; }
  }

  public class Postgraduate : Trainee
  {
    [Field]
    public DateTime GraduationDate { get; set; }
  }

  [HierarchyRoot]
  public class Course : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Version]
    public int Version { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public EntitySet<ITeacher> Teachers { get; set; }
  }

  public interface ITeacher : IEntity
  {
    [Field]
    long Id { get; }

    [Field, Version]
    new int Version { get; }

    [Field]
    string FirstName { get; set; }

    [Field]
    string LastName { get; set; }

    [Field]
    [Association(PairTo = "Teachers")]
    EntitySet<Course> Courses { get; }
  }

  [HierarchyRoot]
  public class Teacher : Entity, ITeacher
  {
    [Key]
    public long Id { get; set; }

    public int Version { get; private set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public EntitySet<Course> Courses { get; private set; }
  }

  #endregion


  public static class Extensions
  {
    public static T ChangeStructValue<T>(this T value)
      where T : struct
    {
      if (value is Guid) {
        return (T) (object) Guid.NewGuid();
      }
      if (value is DateTime) {
        return (T) (object) ((DateTime) (object) value).AddDays(2);
      }
      if (value is bool)
        return (T) (object) (!((bool) (object) value));
      if (value is byte) {
        var a = (byte) (object) value;
        return (T) (object) (byte) (a + a);
      }
      if (value is sbyte) {
        var a = (sbyte) (object) value;
        return (T) (object) (sbyte) (a + a);
      }
      if (value is short) {
        var a = (short) (object) value;
        return (T) (object) (short) (a + a);
      }
      if (value is ushort) {
        var a = (ushort) (object) value;
        return (T) (object) (ushort) (a + a);
      }

      return (dynamic) value + (dynamic) value;
    }

    public static T ChangeClassValue<T>(this T value)
      where T : class
    {
      if (value is string) {
        return (T) (object) ((string) (object) value + (string) (object) value);
      }
      throw new NotSupportedException(string.Format("Type '{0}' is not supported", typeof(T)));
    }
  }
}
