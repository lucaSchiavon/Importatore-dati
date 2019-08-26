using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrjImportaCategoriaVarie
{
    class Program
    {
        static void Main(string[] args)
        {
            
            StreamWriter sw = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "") + @"\Importatore.log", true);
           
            try
            {
                
                Console.WriteLine("start");
             
                sw.WriteLine("start");
              
                ConsoleApp1.IniParser fileIni = new ConsoleApp1.IniParser(Path.Combine(Environment.CurrentDirectory, "") + @"\Inizializzazione.ini");
                Console.WriteLine("read ini file: " + Path.Combine(Environment.CurrentDirectory, "") + @"\Inizializzazione.ini");
                sw.WriteLine("read ini file: " + Path.Combine(Environment.CurrentDirectory, "") + @"\Inizializzazione.ini");
               
            string connStr = fileIni.GetSetting("IMPORTATORE", "connStr");
                string NuovaSedeId = fileIni.GetSetting("IMPORTATORE", "NuovaSedeId");
                string SedeDaClonareId = fileIni.GetSetting("IMPORTATORE", "SedeDaClonareId");

                //string Sql = "SELECT [TipografiaID],[RagioneSociale],[HostUrl],[ConnectionString],(SELECT SUBSTRING(SUBSTRING([ConnectionString], CHARINDEX('Initial Catalog=', [ConnectionString]) + LEN('Initial Catalog='), 100), 0, CHARINDEX(';', SUBSTRING([ConnectionString], CHARINDEX('Initial Catalog=', [ConnectionString]) + LEN('Initial Catalog='), 100)))) AS DbName FROM [edigit_v2].[dbo].[TBL_Tipografie]";
                string Sql = @"INSERT INTO [dbo].[SediCategorie]
           ([SedeID]
      ,[CategoriaID]
      ,[ProdottoID]
      ,[PathPDF]
      ,[PathPDFStampa]
      ,[ImmagineAnteprima])
           SELECT  " + NuovaSedeId +
      @",[CategoriaID]
      ,[ProdottoID]
      ,[PathPDF]
      ,[PathPDFStampa]
      ,[ImmagineAnteprima]
  FROM [SediCategorie] where SedeID=" + SedeDaClonareId;

               ExecuteNonQuery(connStr, Sql);

                DataTable DtSediCategorieSedeDaClonare = getDataTable(connStr, @"SELECT * FROM [SediCategorie] where sedeid=" + SedeDaClonareId);

                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (DataRow dr in DtSediCategorieSedeDaClonare.Rows)
                {

                    DataTable DtGetSedeCategoriaIdNew = getDataTable(connStr, @" SELECT [SedeCategoriaID] FROM [SediCategorie] where sedeid=" + NuovaSedeId + " and categoriaid=" + dr["CategoriaID"] + " and prodottoid=" + dr["ProdottoID"]);
                    //chiave=idSedeDaClonare e valore nuovoid
                    dict.Add(dr["SedeCategoriaID"].ToString(), DtGetSedeCategoriaIdNew.Rows[0][0].ToString());
                }



  //              string sqlInsertIntoSediMarkups_Rel = @"INSERT INTO [dbo].[SediMarkups_Rel]
  //         ([SedeID]
  //    ,[MarkupID]
  //    ,[Ordinamento]
  //    ,[SedeCategoriaID]
  //    ,[TipoMarkup]
  //    ,[IsEditDisattivo]
  //    ,[IsPrefissoModificabile])
  //         SELECT  " + NuovaSedeId + 
  //    @",[MarkupID]
  //    ,[Ordinamento]
  //    ," + dict[ + 
  //    @",[TipoMarkup]
  //    ,[IsEditDisattivo]
  //    ,[IsPrefissoModificabile]
  //FROM [SediMarkups_Rel] where SedeID=" + SedeDaClonareId;

                //recupero i dati della tabella SediMarkups_Rel della sede da clonare
                DataTable dtSediMarkups_Rel = getDataTable(connStr, @"SELECT [SedeMarkupRelID]
                  ,[SedeID]
                  ,[MarkupID]
                  ,[Ordinamento]
                  ,[SedeCategoriaID]
                  ,[TipoMarkup]
                  ,[IsEditDisattivo]
                  ,[IsPrefissoModificabile]
              FROM [SediMarkups_Rel] where sedeid = " + SedeDaClonareId);

              

                    //DataTable dtOutput = new DataTable();
                    double Totale=0;
            foreach (DataRow dr in dtSediMarkups_Rel.Rows)
            {
                    

                    string sqlInsert = @"INSERT INTO [dbo].[SediMarkups_Rel]
           ([SedeID]
           ,[MarkupID]
           ,[Ordinamento]
           ,[SedeCategoriaID]
           ,[TipoMarkup])
     VALUES("  + NuovaSedeId + "," + dr["MarkupID"] + "," + dr["Ordinamento"] + "," + dict[dr["SedeCategoriaID"].ToString()] + "," + dr["TipoMarkup"]  + ")";

                    ExecuteNonQuery(connStr, sqlInsert);

                }


               
                Console.WriteLine("finish");
                sw.WriteLine("finish");
                sw.Dispose();
                sw.Close();
              
                Console.Read();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sw.WriteLine(ex.Message);
                sw.WriteLine("finish");
                sw.Dispose();
                sw.Close();
              
            }

        }

        private static  DataTable getDataTable(string connstring, string sqlQuery)
        {

            DataTable dt;
                using (SqlConnection con = new SqlConnection(connstring))
                {
                    con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(sqlQuery, con))
                    {
                        dt = new DataTable();
                        da.Fill(dt);

                    }
                }
            return dt;
            }

        private static void ExecuteNonQuery(string connstring, string sqlQuery)
        {

            using (SqlConnection connection = new SqlConnection(
                connstring))
            {
                SqlCommand command = new SqlCommand(sqlQuery, connection);
                command.Connection.Open();
                command.ExecuteNonQuery();

            }
        }
    }

   
}
