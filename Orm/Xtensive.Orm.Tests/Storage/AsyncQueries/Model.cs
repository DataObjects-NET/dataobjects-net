﻿// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.07.01

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
      : base(session)
    { }
  }

  public class Teacher : Human
  {
    [Field]
    [Association(PairTo = "Teacher")]
    public EntitySet<DisceplinesOfCourse> Disciplines { get; set; }

    public Teacher(Session session)
      : base(session)
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
  public class Discepline : Entity
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
  public class StatRecord : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int IntFactor { get; set; }

    [Field]
    public long LongFactor { get; set; }

    [Field]
    public float FloatFactor { get; set; }

    [Field]
    public double DoubleFactor { get; set; }

    [Field]
    public decimal DecimalFactor { get; set; }

    public StatRecord(Session session)
      : base(session)
    { }
  }

  public enum Gender
  {
    Male,
    Female
  }
}

