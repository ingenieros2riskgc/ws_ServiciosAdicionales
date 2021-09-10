using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using WSInspektor.Classes;
using Encryption;
using System.Configuration;
using System.Net;
using System.IO;
using System.Data.OleDb;
using ThirdParty.Json.LitJson;
using ListasCliente.Classes;
using System.Web.Script.Serialization;
using System.Text;

namespace WSInspektor
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]


    public class WSInspektor : System.Web.Services.WebService
    {
        private cInspektor cInspektor = new cInspektor();
        private cCuenta cCuenta = new cCuenta();
        private cDataBase cDataBase = new cDataBase();
        private WSLog WSLog = new WSLog();
        private cNotificaciones cNotificaciones = new cNotificaciones();
        private Decryption Decryption = new Decryption();

        private String IdUsuario;
        private String IdEmpresa = ConfigurationManager.AppSettings["Company"].ToString();
        private String UsuarioWS = ConfigurationManager.AppSettings["User"].ToString();
        private String SeeLog = ConfigurationManager.AppSettings["SeeLog"].ToString();
        private String NConsulta = ConfigurationManager.AppSettings["NConsulta"].ToString();
        private String from_mail = ConfigurationManager.AppSettings["from"].ToString();
        string Coincidencias = string.Empty;


        [WebMethod]
        public string LoadWSInspektor(string Numeiden, string Nombre, string Password)
        {
            if (string.IsNullOrEmpty(Numeiden))
                Numeiden = "";
            if (string.IsNullOrEmpty(Nombre))
                Nombre = "";
            if (SeeLog == "S")
            {
                WSLog.errorMessage("INICIO TX");
                WSLog.errorMessage("Datos Recibidos 1 >> Numeiden:" + Numeiden + " Nombre:" + Nombre);
            }
            try
            {
                inicializarValores();
                DataTable dtInformacion = new DataTable();
                dtInformacion = cCuenta.autenticarUsuario(UsuarioWS, Password, IdEmpresa);
                ///Obtener la dirección ip de la maquina que genera la peticion
                //string IpCliente = cCuenta.GetIPAddress();

                if (dtInformacion.Rows.Count > 0)
                {
                    IdUsuario = dtInformacion.Rows[0]["IdUsuario"].ToString().Trim();
                    if (Numeiden.Trim() != "" || Nombre.Trim() != "")
                    {
                        return Consulta(Numeiden, Nombre);
                    }
                    else
                    {
                        if (SeeLog == "S")
                        {
                            WSLog.errorMessage("FIN TX Debe ingresar al menos un parámetro de consulta.");
                        }
                        return "Debe ingresar al menos un parámetro de consulta.";
                    }
                }
                else
                {
                    if (SeeLog == "S")
                    {
                        WSLog.errorMessage("FIN TX No se tiene Acceso a la Base de Datos de Inspektor. Fallo el Usuario o clave , favor comunicarse con Risk Consulting.");
                    }
                    return "No se tiene Acceso a la Base de Datos de Inspektor. Fallo el Usuario o clave , favor comunicarse con Risk Consulting.";
                }
            }
            catch (Exception ex)
            {
                if (SeeLog == "S")
                {
                    WSLog.errorMessage("FIN TX ERROR: ConsultaListas >> " + ex.Message + ", " + ex.StackTrace + ex.TargetSite + ex.HelpLink + ex.InnerException + ex.Source);
                }
                return "Error al cargar la información.";
            }
        }

        /// Consulta principal
        private String Consulta(string Documento, string Nombre)
        {
            DataTable infoDocumentoNombre = new DataTable();
            DataTable infoDocumento = new DataTable();
            DataTable infoNombre = new DataTable();
            DataTable infoDocumentoNombrePropias = new DataTable();

            DataTable informacionListas = new DataTable();
            string IdConsulta = string.Empty; ;
            string IdConsultaEmpresa = string.Empty;
            string resultado;
            informacionListas = infoConsulta();

            try
            {

                if (string.IsNullOrEmpty(Documento))
                    Documento = "";
                if (string.IsNullOrEmpty(Nombre))
                    Nombre = "";

                //Implementar el contador de limite de consultas
                int x = 5;
                if (x < 10)
                {
                    IdConsulta = cInspektor.agregarConsultaProcuraduria(IdUsuario, IdEmpresa);
                    IdConsultaEmpresa = cInspektor.IdEmpresaConsulta(IdConsulta);

                    cInspektor.detalleConsultaProcuraduria(IdConsulta, Nombre, Documento, "", "", "", "", "", "", "", "", "");
                    AgregarConsultaListas(IdConsulta, Documento, Nombre, "", "", "WS-Inspektor");
                    //IdGrupoLista

                    //Prioridad 1
                    infoDocumentoNombre = cInspektor.ConsultaDocumentoNombre(Documento, Nombre, IdEmpresa, notIn(informacionListas));
                    if (infoDocumentoNombre.Rows.Count > 0)
                    {
                        if (infoDocumentoNombre.Rows.Count >= 1000)
                        {
                            if (SeeLog == "S")
                            {
                                WSLog.errorMessage("FIN TX >> ConsultaDocumentoNombre");
                            }
                            return "La consulta devuelve mas de 1000 registros.";
                        }
                        else
                        {
                            for (int i = 0; i < infoDocumentoNombre.Rows.Count; i++)
                            {
                                informacionListas.Rows.Add(new Object[] { infoDocumentoNombre.Rows[i]["IdLista"].ToString(),
                                                              infoDocumentoNombre.Rows[i]["Prioridad"].ToString(),
                                                              infoDocumentoNombre.Rows[i]["TipoDocumento"].ToString(),
                                                              infoDocumentoNombre.Rows[i]["DocumentoIdentidad"].ToString(),
                                                              infoDocumentoNombre.Rows[i]["NombreCompleto"].ToString(),
                                                              infoDocumentoNombre.Rows[i]["IdTipoLista"].ToString(),
                                                              infoDocumentoNombre.Rows[i]["NombreTipoLista"].ToString(),
                                                              infoDocumentoNombre.Rows[i]["IdGrupoLista"].ToString(),
                                                              infoDocumentoNombre.Rows[i]["NombreGrupoLista"].ToString(),
                                                              infoDocumentoNombre.Rows[i]["FuenteConsulta"].ToString(),
                                                              infoDocumentoNombre.Rows[i]["Delito"].ToString(),
                                                              infoDocumentoNombre.Rows[i]["Link"].ToString()
                                                            });
                                AgregarDetalleConsultaListas(IdConsulta, informacionListas.Rows[i]["Prioridad"].ToString().Trim(), informacionListas.Rows[i]["IdLista"].ToString().Trim());
                            }
                            Coincidencias = "1";
                        }
                    }

                    //Prioridad 2
                    infoDocumento = cInspektor.ConsultaDocumento(Documento, notIn(informacionListas), IdEmpresa);
                    if (infoDocumento.Rows.Count > 0)
                    {
                        if (infoDocumento.Rows.Count >= 1000)
                        {
                            if (SeeLog == "S")
                            {
                                WSLog.errorMessage("FIN TX >> ConsultaDocumento. La consulta devuelve mas de 1000 registros.");
                            }
                            return "La consulta devuelve mas de 1000 registros.";
                        }
                        else
                        {
                            for (int i = 0; i < infoDocumento.Rows.Count; i++)
                            {
                                informacionListas.Rows.Add(new Object[] { infoDocumento.Rows[i]["IdLista"].ToString(),
                                                              infoDocumento.Rows[i]["Prioridad"].ToString(),
                                                              infoDocumento.Rows[i]["TipoDocumento"].ToString(),
                                                              infoDocumento.Rows[i]["DocumentoIdentidad"].ToString(),
                                                              infoDocumento.Rows[i]["NombreCompleto"].ToString(),
                                                              infoDocumento.Rows[i]["IdTipoLista"].ToString(),
                                                              infoDocumento.Rows[i]["NombreTipoLista"].ToString(),
                                                              infoDocumento.Rows[i]["IdGrupoLista"].ToString(),
                                                              infoDocumento.Rows[i]["NombreGrupoLista"].ToString(),
                                                              infoDocumento.Rows[i]["FuenteConsulta"].ToString(),
                                                              infoDocumento.Rows[i]["Delito"].ToString(),
                                                              infoDocumento.Rows[i]["Link"].ToString()
                                                            });
                                AgregarDetalleConsultaListas(IdConsulta, informacionListas.Rows[i]["Prioridad"].ToString().Trim(), informacionListas.Rows[i]["IdLista"].ToString().Trim());
                            }
                            if (Coincidencias == string.Empty)
                                Coincidencias = "2";
                        }
                    }

                    //Prioridad 3
                    infoNombre = cInspektor.ConsultaNombre(Nombre, notIn(informacionListas), IdEmpresa);
                    if (infoNombre.Rows.Count > 0)
                    {
                        if (infoNombre.Rows.Count >= 1000)
                        {
                            if (SeeLog == "S")
                            {
                                WSLog.errorMessage("FIN TX >> ConsultaNombre. La consulta devuelve mas de 1000 registros.");
                            }
                            return "La consulta devuelve mas de 1000 registros.";
                        }
                        else
                        {
                            for (int i = 0; i < infoNombre.Rows.Count; i++)
                            {
                                informacionListas.Rows.Add(new Object[] { infoNombre.Rows[i]["IdLista"].ToString(),
                                                              infoNombre.Rows[i]["Prioridad"].ToString(),
                                                              infoNombre.Rows[i]["TipoDocumento"].ToString(),
                                                              infoNombre.Rows[i]["DocumentoIdentidad"].ToString(),
                                                              infoNombre.Rows[i]["NombreCompleto"].ToString(),
                                                              infoNombre.Rows[i]["IdTipoLista"].ToString(),
                                                              infoNombre.Rows[i]["NombreTipoLista"].ToString(),
                                                              infoNombre.Rows[i]["IdGrupoLista"].ToString(),
                                                              infoNombre.Rows[i]["NombreGrupoLista"].ToString(),
                                                              infoNombre.Rows[i]["FuenteConsulta"].ToString(),
                                                              infoNombre.Rows[i]["Delito"].ToString(),
                                                              infoNombre.Rows[i]["Link"].ToString()
                                                            });
                                AgregarDetalleConsultaListas(IdConsulta, informacionListas.Rows[i]["Prioridad"].ToString().Trim(), informacionListas.Rows[i]["IdLista"].ToString().Trim());
                            }
                            if (Coincidencias == string.Empty)
                                Coincidencias = "3";
                        }
                    }

                    //Listas Propias
                    infoDocumentoNombrePropias = cInspektor.ConsultaListasP(Documento, Nombre, IdEmpresa);
                    if (infoDocumentoNombrePropias.Rows.Count > 0)
                    {
                        if (infoDocumentoNombrePropias.Rows.Count >= 1000)
                        {
                            if (SeeLog == "S")
                            {
                                WSLog.errorMessage("FIN TX >> ConsultaListasP. La consulta devuelve mas de 1000 registros.");
                            }
                            return "La consulta devuelve mas de 1000 registros.";
                        }
                        else
                        {
                            for (int i = 0; i < infoDocumentoNombrePropias.Rows.Count; i++)
                            {
                                informacionListas.Rows.Add(new Object[] { "-99",
                                                              "-1",
                                                              infoDocumentoNombrePropias.Rows[i]["TipoDocumento"].ToString(),
                                                              infoDocumentoNombrePropias.Rows[i]["DocumentoIdentidad"].ToString(),
                                                              infoDocumentoNombrePropias.Rows[i]["NombreCompleto"].ToString(),
                                                              "-99",
                                                              "Lista Propia"
                                                            });

                                AgregarDetalleConsultaListasPropias(IdConsulta, infoDocumentoNombrePropias.Rows[i]["TipoDocumento"].ToString().Trim(),
          infoDocumentoNombrePropias.Rows[i]["DocumentoIdentidad"].ToString().Trim(), infoDocumentoNombrePropias.Rows[i]["NombreCompleto"].ToString().Trim(), infoDocumentoNombrePropias.Rows[i]["FuenteConsulta"].ToString().Trim(),
          infoDocumentoNombrePropias.Rows[i]["TipoPersona"].ToString().Trim(), infoDocumentoNombrePropias.Rows[i]["Alias"].ToString().Trim(), infoDocumentoNombrePropias.Rows[i]["Delito"].ToString().Trim(), infoDocumentoNombrePropias.Rows[i]["Zona"].ToString().Trim(),
          infoDocumentoNombrePropias.Rows[i]["Link"].ToString().Trim(), infoDocumentoNombrePropias.Rows[i]["OtraInformacion"].ToString().Trim(), infoDocumentoNombrePropias.Rows[i]["IdTipoListaPropia"].ToString().Trim());

                            }
                            if (Coincidencias == string.Empty)
                                Coincidencias = "4";
                        }
                    }

                    string ResultProcuraduria = ConsultaProcuraduria("1",Documento);
                    string ResultRama = ConsultaRamaJudicial(Documento);
                    string ResultRues = ConsultaRues(Documento);
                    //cInspektor.insertarLog("0", Documento, Nombre, "", informacionListas.Rows.Count.ToString().Trim(), "Consulta de listas", "", IdUsuario, IdEmpresa);
                    if (informacionListas.Rows.Count >= 1000)
                    {
                        if (SeeLog == "S")
                        {
                            WSLog.errorMessage("FIN TX >> insertarLog. La consulta devuelve mas de 1000 registros.");
                        }
                        return "La consulta devuelve mas de 1000 registros.";
                    }
                    else
                    {
                        if (informacionListas.Rows.Count > 0)
                        {

                            resultado = "Número de consulta: " + IdConsultaEmpresa + "#";
                            resultado += "Cantidad de coincidencias: " + informacionListas.Rows.Count + "#";
                            /*for (int rows = 0; rows < informacionListas.Rows.Count; rows++)
                            {
                                resultado += "No: " + (rows + 1).ToString() + "|";
                                resultado += "Prioridad: " + informacionListas.Rows[rows]["Prioridad"].ToString().Trim().Replace("" + (char)13, String.Empty).Replace("" + (char)10, String.Empty).Replace("|", String.Empty).Replace("#", String.Empty) + "|";
                                resultado += "Tipo documento: " + informacionListas.Rows[rows]["TipoDocumento"].ToString().Trim().Replace("" + (char)13, String.Empty).Replace("" + (char)10, String.Empty).Replace("|", String.Empty).Replace("#", String.Empty) + "|";
                                resultado += "Numero documento: " + informacionListas.Rows[rows]["DocumentoIdentidad"].ToString().Trim().Replace("" + (char)13, String.Empty).Replace("" + (char)10, String.Empty).Replace("|", String.Empty).Replace("#", String.Empty) + "|";
                                resultado += "Nombre: " + informacionListas.Rows[rows]["NombreCompleto"].ToString().Trim().Replace("" + (char)13, String.Empty).Replace("" + (char)10, String.Empty).Replace("|", String.Empty).Replace("#", String.Empty) + "|";
                                resultado += "Número tipo lista: " + informacionListas.Rows[rows]["IdTipoLista"].ToString().Trim().Replace("" + (char)13, String.Empty).Replace("" + (char)10, String.Empty).Replace("|", String.Empty).Replace("#", String.Empty) + "|";
                                if (IdEmpresa == "169")
                                {
                                    resultado += "Lista: " + informacionListas.Rows[rows]["NombreTipoLista"].ToString().Trim().Replace("" + (char)13, String.Empty).Replace("" + (char)10, String.Empty).Replace("|", String.Empty).Replace("#", String.Empty) + "|";
                                    resultado += "GrupoLista: " + informacionListas.Rows[rows]["IdGrupoLista"].ToString().Trim().Replace("" + (char)13, String.Empty).Replace("" + (char)10, String.Empty).Replace("|", String.Empty).Replace("#", String.Empty) + "|";
                                }
                                else if (IdEmpresa == "248" || IdEmpresa == "1489")
                                {
                                    resultado += "Lista: " + informacionListas.Rows[rows]["NombreTipoLista"].ToString().Trim().Replace("" + (char)13, String.Empty).Replace("" + (char)10, String.Empty).Replace("|", String.Empty).Replace("#", String.Empty) + "#";
                                    resultado += "FuenteConsulta: " + informacionListas.Rows[rows]["FuenteConsulta"].ToString().Trim().Replace("" + (char)13, String.Empty).Replace("" + (char)10, String.Empty).Replace("|", String.Empty).Replace("#", String.Empty) + "#";
                                    resultado += "Delito: " + informacionListas.Rows[rows]["Delito"].ToString().Trim().Replace("" + (char)13, String.Empty).Replace("" + (char)10, String.Empty).Replace("|", String.Empty).Replace("#", String.Empty) + "#";
                                    resultado += "Link: " + informacionListas.Rows[rows]["Link"].ToString().Trim().Replace("" + (char)13, String.Empty).Replace("" + (char)10, String.Empty).Replace("|", String.Empty).Replace("#", String.Empty) + "#";
                                }
                                else
                                {
                                    resultado += "Lista: " + informacionListas.Rows[rows]["NombreTipoLista"].ToString().Trim().Replace("" + (char)13, String.Empty).Replace("" + (char)10, String.Empty).Replace("|", String.Empty).Replace("#", String.Empty) + "#";

                                }
                                
                            }*/
                            if (ResultProcuraduria != string.Empty)
                                resultado += "Procuraduria: " + ResultProcuraduria + "#";
                            if (ResultRama != string.Empty)
                                resultado += "RamaJudicial: " + ResultRama + "#";
                            if (ResultRues != string.Empty)
                                resultado += "RUES: " + ResultRues + "#";
                            //Valiamos si hay o no correo para notificaciones
                            if (!string.IsNullOrEmpty(from_mail))
                                CargaInfoNotificaciones(Coincidencias, IdConsultaEmpresa, Documento, Nombre);
                            if (SeeLog == "S")
                            {
                                WSLog.errorMessage("FIN TX > IdConsultaEmpresa:" + IdConsultaEmpresa + " Coincidencias:" + informacionListas.Rows.Count);
                            }
                            return resultado.Remove(resultado.Length - 1, 1);
                        }
                        else
                        {
                            if (SeeLog == "S")
                            {
                                WSLog.errorMessage("FIN TX > IdConsultaEmpresa:" + IdConsultaEmpresa + " Coincidencias:" + informacionListas.Rows.Count);
                            }
                            //Jaramillo Mora
                            if (IdEmpresa == "33")
                            {
                                resultado = "-1";
                            }
                            //Sumas y soluciones
                            else if (NConsulta == "1")
                            {
                                resultado = "Número de consulta: " + IdConsultaEmpresa + "#No existen registros asociados a los parámetros de consulta.";
                            }
                            else
                            {
                                resultado = "No existen registros asociados a los parámetros de consulta.";
                            }
                            if (ResultProcuraduria != string.Empty)
                                resultado += "Procuraduria: " + ResultProcuraduria + "#";
                            if (ResultRama != string.Empty)
                                resultado += "RamaJudicial: " + ResultRama + "#";
                            if (ResultRues != string.Empty)
                                resultado += "RUES: " + ResultRues + "#";
                            return resultado;
                        }
                    }
                }
                else
                {
                    resultado = "Se agotaron las consultas";
                }
                return resultado;
            }
            catch (Exception ex)
            {
                WSLog.errorMessage(ex.Message);
                return "Exception code: 0173|Unmanaged system exception, check transaction log";
            }
        }

        public void CargaInfoNotificaciones(string Coincidencias, string IdConsultaEmpresa, string Numeiden, string Nombre)
        {
            DataTable DtInfo = new DataTable();
            DtInfo = cNotificaciones.SelectNotificaciones(IdEmpresa);



            if (DtInfo.Rows.Count == 1)
            {
                if (Coincidencias == "1" && DtInfo.Rows[0]["Prioridad1"].ToString().Trim() == "True")
                {
                    cNotificaciones.EnviarCorreo(IdConsultaEmpresa, Coincidencias, Numeiden, Nombre, DtInfo.Rows[0]["CorreosPrioridad1"].ToString().Trim(), cCuenta.LoadUsuario(IdUsuario));
                }
                else if (Coincidencias == "2" && DtInfo.Rows[0]["Prioridad2"].ToString().Trim() == "True")
                {
                    cNotificaciones.EnviarCorreo(IdConsultaEmpresa, Coincidencias, Numeiden, Nombre, DtInfo.Rows[0]["CorreosPrioridad2"].ToString().Trim(), cCuenta.LoadUsuario(IdUsuario));
                }
                else if (Coincidencias == "3" && DtInfo.Rows[0]["Prioridad3"].ToString().Trim() == "True")
                {
                    cNotificaciones.EnviarCorreo(IdConsultaEmpresa, Coincidencias, Numeiden, Nombre, DtInfo.Rows[0]["CorreosPrioridad3"].ToString().Trim(), cCuenta.LoadUsuario(IdUsuario));
                }
                else if (Coincidencias == "4" && DtInfo.Rows[0]["Prioridad1"].ToString().Trim() == "True")
                {
                    cNotificaciones.EnviarCorreo(IdConsultaEmpresa, "Listas Propias", Numeiden, Nombre, DtInfo.Rows[0]["CorreosPrioridad1"].ToString().Trim(), cCuenta.LoadUsuario(IdUsuario));
                }
            }
        }

        private DataTable infoConsulta()
        {
            DataTable info = new DataTable();
            info.Columns.Add("IdLista", typeof(string));
            info.Columns.Add("Prioridad", typeof(string));
            info.Columns.Add("TipoDocumento", typeof(string));
            info.Columns.Add("DocumentoIdentidad", typeof(string));
            info.Columns.Add("NombreCompleto", typeof(string));
            info.Columns.Add("IdTipoLista", typeof(string));
            info.Columns.Add("NombreTipoLista", typeof(string));
            info.Columns.Add("IdGrupoLista", typeof(string));
            info.Columns.Add("NombreGrupoLista", typeof(string));
            info.Columns.Add("FuenteConsulta", typeof(string));
            info.Columns.Add("Delito", typeof(string));
            info.Columns.Add("Link", typeof(string));
            return info;
        }

        private String notIn(DataTable info)
        {
            String cadena = "";
            if (info.Rows.Count > 0)
            {
                for (int i = 0; i < info.Rows.Count; i++)
                {
                    if (i == (info.Rows.Count - 1))
                    {
                        cadena += info.Rows[i]["IdLista"].ToString().Trim();
                    }
                    else
                    {
                        cadena += info.Rows[i]["IdLista"].ToString().Trim() + ", ";
                    }
                }
            }
            return cadena.Trim();
        }

        private void inicializarValores()
        {
            IdUsuario = "0";
        }

        public void AgregarConsultaListas(String IdConsulta, String DocumentoConsulta, String NombreConsulta, String AliasConsulta,
            String CantCoincidencias, String TipoConsulta)
        {
            try
            {
                OleDbParameter[] parameters;
                OleDbParameter parameter;
                /*NombreConsulta = NombreConsulta.Replace("'", "''");
                AliasConsulta = AliasConsulta.Replace("'", "''");
                cDataBase.conectar();
                cDataBase.ejecutarConsulta("insert into Listas.Consulta (IdConsulta,FechaConsulta,IdTipoListaConsulta,DocumentoConsulta,NombreConsulta,AliasConsulta,CantCoincidencias,TipoConsulta,IdUsuario) values ('" + IdConsulta + "',GETDATE(),'0','" + DocumentoConsulta + "','" + NombreConsulta + "','" + AliasConsulta + "','" + CantCoincidencias + "','" + TipoConsulta + "','" + IdUsuario.ToString() + "')");
                cDataBase.desconectar();*/
                parameters = new OleDbParameter[7];
                parameter = new OleDbParameter("@IdConsulta", OleDbType.Integer, 18);
                parameter.Value = Convert.ToInt32(IdConsulta);
                parameters[0] = parameter;
                parameter = new OleDbParameter("@NombreConsulta", OleDbType.VarChar);
                parameter.Value = NombreConsulta;
                parameters[1] = parameter;
                parameter = new OleDbParameter("@DocumentoConsulta", OleDbType.VarChar);
                parameter.Value = DocumentoConsulta;
                parameters[2] = parameter;
                parameter = new OleDbParameter("@AliasConsulta", OleDbType.VarChar);
                parameter.Value = AliasConsulta;
                parameters[3] = parameter;
                parameter = new OleDbParameter("@CantCoincidencias", OleDbType.VarChar);
                parameter.Value = CantCoincidencias;
                parameters[4] = parameter;
                parameter = new OleDbParameter("@TipoConsulta", OleDbType.VarChar);
                parameter.Value = TipoConsulta;
                parameters[5] = parameter;
                parameter = new OleDbParameter("@IdUsuario", OleDbType.Integer, 18);
                parameter.Value = Convert.ToInt32(IdUsuario);
                parameters[6] = parameter;


                cDataBase.conectar();
                cDataBase.ejecutarSPParametros("[Listas].[InsertaConsultaLista] ", parameters);
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                WSLog.errorMessage(ex.Message + ", " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        public void AgregarDetalleConsultaListas(String IdConsulta, String Prioridad, String IdLista)
        {
            try
            {
                cDataBase.conectar();
                cDataBase.ejecutarConsulta("insert into Listas.DetalleConsulta (IdConsulta,IdUsuario,Prioridad,IdLista) values ('" + IdConsulta + "','" + IdUsuario.ToString() + "','" + Prioridad + "','" + IdLista + "')");
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                WSLog.errorMessage(ex.Message + ", " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        public void AgregarDetalleConsultaListasPropias(String IdConsulta, String TipoDocumento, String DocumentoIdentidad, String NombreCompleto, String FuenteConsulta, String TipoPersona, String Alias, String Delito, String Zona, String Link, String OtraInformacion, string IdTipoListaPropia)
        {
            try
            {
                cDataBase.conectar();
                cDataBase.ejecutarConsulta("insert into Listas.DetalleConsultaPropia (IdConsulta, IdUsuario, FechaRegistro, TipoDocumento, DocumentoIdentidad, NombreCompleto, FuenteConsulta, TipoPersona, Alias, Delito, Zona, Link, OtraInformacion,IdTipoListaPropia) VALUES ('" + IdConsulta + "', '" + IdUsuario.ToString() + "', " + "getdate()" + ", '" + TipoDocumento + "', '" + DocumentoIdentidad + "', '" + NombreCompleto + "', '" + FuenteConsulta + "', '" + TipoPersona + "', '" + Alias + "', '" + Delito + "', '" + Zona + "', '" + Link + "', '" + OtraInformacion + "'," + IdTipoListaPropia + ")");
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                WSLog.errorMessage(ex.Message + ", " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        public string DireccionIP()
        {
            string DireccionIPClient = string.Empty;
            string URL = "http://whatismyipaddress.com/";
            HttpWebRequest hRequest = ((HttpWebRequest)WebRequest.Create(URL));
            hRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 4.01; Windows CE; Smartphone; 176x220)";
            StreamReader reader = new StreamReader(hRequest.GetResponse().GetResponseStream());
            string res = reader.ReadToEnd();
            string a = res.Remove(0, res.IndexOf("//whatismyipaddress.com/ip/") + 27);
            string b = a.Substring(0, a.IndexOf(" style="));
            DireccionIPClient = b.Remove(b.Length - 1, 1);
            return DireccionIPClient;
        }
        
        private string ConsultaProcuraduria(string TipoIden, string NumeIden)
        {
            //divResponseRamaJudicial.Visible = true;
            string RtaSrvProcuraduria = string.Empty;
            try
            {
                //Consulta Servicio Web
                //string url = cCuenta.URL_SRV("1") + TipoIden + "&IdNumber=" + NumeIden.ToString() + "&Token=" + TokenProc;
                //string url = cCuenta.URL_SRV("1") + TipoIden + "&NumeroIdentificacion=" + NumeIden.ToString() + "&Token=" + TokenProc;
                //string url = "http://localhost/rama/procuraduria.php?TD=" + TipoIden + "&NumeroIdentificacion=" + NumeIden.ToString();
                string url = "http://10.244.16.229/Procuraduria/public/procuraduria.php?TD=" + TipoIden + "&NumeroIdentificacion=" + NumeIden.ToString();
                //string url = cCuenta.URL_SRV("11") + TipoIden + "&NumeroIdentificacion=" + NumeIden.ToString();

                string ProcuraduriaSN = string.Empty;
                string jsonResponseProcuraduria = string.Empty;
                string sinAntecedentesText = string.Empty;
                string TokenProc = "1F199A15";
                //HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(string.Format(url));
                //webReq.Method = "GET";
                //webReq.Timeout = 45000;
                //webReq.Headers.Add("Authorization", TokenProc);
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(string.Format(url));
                webReq.Method = "GET";
                webReq.Headers.Add("Authorization", TokenProc);
                StreamReader reader = new StreamReader(webReq.GetResponse().GetResponseStream());
                string strNameProcuraduria = string.Empty;
                strNameProcuraduria = reader.ReadToEnd();
                //webReq.Timeout = 10000;
                //divReponseProcuraduria.Visible = true;
                
                //StreamReader reader = new StreamReader(webReq.GetResponse().GetResponseStream());

                var resultQR = reader.ReadToEnd();
                //string jsonStringsign = resultQR;
                string jsonStringsign = strNameProcuraduria;
                JsonData json = JsonMapper.ToObject(jsonStringsign);
                string htmlResponse = json["html_response"].ToString();
                JsonData data = json["data"].ToJson();
                jsonResponseProcuraduria = data.ToString();
                if (json["not_criminal_records"] != null)
                {
                    JsonData dataSinAntecedentes = json["not_criminal_records"].ToJson();
                    sinAntecedentesText = dataSinAntecedentes.ToString();
                }

                StringWriter myWriter = new StringWriter();

                // Decode the encoded string.
                HttpUtility.HtmlDecode(htmlResponse, myWriter);


                RtaSrvProcuraduria = myWriter.ToString().Replace("\\r\\n ", "").Replace("<span>", "&nbsp;").Replace("h2", "b").Replace("h1", "b");
                
                //LtlResponseProcuraduria.Text = RtaSrvProcuraduria.Remove(RtaSrvProcuraduria.Length - 1, 1).Remove(0, 1);
                //LtlResponseProcuraduria.Text = "<FONT COLOR=\"Black\" SIZE=\"2\">DataLaft - Consulta Procuraduría OnLine</FONT> <br /><br />" + RtaSrvProcuraduria.Replace("\\\\r\\\\n", "").Replace("\"", "").Replace("\\", "").Replace("class=tablas", "class=\"table borderless\"");
                //cProcuraduria.agregarAuditoriaProcuraduria(NumeIden, LtlResponseProcuraduria.Text.Trim(), "0");
                //Session["Red"] = 0;
                //if (LtlResponseProcuraduria.Text.Trim() == "" || LtlResponseProcuraduria.Text.Contains("EL NÚMERO DE IDENTIFICACIÓN INGRESADO NO SE ENCUENTRA REGISTRADO EN EL SISTEMA"))
                //{
                //Session["Red"] = 1;
                //LtlResponseProcuraduria.Text = "<FONT COLOR=\"Red\" SIZE=\"4\">No existe información con los parámetros de búsqueda.</FONT>";//El número de identificación ingresado no se encuentra registrado en el sistema
                //}
                //Se valida si el regisro consultado presenta antecedentes en la Procuraduria
                //if (RtaSrvProcuraduria.Contains("PENALES") || RtaSrvProcuraduria.Contains("INHABILIDAD") || RtaSrvProcuraduria.Contains("INHABILIDADES"))
                //{
                //Enviar notificacion
                //NotificacionesSrvAdd = "1";
                //CargaInfoNotificacionesSrvAdd(NotificacionesSrvAdd);
                //}
            }
            catch (Exception ex)
            {
                WSLog cError = new WSLog();
                cError.errorMessage(ex.Message + ", " + ex.StackTrace);
                //Session["Red"] = 1;
                //divReponseProcuraduria.Visible = false;
                //Mensaje1("El servicio de Procuraduria OnLine no se encuentra disponible.</br>" + ex.Message);
                //LtlResponseProcuraduria.Text = "<FONT COLOR=\"Red\" SIZE=\"4\">El servicio de Procuraduria OnLine no se encuentra disponible.<br /> " + ex.Message + "</FONT>";
            }
            return RtaSrvProcuraduria;
        }
        private string ConsultaRamaJudicial(string NumeIden)
        {
            string RtaSrvRamaJudicial = string.Empty;
            try
            {
                //Consulta Servicio Web
                //string url = cCuenta.URL_SRV("2") + NumeIden.ToString() + "&tipo=3";
                //string url = "http://127.0.0.1:8000/public/api/rama?documento=" + NumeIden.ToString() + "&tipo=3";
                string url = "http://10.244.16.229/Rama/public/rama_judicial.php?documento=" + NumeIden.ToString() + "&tipo=3";
                //Trama.Visible = true;
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(string.Format(url));
                webReq.Timeout = 45000;
                webReq.Method = "GET";
                //webReq.Timeout = 60000;
                //divResponseRamaJudicial.Visible = true;
                string jsonResponseRamaJudicial = string.Empty;
                string jsonResponseRamaJudicialSinANtecedentes = string.Empty;
                
                try
                {
                    StreamReader reader = new StreamReader(webReq.GetResponse().GetResponseStream());
                    var resultQR = reader.ReadToEnd();
                    string jsonStringsign = resultQR;

                    JsonData json = JsonMapper.ToObject(jsonStringsign);
                    string htmlResponse = json["html_response"].ToString();
                    JsonData data = json["data"].ToJson();
                    jsonResponseRamaJudicial = data.ToString();
                    JsonData dataResponseSinAntecedente = json["not_criminal_records"].ToJson();
                    jsonResponseRamaJudicialSinANtecedentes = dataResponseSinAntecedente.ToString();
                    StringWriter myWriter = new StringWriter();

                    // Decode the encoded string.
                    HttpUtility.HtmlDecode(htmlResponse, myWriter);

                    string encabezado = "<FONT COLOR=\"Black\" SIZE=\"2\">DataLaft - Consulta Rama Judicial OnLine</FONT> <br /><br />" + "<b>REPUBLICA DE COLOMBIA<br />RAMA JUDICIAL DEL PODER PUBLICO<br />CONSEJO SUPERIOR DE LA JUDICATURA<br />JUZGADOS DE EJECUCION DE PENAS Y MEDIDAS DE SEGURIDAD<br /><br />" +
                    "LISTADO DE PROCESOS QUE COINCIDEN CON LOS PARÁMETROS DE LA BÚSQUEDA</b><br /><br />NÚMERO DE IDENTIFICACIÓN CONSULTADO: " + NumeIden.ToString() + "<br />";

                    RtaSrvRamaJudicial = encabezado + myWriter.ToString().Replace("<h1>", "").Replace("</h1>", "").Replace("<p>", "").Replace("</p>", "").Replace("+2", "1").Replace("border=3", "border=1").Replace("href=", "").Replace("100%", "80%").Replace("size=\"2\"", "size=\"1\"").Replace(">REPUBLICA", ">").Replace("DE COLOMBIA</", "</").Replace(">RAMA", ">").Replace("JUDICIAL DEL PODER PUBLICO</", "</").Replace(">CONSEJO", ">").Replace("SUPERIOR DE LA JUDICATURA</", "</").Replace(">JUZGADOS", ">").Replace("DE EJECUCION DE PENAS Y MEDIDAS DE SEGURIDAD</", "</").Replace(">LISTADO", ">").Replace("DE PROCESOS QUE COINCIDEN CON LOS PAR&Aacute;METROS DE LA B&Uacute;SQUEDA</", "</").Replace("<p align=\"center\">", "").Replace("<title>Resultados de la búsqueda</title>", "").Replace("&nbsp;", "").Replace("<img src=\"logo_escudo_girando.gif\" width=\"117\" height=\"117\" lowsrc=\"logo_escudo_girando.gif\" align=\"middle\" border=\"0\">", "").Replace("<img src=\"logo_csj.jpg\" width=\"103\" height=\"128\" align=\"center\" border=\"0\">", "").Replace("height=\"119\"", "");
                    //RtaSrvRamaJudicial = encabezado + htmlResponse.Replace("<h1>", "").Replace("</h1>", "").Replace("<p>", "").Replace("</p>", "").Replace("+2", "1").Replace("border=3", "border=1").Replace("href=", "").Replace("100%", "80%").Replace("size=\"2\"", "size=\"1\"").Replace(">REPUBLICA", ">").Replace("DE COLOMBIA</", "</").Replace(">RAMA", ">").Replace("JUDICIAL DEL PODER PUBLICO</", "</").Replace(">CONSEJO", ">").Replace("SUPERIOR DE LA JUDICATURA</", "</").Replace(">JUZGADOS", ">").Replace("DE EJECUCION DE PENAS Y MEDIDAS DE SEGURIDAD</", "</").Replace(">LISTADO", ">").Replace("DE PROCESOS QUE COINCIDEN CON LOS PAR&Aacute;METROS DE LA B&Uacute;SQUEDA</", "</").Replace("<p align=\"center\">", "").Replace("<title>Resultados de la búsqueda</title>", "").Replace("&nbsp;", "").Replace("<img src=\"logo_escudo_girando.gif\" width=\"117\" height=\"117\" lowsrc=\"logo_escudo_girando.gif\" align=\"middle\" border=\"0\">", "").Replace("<img src=\"logo_csj.jpg\" width=\"103\" height=\"128\" align=\"center\" border=\"0\">", "").Replace("height=\"119\"", "");
                    //RtaSrvRamaJudicial = encabezado + reader.ReadToEnd().Replace("<h1>", "").Replace("</h1>", "").Replace("<p>", "").Replace("</p>", "").Replace("+2", "1").Replace("border=3", "border=1").Replace("href=", "").Replace("100%", "80%").Replace("size=\"2\"", "size=\"1\"").Replace(">REPUBLICA", ">").Replace("DE COLOMBIA</", "</").Replace(">RAMA", ">").Replace("JUDICIAL DEL PODER PUBLICO</", "</").Replace(">CONSEJO", ">").Replace("SUPERIOR DE LA JUDICATURA</", "</").Replace(">JUZGADOS", ">").Replace("DE EJECUCION DE PENAS Y MEDIDAS DE SEGURIDAD</", "</").Replace(">LISTADO", ">").Replace("DE PROCESOS QUE COINCIDEN CON LOS PAR&Aacute;METROS DE LA B&Uacute;SQUEDA</", "</").Replace("<p align=\"center\">", "").Replace("<title>Resultados de la búsqueda</title>", "").Replace("&nbsp;", "").Replace("<img src=\"logo_escudo_girando.gif\" width=\"117\" height=\"117\" lowsrc=\"logo_escudo_girando.gif\" align=\"middle\" border=\"0\">", "").Replace("<img src=\"logo_csj.jpg\" width=\"103\" height=\"128\" align=\"center\" border=\"0\">", "").Replace("height=\"119\"", "");
                    //Informacion de los JEPMS
                    RtaSrvRamaJudicial = RtaSrvRamaJudicial.Replace("bogotajepms", "<b></br></br>JEPMS BOGOTA</b>").Replace("villavicenciojepms", "<b></br></br>JEPMS VILLAVICENCIO</b>").Replace("armeniajepms", "<b></br></br>JEPMS ARMENIA</b>").Replace("bucaramangajepms", "<b></br></br>JEPMS BUCARAMANGA</b>").Replace("bugajepms", "<b></br></br>JEPMS BUGA</b>").Replace("florenciajepms", "<b></br></br>JEPMS FLORENCIA</b>").Replace("ibaguejepms", "<b></br></br>JEPMS IBAGUE</b>").Replace("medellinjepms", "<b></br></br>JEPMS MEDELLIN</b>").Replace("neivajepms", "<b></br></br>JEPMS NEIVA</b>").Replace("palmirajepms", "<b></br></br>JEPMS PALMIRA</b>").Replace("pastojepms", "<b></br></br>JEPMS PASTO</b>").Replace("pereirajepms", "<b></br></br>JEPMS PEREIRA</b>").Replace("popayanjepms", "<b></br></br>JEPMS POPAYAN</b>").Replace("calijepms", "<b></br></br>JEPMS CALI</b>").Replace("tunjajepms", "<b></br></br>JEPMS TUNJA</b>");
                    //Sin info
                    RtaSrvRamaJudicial = RtaSrvRamaJudicial.Replace("<body><div id='resultado'></div></body></html>", "<div id='resultado'><br /><br /><FONT COLOR=\"Blue\" SIZE=\"4\">NO EXISTE INFORMACIÓN CON LOS PARÁMETROS DE LA BÚSQUEDA</FONT><br /></div></body></html>");
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.Timeout || e.Status == WebExceptionStatus.ConnectFailure)
                    {
                        RtaSrvRamaJudicial = "<div id='resultado'><br /><br /><FONT COLOR=\"RED\" SIZE=\"4\">Rama judicial caida, porfavor intentelo mas tarde.</FONT><br /></div>";
                    }
                }
                //LtlResponseRamaJudicial.Text = RtaSrvRamaJudicial;

                if (RtaSrvRamaJudicial == "")
                {
                    RtaSrvRamaJudicial = "No existe información con los parámetros de búsqueda.";//El número de identificación ingresado no se encuentra registrado en el sistema
                }

                //if (RtaSrvRamaJudicial.Contains("NUMERO RADICACION"))
                //{
                //    if (string.IsNullOrEmpty(NotificacionesSrvAdd))
                //        NotificacionesSrvAdd = "2";
                //    else
                //        NotificacionesSrvAdd = "3";

                //    //CargaInfoNotificacionesSrvAdd(NotificacionesSrvAdd);
                //}
            }
            catch (Exception ex)
            {
                //divResponseRamaJudicial.Visible = false;
                string encabezadoError = "<FONT COLOR=\"Black\" SIZE=\"2\">DataLaft - Consulta Rama Judicial OnLine</FONT> <br /><br />" + "<b>REPUBLICA DE COLOMBIA<br />RAMA JUDICIAL DEL PODER PUBLICO<br />CONSEJO SUPERIOR DE LA JUDICATURA<br />JUZGADOS DE EJECUCION DE PENAS Y MEDIDAS DE SEGURIDAD<br /><br />" +
                "LISTADO DE PROCESOS QUE COINCIDEN CON LOS PARÁMETROS DE LA BÚSQUEDA</b><br /><br />NÚMERO DE IDENTIFICACIÓN CONSULTADO: " + NumeIden.ToString() + "<br />";
                RtaSrvRamaJudicial = encabezadoError + "<br /><br /><FONT COLOR=\"Red\" SIZE=\"4\">El servicio de Rama Judicial OnLine no se encuentra disponible.<br /> " + ex.Message + "</FONT>";
                //Mensaje1("El servicio de Rama Judicial OnLine no se encuentra disponible.</br>");
            }
            return RtaSrvRamaJudicial;
        }
        private string ConsultaRues(string documento)
        {
            string respuesta = string.Empty;
            cXsHttpResponse<cInfoRues> obj = null;
            try
            {
                obj = new JavaScriptSerializer().Deserialize<cXsHttpResponse<cInfoRues>>(DescargarDataClienteHttp(5, null, documento));
                if (obj.HasError)
                    throw new Exception(obj.ErrorMessage);
            }
            catch (Exception ex)
            {
                //divErrorRues.Visible = true;
                //lblErrorRues.Text = ex.Message;
                //jsonNewConsulta["rues_mensaje"] = mensajeServicioInactivo;
                respuesta = "El servicio se encuentra fuera de línea temporalmente. Lamentamos las molestias ocasionadas.";
            }
            if (obj != null && obj.Data != null)
            {
                /*jsonNewConsulta["rues"] = new JavaScriptSerializer().Serialize(obj.Data);
                jsonNewConsulta["rues_mensaje"] = "";
                divResultadosRues.Visible = true;*/
                respuesta = new JavaScriptSerializer().Serialize(obj.Data).ToString();
                List<cInfoRues> lst = new List<cInfoRues>();
                lst.Add(obj.Data);
                /*rptRues.DataSource = lst;
                rptRues.DataBind();*/
            }
            else if (obj != null && obj.Data == null)
            {
                //jsonNewConsulta["rues_mensaje"] = "No hay coincidencias";
                //divResultadosRues.Visible = false;
                //divRuesSinResultados.Visible = true;
                respuesta = "No hay coincidencias";
            }
            return respuesta;
        }
        private string DescargarDataClienteHttp(int serviceType, string documentType, string documentId)
        {
            try
            {
                string data = string.Empty;
                string url = "http://10.244.16.231:2000/";
                var client = new WebClient1();
                client.Encoding = Encoding.UTF8;
                if (!string.IsNullOrEmpty(documentType))
                    data = client.DownloadString($"{url}/{serviceType}/{documentType}/{documentId}");
                else
                    data = client.DownloadString($"{url}/{serviceType}/{documentId}");

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private class WebClient1 : WebClient
        {
            int Timeout = 20000;
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = Timeout;
                return w;
            }
        }
    }
}