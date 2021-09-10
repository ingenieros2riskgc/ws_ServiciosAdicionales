using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OleDb;
using WSInspektor.Classes;
using System.Net;

namespace WSInspektor.Classes
{
    public class cCuenta
    {
        private cDataBase cDataBase = new cDataBase();

        public DataTable LoadEmpresa(String Usuario)
        {
            DataTable dtInformacion = new DataTable();
            try
            {
                cDataBase.conectar();
                dtInformacion = cDataBase.ejecutarConsulta("select IdEmpresa from Seguridad.usuarios where usuario = " + Usuario);
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                dtInformacion.Rows.Clear();
                dtInformacion.Columns.Clear();
                throw new Exception(ex.Message);
            }
            return dtInformacion;
        }

        public string LoadUsuario(string IdUsuario)
        {
            DataTable dtInformacion = new DataTable();
            try
            {
                cDataBase.conectar();
                dtInformacion = cDataBase.ejecutarConsulta("SELECT RTRIM(Nombres) + ' ' + RTRIM(Apellidos) NombreUsuario FROM Seguridad.Usuarios WHERE IdUsuario = " + IdUsuario);
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                dtInformacion.Rows.Clear();
                dtInformacion.Columns.Clear();
                throw new Exception(ex.Message);
            }
            return dtInformacion.Rows[0]["NombreUsuario"].ToString().Trim();
        }

        public DataTable autenticarUsuario(String Usuario, String Contrasena, String IdEmpresa)
        {
            DataTable dtInformacion = new DataTable();
            try
            {
                cDataBase.conectar();
                dtInformacion = cDataBase.ejecutarConsulta("SELECT LTRIM(RTRIM(ISNULL(Seguridad.Usuarios.Nombres, ''))) + ' ' + LTRIM(RTRIM(ISNULL(Seguridad.Usuarios.Apellidos, ''))) AS NombreUsuario, Seguridad.Usuarios.IdUsuario, LTRIM(RTRIM(Seguridad.Roles.NombreRol)) AS NombreRol, LTRIM(RTRIM(Seguridad.Usuarios.Usuario)) AS Usuario FROM Seguridad.Usuarios INNER JOIN Seguridad.Roles ON Seguridad.Usuarios.IdRol = Seguridad.Roles.IdRol INNER JOIN Seguridad.Empresas ON Seguridad.Usuarios.IdEmpresa = Seguridad.Empresas.IdEmpresa WHERE (Seguridad.Usuarios.Usuario = N'" + Usuario + "') AND (Seguridad.Usuarios.Contrasena = N'" + Contrasena + "') AND (Seguridad.Usuarios.Bloqueado = 0) AND (Seguridad.Usuarios.IdEmpresa = " + IdEmpresa + ") AND (Seguridad.Empresas.Bloqueado = 0) AND (Seguridad.Usuarios.WS_User = 1)  ");
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                dtInformacion.Rows.Clear();
                dtInformacion.Columns.Clear();
                throw new Exception(ex.Message);
            }
            return dtInformacion;
        }

        public string GetIPAddress()
        {
            string strHostName = System.Net.Dns.GetHostName();
            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            string ip = addr[1].ToString();
            return ip;
        }
      

    }
}