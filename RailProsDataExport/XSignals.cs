using RailProsDataExport.Properties;
using Renci.SshNet;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

public class XSignalsDataContext
{
    private readonly SqlConnectionStringBuilder _connectionStringBuilder;
    private static readonly string AppLevel = Settings.Default.AppLevel.ToUpper();
    private readonly string ConnectionString;

    public XSignalsDataContext()
    {
        switch (AppLevel.ToUpper())
        {
            case "D":
                ConnectionString = "data source=engdevsql.nscorp.com;initial catalog=XSignals;user Id=XSIGNLDV;password=hzentybr;App=RailProsDataExport";
                break;
            case "Q":
                ConnectionString = "data source=engqasql.nscorp.com;initial catalog=XSignals;user id=XSIGNLQA;password=pzvfbgik;App=RailProsDataExport";
                break;
            case "P":
                ConnectionString = "data source=engsql.nscorp.com;initial catalog=XSignals;user id=ESXSIGNL;password=dzoyktwm;App=RailProsDataExport";
                break;
        }
        _connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
    }

    // Runs stored procedure to compare projects and projects_changes
    // Takes rows with differences and creates pipe delimited file and inserts into FTP site
    public void GetChangesFile()
    {
        using (SqlConnection sqlCon = new SqlConnection(_connectionStringBuilder.ConnectionString))
        {
            sqlCon.Open();

            using (SqlCommand sqlCmd = new SqlCommand("stpGetProjectTableDifferences", sqlCon))
            {
                sqlCmd.CommandType = CommandType.StoredProcedure;
                SqlDataReader reader = sqlCmd.ExecuteReader();

                // Export runs at most once a day, appending today's date should result in a unique file name
                string filePath = "//engqasql.nscorp.com/D$/XSignals/RailPros/";
                switch (AppLevel.ToUpper())
                {
                    case "D":
                        filePath = "//engqasql.nscorp.com/D$/XSignals/RailPros/";
                        break;
                    case "Q":
                        filePath = "//engqasql.nscorp.com/D$/XSignals/RailPros/";
                        break;
                    case "P":
                        filePath = "//engqasql.nscorp.com/D$/XSignals/RailPros/";
                        break;
                }
                
                string fileName = "NS_Outbound_" + DateTime.Today.ToString("d").Replace("/", "_") + ".csv";
                string fullFilePath = Path.Combine(filePath, fileName);
                using (StreamWriter sw = new StreamWriter(fullFilePath))
                {
                    object[] output = new object[reader.FieldCount];

                    for (int i = 0; i < reader.FieldCount; i++)
                        output[i] = reader.GetName(i);

                    sw.WriteLine(string.Join("|", output));

                    while (reader.Read())
                    {
                        reader.GetValues(output);
                        sw.WriteLine(string.Join("|", output));
                    }
                }

                reader.Close();
                sqlCon.Close();
                using (var sftp = new SftpClient("nsftp.nscorp.com", 22, "esfedpro", "xigfovpe"))
                {
                    sftp.Connect();

                    string remoteFilePath = "/outbound/" + fileName;

                    using (var fileStream = new FileStream(fullFilePath, FileMode.Open))
                    {
                        sftp.UploadFile(fileStream, remoteFilePath);
                    }

                    sftp.Disconnect();
                }
            }

            //SqlCommand sqlCmd = new SqlCommand(commandText, sqlCon);
            //SqlDataReader reader = sqlCmd.ExecuteReader();         
        }
    }
}
