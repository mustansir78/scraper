using CsvHelper;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace consolePOC
{
    public class PYLLProcess
    {
        public void SaveCSVToSQLServer(string filePath)
        {
            try
            {
                CopyToTempTable(filePath);
                FilterDataFromTempTable();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void CopyToTempTable(string filePath)
        {
            StreamReader reader = new StreamReader(filePath);
            var csv = new CsvReader(reader);
            csv.Configuration.RegisterClassMap<PYLLMap>();

            var records = csv.GetRecords<PYLLRecord>().ToList();
            reader.Close();

            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings[Constants.DEFAULT_SQL_CONNECTION].ConnectionString);
            try
            {
                sqlConn.Open();
                SqlCommand sqlcmdCleanUp = new SqlCommand();
                sqlcmdCleanUp.CommandText = "CleanPYLL";
                sqlcmdCleanUp.CommandType = CommandType.StoredProcedure;
                sqlcmdCleanUp.Connection = sqlConn;
                sqlcmdCleanUp.ExecuteNonQuery();

                foreach (PYLLRecord record in records)
                {
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.CommandText = "AddPYLLRecord";
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Connection = sqlConn;

                    sqlCmd.Parameters.AddWithValue("@Reporting_Period", record.Reporting_Period);
                    sqlCmd.Parameters.AddWithValue("@Breakdown", record.Breakdown);
                    sqlCmd.Parameters.AddWithValue("@Level", record.Level);
                    sqlCmd.Parameters.AddWithValue("@Level_Description", record.Level_Description);
                    sqlCmd.Parameters.AddWithValue("@Condition", record.Condition);
                    sqlCmd.Parameters.AddWithValue("@Gender", record.Gender);
                    sqlCmd.Parameters.AddWithValue("@DSR", record.DSR);
                    sqlCmd.Parameters.AddWithValue("@CI_Lower", record.CI_Lower);
                    sqlCmd.Parameters.AddWithValue("@CI_Upper", record.CI_Upper);
                    sqlCmd.Parameters.AddWithValue("@Registered_Patients", record.Registered_Patients);
                    sqlCmd.Parameters.AddWithValue("@Years_of_life_lost", record.Years_of_life_lost);
                    sqlCmd.Parameters.AddWithValue("@Observed_deaths", record.Observed_deaths);

                    sqlCmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                sqlConn.Close();
            }
        }

        private void FilterDataFromTempTable()
        {
            int currentYear = DateTime.Today.Year;
            string sqlText = string.Empty;
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings[Constants.DEFAULT_SQL_CONNECTION].ConnectionString);
            SqlCommand cmd;
            try
            {
                sqlConn.Open();
                bool _exit = false;
                while (!_exit)
                {
                    sqlText = String.Format("select count(*) as records from PYLL_Temp where [Reporting Period] = '{0}'", currentYear);
                    cmd = new SqlCommand(sqlText, sqlConn);
                    int rowCount = (int)cmd.ExecuteScalar();
                    if (rowCount == 0)
                    {
                        currentYear--;
                        if (currentYear == 0)
                        {
                            _exit = true;
                        }
                    }
                    else
                    {
                        _exit = true;
                    }
                }
                if (currentYear != 0)
                {
                    sqlText = String.Format("insert into PYLL select * from PYLL_Temp where [Reporting Period] = '{0}' and gender = 'Male'", currentYear);
                    cmd = new SqlCommand(sqlText, sqlConn);
                    cmd.ExecuteNonQuery();
                    sqlText = String.Format("insert into PYLL select * from PYLL_Temp where [Reporting Period] = '{0}' and gender = 'Female'", currentYear);
                    cmd = new SqlCommand(sqlText, sqlConn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlConn.Close();
            }
        }
    }
}