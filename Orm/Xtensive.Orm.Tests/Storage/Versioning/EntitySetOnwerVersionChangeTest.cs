// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.03.02

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.VersioningConventionTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  public abstract class EntitySetOnwerVersionChangeTest : AutoBuildTest
  {
    protected bool ShouldOwnerVersionChange
    {
      get { return !Domain.Configuration.VersioningConvention.DenyEntitySetOwnerVersionChange; }
    }

    protected abstract void ApplyVersioningPolicy(DomainConfiguration configuration);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Author).Assembly, typeof (Author).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      ApplyVersioningPolicy(configuration);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new User();
        new AuthenticationToken();
        new Category();
        new PackedProduct();
        new UnpackedProduct();
        new Chat();
        new PlainTextMessage();
        new MultimediaMessage();
        new UrlMessage();

        new Book();
        new Author();

        new Order();
        new Customer();

        new Organization();
        new PartTimeEmployee();
        new FullTimeEmployee();

        new Bag();
        new HealBottle();
        new Armor();
        new KnowledgeScroll();

        new Museum();
        new ArtWork();
        new Subject();
        new Undergraduate();
        new Postgraduate();
        new Course();
        new Teacher();

        transaction.Complete();
      }
    }

    #region Zero-To-Many

    [Test]
    public void ZeroToManyGeneralTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var user = session.Query.All<User>().First();
          var authentication = session.Query.All<AuthenticationToken>().First();

          var currentUserVersion = user.Version;
          var currentAuthenticationVersion = authentication.Version;

          user.Authentications.Add(authentication);

          var expectedUserVersion = currentUserVersion + 1;
          var expectedAuthenticationVersion = currentAuthenticationVersion;

          Assert.That(user.Version, Is.EqualTo(expectedUserVersion));
          Assert.That(authentication.Version, Is.EqualTo(expectedAuthenticationVersion));
        }
      }
    }

    [Test]
    public void ZeroToManyAbstractTest()
    {
      using (var sesson = Domain.OpenSession()) {
        using (var transaction = sesson.OpenTransaction()) {
          var category = sesson.Query.All<Category>().First();
          var packedProduct = sesson.Query.All<PackedProduct>().First();
          var unpackedProduct = sesson.Query.All<UnpackedProduct>().First();

          var categoryVersion = category.Version;
          var packedProductVersion = packedProduct.Version;
          var unpackedProductVersion = unpackedProduct.Version;

          category.Products.Add(packedProduct);

          var expectedCategoryVersion = categoryVersion + 1;
          var expectedPackedProductVersion = packedProductVersion;
          var expectedUnpackedProductVersion = unpackedProductVersion;

          Assert.That(category.Version, Is.EqualTo(expectedCategoryVersion));
          Assert.That(packedProduct.Version, Is.EqualTo(expectedPackedProductVersion));
          Assert.That(unpackedProduct.Version, Is.EqualTo(expectedUnpackedProductVersion));

          category.Products.Add(unpackedProduct);
          expectedCategoryVersion = categoryVersion + 1;
          expectedPackedProductVersion = packedProductVersion;
          expectedUnpackedProductVersion = unpackedProductVersion;

          Assert.That(category.Version, Is.EqualTo(expectedCategoryVersion));
          Assert.That(packedProduct.Version, Is.EqualTo(expectedPackedProductVersion));
          Assert.That(unpackedProduct.Version, Is.EqualTo(expectedUnpackedProductVersion));
        }
      }
    }

    [Test]
    public void ZeroToManyInterfaceTest()
    {
      using (var sesson = Domain.OpenSession()) {
        using (var transaction = sesson.OpenTransaction()) {
          var chat = sesson.Query.All<Chat>().First();
          var ptMessage = sesson.Query.All<PlainTextMessage>().First();
          var mmMessage = sesson.Query.All<MultimediaMessage>().First();
          var urlMessage = sesson.Query.All<UrlMessage>().First();

          var chatVersion = chat.Version;
          var ptMessageVersion = ptMessage.Version;
          var mmMesageVersion = mmMessage.Version;
          var urlMessaveVersion = urlMessage.Version;

          chat.Messages.Add(ptMessage);

          var expectedChatVersion = chatVersion + 1;
          var expectedPtMessageVersion = ptMessageVersion;
          var expectedMmMessageVersion = mmMesageVersion;
          var expectedUrlMessageVersion = urlMessaveVersion;

          Assert.That(chat.Version, Is.EqualTo(expectedChatVersion));
          Assert.That(ptMessage.Version, Is.EqualTo(expectedPtMessageVersion));
          Assert.That(mmMessage.Version, Is.EqualTo(expectedMmMessageVersion));
          Assert.That(urlMessage.Version, Is.EqualTo(expectedUrlMessageVersion));

          chat.Messages.Add(mmMessage);
          expectedChatVersion = chatVersion + 1;
          expectedPtMessageVersion = ptMessageVersion;
          expectedMmMessageVersion = mmMesageVersion;
          expectedUrlMessageVersion = urlMessaveVersion;

          Assert.That(chat.Version, Is.EqualTo(expectedChatVersion));
          Assert.That(ptMessage.Version, Is.EqualTo(expectedPtMessageVersion));
          Assert.That(mmMessage.Version, Is.EqualTo(expectedMmMessageVersion));
          Assert.That(urlMessage.Version, Is.EqualTo(expectedUrlMessageVersion));

          chat.Messages.Add(urlMessage);
          expectedChatVersion = chatVersion + 1;
          expectedPtMessageVersion = ptMessageVersion;
          expectedMmMessageVersion = mmMesageVersion;
          expectedUrlMessageVersion = urlMessaveVersion;

          Assert.That(chat.Version, Is.EqualTo(expectedChatVersion));
          Assert.That(ptMessage.Version, Is.EqualTo(expectedPtMessageVersion));
          Assert.That(mmMessage.Version, Is.EqualTo(expectedMmMessageVersion));
          Assert.That(urlMessage.Version, Is.EqualTo(expectedUrlMessageVersion));
        }
      }
    }

    #endregion

    #region OneToMany

    [Test]
    public void OneToManyGeneralTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var museum = session.Query.All<Museum>().First();
          var artWork = session.Query.All<ArtWork>().First();

          var currentMuseumVersion = museum.Version;
          var currentArtWorkVersion = artWork.Version;

          artWork.Museum = museum;

          var expectedMuseumVersion = (ShouldOwnerVersionChange) ? currentMuseumVersion + 1 : currentMuseumVersion;
          var expectedArtWorkVersion = currentArtWorkVersion + 1;

          Assert.That(artWork.Version, Is.EqualTo(expectedArtWorkVersion));
          Assert.That(museum.Version, Is.EqualTo(expectedMuseumVersion));
        }

        using (var transaction = session.OpenTransaction()) {
          var museum = session.Query.All<Museum>().First();
          var artWork = session.Query.All<ArtWork>().First();

          var currentMuseumVersion = museum.Version;
          var currentArtWorkVersion = artWork.Version;

          museum.Works.Add(artWork);

          var expectedMuseumVersion = (ShouldOwnerVersionChange) ? currentMuseumVersion + 1 : currentMuseumVersion;
          var expectedArtWorkVersion = currentArtWorkVersion + 1;

          Assert.That(artWork.Version, Is.EqualTo(expectedArtWorkVersion));
          Assert.That(museum.Version, Is.EqualTo(expectedMuseumVersion));
        }
      }
    }

    [Test]
    public void OneToManyAbstractTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var organization = session.Query.All<Organization>().First();
          var ptEmployee = session.Query.All<PartTimeEmployee>().First();
          var ftEmployee = session.Query.All<FullTimeEmployee>().First();

          var organizationVersion = organization.Version;
          var ptEmployeeVersion = ptEmployee.Version;
          var ftEmployeeVersion = ftEmployee.Version;

          organization.Emplpoyees.Add(ptEmployee);

          var expectedOrganizationVersion = (ShouldOwnerVersionChange) ? organizationVersion + 1 : organizationVersion;
          var expectedPtEmployeeVersion = ptEmployeeVersion + 1;
          var expectedFtEmployeeVersion = ftEmployeeVersion;

          Assert.That(organization.Version, Is.EqualTo(expectedOrganizationVersion));
          Assert.That(ptEmployee.Version, Is.EqualTo(expectedPtEmployeeVersion));
          Assert.That(ftEmployee.Version, Is.EqualTo(expectedFtEmployeeVersion));

          organization.Emplpoyees.Add(ftEmployee);

          expectedOrganizationVersion = (ShouldOwnerVersionChange) ? organizationVersion + 1 : organizationVersion;
          expectedPtEmployeeVersion = ptEmployeeVersion + 1;
          expectedFtEmployeeVersion = ftEmployeeVersion + 1;

          Assert.That(organization.Version, Is.EqualTo(expectedOrganizationVersion));
          Assert.That(ptEmployee.Version, Is.EqualTo(expectedPtEmployeeVersion));
          Assert.That(ftEmployee.Version, Is.EqualTo(expectedFtEmployeeVersion));
        }

        using (var transaction = session.OpenTransaction()) {
          var organization = session.Query.All<Organization>().First();
          var ptEmployee = session.Query.All<PartTimeEmployee>().First();
          var ftEmployee = session.Query.All<FullTimeEmployee>().First();

          var organizationVersion = organization.Version;
          var ptEmployeeVersion = ptEmployee.Version;
          var ftEmployeeVersion = ftEmployee.Version;

          ptEmployee.Employer = organization;

          var expectedOrganizationVersion = (ShouldOwnerVersionChange) ? organizationVersion + 1 : organizationVersion;
          var expectedPtEmployeeVersion = ptEmployeeVersion + 1;
          var expectedFtEmployeeVersion = ftEmployeeVersion;

          Assert.That(organization.Version, Is.EqualTo(expectedOrganizationVersion));
          Assert.That(ptEmployee.Version, Is.EqualTo(expectedPtEmployeeVersion));
          Assert.That(ftEmployee.Version, Is.EqualTo(expectedFtEmployeeVersion));

          ftEmployee.Employer = organization;

          expectedOrganizationVersion = (ShouldOwnerVersionChange) ? organizationVersion + 1 : organizationVersion;
          expectedPtEmployeeVersion = ptEmployeeVersion + 1;
          expectedFtEmployeeVersion = ftEmployeeVersion + 1;

          Assert.That(organization.Version, Is.EqualTo(expectedOrganizationVersion));
          Assert.That(ptEmployee.Version, Is.EqualTo(expectedPtEmployeeVersion));
          Assert.That(ftEmployee.Version, Is.EqualTo(expectedFtEmployeeVersion));
        }
      }
    }

    [Test]
    public void OneToManyInterfaceTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var bag = session.Query.All<Bag>().First();
          var healBottle = session.Query.All<HealBottle>().First();
          var armor = session.Query.All<Armor>().First();
          var knowledgeScroll = session.Query.All<KnowledgeScroll>().First();

          var bagVersion = bag.Version;
          var healBottleVersion = healBottle.Version;
          var armorVersion = armor.Version;
          var knowledgeScrollVersion = knowledgeScroll.Version;

          bag.Items.Add(healBottle);

          var expectedBagVersion = (ShouldOwnerVersionChange) ? bagVersion + 1 : bagVersion;
          var expectedHealBottleVersion = healBottleVersion + 1;
          var expectedArmorVersion = armorVersion;
          var expectedKSVersion = knowledgeScrollVersion;

          Assert.That(bag.Version, Is.EqualTo(expectedBagVersion));
          Assert.That(healBottle.Version, Is.EqualTo(expectedHealBottleVersion));
          Assert.That(armor.Version, Is.EqualTo(expectedArmorVersion));
          Assert.That(knowledgeScroll.Version, Is.EqualTo(expectedKSVersion));

          bag.Items.Add(armor);

          expectedBagVersion = (ShouldOwnerVersionChange) ? bagVersion + 1 : bagVersion;
          expectedHealBottleVersion = healBottleVersion + 1;
          expectedArmorVersion = armorVersion + 1;
          expectedKSVersion = knowledgeScrollVersion;

          Assert.That(bag.Version, Is.EqualTo(expectedBagVersion));
          Assert.That(healBottle.Version, Is.EqualTo(expectedHealBottleVersion));
          Assert.That(armor.Version, Is.EqualTo(expectedArmorVersion));
          Assert.That(knowledgeScroll.Version, Is.EqualTo(expectedKSVersion));

          bag.Items.Add(knowledgeScroll);

          expectedBagVersion = (ShouldOwnerVersionChange) ? bagVersion + 1 : bagVersion;
          expectedHealBottleVersion = healBottleVersion + 1;
          expectedArmorVersion = armorVersion + 1;
          expectedKSVersion = knowledgeScrollVersion + 1;

          Assert.That(bag.Version, Is.EqualTo(expectedBagVersion));
          Assert.That(healBottle.Version, Is.EqualTo(expectedHealBottleVersion));
          Assert.That(armor.Version, Is.EqualTo(expectedArmorVersion));
          Assert.That(knowledgeScroll.Version, Is.EqualTo(expectedKSVersion));
        }

        using (var transaction = session.OpenTransaction()) {
          var bag = session.Query.All<Bag>().First();
          var healBottle = session.Query.All<HealBottle>().First();
          var armor = session.Query.All<Armor>().First();
          var knowledgeScroll = session.Query.All<KnowledgeScroll>().First();

          var bagVersion = bag.Version;
          var healBottleVersion = healBottle.Version;
          var armorVersion = armor.Version;
          var knowledgeScrollVersion = knowledgeScroll.Version;

          healBottle.Bag = bag;

          var expectedBagVersion = (ShouldOwnerVersionChange) ? bagVersion + 1 : bagVersion;
          var expectedHealBottleVersion = healBottleVersion + 1;
          var expectedArmorVersion = armorVersion;
          var expectedKSVersion = knowledgeScrollVersion;

          Assert.That(bag.Version, Is.EqualTo(expectedBagVersion));
          Assert.That(healBottle.Version, Is.EqualTo(expectedHealBottleVersion));
          Assert.That(armor.Version, Is.EqualTo(expectedArmorVersion));
          Assert.That(knowledgeScroll.Version, Is.EqualTo(expectedKSVersion));

          armor.Bag = bag;

          expectedBagVersion = (ShouldOwnerVersionChange) ? bagVersion + 1 : bagVersion;
          expectedHealBottleVersion = healBottleVersion + 1;
          expectedArmorVersion = armorVersion + 1;
          expectedKSVersion = knowledgeScrollVersion;

          Assert.That(bag.Version, Is.EqualTo(expectedBagVersion));
          Assert.That(healBottle.Version, Is.EqualTo(expectedHealBottleVersion));
          Assert.That(armor.Version, Is.EqualTo(expectedArmorVersion));
          Assert.That(knowledgeScroll.Version, Is.EqualTo(expectedKSVersion));

          knowledgeScroll.Bag = bag;

          expectedBagVersion = (ShouldOwnerVersionChange) ? bagVersion + 1 : bagVersion;
          expectedHealBottleVersion = healBottleVersion + 1;
          expectedArmorVersion = armorVersion + 1;
          expectedKSVersion = knowledgeScrollVersion + 1;

          Assert.That(bag.Version, Is.EqualTo(expectedBagVersion));
          Assert.That(healBottle.Version, Is.EqualTo(expectedHealBottleVersion));
          Assert.That(armor.Version, Is.EqualTo(expectedArmorVersion));
          Assert.That(knowledgeScroll.Version, Is.EqualTo(expectedKSVersion));
        }
      }
    }

    #endregion

    #region ManyToMany

    [Test]
    public void ManyToManyGeneralTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var author = session.Query.All<Author>().First();
          var book = session.Query.All<Book>().First();

          var currentAuthorVersion = author.Version;
          var currentBookVersion = book.Version;

          author.Books.Add(book);

          var expectedAuthorVersion = currentAuthorVersion + 1;
          var expectedBookVersion = currentBookVersion + 1;

          Assert.That(author.Version, Is.EqualTo(expectedAuthorVersion));
          Assert.That(author.Version, Is.EqualTo(expectedBookVersion));
        }

        using (var transacion = session.OpenTransaction()) {
          var author = session.Query.All<Author>().First();
          var book = session.Query.All<Book>().First();

          var currentAuthorVersion = author.Version;
          var currentBookVersion = book.Version;

          book.Authors.Add(author);

          var expectedAuthorVersion = currentAuthorVersion + 1;
          var expectedBookVersion = currentBookVersion + 1;

          Assert.That(author.Version, Is.EqualTo(expectedAuthorVersion));
          Assert.That(author.Version, Is.EqualTo(expectedBookVersion));
        }
      }
    }

    [Test]
    public void ManyToManyAbstractTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var subject = session.Query.All<Subject>().First();
          var uGraduate = session.Query.All<Undergraduate>().First();
          var pGraduate = session.Query.All<Postgraduate>().First();

          var subjectVersion = subject.Version;
          var uGraduageVersion = uGraduate.Version;
          var pGraduateVersion = pGraduate.Version;

          subject.Trainees.Add(uGraduate);

          var expectedSubjectVersion = subjectVersion + 1;
          var expectedUGraduateVersion = uGraduageVersion + 1;
          var expectedPGraduateVersion = pGraduateVersion;

          Assert.That(subject.Version, Is.EqualTo(expectedSubjectVersion));
          Assert.That(uGraduate.Version, Is.EqualTo(expectedUGraduateVersion));
          Assert.That(pGraduate.Version, Is.EqualTo(expectedPGraduateVersion));

          subject.Trainees.Add(pGraduate);

          expectedSubjectVersion = subjectVersion + 1;
          expectedUGraduateVersion = uGraduageVersion + 1;
          expectedPGraduateVersion = pGraduateVersion + 1;

          Assert.That(subject.Version, Is.EqualTo(expectedSubjectVersion));
          Assert.That(uGraduate.Version, Is.EqualTo(expectedUGraduateVersion));
          Assert.That(pGraduate.Version, Is.EqualTo(expectedPGraduateVersion));
        }

        using (var transaction = session.OpenTransaction()) {
          var subject = session.Query.All<Subject>().First();
          var uGraduate = session.Query.All<Undergraduate>().First();
          var pGraduate = session.Query.All<Postgraduate>().First();

          var subjectVersion = subject.Version;
          var uGraduageVersion = uGraduate.Version;
          var pGraduateVersion = pGraduate.Version;

          uGraduate.Subjects.Add(subject);

          var expectedSubjectVersion = subjectVersion + 1;
          var expectedUGraduateVersion = uGraduageVersion + 1;
          var expectedPGraduateVersion = pGraduateVersion;

          Assert.That(subject.Version, Is.EqualTo(expectedSubjectVersion));
          Assert.That(uGraduate.Version, Is.EqualTo(expectedUGraduateVersion));
          Assert.That(pGraduate.Version, Is.EqualTo(expectedPGraduateVersion));

          pGraduate.Subjects.Add(subject);

          expectedSubjectVersion = subjectVersion + 1;
          expectedUGraduateVersion = uGraduageVersion + 1;
          expectedPGraduateVersion = pGraduateVersion + 1;

          Assert.That(subject.Version, Is.EqualTo(expectedSubjectVersion));
          Assert.That(uGraduate.Version, Is.EqualTo(expectedUGraduateVersion));
          Assert.That(pGraduate.Version, Is.EqualTo(expectedPGraduateVersion));
        }
      }
    }

    [Test]
    public void ManyToManyInterfaceTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var course = session.Query.All<Course>().First();
          var teacher = session.Query.All<Teacher>().First();

          var courseVersion = course.Version;
          var teacherVersion = teacher.Version;

          course.Teachers.Add(teacher);

          var expectedCourseVersion = courseVersion + 1;
          var expectedTeacherVersion = teacherVersion + 1;

          Assert.That(course.Version, Is.EqualTo(expectedCourseVersion));
          Assert.That(teacher.Version, Is.EqualTo(expectedTeacherVersion));
        }

        using (var transaction = session.OpenTransaction()) {
          var course = session.Query.All<Course>().First();
          var teacher = session.Query.All<Teacher>().First();

          var courseVersion = course.Version;
          var teacherVersion = teacher.Version;

          teacher.Courses.Add(course);

          var expectedCourseVersion = courseVersion + 1;
          var expectedTeacherVersion = teacherVersion + 1;

          Assert.That(course.Version, Is.EqualTo(expectedCourseVersion));
          Assert.That(teacher.Version, Is.EqualTo(expectedTeacherVersion));
        }
      }
    }

    #endregion
  }

  public sealed class EntitySetOptimisticVersioningTest : EntitySetOnwerVersionChangeTest
  {
    protected override void ApplyVersioningPolicy(DomainConfiguration configuration)
    {
      configuration.VersioningConvention.DenyEntitySetOwnerVersionChange = true;
    }
  }

  public sealed class EntitySetPessimizticVersioningTest : EntitySetOnwerVersionChangeTest
  {
    protected override void ApplyVersioningPolicy(DomainConfiguration configuration)
    {
      configuration.VersioningConvention.DenyEntitySetOwnerVersionChange = false;
    }
  }
}
