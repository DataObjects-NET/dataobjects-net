using Npgsql;
using System;
using System.Globalization;
using System.Text;
using Xtensive.Core;
using Xtensive.Sql.PostgreSql.Resources;

namespace Xtensive.Sql.PostgreSql
{
  internal static class ConnectionFactory
  {
    private static readonly string[] mTrueStrings = new[] {"t", "true", "1", "on", "yes", "y"};
    private static readonly string[] mFalseStrings = new[] {"f", "false", "0", "off", "no", "n"};

    private static readonly char[] mInvalidChars;
    private static readonly string mInvalidCharacterExceptionMessageTemplate;

    private static readonly string mAllowedBoolStringsText;

    public static NpgsqlConnection CreateConnection(UrlInfo url)
    {
      var connectionString = BuildConnectionString(url);
      return new NpgsqlConnection(connectionString);
    }

    private static string BuildConnectionString(UrlInfo url)
    {
      var csBuilder = new StringBuilder();

      if (url.Host!="") {
        Validate(url.Host, "Host");
        csBuilder.Append("Server=");
        csBuilder.Append(url.Host);
      }
      else
        throw new ArgumentException("URL doesn't contain Host specification.");

      if (url.Port!=0) {
        csBuilder.Append(";Port=");
        csBuilder.Append(url.Port);
      }

      if (url.User!="") {
        Validate(url.User, "User");
        csBuilder.Append(";User Id=");
        csBuilder.Append(url.User);
        Validate(url.Password, "Password");
        csBuilder.Append(";Password=");
        csBuilder.Append(url.Password);
      }
      else
        throw new ArgumentException("URL doesn't contain User/Password specification.");

      if (url.Resource!="") {
        Validate(url.Resource, "Database");
        csBuilder.Append(";Database=");
        csBuilder.Append(url.Resource);
      }
      else
        throw new ArgumentException("URL doesn't contain Database specification.");

      string protocol;
      url.Params.TryGetValue("Protocol", out protocol);
      if (protocol!=null) {
        if (protocol!="3" && protocol!="2")
          throw IncorrectUrl("Protocol", "2 or 3");
        csBuilder.AppendFormat(";Protocol={0}", protocol);
      }

      string encoding;
      url.Params.TryGetValue("Encoding", out encoding);
      if (encoding!=null) {
        encoding = encoding.ToUpper();
        if (encoding!="ASCII" && encoding!="UNICODE")
          throw IncorrectUrl("Encoding", "ASCII or UNICODE.");
        csBuilder.AppendFormat(";Encoding={0}", encoding);
      }

      string pooling;
      url.Params.TryGetValue("Pooling", out pooling);
      if (pooling!=null) {
        if (!IsBoolString(pooling))
          throw IncorrectUrl("Pooling", mAllowedBoolStringsText);
        csBuilder.AppendFormat(";Pooling={0}", ConvertBoolStringToNpgsqlBoolString(pooling));

        //reading pooling settings

        string minps;
        url.Params.TryGetValue("MinPoolSize", out minps);
        if (minps!=null) {
          int minpsi = -1;
          try {
            minpsi = Int32.Parse(minps);
          }
          catch {
            throw IncorrectUrl("MinPoolSize", "a positive integer");
          }
          if (minpsi < 1)
            throw IncorrectUrl("MinPoolSize", "a positive integer");

          csBuilder.AppendFormat(";MinPoolSize={0}", minpsi);
        }


        string maxps;
        url.Params.TryGetValue("MaxPoolSize", out maxps);
        if (maxps!=null) {
          int maxpsi = -1;
          try {
            maxpsi = Int32.Parse(maxps);
          }
          catch {
            throw IncorrectUrl("MaxPoolSize", "a positive integer");
          }
          if (maxpsi < 1)
            throw IncorrectUrl("MaxPoolSize", "a positive integer");

          csBuilder.AppendFormat(";MaxPoolSize={0}", maxpsi);
        }
      }


      string timeout;
      url.Params.TryGetValue("Timeout", out timeout);
      if (timeout!=null) {
        int toi = -1;
        try {
          toi = Int32.Parse(timeout);
        }
        catch {
          throw IncorrectUrl("Timeout", "a non-negative integer.");
        }
        if (toi < 0)
          throw IncorrectUrl("Timeout", "a non-negative integer.");
        csBuilder.Append(";Timeout=");
        csBuilder.Append(toi);
      }

      string ssl;
      url.Params.TryGetValue("SSL", out ssl);
      if (ssl!=null) {
        if (!IsBoolString(ssl))
          throw IncorrectUrl("SSL", mAllowedBoolStringsText);
        csBuilder.AppendFormat(";SSL={0}", ConvertBoolStringToNpgsqlBoolString(ssl));
      }

      string sslmode;
      url.Params.TryGetValue("Sslmode", out sslmode);
      if (sslmode!=null) {
        if (sslmode!="Prefer" && sslmode!="Require" && sslmode!="Allow" && sslmode!="Disable")
          throw IncorrectUrl("Sslmode", "Prefer, Require, Allow, or Disable.");
        csBuilder.AppendFormat(";Sslmode={0}", sslmode);
      }
      csBuilder.Append(";");

      return csBuilder.ToString();
    }

    private static bool IsBoolString(string s)
    {
      return IsFalseString(s) || IsTrueString(s);
    }

    private static bool IsTrueString(string s)
    {
      return ArrayContains(mTrueStrings, s.ToLower(CultureInfo.InvariantCulture));
    }

    private static bool IsFalseString(string s)
    {
      return ArrayContains(mFalseStrings, s.ToLower(CultureInfo.InvariantCulture));
    }

    private static bool ArrayContains(string[] a, string elem)
    {
      int l = a.Length;
      for (int i = 0; i < l; i++) {
        if (a[i]==elem)
          return true;
      }
      return false;
    }

    private static string ConvertBoolStringToNpgsqlBoolString(string s)
    {
      if (IsTrueString(s))
        return "True";
      if (IsFalseString(s))
        return "False";
      throw new ArgumentException("Invalid value: " + s, "s");
    }

    private static void Validate(string parameter, string parameterName)
    {
      if (parameter.IndexOfAny(mInvalidChars) > 0)
        throw new ArgumentException(String.Format(mInvalidCharacterExceptionMessageTemplate, parameterName));
    }

    private static ArgumentException IncorrectUrl(string parameterName, string expectedValue)
    {
      return new ArgumentException(string.Format(
        Strings.ExUrlContainsInvalidXSpecificationXHasToBeY, parameterName, expectedValue));
    }

    // Type initializer

    static ConnectionFactory()
    {
      mInvalidChars = new[] {'=', ';'};
      string[] invalidStrings = new string[mInvalidChars.Length];
      for (int i = 0; i < mInvalidChars.Length; i++) {
        invalidStrings[i] = new String(mInvalidChars[i], 1);
      }

      mInvalidCharacterExceptionMessageTemplate =
        "URL contains invalid {0} specification. {0} cannot contain any of these: '" +
        string.Join("', '", invalidStrings) + "' .";

      mAllowedBoolStringsText =
        string.Format("{0} or {1}", string.Join("/", mTrueStrings), string.Join("/", mFalseStrings));
    }
  }
}