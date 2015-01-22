// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.08.28

#if NET45
using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  [TestFixture]
  public abstract class AsyncQueriesBaseTest : AutoBuildTest
  {
    protected const int DisceplinesPerCourse = 20;
    protected const int InitialSpecialitiesCount = 1;
    protected const int InitialCoursesCount = 2;
    protected const int StudentsPerGroup = 10;
    protected const int InitialGroupsCount = 2;
    protected const int InitialDisceplinesCount = DisceplinesPerCourse;
    protected const int InitialStudentsCount = StudentsPerGroup * InitialGroupsCount;
    protected const int InitialTeachersCount = InitialDisceplinesCount;

    protected virtual SessionOptions SessionOptions
    {
      get { return SessionOptions.Default; }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(Human).Assembly, typeof(Human).Namespace);
      configuration.Sessions["Default"].Options = SessionOptions;
      return configuration;
    }

    protected sealed override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate()) {
        PopulateSpecialities(session);
        PopulateDisceplines(session);
        PopulateCourses(session);
        PopulateTeachers(session);
        PopulateDisceplinesOfCourses(session);
        PopulateGroups(session);
        PopulateStudents(session);
      }
    }

    private void PopulateDisceplines(Session session)
    {
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < DisceplinesPerCourse; i++) {
          new Discepline(session) {Name = string.Format("Discepline {0}", i + 1)};
        }
        transaction.Complete();
      }
    }

    private void PopulateCourses(Session session)
    {
      using (var transaction = session.OpenTransaction()) {
        var speciality = session.Query.All<Speciality>().First();
        var currentCourse = new Course(session) { Name = "Some course", Year = DateTime.Now.Year, Speciality = speciality };
        var previousCourse = new Course(session) { Name = "Some course", Year = DateTime.Now.Year - 1, Speciality = speciality };
        transaction.Complete();
      }
    }

    private void PopulateSpecialities(Session session)
    {
      using (var transaction = session.OpenTransaction()) {
        new Speciality(session) { Name = "Some speciality" };
        transaction.Complete();
      }
    }

    private void PopulateDisceplinesOfCourses(Session session)
    {
      using (var transaction = session.OpenTransaction()) {
        var courses = session.Query.All<Course>().ToList();
        var disceplines = session.Query.All<Discepline>().ToList();
        var teachers = session.Query.All<Teacher>().ToList();
        
        for (int i = 0; i < disceplines.Count; i++) {
          new DisceplinesOfCourse(session, courses[0], disceplines[i]) {Teacher = teachers[i]};
          new DisceplinesOfCourse(session, courses[1], disceplines[i]) {Teacher = teachers[i]};
        }
        transaction.Complete();
      }
    }

    private void PopulateGroups(Session session)
    {
      using (var transaction = session.OpenTransaction()) {
        var courses = session.Query.All<Course>();
        foreach (var course in courses) {
          new Group {Name = course.Year==DateTime.Now.Year ? "Current year group" : "Last year group"}.Courses.Add(course);
        }
        transaction.Complete();
      }
    }

    private void PopulateTeachers(Session session)
    {
      using (var transaction = session.OpenTransaction()) {
        var allDisceplines = session.Query.All<Discepline>();
        var index = 1;
        foreach (var allDiscepline in allDisceplines) {
          new Teacher(session) {
                          Name = string.Format("Name Of Teacher {0}", index),
                          Surname = string.Format("Surname Of Teacher {0}", index),
                          DateOfBirth = DateTime.Now.AddYears(-(index + 20)),
                          Gender = (index % 2==1) ? Gender.Male : Gender.Female
                        };
          index++;
        }
        transaction.Complete();
      }
    }

    private void PopulateStudents(Session session)
    {
      var random = new Random();
      using (var transaction = session.OpenTransaction()) {
        var allGroups = session.Query.All<Group>();
        var index = 1;
        foreach (var @group in allGroups) {
          for (int i = 0; i < StudentsPerGroup; i++) {
            @group.Students.Add(new Student(session) {
                                              Name = string.Format("Name Of Student {0}", index),
                                              Surname = string.Format("Surname Of Student {0}", index),
                                              DateOfBirth = DateTime.Now.AddYears(-random.Next(20, 25)),
                                              Gender = (index % 2==1) ? Gender.Male : Gender.Female
                                            });
          }
          transaction.Complete();
        }
      }
    }
  }
}
#endif
