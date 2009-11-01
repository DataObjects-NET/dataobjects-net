using System;
using System.Data.Common;
using System.Globalization;
using System.Text;
using Npgsql;

namespace Xtensive.Sql.Common.PgSql
{
  public class PgSqlConnection : Connection
  {
    static PgSqlConnection()
    {
      mInvalidChars = new[] {'=', ';'};
      string[] invalidStrings = new string[mInvalidChars.Length];
      for (int i = 0; i < mInvalidChars.Length; i++) {
        invalidStrings[i] = new String(mInvalidChars[i], 1);
      }
      mInvalidCharacterExceptionMessageTemplate = "URL contains invalid {0} specification. {0} cannot contain any of these: '" + String.Join("', '", invalidStrings) + "' .";
    }

    public PgSqlConnection(Driver driver, ConnectionInfo info)
      : base(driver, GetRealConnection(info), info)
    {
    }

    public static DbConnection GetRealConnection(ConnectionInfo info)
    {
      string invalidValueExceptionMessageTemplate = "URL contains invalid {0} specification. {0} has to be {1}.";

      string allowedBoolStringsText = String.Format("{0} or {1}", String.Join("/", mTrueStrings), String.Join("/", mFalseStrings));

      //      ValidateConnectionInfo(info);


      StringBuilder csBuilder = new StringBuilder();

      if (info.Host!="") {
        Validate(info.Host, "Host");
        csBuilder.Append("Server=");
        csBuilder.Append(info.Host);
      }
      else
        throw new ArgumentException("URL doesn't contain Host specification.");

      if (info.Port!=0) {
        //        Validate(info.Port, "Port");
        csBuilder.Append(";Port=");
        csBuilder.Append(info.Port);
      }

      if (info.User!="") {
        Validate(info.User, "User");
        csBuilder.Append(";User Id=");
        csBuilder.Append(info.User);
        Validate(info.Password, "Password");
        csBuilder.Append(";Password=");
        csBuilder.Append(info.Password);
      }
      else
        throw new ArgumentException("URL doesn't contain User/Password specification.");

      if (info.Database!="") {
        Validate(info.Database, "Database");
        csBuilder.Append(";Database=");
        csBuilder.Append(info.Database);
      }
      else
        throw new ArgumentException("URL doesn't contain Database specification.");

      string protocol;
      info.Params.TryGetValue("Protocol", out protocol);
      if (protocol!=null) {
        if (protocol!="3" && protocol!="2")
          throw new ArgumentException(String.Format(invalidValueExceptionMessageTemplate, "Protocol", "2 or 3"));
        csBuilder.AppendFormat(";Protocol={0}", protocol);
      }

      string encoding;
      info.Params.TryGetValue("Encoding", out encoding);
      encoding = encoding.ToUpper();
      if (encoding!=null) {
        if (encoding!="ASCII" && encoding!="UNICODE")
          throw new ArgumentException(String.Format(invalidValueExceptionMessageTemplate, "Encoding", "ASCII or UNICODE."));
        csBuilder.AppendFormat(";Encoding={0}", encoding);
      }

      string pooling;
      info.Params.TryGetValue("Pooling", out pooling);
      if (pooling!=null) {
        if (!IsBoolString(pooling))
          throw new ArgumentException(String.Format(invalidValueExceptionMessageTemplate, "Pooling", allowedBoolStringsText));
        csBuilder.AppendFormat(";Pooling={0}", ConvertBoolStringToNpgsqlBoolString(pooling));

        //reading pooling settings

        string minps;
        info.Params.TryGetValue("MinPoolSize", out minps);
        if (minps!=null) {
          int minpsi = -1;
          try {
            minpsi = Int32.Parse(minps);
          }
          catch (Exception ex) {
            throw new ArgumentException(String.Format(invalidValueExceptionMessageTemplate, "MinPoolSize", "a positive integer"), ex);
          }
          if (minpsi < 1)
            throw new ArgumentException(String.Format(invalidValueExceptionMessageTemplate, "MinPoolSize", "a positive integer"));

          csBuilder.AppendFormat(";MinPoolSize={0}", minpsi);
        }


        string maxps;
        info.Params.TryGetValue("MaxPoolSize", out maxps);
        if (maxps!=null) {
          int maxpsi = -1;
          try {
            maxpsi = Int32.Parse(maxps);
          }
          catch (Exception ex) {
            throw new ArgumentException(String.Format(invalidValueExceptionMessageTemplate, "MaxPoolSize", "a positive integer"), ex);
          }
          if (maxpsi < 1)
            throw new ArgumentException(String.Format(invalidValueExceptionMessageTemplate, "MaxPoolSize", "a positive integer"));

          csBuilder.AppendFormat(";MaxPoolSize={0}", maxpsi);
        }
      }


      string timeout;
      info.Params.TryGetValue("Timeout", out timeout);
      if (timeout!=null) {
        int toi = -1;
        try {
          toi = Int32.Parse(timeout);
        }
        catch (Exception ex) {
          throw new ArgumentException(String.Format(invalidValueExceptionMessageTemplate, "Timeout", "a non-negative integer."), ex);
        }
        if (toi < 0)
          throw new ArgumentException(String.Format(invalidValueExceptionMessageTemplate, "Timeout", "a non-negative integer."));
        csBuilder.Append(";Timeout=");
        csBuilder.Append(toi);
      }

      string ssl;
      info.Params.TryGetValue("SSL", out ssl);
      if (ssl!=null) {
        if (!IsBoolString(ssl))
          throw new ArgumentException(String.Format(invalidValueExceptionMessageTemplate, "SSL", allowedBoolStringsText));
        csBuilder.AppendFormat(";SSL={0}", ConvertBoolStringToNpgsqlBoolString(ssl));
      }

      string sslmode;
      info.Params.TryGetValue("Sslmode", out sslmode);
      if (sslmode!=null) {
        if (sslmode!="Prefer" && sslmode!="Require" && sslmode!="Allow" && sslmode!="Disable")
          throw new ArgumentException(String.Format(invalidValueExceptionMessageTemplate, "Sslmode", "Prefer, Require, Allow, or Disable."));
        csBuilder.AppendFormat(";Sslmode={0}", sslmode);
      }
      csBuilder.Append(";");

      return new NpgsqlConnection(csBuilder.ToString());
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


    private static readonly string[] mTrueStrings = new[] {"t", "true", "1", "on", "yes", "y"};
    private static readonly string[] mFalseStrings = new[] {"f", "false", "0", "off", "no", "n"};

    private static readonly char[] mInvalidChars;

    private static readonly string mInvalidCharacterExceptionMessageTemplate;
  }
}