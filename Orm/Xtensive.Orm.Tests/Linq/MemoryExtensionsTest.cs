#if NET10_0_OR_GREATER
using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{

  [Category("Linq")]
  [TestFixture(Description = "Test cases when System.MemoryExtensions are applied instead of more general versions of methods from EnumerableExtensions")]
  public class MemoryExtensionsTest : ChinookDOModelTest
  {
    private int[] existingGenreIds;
    private string[] existingGenreNames;

    protected override void CheckRequirements()
    {
      Expression<Func<bool>> testExpr = () => new int[] { 1, 2 }.Contains(2);
      if (testExpr.Body is MethodCallExpression mc && mc.Method.DeclaringType == typeof(MemoryExtensions)) {
        // in C# 14+ the expression will use implicit cast to ReadOnlySpan<T>
        // and use Contains extension method from MemoryExtensions.
        // In previous versions of language they use IEnumerable<int>.Contains() extension method.
        // What's funny is that in code you cannot create ReadOnlySpans expressions, directly or via .AsSpan() :-)
        return;
      }
      throw new IgnoreException("Wrong version of language. Test is inapplicable.");
    }

    public override void SetUp()
    {
      base.SetUp();
      existingGenreIds = Genres.Select(g=>g.GenreId).OrderBy(id => id).ToArray();
      existingGenreNames = Genres.Select(g => g.Name).ToArray();
    }

    #region Contains over ReadOnlySpan<int>

    [Test]
    public void IntArrayContainsInAllTest()
    {
      var existingIds = existingGenreIds.Skip(5).Take(5).ToArray();

      var query = Session.Query.All<Genre>()
        .All(g => existingIds.Contains(g.GenreId));
      Assert.That(query, Is.False);

      query = Session.Query.All<Genre>()
        .All(g => existingGenreIds.Contains(g.GenreId));
      Assert.That(query, Is.True);

      var inexistentIds = Enumerable.Range(existingGenreIds[^1] + 10, 6).ToArray();
    }

    [Test]
    public void IntArrayContainsInAnyTest()
    {
      var existingIds = existingGenreIds.Skip(5).Take(5).ToArray();

      var query = Session.Query.All<Genre>()
        .Any(g => existingIds.Contains(g.GenreId));
      Assert.That(query, Is.True);

      var inexistentIds = Enumerable.Range(existingGenreIds[^1] + 10, 6).ToArray();
      query = Session.Query.All<Genre>() .Any(g => inexistentIds.Contains(g.GenreId));
      Assert.That(query, Is.False);
    }

    [Test]
    public void IntArrayContainsInCountTest()
    {
      var existingIds = existingGenreIds.Skip(5).Take(5).ToArray();
      var query = Session.Query.All<Genre>()
        .Count(g => existingIds.Contains(g.GenreId));
      Assert.That(query, Is.EqualTo(existingIds.Length));

      var inexistentIds = Enumerable.Range(existingGenreIds[^1] + 10, 6).ToArray();
      query = Session.Query.All<Genre>()
        .Count(g => inexistentIds.Contains(g.GenreId));
      Assert.That(query, Is.EqualTo(0));
    }

    [Test]
    public void IntArrayContainsInGroupByTest()
    {
      var existingIds = existingGenreIds.Skip(5).Take(5).ToArray();
      var query = Session.Query.All<Genre>()
        .GroupBy(g => existingIds.Contains(g.GenreId)).ToArray();
      Assert.That(query.Length, Is.EqualTo(2));
      var firstGroup = query[0];
      Assert.That(firstGroup.Key, Is.False);
      Assert.That(firstGroup.Count(), Is.EqualTo(existingGenreIds.Length - existingIds.Length));

      var secondGroup = query[1];
      Assert.That(secondGroup.Key, Is.True);
      Assert.That(secondGroup.Count(), Is.EqualTo(existingIds.Length));


      var inexistentIds = Enumerable.Range(existingGenreIds[^1] + 10, 6).ToArray();
      query = Session.Query.All<Genre>()
        .GroupBy(g => inexistentIds.Contains(g.GenreId)).ToArray();
      Assert.That(query.Length, Is.EqualTo(1));
      firstGroup = query[0];
      Assert.That(firstGroup.Key, Is.False);
      Assert.That(firstGroup.Count(), Is.EqualTo(existingGenreIds.Length));
    }

    [Test]
    public void IntArrayContainsInOrderByTest()
    {
      var existingIds = existingGenreIds.Skip(5).Take(5).ToArray();
      var genres = Session.Query.All<Genre>()
        .OrderBy(g => existingIds.Contains(g.GenreId)).ToArray();

      Assert.That(genres.Take(existingGenreIds.Length - existingIds.Length).All(g => !existingIds.Contains(g.GenreId)), Is.True);
      Assert.That(genres.Skip(existingGenreIds.Length - existingIds.Length).All(g => existingIds.Contains(g.GenreId)), Is.True);

      var inexistentIds = Enumerable.Range(existingGenreIds[^1] + 10, 6).ToArray();
      genres = Session.Query.All<Genre>()
        .OrderBy(g => inexistentIds.Contains(g.GenreId)).ToArray();
      Assert.That(genres.All(g => !inexistentIds.Contains(g.GenreId)), Is.True);
    }

    [Test]
    public void IntArrayContainsInSelectTest()
    {
      var existingIds = existingGenreIds.Skip(5).Take(5).ToArray();

      var queryResult = Session.Query.All<Genre>()
        .Select(g => existingIds.Contains(g.GenreId)).ToArray();
      Assert.That(queryResult.Length, Is.EqualTo(existingGenreIds.Length));
      Assert.That(queryResult.Count(b => b == true), Is.EqualTo(existingIds.Length));

      var inexistentIds = Enumerable.Range(existingGenreIds[^1] + 10, 6).ToArray();
      queryResult = Session.Query.All<Genre>()
        .Select(g => inexistentIds.Contains(g.GenreId)).ToArray();
      Assert.That(queryResult.Length, Is.EqualTo(existingGenreIds.Length));
      Assert.That(queryResult.Count(b => b == true), Is.EqualTo(0));
    }

    [Test]
    public void IntArrayContainsInWhereTest()
    {
      var existingIds = existingGenreIds.Skip(5).Take(5).ToArray();
      var genres = Session.Query.All<Genre>()
        .Where(g => existingIds.Contains(g.GenreId)).ToArray();
      Assert.That(genres.Length, Is.EqualTo(existingIds.Length));
      Assert.That(genres.All(g => existingIds.Contains(g.GenreId)));


      var inexistentIds = Enumerable.Range(existingGenreIds[^1] + 10, 6).ToArray();
      genres = Session.Query.All<Genre>()
        .Where(g => inexistentIds.Contains(g.GenreId)).ToArray();

      Assert.That(genres.Length, Is.EqualTo(0));
      Assert.That(genres.All(g => !existingIds.Contains(g.GenreId)));
    }

    [Test]
    public void IntArrayContainsInFirstTest()
    {
      var existingIds = existingGenreIds.Skip(5).Take(5).ToArray();
      var genre = Session.Query.All<Genre>()
        .First(g => existingIds.Contains(g.GenreId));
      Assert.That(genre.GenreId, Is.EqualTo(existingIds[0]));
    }

    [Test]
    public void IntArrayContainsInFirstOrDefaultTest()
    {
      var existingIds = existingGenreIds.Skip(5).Take(5).ToArray();
      var genre = Session.Query.All<Genre>()
        .FirstOrDefault(g => existingIds.Contains(g.GenreId));

      Assert.That(genre, Is.Not.Null);
      Assert.That(genre.GenreId, Is.EqualTo(existingIds[0]));


      var inexistentIds = Enumerable.Range(existingGenreIds[^1] + 10, 6).ToArray();
      genre = Session.Query.All<Genre>()
        .FirstOrDefault(g => inexistentIds.Contains(g.GenreId));
      Assert.That(genre, Is.Null);
    }

    [Test]
    public void IntArrayContainsInSingleTest2()
    {
      var existingIds = existingGenreIds.Skip(5).Take(1).ToArray();
      var genre = Session.Query.All<Genre>()
          .Single(g => existingIds.Contains(g.GenreId));

      Assert.That(genre, Is.Not.Null);
      Assert.That(genre.GenreId, Is.EqualTo(existingIds[0]));
    }

    [Test]
    public void IntArrayContainsInSingleOrDefaultTest2()
    {
      var existingIds = existingGenreIds.Skip(5).Take(1).ToArray();
      var genre = Session.Query.All<Genre>()
        .SingleOrDefault(g => existingIds.Contains(g.GenreId));

      Assert.That(genre, Is.Not.Null);
      Assert.That(genre.GenreId, Is.EqualTo(existingIds[0]));

      var inexistentIds = Enumerable.Range(existingGenreIds[^1] + 10, 1).ToArray();

      genre = Session.Query.All<Genre>()
        .SingleOrDefault(g => inexistentIds.Contains(g.GenreId));

      Assert.That(genre, Is.Null);
    }

    #endregion

    #region Contains over ReadOnlySpan<string>

    [Test]
    public void StringArrayContainsInAllTest()
    {
      var existingNames = existingGenreNames.Skip(5).Take(5).ToArray();

      var query = Session.Query.All<Genre>()
        .All(g => existingNames.Contains(g.Name));
      Assert.That(query, Is.False);

      query = Session.Query.All<Genre>()
        .All(g => existingGenreNames.Contains(g.Name));
      Assert.That(query, Is.True);
    }

    [Test]
    public void StringArrayContainsInAnyTest()
    {
      var existingNames = existingGenreNames.Skip(5).Take(5).ToArray();

      var query = Session.Query.All<Genre>()
        .Any(g => existingNames.Contains(g.Name));
      Assert.That(query, Is.True);

      var inexistentNames = new string[] { "bbbaaarrrvvv", "oeoeoeoeoe", "qqkkqqkk", "pweoirsl", "eienhjg", "rrooroor" };
      query = Session.Query.All<Genre>().Any(g => inexistentNames.Contains(g.Name));
      Assert.That(query, Is.False);
    }

    [Test]
    public void StringArrayContainsInCountTest()
    {
      var existingNames = existingGenreNames.Skip(5).Take(5).ToArray();
      var query = Session.Query.All<Genre>()
        .Count(g => existingNames.Contains(g.Name));
      Assert.That(query, Is.EqualTo(existingNames.Length));

      var inexistentNames = new string[] { "bbbaaarrrvvv", "oeoeoeoeoe", "qqkkqqkk", "pweoirsl", "eienhjg", "rrooroor" };
      query = Session.Query.All<Genre>()
        .Count(g => inexistentNames.Contains(g.Name));
      Assert.That(query, Is.EqualTo(0));
    }

    [Test]
    public void StringArrayContainsInGroupByTest()
    {
      var existingNames = existingGenreNames.Skip(5).Take(5).ToArray();
      var query = Session.Query.All<Genre>()
        .GroupBy(g => existingNames.Contains(g.Name)).ToArray();
      Assert.That(query.Length, Is.EqualTo(2));
      var firstGroup = query[0];
      Assert.That(firstGroup.Key, Is.False);
      Assert.That(firstGroup.Count(), Is.EqualTo(existingGenreNames.Length - existingNames.Length));

      var secondGroup = query[1];
      Assert.That(secondGroup.Key, Is.True);
      Assert.That(secondGroup.Count(), Is.EqualTo(existingNames.Length));

      var inexistentNames = new string[] { "bbbaaarrrvvv", "oeoeoeoeoe", "qqkkqqkk", "pweoirsl", "eienhjg", "rrooroor" };
      query = Session.Query.All<Genre>()
        .GroupBy(g => inexistentNames.Contains(g.Name)).ToArray();
      Assert.That(query.Length, Is.EqualTo(1));
      firstGroup = query[0];
      Assert.That(firstGroup.Key, Is.False);
      Assert.That(firstGroup.Count(), Is.EqualTo(existingGenreNames.Length));
    }

    [Test]
    public void StringArrayContainsInOrderByTest()
    {
      var existingNames = existingGenreNames.Skip(5).Take(5).ToArray();
      var genres = Session.Query.All<Genre>()
        .OrderBy(g => existingNames.Contains(g.Name)).ToArray();

      Assert.That(genres.Take(existingGenreNames.Length - existingNames.Length).All(g => !existingNames.Contains(g.Name)), Is.True);
      Assert.That(genres.Skip(existingGenreNames.Length - existingNames.Length).All(g => existingNames.Contains(g.Name)), Is.True);

      var inexistentNames = new string[] { "bbbaaarrrvvv", "oeoeoeoeoe", "qqkkqqkk", "pweoirsl", "eienhjg", "rrooroor" };
      genres = Session.Query.All<Genre>()
        .OrderBy(g => inexistentNames.Contains(g.Name)).ToArray();
      Assert.That(genres.All(g => !inexistentNames.Contains(g.Name)), Is.True);
    }

    [Test]
    public void StringArrayContainsInSelectTest()
    {
      var existingNames = existingGenreNames.Skip(5).Take(5).ToArray();

      var queryResult = Session.Query.All<Genre>()
        .Select(g => existingNames.Contains(g.Name)).ToArray();
      Assert.That(queryResult.Length, Is.EqualTo(existingGenreNames.Length));
      Assert.That(queryResult.Count(b => b == true), Is.EqualTo(existingNames.Length));

      var inexistentNames = new string[] { "bbbaaarrrvvv", "oeoeoeoeoe", "qqkkqqkk", "pweoirsl", "eienhjg", "rrooroor" };
      queryResult = Session.Query.All<Genre>()
        .Select(g => inexistentNames.Contains(g.Name)).ToArray();
      Assert.That(queryResult.Length, Is.EqualTo(existingGenreNames.Length));
      Assert.That(queryResult.Count(b => b == true), Is.EqualTo(0));
    }

    [Test]
    public void StringArrayContainsInWhereTest()
    {
      var existingNames = existingGenreNames.Skip(5).Take(5).ToArray();
      var genres = Session.Query.All<Genre>()
        .Where(g => existingNames.Contains(g.Name)).ToArray();
      Assert.That(genres.Length, Is.EqualTo(existingNames.Length));
      Assert.That(genres.All(g => existingNames.Contains(g.Name)));


      var inexistentNames = new string[] { "bbbaaarrrvvv", "oeoeoeoeoe", "qqkkqqkk", "pweoirsl", "eienhjg", "rrooroor" };
      genres = Session.Query.All<Genre>()
        .Where(g => inexistentNames.Contains(g.Name)).ToArray();

      Assert.That(genres.Length, Is.EqualTo(0));
      Assert.That(genres.All(g => !existingNames.Contains(g.Name)));
    }

    [Test]
    public void StringArrayContainsInFirstTest()
    {
      var existingNames = existingGenreNames.Skip(5).Take(5).ToArray();
      var genre = Session.Query.All<Genre>()
        .First(g => existingNames.Contains(g.Name));
      Assert.That(genre.Name, Is.EqualTo(existingNames[0]));
    }

    [Test]
    public void StringArrayContainsInFirstOrDefaultTest()
    {
      var existingNames = existingGenreNames.Skip(5).Take(5).ToArray();
      var genre = Session.Query.All<Genre>()
        .FirstOrDefault(g => existingNames.Contains(g.Name));

      Assert.That(genre, Is.Not.Null);
      Assert.That(genre.Name, Is.EqualTo(existingNames[0]));


      var inexistentNames = new string[] { "bbbaaarrrvvv", "oeoeoeoeoe", "qqkkqqkk", "pweoirsl", "eienhjg", "rrooroor" };
      genre = Session.Query.All<Genre>()
        .FirstOrDefault(g => inexistentNames.Contains(g.Name));
      Assert.That(genre, Is.Null);
    }

    [Test]
    public void StringArrayContainsInSingleTest2()
    {
      var existingNames = existingGenreNames.Skip(5).Take(1).ToArray();
      var genre = Session.Query.All<Genre>()
          .Single(g => existingNames.Contains(g.Name));

      Assert.That(genre, Is.Not.Null);
      Assert.That(genre.Name, Is.EqualTo(existingNames[0]));
    }

    [Test]
    public void StringArrayContainsInSingleOrDefaultTest2()
    {
      var existingNames = existingGenreNames.Skip(5).Take(1).ToArray();
      var genre = Session.Query.All<Genre>()
        .SingleOrDefault(g => existingNames.Contains(g.Name));

      Assert.That(genre, Is.Not.Null);
      Assert.That(genre.Name, Is.EqualTo(existingNames[0]));

      var inexistentNames = new string[] { "bbbaaarrrvvv" };

      genre = Session.Query.All<Genre>()
        .SingleOrDefault(g => inexistentNames.Contains(g.Name));

      Assert.That(genre, Is.Null);
    }

    #endregion
  }
}
#endif