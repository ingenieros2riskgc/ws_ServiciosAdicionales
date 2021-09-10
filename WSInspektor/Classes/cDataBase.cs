using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OleDb;
using Encryption;

namespace WSInspektor.Classes
{
    public class cDataBase
    {
        private OleDbConnection oleDbCnn;
        private Decryption Decryption = new Decryption();

        public cDataBase()
        {
            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/MyWebSiteRoot");

            if (rootWebConfig.ConnectionStrings.ConnectionStrings.Count > 0)
            {
                System.Configuration.ConnectionStringSettings connString = rootWebConfig.ConnectionStrings.ConnectionStrings["AY5T0BVOEigES7h0SKXRcl/ja6ofFUO2"];
                //Decryption.textoToDecrypt().ToString()
                oleDbCnn = new OleDbConnection(Decryption.textoToDecrypt(connString.ToString()).ToString());
            }
        }

        public void conectar()
        {
            oleDbCnn.Open();
        }

        public void desconectar()
        {
            oleDbCnn.Close();
        }

        public DataTable ejecutarConsulta(String txtQuery)
        {
            DataTable dtInformation = new DataTable();
            OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(txtQuery, oleDbCnn);
            oleDbDataAdapter.SelectCommand.CommandType = CommandType.Text;
            oleDbDataAdapter.SelectCommand.CommandTimeout = 3600;
            oleDbDataAdapter.Fill(dtInformation);
            return dtInformation;
        }

        public void ejecutarQuery(String txtQuery)
        {
            OleDbCommand oleDbCmn = new OleDbCommand(txtQuery, oleDbCnn);
            oleDbCmn.CommandType = CommandType.Text;
            oleDbCmn.CommandTimeout = 3600;
            oleDbCmn.ExecuteNonQuery();
        }

        public void ejecutarSPParametros(String txtNombreSP, OleDbParameter[] objParameter)
        {
            OleDbCommand oleDbCmm = new OleDbCommand(txtNombreSP, oleDbCnn);
            oleDbCmm.CommandType = CommandType.StoredProcedure;
            foreach (OleDbParameter objParametro in objParameter)
            {
                oleDbCmm.Parameters.Add(objParametro);
            }
            oleDbCmm.CommandTimeout = 3600;
            oleDbCmm.ExecuteNonQuery();
        }

    }
}