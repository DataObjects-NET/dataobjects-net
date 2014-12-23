// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.08.28

#if NET45
using System;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries.Model
{
  [HierarchyRoot]
  public abstract class Human : Entity
  {
    [Field, Key]
    public int Id { get; protected set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Surname { get; set; }

    [Field]
    public DateTime DateOfBirth { get; set; }

    [Field]
    public Gender Gender { get; set; }

    public Human(Session session)
      :base(session)
    { }
  }

  [TableMapping("FuncyTableName")]
  public class Teacher : Human
  {
    [Field]
    [Association(PairTo = "Teacher")]
    public EntitySet<DisceplinesOfCourse> Disciplines { get; set; }

    public Teacher(Session session)
      :base(session)
    { }
  }

  public class Student : Human
  {
    [Field]
    public Group Group { get; set; }

    public Student(Session session)
      : base(session)
    { }
  }

  [HierarchyRoot]
  [Index("Id", Name="PK_Group1")]
  public class Group : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Groups")]
    public EntitySet<Course> Courses { get; set; }

    [Field]
    [Association(PairTo = "Group")]
    public EntitySet<Student> Students { get; set; } 
  }

  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class DisceplinesOfCourse : Entity
  {
    [Field, Key(0)]
    public Course Course { get; set; }

    [Field, Key(1)]
    public Discepline Discepline { get; set; }

    [Field]
    public Teacher Teacher { get; set; }

    public DisceplinesOfCourse(Session session, Course course, Discepline discepline)
      : base(session, course, discepline)
    { }
  }

  [HierarchyRoot]
  [Index("Name", "Year", Unique = true, Clustered = false)]
  public class Course : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 255)]
    public string Name { get; set; }

    [Field]
    public int Year { get; set; }

    [Field]
    public Speciality Speciality { get; set; }

    [Field]
    public EntitySet<Group> Groups { get; set; }

    public Course(Session session)
      : base(session)
    { }
  }

  [HierarchyRoot]
  public class Speciality : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public EntitySet<Course> Courses { get; set; }

    public Speciality(Session session)
      : base(session)
    { }
  }

  [HierarchyRoot]
  public class Discepline: Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public Discepline(Session session)
      : base(session)
    { }
  }

  [HierarchyRoot]
  public class ClassWithParameterizedConstructor : Entity
  {
    [Key, Field]
    public int Id { get; set; }

    [Field]
    public string TextField { get; set; }

    public ClassWithParameterizedConstructor(Session session)
      : base(session)
    { }
  }
  
  public enum Gender
  {
    Male,
    Female
  }
}
#endif
