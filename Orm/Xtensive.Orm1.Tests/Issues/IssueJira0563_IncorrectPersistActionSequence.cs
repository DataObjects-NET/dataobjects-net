// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.10.27

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Validation;
using model1 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel1;
using model2 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel2;
using model3 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel3;
using model4 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel4;
using model5 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel5;
using model6 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel6;
using model7 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel7;
using model8 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel8;
using model9 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel9;
using model10 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel10;
using model11 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel11;
using model12 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel12;
using model13 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel13;
using model14 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel14;
using model15 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel15;
using model16 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel16;
using model17 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel17;
using model18 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel18;
using model19 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel19;
using model20 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel20;
using model21 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel21;
using model22 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel22;
using model23 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel23;
using model24 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel24;
using model25 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel25;
using model26 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel26;
using model27 = Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel27;


#region models
//ZeroToOne Location in Job is nullable
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel1
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field, NotNullConstraint]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//ZeroToOne Location in Job is not nullable
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel2
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false), NotNullConstraint]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToOne all nullable not paired
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel3
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field, NotNullConstraint]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToOne Location is nullable and Job is not nullable, not paired
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel4
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field, NotNullConstraint]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      location.Job = this;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false)]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToOne Location is not nullable and Job is nullable, not paired
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel5
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false), NotNullConstraint]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToOne Location and Job both are not nullable, not paired
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel6
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false), NotNullConstraint]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      location.Job = this;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false)]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToOne all nullable, paired on Location side
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel7
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field, NotNullConstraint]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    [Association(PairTo = "Location")]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToOne all nullable, paired on Job side
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel8
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field, NotNullConstraint]
    [Association(PairTo = "Job")]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToOne Location is nullable and Job is not nullable, paired on Location side
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel9
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field, NotNullConstraint]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      location.Job = this;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false)]
    [Association(PairTo = "Location")]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToOne Location is nullable and Job is not nullable, paired on Job side
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel10
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field, NotNullConstraint]
    [Association(PairTo = "Job")]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false)]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToOne Location is not nullable and Job is nullable, paired on Location side
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel11
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false), NotNullConstraint]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    [Association(PairTo = "Location")]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToOne Location is not nullable and Job is nullable, paired on Job side
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel12
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false), NotNullConstraint]
    [Association(PairTo = "Job")]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToOne Location and Job both are not nullable, paired on Location side
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel13
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false), NotNullConstraint]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      location.Job = this;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false)]
    [Association(PairTo = "Location")]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToOne Location and Job both are not nullable, paired on Job side
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel14
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false), NotNullConstraint]
    [Association(PairTo = "Job")]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      location.Job = this;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false)]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//Loop big Test
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel15
{
  [HierarchyRoot]
  public class A : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Nullable = false)]
    public B B { get; set; }

    [Field(Nullable = false)]
    public NN NN { get; set; }
  }

  [HierarchyRoot]
  public class B : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Nullable = false)]
    public C C { get; set; }
  }

  [HierarchyRoot]
  public class C : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Nullable = false)]
    public D D { get; set; }
  }

  [HierarchyRoot]
  public class D : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Nullable = false)]
    public A A { get; set; }
  }

  [HierarchyRoot]
  public class NN : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public A A { get; set; }
  }

  [HierarchyRoot]
  public class NNN : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Nullable = false)]
    public NN NN { get; set; }
  }

  [HierarchyRoot]
  public class NNNN : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Nullable = false)]
    public NNN NNN { get; set; }
  }
}

