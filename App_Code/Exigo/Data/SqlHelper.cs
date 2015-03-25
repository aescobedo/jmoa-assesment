using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;

public class SqlHelper
{
    public SqlHelper()
    {
        _connectionString = GlobalSettings.ConnectionStrings.Sql;
    }

    public string ConnectionString
    {
        get { return _connectionString; }
    }
    private string _connectionString;

    public SqlDataReader GetReader(string cmdText, params object[] paramList)
    {
        using (var connection = new SqlConnection(GlobalSettings.ConnectionStrings.Sql))
        {
            using (var command = GetParamCommand(cmdText, paramList))
	        {
                try
                {
                    connection.Open();
                    command.Connection = connection;
                    SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                    return reader;
                }
                catch(Exception ex)
                {
                    connection.Close();
                    throw ex;
                }
            }
        }
    }
    public DataSet GetSet(string cmdText, params object[] paramList)
    {
        using (var connection = new SqlConnection(GlobalSettings.ConnectionStrings.Sql))
        {
            using (var command = GetParamCommand(cmdText, paramList))
	        {
                try
                {
                    connection.Open();
                    command.Connection = connection;
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    DataSet dataset = new DataSet();
                    dataAdapter.Fill(dataset);
                    return dataset;
                }
                catch(Exception ex)
                {
                    connection.Close();
                    throw ex;
                }
            }
        }
    }
    public DataTable GetTable(string cmdText, params object[] paramList)
    {
        using (var connection = new SqlConnection(GlobalSettings.ConnectionStrings.Sql))
        {
            using (var command = GetParamCommand(cmdText, paramList))
	        {
                try
                {
                    connection.Open();
                    command.Connection = connection;
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    DataTable datatable = new DataTable();
                    dataAdapter.Fill(datatable);
                    return datatable;
                }
                catch(Exception ex)
                {
                    connection.Close();
                    throw ex;
                }
            }
        }
    }
    public DataRow GetRow(string cmdText, params object[] paramList)
    {
        using (var connection = new SqlConnection(GlobalSettings.ConnectionStrings.Sql))
        {
            using (var command = GetParamCommand(cmdText, paramList))
	        {
                try
                {
                    connection.Open();
                    command.Connection = connection;
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    DataTable datatable = new DataTable();
                    dataAdapter.Fill(datatable);
                    if(datatable.Rows.Count > 0) return datatable.Rows[0];
                    else return null;
                }
                catch(Exception ex)
                {
                    connection.Close();
                    throw ex;
                }
            }
        }
    }
    public object GetField(string cmdText, params object[] paramList)
    {
        using (var connection = new SqlConnection(GlobalSettings.ConnectionStrings.Sql))
        {
            using (var command = GetParamCommand(cmdText, paramList))
	        {
                try
                {
                    connection.Open();
                    command.Connection = connection;
                    object o = command.ExecuteScalar();
                    return o;
                }
                catch(Exception ex)
                {
                    connection.Close();
                    throw ex;
                }
            }
        }
    }
    public int Execute(string cmdText, params object[] paramList)
    {
        using (var connection = new SqlConnection(GlobalSettings.ConnectionStrings.Sql))
        {
            using (var command = GetParamCommand(cmdText, paramList))
	        {
                try
                {
                    connection.Open();
                    command.Connection = connection;
                    command.CommandTimeout = 1200; // 20 minutes
                    return command.ExecuteNonQuery();
                }
                catch(Exception ex)
                {
                    connection.Close();
                    throw ex;
                }
            }
        }
    }

    private SqlCommand GetParamCommand(string cmdText, object[] paramList)
    {
        SqlCommand cmd = new SqlCommand();


        if(paramList.Length > 0)
        {
            object[] a = new object[paramList.Length];
            for(int i = 0; i < paramList.Length; i++)
            {
                a[i] = "@Param" + i.ToString();
            }
            cmd.CommandText = string.Format(cmdText, a);
            for(int i = 0; i < paramList.Length; i++)
            {
                if(paramList[i].GetType().Equals(typeof(String)))
                {
                    cmd.Parameters.Add("@Param" + i.ToString(), SqlDbType.NVarChar, 8000).Value = paramList[i];
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Param" + i.ToString(), paramList[i]);
                }
            }
        }
        else
        {
            cmd.CommandText = cmdText;
        }

        return cmd;
    }
}
