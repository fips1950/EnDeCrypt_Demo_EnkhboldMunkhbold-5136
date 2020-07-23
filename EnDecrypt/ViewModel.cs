using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace EnDecrypt
{
  public class ViewModel : INotifyPropertyChanged
  {
    private string _info;
    public string Info
    {
      get => this._info; set
      { this._info = value; OnPropertyChanged(); }
    }

    private string ConnectionString
    {
      get => $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={Environment.CurrentDirectory}\\Database1.mdf;Integrated Security=True";
    }

    private string FileName { get => @"XMLFile1.xml"; }

    public ICommand Cmd { get => new RelayCommand(CmdExec); }

    private void CmdExec(object obj)
    {
      switch (obj.ToString())
      {
        case "Create": Create(); break;
        case "Insert": Insert(); break;
        case "Load": Load(); break;
        case "Test": Test(); break;
        default: break;
      }
    }

    // Model methods
    internal void Create()
    {
      using (SqlConnection cn = new SqlConnection(ConnectionString))
      {
        cn.Open();
        using (SqlCommand cmd = new SqlCommand() { Connection = cn })
        {
          // delete previous table in SQL Server 2016 and above
          cmd.CommandText = "DROP TABLE IF EXISTS Table1";
          //' delete previous table in versions
          //cmd.CommandText = "If OBJECT_ID('Table1', 'U') IS NOT NULL DROP TABLE IF EXISTS Table1;"
          cmd.ExecuteNonQuery();
          // Create Table
          cmd.CommandText = "CREATE Table Table1([PkId] Integer Identity, [EncLicense] varbinary(Max), CONSTRAINT [PK_Table1] PRIMARY KEY ([PkId]));";
          cmd.ExecuteNonQuery();
          Info = "DataTable created";
        }
      }
    }

    internal void Insert()
    {
      // Load string from file to display
      string inpXML;
      using (StreamReader rdr = new StreamReader(FileName)) inpXML = rdr.ReadToEnd();
      Info = inpXML;
      try
      {
        // encrypt data
        byte[] encryptedData = EnDeCrypt.EncryptStringToBytes(inpXML);
        // insert to datatable
        using (SqlConnection cn = new SqlConnection(ConnectionString))
        {
          cn.Open();
          using (SqlCommand cmd = new SqlCommand() { Connection = cn })
          {
            // Insert record
            cmd.CommandText = "INSERT Table1([EncLicense]) VALUES (@output);";
            cmd.Parameters.AddWithValue("output", encryptedData);
            cmd.ExecuteNonQuery();
            Info = $"Record stored{Environment.NewLine}" + Info;
          }
        }
      }
      catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    internal void Load()
    {
      try
      {
        using (SqlConnection cn = new SqlConnection(ConnectionString))
        {
          cn.Open();
          using (SqlCommand cmd = new SqlCommand() { Connection = cn })
          {
            // Insert record
            cmd.CommandText = "SELECT [EncLicense] FROM Table1;";
            SqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            Info = EnDeCrypt.DecryptStringFromBytes((byte[])rdr[0]);
          }
        }
      }
      catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private void Test()
    {
      // Load string from file to display
      string inpString;
      using (StreamReader rdr = new StreamReader(FileName)) inpString = rdr.ReadToEnd();
      //
      // encrypt data
      byte[] encrypted = EnDeCrypt.EncryptStringToBytes(inpString);
      // decrypt
      string decrypted = EnDeCrypt.DecryptStringFromBytes(encrypted);
      Info = decrypted;
    }

    #region PropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    internal void OnPropertyChanged([CallerMemberName] string propName = "") =>
     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    #endregion

  }
}
