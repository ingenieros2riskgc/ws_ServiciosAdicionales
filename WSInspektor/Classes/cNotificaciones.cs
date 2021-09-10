using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OleDb;
using WSInspektor.Classes;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;

namespace WSInspektor.Classes
{
    public class cNotificaciones
    {
        MailMessage mms = new MailMessage();
        SmtpClient smt = new SmtpClient();

        private cDataBase cDataBase = new cDataBase();
        private WSLog WSLog = new WSLog();


        public DataTable SelectNotificaciones(string IdEmpresa)
        {
            DataTable dtInformacion = new DataTable();
            try
            {
                cDataBase.conectar();
                dtInformacion = cDataBase.ejecutarConsulta("SELECT * FROM Notificaciones.CorreosDestino WHERE IdEmpresa = " + IdEmpresa);
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                WSLog.errorMessage(ex.Message + ", " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
            return dtInformacion;
        }

        public bool EnviarCorreo(string IdConsultaEmpresa, string Prioridad, string Numeiden, string Nombre, string Para, string NombreUsuario)
        {
            string De = ConfigurationManager.AppSettings["from"].ToString();
            string Pss = ConfigurationManager.AppSettings["pwd"].ToString();
            string Mensaje = "";
            Mensaje = "Se encontraron coincidencias de Prioridad " + Prioridad + " para la consulta realizada con los siguientes parámetros de búsqueda:" + "<br>";
            Mensaje = Mensaje + "<br>";
            //Mensaje = Mensaje + "<br>" + "<B>Plan Estratégico</B>" + "<br>";
            Mensaje = Mensaje + " Número de Identificación : <B>" + Numeiden + "</B><br>";
            Mensaje = Mensaje + " Nombre : <B>" + Nombre + "</B><br>";
            Mensaje = Mensaje + " Nombre de Usuario Consulta : " + NombreUsuario + "<br>";
            Mensaje = Mensaje + "<br>";
            Mensaje = Mensaje + "Para obtener mayor información, lo invitamos a consulta esta coincidencia en nuestra aplicación Inspektor® https://inspektor.datalaft.com/ accediendo por la opción Consulta Reporte e ingresando el número de consulta <B>" + IdConsultaEmpresa + "</B>";
            Mensaje = Mensaje + "<br>" + " <br>";
            Mensaje = Mensaje + "Este es un mensaje enviado automáticamente por Inspektor®, por favor no responderlo.";

            string Asunto = "Notificación de coincidencia Prioridad " + Prioridad + "       Consulta Individual No. " + IdConsultaEmpresa;

            try
            {
                mms.From = new MailAddress(De);
                //for para enviar correos a lista de destinatarios
                string[] strSeparator = new string[] { ";" };
                string[] arrName = Para.Split(strSeparator, StringSplitOptions.None);
                int i = arrName.Length;
                for (int j = 0; j < i; j++)
                {
                    mms.To.Add(new MailAddress(arrName[j]));
                }

                mms.Body = Mensaje;
                mms.IsBodyHtml = true;
                mms.Subject = Asunto;
                smt.Host = ConfigurationManager.AppSettings["host"].ToString();
                smt.Port = Convert.ToInt16(ConfigurationManager.AppSettings["port"].ToString());
                smt.Credentials = new NetworkCredential(De, Pss);
                if (ConfigurationManager.AppSettings["enableSsl"].ToString() == "S")
                    smt.EnableSsl = true;
                else
                    smt.EnableSsl = false;
                smt.Send(mms);
                return true;
            }
            catch (Exception ex)
            {
                WSLog.errorMessage(ex.Message + ", " + ex.StackTrace);
                return true;
            }
        }
    }
}