//OneToMany, not paired
//Location is entity, 
//Job is EntitySet
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel16
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      location.Jobs.Add(this);
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public EntitySet<Job> Jobs { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToMany, not paired
//Location is entity [Nullable = false], 
//Job is EntitySet
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel17
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false)]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      location.Jobs.Add(this);
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public EntitySet<Job> Jobs { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToMany, not paired
//Location is EntitySet, 
//Job is Entity
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel18
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public EntitySet<Location> Locations { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Locations.Add(location);
      location.Job = this;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToMany, not paired
//Location is EntitySet,
//Job is Entity [Nullable = false]
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel19
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public EntitySet<Location> Locations { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Locations.Add(location);
      location.Job = this;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false)]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToMany, association in Job
//Location is entity, 
//Job is EntitySet
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel20
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    [Association(PairTo = "Jobs")]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      location.Jobs.Add(this);
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public EntitySet<Job> Jobs { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToMany, association in Job
//Location is entity [Nullable = false], 
//Job is EntitySet
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel21
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false)]
    [Association(PairTo = "Jobs")]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      location.Jobs.Add(this);
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public EntitySet<Job> Jobs { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToMany, association in Job
//Location is EntitySet, 
//Job is Entity
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel22
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    [Association(PairTo = "Job")]
    public EntitySet<Location> Locations { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Locations.Add(location);
      location.Job = this;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToMany, association in Job
//Location is EntitySet,
//Job is Entity [Nullable = false]
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel23
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    [Association(PairTo = "Job")]
    public EntitySet<Location> Locations { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Locations.Add(location);
      location.Job = this;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false)]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToMany, association in Location
//Location is entity, 
//Job is EntitySet
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel24
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      location.Jobs.Add(this);
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    [Association(PairTo = "Location")]
    public EntitySet<Job> Jobs { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToMany, association in Location
//Location is entity [Nullable = false], 
//Job is EntitySet
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel25
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false)]
    public Location Location { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Location = location;
      location.Jobs.Add(this);
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    [Association(PairTo = "Location")]
    public EntitySet<Job> Jobs { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToMany, association in Location
//Location is EntitySet, 
//Job is Entity
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel26
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public EntitySet<Location> Locations { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Locations.Add(location);
      location.Job = this;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    [Association(PairTo = "Locations")]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}

//OneToMany, association in Location
//Location is EntitySet,
//Job is Entity [Nullable = false]
namespace Xtensive.Orm.Tests.Issues.IssueJira0563_IncorrectPersistActionSequenceModel27
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Job Job { get; set; }

    public Invoice(Session session, Job job)
      : base(session)
    {
      Job = job;
      job.Invoice = this;
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public EntitySet<Location> Locations { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public Opportunity Opportunity { get; set; }

    public Job(Session session, Customer customer, Location location)
      : base(session)
    {
      Customer = customer;
      Locations.Add(location);
      location.Job = this;
      Opportunity = new Opportunity();
    }
  }

  [HierarchyRoot]
  public class Opportunity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, NotNullConstraint]
    [Association("Opportunity")]
    public Job Job { get; private set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Nullable = false)]
    [Association(PairTo="Locations")]
    public Job Job { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; set; }
  }
}
#endregion

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0563_IncorrectPersistActionSequence : AutoBuildTest
  {
    [Test]
    public void Model01Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model1.Customer).Assembly, typeof (model1.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model1.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model1.Customer>().First();
          var location = new model1.Location();
          var job = new model1.Job(session, customer, location);
          new model1.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model02Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model2.Customer).Assembly, typeof (model2.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model2.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model2.Customer>().First();
          var location = new model2.Location();
          var job = new model2.Job(session, customer, location);
          new model2.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model03Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model3.Customer).Assembly, typeof (model3.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model3.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model3.Customer>().First();
          var location = new model3.Location();
          var job = new model3.Job(session, customer, location);
          new model3.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model04Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model4.Customer).Assembly, typeof (model4.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model4.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model4.Customer>().First();
          var location = new model4.Location();
          var job = new model4.Job(session, customer, location);
          new model4.Invoice(session, job);
          location.Job = job;
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model05Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model5.Customer).Assembly, typeof (model5.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model5.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model5.Customer>().First();
          var location = new model5.Location();
          var job = new model5.Job(session, customer, location);
          new model5.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model06Test()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      Require.ProviderIsNot(StorageProvider.PostgreSql | StorageProvider.Sqlite | StorageProvider.Oracle);//do not use Sorting before insert
      var config = BuildConfiguration();
      config.Types.Register(typeof (model6.Customer).Assembly, typeof (model6.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model6.Customer();
          tx.Complete();
        }

        Assert.Throws<CheckConstraintViolationException>(() => {
          using (var session = domain.OpenSession())
          using (session.Activate())
          using (var tx = session.OpenTransaction()) {
            var customer = session.Query.All<model6.Customer>().First();
            var location = new model6.Location();
            var job = new model6.Job(session, customer, location);
            new model6.Invoice(session, job);
            tx.Complete();
          }
        });
      }
    }

    [Test]
    public void Model06TestForMySql()
    {
      Require.ProviderIs(StorageProvider.MySql | StorageProvider.SqlServerCe);
      var config = BuildConfiguration();
      config.Types.Register(typeof (model6.Customer).Assembly, typeof (model6.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model6.Customer();
          tx.Complete();
        }

        Assert.Throws<StorageException>(() => {
          using (var session = domain.OpenSession())
          using (session.Activate())
          using (var tx = session.OpenTransaction()) {
            var customer = session.Query.All<model6.Customer>().First();
            var location = new model6.Location();
            var job = new model6.Job(session, customer, location);
            new model6.Invoice(session, job);
            tx.Complete();
          }
        });
      }
    }

    [Test]
    public void Model07Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model7.Customer).Assembly, typeof (model7.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model7.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model7.Customer>().First();
          var location = new model7.Location();
          var job = new model7.Job(session, customer, location);
          new model7.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model08Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model8.Customer).Assembly, typeof (model8.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model8.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model8.Customer>().First();
          var location = new model8.Location();
          var job = new model8.Job(session, customer, location);
          new model8.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model09Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model9.Customer).Assembly, typeof (model9.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model9.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model9.Customer>().First();
          var location = new model9.Location();
          var job = new model9.Job(session, customer, location);
          new model9.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model10Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model10.Customer).Assembly, typeof (model10.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model10.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model10.Customer>().First();
          var location = new model10.Location();
          var job = new model10.Job(session, customer, location);
          new model10.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model11Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model11.Customer).Assembly, typeof (model11.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model11.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model11.Customer>().First();
          var location = new model11.Location();
          var job = new model11.Job(session, customer, location);
          new model11.Invoice(session, job);
          location.Job = job;
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model12Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model12.Customer).Assembly, typeof (model12.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model12.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model12.Customer>().First();
          var location = new model12.Location();
          var job = new model12.Job(session, customer, location);
          new model12.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model13Test()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      Require.ProviderIsNot(StorageProvider.PostgreSql | StorageProvider.Sqlite | StorageProvider.Oracle);//do not use Sorting before insert
      var config = BuildConfiguration();
      config.Types.Register(typeof (model13.Customer).Assembly, typeof (model13.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model13.Customer();
          tx.Complete();
        }

        Assert.Throws<CheckConstraintViolationException>(() => {
          using (var session = domain.OpenSession())
          using (session.Activate())
          using (var tx = session.OpenTransaction()) {
            var customer = session.Query.All<model13.Customer>().First();
            var location = new model13.Location();
            var job = new model13.Job(session, customer, location);
            new model13.Invoice(session, job);
            tx.Complete();
          }
        });
      }
    }

    [Test]
    public void Model13TestForMySql()
    {
      Require.ProviderIs(StorageProvider.MySql | StorageProvider.SqlServerCe);
      var config = BuildConfiguration();
      config.Types.Register(typeof (model13.Customer).Assembly, typeof (model13.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model13.Customer();
          tx.Complete();
        }

        Assert.Throws<StorageException>(() => {
          using (var session = domain.OpenSession())
          using (session.Activate())
          using (var tx = session.OpenTransaction()) {
            var customer = session.Query.All<model13.Customer>().First();
            var location = new model13.Location();
            var job = new model13.Job(session, customer, location);
            new model13.Invoice(session, job);
            tx.Complete();
          }
        });
      }
    }

    [Test]
    public void Model14Test()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      Require.ProviderIsNot(StorageProvider.PostgreSql | StorageProvider.Sqlite | StorageProvider.Oracle);//do not use Sorting before insert
      var config = BuildConfiguration();
      config.Types.Register(typeof (model14.Customer).Assembly, typeof (model14.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model14.Customer();
          tx.Complete();
        }

        Assert.Throws<CheckConstraintViolationException>(() => {
          using (var session = domain.OpenSession())
          using (session.Activate())
          using (var tx = session.OpenTransaction()) {
            var customer = session.Query.All<model14.Customer>().First();
            var location = new model14.Location();
            var job = new model14.Job(session, customer, location);
            new model14.Invoice(session, job);
            tx.Complete();
          }
        });
      }
    }

    [Test]
    public void Model14TestForMySql()
    {
      Require.ProviderIs(StorageProvider.MySql | StorageProvider.SqlServerCe);

      var config = BuildConfiguration();
      config.Types.Register(typeof (model14.Customer).Assembly, typeof (model14.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model14.Customer();
          tx.Complete();
        }

        Assert.Throws<StorageException>(() => {
          using (var session = domain.OpenSession())
          using (session.Activate())
          using (var tx = session.OpenTransaction()) {
            var customer = session.Query.All<model14.Customer>().First();
            var location = new model14.Location();
            var job = new model14.Job(session, customer, location);
            new model14.Invoice(session, job);
            tx.Complete();
          }
        });
      }
    }

    [Test]
    public void Model15Test()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      Require.ProviderIsNot(StorageProvider.PostgreSql | StorageProvider.Sqlite | StorageProvider.Oracle);//do not use Sorting before insert
      var configuration = BuildConfiguration();
      configuration.Types.Register(typeof (model15.A).Assembly, typeof (model15.A).Namespace);

      Assert.Throws<CheckConstraintViolationException>(() => {
        using (var domain = Domain.Build(configuration)) {
          using (var session = domain.OpenSession())
          using (session.Activate())
          using (var transaction = session.OpenTransaction()) {
            var a = new model15.A();
            var b = new model15.B();
            a.B = b;
            var c = new model15.C();
            b.C = c;
            var d = new model15.D();
            d.A = a;
            c.D = d;
            var nnnn = new model15.NNNN() { NNN = new model15.NNN() { NN = new model15.NN { A = a } } };
            a.NN = nnnn.NNN.NN;
            transaction.Complete();
          }
        }
      });
    }

    [Test]
    public void Model15TestForMySQl()
    {
      Require.ProviderIs(StorageProvider.MySql | StorageProvider.SqlServerCe);

      var configuration = BuildConfiguration();
      configuration.Types.Register(typeof (model15.A).Assembly, typeof (model15.A).Namespace);

      Assert.Throws<StorageException>(() => {
        using (var domain = Domain.Build(configuration)) {
          using (var session = domain.OpenSession())
          using (session.Activate())
          using (var transaction = session.OpenTransaction()) {
            var a = new model15.A();
            var b = new model15.B();
            a.B = b;
            var c = new model15.C();
            b.C = c;
            var d = new model15.D();
            d.A = a;
            c.D = d;
            var nnnn = new model15.NNNN() { NNN = new model15.NNN() { NN = new model15.NN { A = a } } };
            a.NN = nnnn.NNN.NN;
            transaction.Complete();
          }
        }
      });
    }

    [Test]
    public void Model16Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model16.Customer).Assembly, typeof (model16.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession()) 
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model16.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate()) 
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model16.Customer>().First();
          var location = new model16.Location();
          var job = new model16.Job(session, customer, location);
          new model16.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model17Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model17.Customer).Assembly, typeof (model17.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model17.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model17.Customer>().First();
          var location = new model17.Location();
          var job = new model17.Job(session, customer, location);
          new model17.Invoice(session, job);
          tx.Complete();
        }
      }
    }
    [Test]
    public void Model18Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model18.Customer).Assembly, typeof (model18.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model18.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model18.Customer>().First();
          var location = new model18.Location();
          var job = new model18.Job(session, customer, location);
          new model18.Invoice(session, job);
          tx.Complete();
        }
      }
    }
    [Test]
    public void Model19Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model19.Customer).Assembly, typeof (model19.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model19.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model19.Customer>().First();
          var location = new model19.Location();
          var job = new model19.Job(session, customer, location);
          new model19.Invoice(session, job);
          tx.Complete();
        }
      }
    }
    [Test]
    public void Model20Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model20.Customer).Assembly, typeof (model20.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model20.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model20.Customer>().First();
          var location = new model20.Location();
          var job = new model20.Job(session, customer, location);
          new model20.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model21Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model21.Customer).Assembly, typeof (model21.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model21.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model21.Customer>().First();
          var location = new model21.Location();
          var job = new model21.Job(session, customer, location);
          new model21.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model22Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model22.Customer).Assembly, typeof (model22.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model22.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model22.Customer>().First();
          var location = new model22.Location();
          var job = new model22.Job(session, customer, location);
          new model22.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model23Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model23.Customer).Assembly, typeof (model23.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model23.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model23.Customer>().First();
          var location = new model23.Location();
          var job = new model23.Job(session, customer, location);
          new model23.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model24Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model24.Customer).Assembly, typeof (model24.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model24.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model24.Customer>().First();
          var location = new model24.Location();
          var job = new model24.Job(session, customer, location);
          new model24.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model25Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model25.Customer).Assembly, typeof (model25.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model25.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model25.Customer>().First();
          var location = new model25.Location();
          var job = new model25.Job(session, customer, location);
          new model25.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model26Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model26.Customer).Assembly, typeof (model26.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model26.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model26.Customer>().First();
          var location = new model26.Location();
          var job = new model26.Job(session, customer, location);
          new model26.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    [Test]
    public void Model27Test()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (model27.Customer).Assembly, typeof (model27.Customer).Namespace);

      using (var domain = Domain.Build(config)) {
        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          new model27.Customer();
          tx.Complete();
        }

        using (var session = domain.OpenSession())
        using (session.Activate())
        using (var tx = session.OpenTransaction()) {
          var customer = session.Query.All<model27.Customer>().First();
          var location = new model27.Location();
          var job = new model27.Job(session, customer, location);
          new model27.Invoice(session, job);
          tx.Complete();
        }
      }
    }

    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
