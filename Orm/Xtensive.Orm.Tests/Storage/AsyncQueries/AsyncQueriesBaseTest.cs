using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.Association;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  [TestFixture]
  public abstract class AsyncQueriesBaseTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(Human).Assembly, typeof(Human).Namespace);
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
      EnsureSessionIsActivated(session);
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 20; i++) {
          new Discepline {Name = string.Format("Discepline {0}", i + 1)};
        }
        transaction.Complete();
      }
    }

    private void PopulateCourses(Session session)
    {
      EnsureSessionIsActivated(session);
      using (var transaction = session.OpenTransaction()) {
        var speciality = session.Query.All<Speciality>().First();
        var currentCourse = new Course {Name = "Some course", Year = DateTime.Now.Year, Speciality = speciality};
        var previousCourse = new Course {Name = "Some course", Year = DateTime.Now.Year - 1, Speciality = speciality};
        transaction.Complete();
      }
    }

    private void PopulateSpecialities(Session session)
    {
      EnsureSessionIsActivated(session);
      using (var transaction = session.OpenTransaction()) {
        new Speciality {Name = "Some speciality"};
        transaction.Complete();
      }
    }

    private void PopulateDisceplinesOfCourses(Session session)
    {
      EnsureSessionIsActivated(session);
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
      EnsureSessionIsActivated(session);
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
      EnsureSessionIsActivated(session);
      using (var transaction = session.OpenTransaction()) {
        var allDisceplines = session.Query.All<Discepline>();
        var index = 1;
        foreach (var allDiscepline in allDisceplines) {
          new Teacher {
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
      EnsureSessionIsActivated(session);
      var random = new Random();
      using (var transaction = session.OpenTransaction()) {
        var allGroups = session.Query.All<Group>();
        var index = 1;
        foreach (var @group in allGroups) {
          for (int i = 0; i < 10; i++) {
            @group.Students.Add(new Student {
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

    private void EnsureSessionIsActivated(Session session)
    {
      if (!session.IsActive)
        session.Activate();
    }
  }
}
