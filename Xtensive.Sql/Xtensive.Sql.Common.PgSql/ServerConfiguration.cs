using System;
using System.Data;
using System.Data.Common;
using Xtensive.Core;

namespace Xtensive.Sql.Common.PgSql
{
  /// <summary>
  /// Represents some database server configuration parameter values.
  /// </summary>
  public class ServerConfiguration
  {
    internal ServerConfiguration(DbConnection conn)
    {
      ArgumentValidator.EnsureArgumentNotNull(conn, "conn");
      //fill config values
      using (DbCommand cmd = conn.CreateCommand()) {
        cmd.CommandText = "SHOW ALL";
        using (DbDataReader dr = cmd.ExecuteReader()) {
          while (dr.Read()) {
            ReadConfig(dr, dr.GetString(0));
          }
        }
      }
    }

    private void ReadConfig(IDataReader dr, string name)
    {
      switch (name.ToLower()) {
//8.0
      case "max_function_args":
        MaxFunctionArgs = GetIntValue(dr);
        break;
      case "max_identifier_length":
        MaxIdentifierLength = GetIntValue(dr);
        break;
      case "max_index_keys":
        MaxIndexKeys = GetIntValue(dr);
        break;
      case "datestyle":
        DateStyle = dr.GetString(SETTING_VALUE_COLUMN_INDEX);
        break;

//8.2
      case "standard_conforming_strings":
        StandardConformingStrings = ToBool(dr.GetString(SETTING_VALUE_COLUMN_INDEX));
        break;
      default:
        break;
      }
    }

    private int GetIntValue(IDataReader dr)
    {
      return Convert.ToInt32(dr.GetValue(SETTING_VALUE_COLUMN_INDEX));
    }

    /// <summary>
    /// Indicates that in the result of 'SHOW ALL' in which column we can
    /// find the setting value.
    /// </summary>
    private const int SETTING_VALUE_COLUMN_INDEX = 1;

    #region MaxIdentifierLength property

    private int mMaxIdentifierLength = 63;

    public int MaxIdentifierLength
    {
      get { return mMaxIdentifierLength; }
      protected set { mMaxIdentifierLength = value; }
    }

    #endregion

    #region MaxFunctionArgs property

    private int mMaxFunctionArgs = 32;

    public int MaxFunctionArgs
    {
      get { return mMaxFunctionArgs; }
      protected set { mMaxFunctionArgs = value; }
    }

    #endregion

    #region MaxIndexKeys property

    private int mMaxIndexKeys = 32;

    public int MaxIndexKeys
    {
      get { return mMaxIndexKeys; }
      protected set { mMaxIndexKeys = value; }
    }

    #endregion

    #region StandardConformingStrings property

    private bool mStandardConformingStrings;

    public bool StandardConformingStrings
    {
      get { return mStandardConformingStrings; }
      protected set { mStandardConformingStrings = value; }
    }

    #endregion

    #region DateStyle property

    private string mDateStyle;

    public string DateStyle
    {
      get { return mDateStyle; }
      protected set { mDateStyle = value; }
    }

    #endregion

    protected bool ToBool(string setting)
    {
      return setting=="on";
    }
  }
}