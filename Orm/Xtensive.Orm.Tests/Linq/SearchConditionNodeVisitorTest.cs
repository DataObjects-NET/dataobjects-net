using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.FullTextSearchCondition;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Internals;
using Xtensive.Orm.FullTextSearchCondition.Nodes;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Linq.SearchConditionNodeTranslationModel;

namespace Xtensive.Orm.Tests.Linq.SearchConditionNodeTranslationModel
{
  [HierarchyRoot]
  public class TestEntityWithFullText : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    [FullText("English")]
    public string Title { get; set; }

    [Field]
    [FullText("English")]
    public string Text { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  public class SearchConditionNodeVisitorTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.SingleKeyRankTableFullText);
    }

    [Test]
    public void Test001()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.SimpleTerm("abc");
      string expectedTranslation = "abc";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test002()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.SimpleTerm("");
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test003()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.SimpleTerm(" ");
      string expectedTranslation = "dummy".Trim();
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test004()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.SimpleTerm(null);
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test005()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.SimpleTerm(" abc ");
      string expectedTranslation = "abc";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test006()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.SimpleTerm("Once upon a time");
      string expectedTranslation = "\"Once upon a time\"";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test007()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.PrefixTerm("dis");
      string expectedTranslation = "\"dis*\"";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test008()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.PrefixTerm("");
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test009()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.PrefixTerm(" ");
      string expectedTranslation = "dummy".Trim();
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test010()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.PrefixTerm(null);
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test011()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.PrefixTerm(" dis ");
      string expectedTranslation = "\"dis*\"";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test012()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.PrefixTerm("kill all humans");
      string expectedTranslation = "\"kill all humans*\"";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test013()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenerationTerm(GenerationType.Inflectional, new[] {"abc"});
      string expectedTranslation = "FORMSOF (INFLECTIONAL, abc)";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test014()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenerationTerm(GenerationType.Inflectional, new string[0]);
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test015()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenerationTerm(GenerationType.Inflectional, null);
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test016()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenerationTerm(GenerationType.Inflectional, new[] { "abc", "" });
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test017()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenerationTerm(GenerationType.Inflectional, new[] { "abc", null });
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test018()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenerationTerm(GenerationType.Inflectional, new[] { "abc", "      " });
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test019()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenerationTerm(GenerationType.Thesaurus, new[] {"abc"});
      string expectedTranslation = "FORMSOF (THESAURUS, abc)";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test020()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenerationTerm(GenerationType.Thesaurus, new[] {"abc", "def", "ghi"});
      string expectedTranslation = "FORMSOF (THESAURUS, abc, def, ghi)";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test021()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenerationTerm(GenerationType.Thesaurus, new[] {" abc ", "def ", " ghi"});
      string expectedTranslation = "FORMSOF (THESAURUS, abc, def, ghi)";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test022()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenerationTerm(GenerationType.Thesaurus, new[] {"a b c", "d f g"});
      string expectedTranslation = "FORMSOF (THESAURUS, \"a b c\", \"d f g\")";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test023()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenerationTerm(GenerationType.Thesaurus, new[] {"Because I do not listen it does not mean I do not care"});
      string expectedTranslation = "FORMSOF (THESAURUS, \"Because I do not listen it does not mean I do not care\")";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test024()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.SimpleTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test025()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.PrefixTerm("abc"));
      string expectedTranslation = "";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test026()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.SimpleTerm("").NearSimpleTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test027()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm(""));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test028()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.SimpleTerm(null).NearSimpleTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test029()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm(null));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test030()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.SimpleTerm("    ").NearSimpleTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test031()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm("   "));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test032()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.PrefixTerm("").NearPrefixTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test033()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.PrefixTerm("abc").NearPrefixTerm(""));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test034()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.PrefixTerm(null).NearPrefixTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test035()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.PrefixTerm("abc").NearPrefixTerm(null));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test036()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.PrefixTerm("").NearPrefixTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test037()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.PrefixTerm("    ").NearPrefixTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test038()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.PrefixTerm("abc").NearPrefixTerm("    "));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test039()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm(" def ").NearSimpleTerm(" sdf rrr "));
      string expectedTranslation = "abc NEAR def NEAR \"sdf rrr\"";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test040()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm("def").NearPrefixTerm("ghi"));
      string expectedTranslation = "abc NEAR def NEAR \"ghi*\"";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test041()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.SimpleTerm("abc").NearPrefixTerm("ghi").NearSimpleTerm("jkl"));
      string expectedTranslation = "abc NEAR \"ghi*\" NEAR jkl";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test042()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.SimpleTerm("abc").NearPrefixTerm("ghi").NearPrefixTerm("jkl"));
      string expectedTranslation = "abc NEAR \"ghi*\" NEAR \"jkl*\"";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test043()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.GenericProximityTerm(f => f.PrefixTerm(" abc ").NearPrefixTerm(" def ghi ").NearPrefixTerm("aaaa"));
      string expectedTranslation = "\"abc*\" NEAR \"def ghi*\" NEAR \"aaaa*\"";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test044()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test045()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.PrefixTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test046()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("").NearSimpleTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test047()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm(""));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test048()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("   ").NearSimpleTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test049()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm("   "));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test050()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm(null).NearSimpleTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test051()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm(null));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test052()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.PrefixTerm("abc").NearPrefixTerm(""));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test053()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.PrefixTerm("").NearPrefixTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test054()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.PrefixTerm("abc").NearPrefixTerm("   "));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test055()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.PrefixTerm("   ").NearPrefixTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test056()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.PrefixTerm("abc").NearPrefixTerm(null));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test057()
    {
      Require.ProviderVersionAtLeast(new Version(11,0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.PrefixTerm(null).NearPrefixTerm("abc"));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test058()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm(" def ").NearSimpleTerm(" aaaaa b c "));
      string expectedTranslation = "NEAR (abc, def, \"aaaaa b c\")";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test059()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm(" def ").NearPrefixTerm(" aaaaa b c "));
      string expectedTranslation = "NEAR (abc, def, \"aaaaa b c*\")";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test060()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearPrefixTerm(" def ").NearSimpleTerm(" aaaaa b c "));
      string expectedTranslation = "NEAR (abc, \"def*\", \"aaaaa b c\")";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test061()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.PrefixTerm("abc").NearSimpleTerm(" def ").NearSimpleTerm(" aaaaa b c "));
      string expectedTranslation = "NEAR (\"abc*\", def, \"aaaaa b c\")";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test062()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.PrefixTerm("abc").NearPrefixTerm(" def ").NearPrefixTerm(" aaaaa b c "));
      string expectedTranslation = "NEAR (\"abc*\", \"def*\", \"aaaaa b c*\")";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentOutOfRangeException))]
    public void Test063()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm("def"), -1);
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test064()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm("def"), 0);
      string expectedTranslation = "NEAR (abc, def, 0)";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test065()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm("def"), 3333);
      string expectedTranslation = "NEAR (abc, def, 3333)";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test066()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm("def"), 4294967294);
      string expectedTranslation = "NEAR (abc, def, 4294967294)";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test067()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm("def"), 4294967295);
      string expectedTranslation = "NEAR (abc, def, 4294967295)";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test068()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm("def"), 4294967296);
      string expectedTranslation = "NEAR (abc, def, MAX)";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test069()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm("def"), long.MaxValue);
      string expectedTranslation = "NEAR (abc, def, MAX)";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test070()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm("def"), 5, false);
      string expectedTranslation = "NEAR (abc, def, 5)";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test071()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.CustomProximityTerm(f => f.SimpleTerm("abc").NearSimpleTerm("def"), 5, true);
      string expectedTranslation = "NEAR (abc, def, 5, TRUE)";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test072()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("")
          .AndSimpleTerm("def")
          .AndSimpleTerm("ghi")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test073()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc")
          .AndSimpleTerm("")
          .AndSimpleTerm("ghi")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test074()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc")
          .AndSimpleTerm("def")
          .AndSimpleTerm("")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test075()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("   ")
          .AndSimpleTerm("def")
          .AndSimpleTerm("ghi")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test076()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc")
          .AndSimpleTerm("   ")
          .AndSimpleTerm("ghi")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test077()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc")
          .AndSimpleTerm("def")
          .AndSimpleTerm("   ")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test078()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm(null)
          .AndSimpleTerm("def")
          .AndSimpleTerm("ghi")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test079()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc")
          .AndSimpleTerm(null)
          .AndSimpleTerm("ghi")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test080()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc")
          .AndSimpleTerm("def")
          .AndSimpleTerm(null)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test081()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("", 0.1f)
          .AndSimpleTerm("def", 0.1f)
          .AndSimpleTerm("ghi", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test082()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc", 0.1f)
          .AndSimpleTerm("", 0.1f)
          .AndSimpleTerm("ghi", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test083()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc", 0.1f)
          .AndSimpleTerm("def", 0.1f)
          .AndSimpleTerm("", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test084()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("   ", 0.1f)
          .AndSimpleTerm("def", 0.1f)
          .AndSimpleTerm("ghi", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test085()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc", 0.1f)
          .AndSimpleTerm("   ", 0.1f)
          .AndSimpleTerm("ghi", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void Test086()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc", 0.1f)
          .AndSimpleTerm("def", 0.1f)
          .AndSimpleTerm("   ", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test087()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm(null, 0.1f)
          .AndSimpleTerm("def", 0.1f)
          .AndSimpleTerm("ghi", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test088()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc", 0.1f)
          .AndSimpleTerm(null, 0.1f)
          .AndSimpleTerm("ghi", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test089()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc", 0.1f)
          .AndSimpleTerm("def", 0.1f)
          .AndSimpleTerm(null, 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }



    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test090()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("")
          .AndPrefixTerm("def")
          .AndPrefixTerm("ghi")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test091()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("abc")
          .AndPrefixTerm("")
          .AndPrefixTerm("ghi")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test092()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("abc")
          .AndPrefixTerm("def")
          .AndPrefixTerm("")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test093()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("   ")
          .AndPrefixTerm("def")
          .AndPrefixTerm("ghi")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test094()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("abc")
          .AndPrefixTerm("   ")
          .AndPrefixTerm("ghi")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test095()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("abc")
          .AndPrefixTerm("def")
          .AndPrefixTerm("   ")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test096()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm(null)
          .AndPrefixTerm("def")
          .AndPrefixTerm("ghi")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test097()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("abc")
          .AndPrefixTerm(null)
          .AndPrefixTerm("ghi")
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test098()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("abc")
          .AndPrefixTerm("def")
          .AndPrefixTerm(null)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test099()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("", 0.1f)
          .AndPrefixTerm("def", 0.1f)
          .AndPrefixTerm("ghi", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test100()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("abc", 0.1f)
          .AndPrefixTerm("", 0.1f)
          .AndPrefixTerm("ghi", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test101()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("abc", 0.1f)
          .AndPrefixTerm("def", 0.1f)
          .AndPrefixTerm("", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test102()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("   ", 0.1f)
          .AndPrefixTerm("def", 0.1f)
          .AndPrefixTerm("ghi", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test103()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("abc", 0.1f)
          .AndPrefixTerm("   ", 0.1f)
          .AndPrefixTerm("ghi", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test104()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("abc", 0.1f)
          .AndPrefixTerm("def", 0.1f)
          .AndPrefixTerm("   ", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test105()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm(null, 0.1f)
          .AndPrefixTerm("def", 0.1f)
          .AndPrefixTerm("ghi", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test106()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("abc", 0.1f)
          .AndPrefixTerm(null, 0.1f)
          .AndPrefixTerm("ghi", 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test107()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.PrefixTerm("abc", 0.1f)
          .AndPrefixTerm("def", 0.1f)
          .AndPrefixTerm(null, 0.1f)
        );
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void Test108()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Thesaurus, null));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test109()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Thesaurus, new []{"abc", "def", null}));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test110()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Thesaurus, new string[0]));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test111()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Thesaurus, new []{"abc", "def", ""}));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test112()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Thesaurus, new []{"abc", "def", "   "}));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test113()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Thesaurus, null, 0.1f));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test114()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Thesaurus, new[] { "abc", "def", null }, 0.1f));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test115()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Thesaurus, new string[0], 0.1f));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test116()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Thesaurus, new[] { "abc", "def", "" }, 0.1f));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test117()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Thesaurus, new[] { "abc", "def", "   " }, 0.1f));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test118()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Thesaurus, new []{"abc"}));
      string expectedTranslation = "ISABOUT (FORMSOF (THESAURUS, abc))";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test119()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Inflectional, new[] {"abc"}));
      string expectedTranslation = "ISABOUT (FORMSOF (INFLECTIONAL, abc))";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test120()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Thesaurus, new[] {"abc"}, 0.1f));
      string expectedTranslation = "ISABOUT (FORMSOF (THESAURUS, abc) WEIGHT (0.100))";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test121()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Inflectional, new[] {"abc"}, 0.1f));
      string expectedTranslation = "ISABOUT (FORMSOF (INFLECTIONAL, abc) WEIGHT (0.100))";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test122()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Inflectional, new[] { "abc", " def ", " ghi jkl " }));
      string expectedTranslation = "ISABOUT (FORMSOF (INFLECTIONAL, abc, def, \"ghi jkl\"))";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test123()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.GenerationTerm(GenerationType.Inflectional, new[] { "abc", " def ", " ghi jkl " }, 0.1f));
      string expectedTranslation = "ISABOUT (FORMSOF (INFLECTIONAL, abc, def, \"ghi jkl\") WEIGHT (0.100))";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test124()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g=>g.SimpleTerm("abc")));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test125()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.PrefixTerm("abc")));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test126()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.SimpleTerm("abc").NearSimpleTerm(null)));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test127()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.SimpleTerm(null).NearSimpleTerm("abc")));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test128()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.SimpleTerm("abc").NearSimpleTerm("")));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test129()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.SimpleTerm("").NearSimpleTerm("abc")));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test130()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.SimpleTerm("abc").NearSimpleTerm("   ")));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test131()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.SimpleTerm("   ").NearSimpleTerm("abc")));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test132()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.PrefixTerm("abc").NearPrefixTerm("")));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test133()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.PrefixTerm("").NearPrefixTerm("abc")));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test134()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.PrefixTerm("abc").NearPrefixTerm("   ")));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test135()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.PrefixTerm("   ").NearPrefixTerm("abc")));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test136()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.PrefixTerm("abc").NearPrefixTerm(null)));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Test137()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.PrefixTerm(null).NearPrefixTerm("abc")));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }



    [Test]
    public void Test138()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.PrefixTerm(" abc ").NearPrefixTerm(" ccc ddd ").NearSimpleTerm(" bbb ccc ").NearSimpleTerm(" aaa ")));
      string expectedTranslation = "ISABOUT (\"abc*\" NEAR \"ccc ddd*\" NEAR \"bbb ccc\" NEAR aaa)";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test139()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.ProximityTerm(g => g.PrefixTerm(" abc ").NearPrefixTerm(" ccc ddd ").NearSimpleTerm(" bbb ccc ").NearSimpleTerm(" aaa "), 0.1f));
      string expectedTranslation = "ISABOUT (\"abc*\" NEAR \"ccc ddd*\" NEAR \"bbb ccc\" NEAR aaa WEIGHT (0.100))";
      CompileAndTest(sc, expectedTranslation);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test140()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(f => f.SimpleTerm("abc", -2.0f));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test141()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(f => f.SimpleTerm("abc", -1.0f));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Test142()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(f => f.SimpleTerm("abc", -0.001f));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test143()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(f => f.SimpleTerm("abc", 0.000f));
      string expectedTranslation = "ISABOUT (abc WEIGHT (0.000))";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test144()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(f => f.SimpleTerm("abc", 0.001f));
      string expectedTranslation = "ISABOUT (abc WEIGHT (0.001))";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test145()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(f => f.SimpleTerm("abc", 0.1f));
      string expectedTranslation = "ISABOUT (abc WEIGHT (0.100))";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test146()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(f => f.SimpleTerm("abc", 0.9f));
      string expectedTranslation = "ISABOUT (abc WEIGHT (0.900))";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test147()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(f => f.SimpleTerm("abc", 0.998f));
      string expectedTranslation = "ISABOUT (abc WEIGHT (0.998))";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test148()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(f => f.SimpleTerm("abc", 1.0f));
      string expectedTranslation = "ISABOUT (abc WEIGHT (1.000))";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void Test149()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(f => f.SimpleTerm("abc", 1.001f));
      string expectedTranslation = "dummy";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test150()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc")
          .AndGenerationTerm(GenerationType.Thesaurus, new []{"abc"})
          .AndPrefixTerm("abc")
          .AndProximityTerm(g=>g.SimpleTerm("abc").NearPrefixTerm("def")));
      string expectedTranslation = "ISABOUT (abc, FORMSOF (THESAURUS, abc), \"abc*\", abc NEAR \"def*\")";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test151()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc", 0.1f)
          .AndGenerationTerm(GenerationType.Thesaurus, new[] { "abc" })
          .AndPrefixTerm("abc")
          .AndProximityTerm(g => g.SimpleTerm("abc").NearPrefixTerm("def")));
      string expectedTranslation = "ISABOUT (abc WEIGHT (0.100), FORMSOF (THESAURUS, abc), \"abc*\", abc NEAR \"def*\")";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test152()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc")
          .AndGenerationTerm(GenerationType.Thesaurus, new[] { "abc" }, 0.1f)
          .AndPrefixTerm("abc")
          .AndProximityTerm(g => g.SimpleTerm("abc").NearPrefixTerm("def")));
      string expectedTranslation = "ISABOUT (abc, FORMSOF (THESAURUS, abc) WEIGHT (0.100), \"abc*\", abc NEAR \"def*\")";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test153()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc")
          .AndGenerationTerm(GenerationType.Thesaurus, new[] { "abc" })
          .AndPrefixTerm("abc", 0.1f)
          .AndProximityTerm(g => g.SimpleTerm("abc").NearPrefixTerm("def")));
      string expectedTranslation = "ISABOUT (abc, FORMSOF (THESAURUS, abc), \"abc*\" WEIGHT (0.100), abc NEAR \"def*\")";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test154()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc")
          .AndGenerationTerm(GenerationType.Thesaurus, new[] { "abc" })
          .AndPrefixTerm("abc")
          .AndProximityTerm(g => g.SimpleTerm("abc").NearPrefixTerm("def"),0.1f));
      string expectedTranslation = "ISABOUT (abc, FORMSOF (THESAURUS, abc), \"abc*\", abc NEAR \"def*\" WEIGHT (0.100))";
      CompileAndTest(sc, expectedTranslation);
    }

    [Test]
    public void Test155()
    {
      Expression<Func<ConditionEndpoint, IOperand>> sc = e => e.WeightedTerm(
        f => f.SimpleTerm("abc", 0.1f)
          .AndGenerationTerm(GenerationType.Thesaurus, new[] { "abc" }, 0.2f)
          .AndPrefixTerm("abc", 0.3f)
          .AndProximityTerm(g => g.SimpleTerm("abc").NearPrefixTerm("def"), 0.4f));
      string expectedTranslation = "ISABOUT (abc WEIGHT (0.100), FORMSOF (THESAURUS, abc) WEIGHT (0.200), \"abc*\" WEIGHT (0.300), abc NEAR \"def*\" WEIGHT (0.400))";
      CompileAndTest(sc, expectedTranslation);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntityWithFullText));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    private void CompileAndTest(Expression<Func<ConditionEndpoint, IOperand>> searchConditionExpression, string expectedTranslation)
    {
      var visitor = Domain.Handler.GetSearchConditionCompiler();
      var searchCondition = searchConditionExpression.Compile();
      searchCondition.Invoke(SearchConditionNodeFactory.CreateConditonRoot()).AcceptVisitor(visitor);
      Assert.That(visitor.CurrentOutput, Is.EqualTo(expectedTranslation));

      using (var sesion = Domain.OpenSession())
      using (var transaction = sesion.OpenTransaction()) {
        //Assert.DoesNotThrow(sesion.Query.ContainsTable<TestEntityWithFullText>(searchConditionExpression).Run);
        sesion.Query.ContainsTable<TestEntityWithFullText>(searchConditionExpression).Run();
      }
    }
  }
}
