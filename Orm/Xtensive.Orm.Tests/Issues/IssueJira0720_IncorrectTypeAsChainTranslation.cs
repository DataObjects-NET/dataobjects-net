// Copyright (C) 2019-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kudelin
// Created:    2019.01.10

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0720_IncorrectTypeAsChainTranslationModels;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0720_IncorrectTypeAsChainTranslation : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(ITestEntity).Assembly, typeof(ITestEntity).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    [Test]
    public void Test1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var te3WithFilledEntitySet = new TestEntity3(session) {
          InterfaceRefCollection = { new TestEntity1(session), new TestEntity2(session), new TestEntity3(session) }
        };
        var te3WithEmptyEntitySet = new TestEntity3(session);

        var te21 = new TestEntity2(session) { InterfaceRef = te3WithEmptyEntitySet };
        var te22 = new TestEntity2(session) { InterfaceRef = te3WithFilledEntitySet };
        var te23 = new TestEntity2(session) { InterfaceRef = new TestEntity1(session) };

        _ = new TestEntity1(session) { InterfaceRef = te21 };
        _ = new TestEntity1(session) { InterfaceRef = te22 };
        _ = new TestEntity1(session) { InterfaceRef = te23 };
        _ = _ = new TestEntity1(session) { InterfaceRef = new TestEntity2(session) };

        var serverResults = session.Query.All<TestEntity1>()
          .Where(x => ((x.InterfaceRef as TestEntity2).InterfaceRef as TestEntity3).InterfaceRefCollection.Any()).ToArray();

        Assert.That(serverResults.Length, Is.EqualTo(1));
        var result = serverResults[0];
        var a = result.InterfaceRef as TestEntity2;

        Assert.That(a, Is.Not.Null);
        Assert.That(a, Is.EqualTo(te22));

        var b = a.InterfaceRef as TestEntity3;
        Assert.That(b, Is.Not.Null);
        Assert.That(b, Is.EqualTo(te3WithFilledEntitySet));
        Assert.That(b.InterfaceRefCollection.Count, Is.EqualTo(3));

        Assert.That(b.InterfaceRefCollection.OfType<TestEntity1>().Any(), Is.True);
        Assert.That(b.InterfaceRefCollection.OfType<TestEntity2>().Any(), Is.True);
        Assert.That(b.InterfaceRefCollection.OfType<TestEntity3>().Any(), Is.True);
      }
    }

    [Test]
    public void Test2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entityForES1 = new TestEntity2(session);
        var entityForES2 = new TestEntity2(session);
        var entityForES3 = new TestEntity2(session);
        var lvl1DirectRef = new TestEntity1(session) { DirectRefCollection = { entityForES1, entityForES2, entityForES3 } };
        var lvl2DirectRef = new TestEntity3(session) { DirectEntityRef = lvl1DirectRef };
        var lvl3DirectRef = new TestEntity2(session) { DirectEntityRef = lvl2DirectRef };

        _ = new TestEntity1(session) {
          InterfaceRef = new TestEntity2(session) {
            InterfaceRef = new TestEntity3(session) {
              InterfaceRef = new TestEntity1(session) {
                DirectEntityRef = lvl3DirectRef
              }
            }
          }
        };
        _ = new TestEntity1(session) {
          InterfaceRef = new TestEntity2(session) {
            InterfaceRef = new TestEntity3(session) {
              InterfaceRef = new TestEntity1(session)
            }
          }
        };
        _ = new TestEntity1(session) {
          InterfaceRef = new TestEntity2(session) {
            InterfaceRef = new TestEntity3(session) {
              InterfaceRef = new TestEntity2(session)
            }
          }
        };
        _ = new TestEntity1(session) { InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity3(session) } };
        _ = new TestEntity1(session) { InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity1(session) } };
        _ = new TestEntity1(session) { InterfaceRef = new TestEntity2(session) };

        var queryResults = session.Query.All<TestEntity1>()
          .Where(x =>
            (((x.InterfaceRef as TestEntity2).InterfaceRef as TestEntity3)
              .InterfaceRef as TestEntity1).DirectEntityRef.DirectEntityRef.DirectEntityRef.DirectRefCollection.Any())
          .ToArray();

        Assert.That(queryResults.Length, Is.EqualTo(1));
        var a = queryResults[0];

        var b = a.InterfaceRef as TestEntity2;
        Assert.That(b, Is.Not.Null);

        var c = b.InterfaceRef as TestEntity3;
        Assert.That(c, Is.Not.Null);

        var d = c.InterfaceRef as TestEntity1;
        Assert.That(d, Is.Not.Null);

        var entitySetOwner = d.DirectEntityRef.DirectEntityRef.DirectEntityRef;
        Assert.That(entitySetOwner.DirectRefCollection.Count, Is.EqualTo(3));
        Assert.That(entitySetOwner.DirectRefCollection.Any(e => e.Id == entityForES1.Id), Is.True);
        Assert.That(entitySetOwner.DirectRefCollection.Any(e => e.Id == entityForES2.Id), Is.True);
        Assert.That(entitySetOwner.DirectRefCollection.Any(e => e.Id == entityForES3.Id), Is.True);
      }
    }

    [Test]
    public void Test3()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        PopulateTest3Data(session);

        var queryResults = session.Query.All<TestEntity1>().Where(
          x => (((x.DirectEntityRef.DirectEntityRef.InterfaceRef as TestEntity2).DirectEntityRef.InterfaceRef as TestEntity3)
              .InterfaceRef as TestEntity1).DirectEntityRef.Structure.ValueS3.InterfaceRefCollection.Any()).ToArray();

        Assert.That(queryResults.Length, Is.EqualTo(1));

        var testEntity1 = queryResults[0];
        var testEntity2 = testEntity1.DirectEntityRef.DirectEntityRef.InterfaceRef as TestEntity2;
        Assert.That(testEntity2, Is.Not.Null);

        var testEntity3 = testEntity2.DirectEntityRef.InterfaceRef as TestEntity3;
        Assert.That(testEntity3, Is.Not.Null);

        var testEntity11 = testEntity3.InterfaceRef as TestEntity1;
        Assert.That(testEntity11, Is.Not.Null);
        Assert.That(testEntity11.DirectEntityRef.Structure.ValueS3.InterfaceRefCollection.Count, Is.EqualTo(3));
        Assert.That(testEntity11.DirectEntityRef.Structure.ValueS3.InterfaceRefCollection.OfType<TestEntity1>().Any(), Is.True);
        Assert.That(testEntity11.DirectEntityRef.Structure.ValueS3.InterfaceRefCollection.OfType<TestEntity2>().Any(), Is.True);
        Assert.That(testEntity11.DirectEntityRef.Structure.ValueS3.InterfaceRefCollection.OfType<TestEntity3>().Any(), Is.True);
      }
    }

    [Test]
    public void Test4()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntity1(session);
        _ = new TestEntity1(session) { DirectRefCollection = { new TestEntity2(session) { InterfaceRef = new TestEntity2(session) } } };
        _ = new TestEntity1(session) {
          DirectRefCollection = { new TestEntity2(session) { InterfaceRef = new TestEntity2(session) { InterfaceRefCollection = { new TestEntity1(session) }} } }
        };
        _ = new TestEntity1(session) { DirectRefCollection = { new TestEntity2(session), new TestEntity2(session) { InterfaceRef = new TestEntity3(session) } } };
        var descendingResult = new TestEntity1(session) {
          DirectRefCollection = { new TestEntity2(session), new TestEntity2(session) { InterfaceRef = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } } }
        };
        var ascendingResult = new TestEntity1(session) {
          DirectRefCollection = { new TestEntity2(session) { InterfaceRef = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }, new TestEntity2(session) } 
        };

        var results = session.Query.All<TestEntity1>()
          .Where(x => ((x.DirectRefCollection.FirstOrDefault() as TestEntity2).InterfaceRef as TestEntity3).InterfaceRefCollection.Any())
          .ToArray();
        Assert.That(results.Length, Is.EqualTo(1));
        Assert.That(results[0], Is.EqualTo(ascendingResult));

        results = session.Query.All<TestEntity1>()
          .Where(x => ((x.DirectRefCollection.OrderByDescending(i => i.Id).FirstOrDefault() as TestEntity2).InterfaceRef as TestEntity3).InterfaceRefCollection.Any())
          .ToArray();

        Assert.That(results.Length, Is.EqualTo(1));
        Assert.That(results[0], Is.EqualTo(descendingResult));
        results = session.Query.All<TestEntity1>()
          .Where(x => ((x.DirectRefCollection.OrderBy(i => i.Id).FirstOrDefault() as TestEntity2).InterfaceRef as TestEntity3).InterfaceRefCollection.Any())
          .ToArray();

        Assert.That(results.Length, Is.EqualTo(1));
        Assert.That(results[0], Is.EqualTo(ascendingResult));
      }
    }

    [Test]
    public void Test5()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        #region Test data

        PopulateTest5Data(session);
        #endregion

        var results = session.Query.All<TestEntity1>()
          .Where(x => ((x.Structure.ValueS3.InterfaceRef as TestEntity2).InterfaceRef as TestEntity3).InterfaceRefCollection.Any())
          .ToArray();

        Assert.That(results.Length, Is.EqualTo(1));
        var testEntity = results[0];
        Assert.That(testEntity.Structure.ValueS1, Is.EqualTo("MMM"));
      }
    }

    [Test]
    public void Test6()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntity1(session);
        _ = new TestEntity1(session);
        _ = new TestEntity1(session) { InterfaceRef = new TestEntity1(session) };
        _ = new TestEntity1(session) { InterfaceRef = new TestEntity1(session) };
        _ = new TestEntity1(session) { InterfaceRef = new TestEntity2(session) { InterfaceRefCollection = { new TestEntity1(session) } } };
        _ = new TestEntity1(session) { InterfaceRef = new TestEntity2(session) };
        _ = new TestEntity1(session) { InterfaceRef = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } };
        _ = new TestEntity1(session) { InterfaceRef = new TestEntity3(session) };

        var results = session.Query.All<TestEntity1>().Select(y => (y.InterfaceRef as TestEntity2).InterfaceRefCollection.Any()).ToArray();
        Assert.That(results.Length, Is.EqualTo(12));
        Assert.That(results.Count(b => b), Is.EqualTo(1));
        Assert.That(results.Count(b => !b), Is.EqualTo(11));
      }
    }

    [Test]
    public void Test7()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntity2(session) { InterfaceRef = new TestEntity1(session) { InterfaceRef = new TestEntity1(session) } };
        _ = new TestEntity2(session) { InterfaceRef = new TestEntity1(session) { InterfaceRef = new TestEntity2(session) } };
        _ = new TestEntity2(session) { InterfaceRef = new TestEntity1(session) { InterfaceRef = new TestEntity3(session) } };
        _ = new TestEntity2(session) { InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity1(session) } };
        _ = new TestEntity2(session) { InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity2(session) } };
        _ = new TestEntity2(session) { InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity3(session), InterfaceRefCollection = { new TestEntity2(session) } } };
        _ = new TestEntity2(session) { InterfaceRef = new TestEntity3(session) { InterfaceRef = new TestEntity1(session) } };
        _ = new TestEntity2(session) { InterfaceRef = new TestEntity3(session) { InterfaceRef = new TestEntity2(session) } };
        _ = new TestEntity2(session) { InterfaceRef = new TestEntity3(session) { InterfaceRef = new TestEntity3(session), InterfaceRefCollection = { new TestEntity1(session) } } };

        var results = session.Query.All<TestEntity1>()
          .Select((y => (y as ITestEntity as TestEntity3).InterfaceRefCollection.Any())).ToArray();
        Assert.That(results.Count(r => r), Is.EqualTo(0));
        Assert.That(results.Count(r => !r), Is.EqualTo(7));

        results = session.Query.All<TestEntity2>()
          .Select((y => (y as ITestEntity as TestEntity3).InterfaceRefCollection.Any())).ToArray();
        Assert.That(results.Count(r => r), Is.EqualTo(0));
        Assert.That(results.Count(r => !r), Is.EqualTo(16));

        results = session.Query.All<TestEntity3>()
          .Select((y => (y as ITestEntity as TestEntity3).InterfaceRefCollection.Any())).ToArray();
        Assert.That(results.Count(r => r), Is.EqualTo(1));
        Assert.That(results.Count(r => !r), Is.EqualTo(5));
      }
    }

    [Test]
    public void Test8()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var a = new TestEntity2(session) { InterfaceRef = new TestEntity1(session) { InterfaceRef = new TestEntity1(session) } };
        var b = new TestEntity2(session) { InterfaceRef = new TestEntity1(session) { InterfaceRef = new TestEntity2(session) } };
        var c = new TestEntity2(session) { InterfaceRef = new TestEntity1(session) { InterfaceRef = new TestEntity3(session) } };
        var d = new TestEntity2(session) { InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity1(session) } };
        var e = new TestEntity2(session) { InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity2(session) } };
        var f = new TestEntity2(session) { InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity3(session), InterfaceRefCollection = { new TestEntity2(session) } } };
        var g = new TestEntity2(session) { InterfaceRef = new TestEntity3(session) { InterfaceRef = new TestEntity1(session) } };
        var h = new TestEntity2(session) { InterfaceRef = new TestEntity3(session) { InterfaceRef = new TestEntity2(session) } };
        var i = new TestEntity2(session) { InterfaceRef = new TestEntity3(session) { InterfaceRef = new TestEntity3(session), InterfaceRefCollection = { new TestEntity1(session) } } };

        _ = new TestEntity1(session) { InterfaceRef = a, DirectRefCollection = { new TestEntity2(session) } };
        _ = new TestEntity1(session) { InterfaceRef = b, DirectRefCollection = { new TestEntity2(session) } };
        _ = new TestEntity1(session) { InterfaceRef = c, DirectRefCollection = { new TestEntity2(session) } };
        _ = new TestEntity1(session) { InterfaceRef = d, DirectRefCollection = { new TestEntity2(session) } };
        _ = new TestEntity1(session) { InterfaceRef = e, DirectRefCollection = { new TestEntity2(session) } };
        _ = new TestEntity1(session) { InterfaceRef = f, DirectRefCollection = { new TestEntity2(session) } };
        _ = new TestEntity1(session) { InterfaceRef = g, DirectRefCollection = { new TestEntity2(session) } };
        _ = new TestEntity1(session) { InterfaceRef = h, DirectRefCollection = { new TestEntity2(session) } };
        _ = new TestEntity1(session) { InterfaceRef = i, DirectRefCollection = { new TestEntity2(session) } };

        var results = session.Query.All<TestEntity1>()
          .Where(x => x.DirectRefCollection.Any(y => ((x.InterfaceRef as TestEntity2).InterfaceRef as TestEntity3).InterfaceRefCollection.Any()))
          .ToArray();

        Assert.That(results.Length, Is.EqualTo(1));
        var result = results[0];
        Assert.That(result.DirectRefCollection.Any(), Is.True);

        var ref1 = result.InterfaceRef as TestEntity2;
        Assert.That(ref1, Is.Not.Null);

        var ref2 = ref1.InterfaceRef as TestEntity3;
        Assert.That(ref2, Is.Not.Null);
        Assert.That(ref2.InterfaceRefCollection.Any(), Is.True);
        Assert.That(ref2.InterfaceRefCollection.OfType<TestEntity1>().Any());
      }
    }

    [Test]
    public void Test9()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        #region TestData

        PopulateTest9Data(session);

        #endregion

        var noResults = session.Query.All<TestEntity1>().Where(
          x => x.DirectRefCollection.Any(
            y => y.InterfaceRefCollection.Any(
              z =>
                ((x.InterfaceRef as TestEntity2).InterfaceRef.Structure.ValueS2.ValueS3 as ITestEntity as TestEntity1).DirectRefCollection.Any() ||
                ((y.InterfaceRef as TestEntity2).InterfaceRef.Structure.ValueS2.ValueS3 as ITestEntity as TestEntity1).DirectRefCollection.Any() ||
                ((z.Structure.ValueS2.ValueS3 as ITestEntity as TestEntity1).DirectRefCollection.Any()
                )))).ToArray();
        Assert.That(noResults.Length, Is.EqualTo(0));

        noResults = session.Query.All<TestEntity1>().Where(
          x => x.DirectRefCollection.Any(
            y => y.InterfaceRefCollection.Any(
              z =>
                ((x.InterfaceRef as TestEntity2).InterfaceRef.Structure.ValueS2.ValueS3 as ITestEntity as TestEntity2).InterfaceRefCollection.Any() ||
                ((y.InterfaceRef as TestEntity2).InterfaceRef.Structure.ValueS2.ValueS3 as ITestEntity as TestEntity2).InterfaceRefCollection.Any() ||
                ((z.Structure.ValueS2.ValueS3 as ITestEntity as TestEntity2).InterfaceRefCollection.Any()
                )))).ToArray();
        Assert.That(noResults.Length, Is.EqualTo(0));

        var hasResults = session.Query.All<TestEntity1>().Where(
          x => x.DirectRefCollection.Any(
            y => y.InterfaceRefCollection.Any(
              z =>
                ((x.InterfaceRef as TestEntity2).InterfaceRef.Structure.ValueS2.ValueS3 as ITestEntity as TestEntity3).InterfaceRefCollection.Any() ||
                ((y.InterfaceRef as TestEntity2).InterfaceRef.Structure.ValueS2.ValueS3 as ITestEntity as TestEntity3).InterfaceRefCollection.Any() ||
                ((z.Structure.ValueS2.ValueS3 as ITestEntity as TestEntity3).InterfaceRefCollection.Any()
                )))).ToArray();
        Assert.That(hasResults.Length, Is.EqualTo(2));
      }
    }

    private static void PopulateTest3Data(Session session)
    {
      var entityForES1 = new TestEntity1(session);
      var entityForES2 = new TestEntity2(session);
      var entityForES3 = new TestEntity3(session);

      _ = new TestEntity1(session) {
        DirectEntityRef = new TestEntity2(session) {
          DirectEntityRef = new TestEntity3(session) {
            InterfaceRef = new TestEntity2(session) {
              DirectEntityRef = new TestEntity3(session) {
                InterfaceRef = new TestEntity3(session) {
                  InterfaceRef = new TestEntity1(session) {
                    DirectEntityRef = new TestEntity2(session) {
                      Structure = new TestStructure1(session) {
                        ValueS3 = new TestEntity3(session) {
                          InterfaceRefCollection = { entityForES1, entityForES2, entityForES3 }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      };

      _ = new TestEntity1(session) {
        DirectEntityRef = new TestEntity2(session) {
          DirectEntityRef = new TestEntity3(session) {
            InterfaceRef = new TestEntity2(session) {
              DirectEntityRef = new TestEntity3(session) {
                InterfaceRef = new TestEntity3(session) {
                  InterfaceRef = new TestEntity1(session) {
                    DirectEntityRef = new TestEntity2(session) {
                      Structure = new TestStructure1(session) {
                        ValueS3 = new TestEntity3(session)
                      }
                    }
                  }
                }
              }
            }
          }
        }
      };

      _ = new TestEntity1(session) {
        DirectEntityRef = new TestEntity2(session) {
          DirectEntityRef = new TestEntity3(session) {
            InterfaceRef = new TestEntity2(session) {
              DirectEntityRef = new TestEntity3(session) {
                InterfaceRef = new TestEntity3(session) {
                  InterfaceRef = new TestEntity2(session)
                }
              }
            }
          }
        }
      };

      _ = new TestEntity1(session) {
        DirectEntityRef = new TestEntity2(session) {
          DirectEntityRef = new TestEntity3(session) {
            InterfaceRef = new TestEntity2(session) { DirectEntityRef = new TestEntity3(session) { InterfaceRef = new TestEntity2(session) } }
          }
        }
      };

      _ = new TestEntity1(session) {
        DirectEntityRef = new TestEntity2(session) {
          DirectEntityRef = new TestEntity3(session) {
            InterfaceRef = new TestEntity3(session)
          }
        }
      };
    }

    private static void PopulateTest5Data(Session session)
    {
      _ = new TestEntity1(session);
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) { ValueS1 = "AAA" }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "BBB",
          ValueS3 = new TestEntity3(session)
        }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "CCC",
          ValueS3 = new TestEntity3(session) { InterfaceRef = new TestEntity1(session) }
        }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "DDD",
          ValueS3 = new TestEntity3(session) { InterfaceRef = new TestEntity2(session) }
        }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "EEE",
          ValueS3 = new TestEntity3(session) { InterfaceRef = new TestEntity3(session) }
        }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "FFF",
          ValueS3 = new TestEntity3(session) { InterfaceRef = new TestEntity2(session) }
        }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "GGG",
          ValueS3 = new TestEntity3(session) { InterfaceRef = new TestEntity2(session) }
        }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "HHH",
          ValueS3 = new TestEntity3(session) { InterfaceRef = new TestEntity2(session) }
        }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "III",
          ValueS3 = new TestEntity3(session) {
            InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity1(session) }
          }
        }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "GGG",
          ValueS3 = new TestEntity3(session) {
            InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity1(session) }
          }
        }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "KKK",
          ValueS3 = new TestEntity3(session) {
            InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity2(session) }
          }
        }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "LLL",
          ValueS3 = new TestEntity3(session) {
            InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity2(session) }
          }
        }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "MMM",
          ValueS3 = new TestEntity3(session) {
            InterfaceRef = new TestEntity2(session) {
              InterfaceRef = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "NNN",
          ValueS3 = new TestEntity3(session) {
            InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity3(session) }
          }
        }
      };
      _ = new TestEntity1(session) {
        Structure = new TestStructure1(session) {
          ValueS1 = "OOO",
          ValueS3 = new TestEntity3(session) {
            InterfaceRef = new TestEntity2(session) { InterfaceRef = new TestEntity3(session) }
          }
        }
      };
    }

    private static void PopulateTest9Data(Session session)
    {
      var esEntity1 = new TestEntity2(session);
      _ = esEntity1.InterfaceRefCollection
        .Add(new TestEntity1(session) {
          InterfaceRef = new TestEntity1(session) {
            Structure = new TestStructure1(session) {
              ValueS2 = new TestStructure2(session) {
                ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
              }
            }
          }
        });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity3(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session)
            }
          }
        }
      });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) }
          }
        }
      });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity3(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) }
          }
        }
      });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) }
          }
        }
      });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity3(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) }
          }
        }
      });
      _ = esEntity1.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) }
          }
        }
      });

      var esEntity2 = new TestEntity2(session);
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity3(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity3(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity3(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session)
            }
          }
        }
      });
      _ = esEntity2.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session)
            }
          }
        }
      });

      var esEntity3 = new TestEntity2(session);
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity3(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity3(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity3(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });
      _ = esEntity3.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });

      var esEntity4 = new TestEntity2(session);
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity3(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity1(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session)
            }
          }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) {
                InterfaceRefCollection = {
                    new TestEntity1(session)
                  }
              }
            }
          }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity3(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } }
          }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity3(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) {
              ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } }
            }
          }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity2(session) {
          Structure = new TestStructure1(session) {
            ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) }
          }
        }
      });
      _ = esEntity4.InterfaceRefCollection.Add(new TestEntity2(session) {
        InterfaceRef = new TestEntity1(session) {
          Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } }
        }
      });

      var esEntity5 = new TestEntity2(session);
      _ = esEntity5.InterfaceRefCollection.Add(new TestEntity1(session) { Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } } } });
      _ = esEntity5.InterfaceRefCollection.Add(new TestEntity2(session) { Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } } } });
      _ = esEntity5.InterfaceRefCollection.Add(new TestEntity3(session) { Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } } });
      _ = esEntity5.InterfaceRefCollection.Add(new TestEntity3(session) { Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } } });

      var esEntity6 = new TestEntity2(session);
      _ = esEntity6.InterfaceRefCollection.Add(new TestEntity1(session) { Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } } } });
      _ = esEntity6.InterfaceRefCollection.Add(new TestEntity2(session) { Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) { InterfaceRefCollection = { new TestEntity1(session) } } } } });
      _ = esEntity6.InterfaceRefCollection.Add(new TestEntity3(session) { Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } } });
      _ = esEntity6.InterfaceRefCollection.Add(new TestEntity3(session) { Structure = new TestStructure1(session) { ValueS2 = new TestStructure2(session) { ValueS3 = new TestEntity3(session) } } });

      var testEntity1 = new TestEntity1(session);
      _ = testEntity1.DirectRefCollection.Add(esEntity1);
      _ = testEntity1.DirectRefCollection.Add(esEntity3);
      _ = testEntity1.DirectRefCollection.Add(esEntity5);

      var testEntity2 = new TestEntity1(session);
      _ = testEntity2.DirectRefCollection.Add(esEntity2);
      _ = testEntity2.DirectRefCollection.Add(esEntity4);
      _ = testEntity2.DirectRefCollection.Add(esEntity6);
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0720_IncorrectTypeAsChainTranslationModels
{
  public interface ITestEntity : IEntity
  {
    [Field, Key]
    int Id { get; }

    [Field]
    TestStructure1 Structure { get; set; }
  }

  public abstract class IntefaceImplementorEntity : Entity, ITestEntity
  {
    public int Id { get; private set; }

    public TestStructure1 Structure { get; set; }

    public IntefaceImplementorEntity(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class TestEntity1 : IntefaceImplementorEntity
  {
    [Field]
    public ITestEntity InterfaceRef { get; set; }

    [Field]
    public TestEntity2 DirectEntityRef { get; set; }

    [Field]
    public EntitySet<TestEntity2> DirectRefCollection { get; set; }

    public TestEntity1(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class TestEntity2 : IntefaceImplementorEntity
  {
    [Field]
    public ITestEntity InterfaceRef { get; set; }

    [Field]
    public TestEntity3 DirectEntityRef { get; set; }

    [Field]
    public EntitySet<ITestEntity> InterfaceRefCollection { get; set; }

    public TestEntity2(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class TestEntity3 : IntefaceImplementorEntity
  {
    [Field]
    public ITestEntity InterfaceRef { get; set; }

    [Field]
    public TestEntity1 DirectEntityRef { get; set; }

    [Field]
    public EntitySet<ITestEntity> InterfaceRefCollection { get; set; }

    public TestEntity3(Session session)
      : base(session)
    {
    }
  }

  public class TestStructure1 : Structure
  {
    [Field]
    public string ValueS1 { get; set; }

    [Field]
    public TestStructure2 ValueS2 { get; set; }

    [Field]
    public TestEntity3 ValueS3 { get; set; }

    public TestStructure1(Session session)
      : base(session)
    {
    }
  }

  public class TestStructure2 : Structure
  {
    [Field]
    public string ValueS1 { get; set; }

    [Field]
    public decimal ValueS2 { get; set; }

    [Field]
    public TestEntity3 ValueS3 { get; set; }

    public TestStructure2(Session session)
      : base(session)
    {
    }
  }
}
