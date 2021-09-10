using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OleDb;


namespace WSInspektor.Classes
{
    public class cInspektor
    {
        private cDataBase cDataBase = new cDataBase();
        private OleDbParameter[] parameters;
        private OleDbParameter parameter;
        private WSLog WSLog = new WSLog(); 

        public DataTable ConsultaDocumentoNombre(String documento, String nombre, String StrIdEmpresa, String notin)
        {
            DataTable dtInformacion = new DataTable();
            String condicion = string.Empty;
            try
            {
                if (documento != "" && nombre != "")
                {
                    cDataBase.conectar();
                    dtInformacion = cDataBase.ejecutarConsulta("exec SP_ConsultaPrioridad1 '" + nombre + "','" + documento + "','" + StrIdEmpresa + "'");
                    cDataBase.desconectar();
                }
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                WSLog.errorMessage("ERROR PRD_1: ConsultaDocumentoNombre >> " + ex.Message + ", " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
            return dtInformacion;
        }

        public DataTable ConsultaDocumento(String documento, String notin, String StrIdEmpresa)
        {
            DataTable dtInformacion = new DataTable();
            String condicion = string.Empty;
            try
            {
                if (documento != "")
                {
                    cDataBase.conectar();
                    dtInformacion = cDataBase.ejecutarConsulta("exec SP_ConsultaPrioridad2 '" + documento + "','" + notin + "','" + StrIdEmpresa + "'");
                    cDataBase.desconectar();
                }
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                WSLog.errorMessage("ERROR PRD_2:" + ex.Message + ", " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
            return dtInformacion;
        }

        public String remplazarCaracteres(String cadena)
        {
            return cadena.Replace("'", "''").Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U").Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u");
        }

        public DataTable ConsultaNombre(String nombre, String notin, String StrIdEmpresa)
        {
            DataTable dtInformacion = new DataTable();
            String condicion = string.Empty;
            nombre = remplazarCaracteres(nombre);
            try
            {
                if (nombre != "")
                {
                    cDataBase.conectar();
                    dtInformacion = cDataBase.ejecutarConsulta("exec SP_ConsultaPrioridad3 '" + nombre + "','" + notin + "','" + StrIdEmpresa + "'");
                    cDataBase.desconectar();
                }
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                WSLog.errorMessage("ERROR PRD_3: " + ex.Message + ", " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
            return dtInformacion;
        }

        public void insertarLog(String IdTipoLista, String Documento, String Nombre, String Alias, String NumeroRegistros, String Operacion, String OtrosParametrosConsulta, String IdUsuario, String IdEmpresa)
        {
            try
            {
                cDataBase.conectar();
                cDataBase.ejecutarQuery("INSERT INTO Seguridad.[Log] (IdUsuario, IdEmpresa, FechaConsulta, IdTipoLista, Documento, Nombre, Alias, NumeroRegistros, Operacion, OtrosParametrosConsulta) VALUES (" + IdUsuario + ", " + IdEmpresa + ", GETDATE(), " + IdTipoLista + ", '" + Documento + "', '" + Nombre + "', '" + Alias + "', " + NumeroRegistros + ", N'" + Operacion + "', N'" + OtrosParametrosConsulta + "')");
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                WSLog.errorMessage("ERROR insertarLog: " + ex.Message + ", " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        public String agregarConsultaProcuraduria(String IdUsuario, String IdEmpresa)
        {
            try
            {
                parameters = new OleDbParameter[4];
                parameter = new OleDbParameter("@IdConsulta", OleDbType.Integer, 18);
                parameter.Direction = ParameterDirection.Output;
                parameters[0] = parameter;
                parameter = new OleDbParameter("@IdUsuario", OleDbType.Integer);
                parameter.Value = Convert.ToInt32(IdUsuario);
                parameters[1] = parameter;
                parameter = new OleDbParameter("@IdEmpresa", OleDbType.Integer);
                parameter.Value = Convert.ToInt32(IdEmpresa);
                parameters[2] = parameter;
                parameter = new OleDbParameter("@IdTipoConsulta", OleDbType.Integer, 18);
                parameter.Value = 3;
                parameters[3] = parameter;
                cDataBase.conectar();
                cDataBase.ejecutarSPParametros("agregarConsultaProcuraduriaWS", parameters);
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                WSLog.errorMessage("ERROR agregarConsultaProcuraduria: " + ex.Message + ", " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
            return parameters[0].Value.ToString().Trim();
        }

        public void detalleConsultaProcuraduria(String IdConsulta, String NombreConsulta, String IdentificacionConsulta, String TipoIdentificacion, 
            String TipoVinculo, String AnoRenovacion, String EstadoMatricula, String Embargado, String Liquidacion, String Afiliado,
            String Multas, String Proponente)
        {
            try
            {
                /*cDataBase.conectar();
                cDataBase.ejecutarQuery("INSERT INTO Procuraduria.DetalleConsulta (IdConsulta, NombreConsulta, IdentificacionConsulta, DocumentoCoincidencia, NombreCoincidencia, NombreLista, Delito, Peps, FechaActualizacion, Validado, TipoIdentificacion, TipoVinculo, AnoRenovacion, EstadoMatricula, Embargado, Liquidacion, Afiliado, Multas, Proponente) VALUES (" + IdConsulta + ", '" + NombreConsulta + "', '" + IdentificacionConsulta + "', '', '', N'', '', '', GETDATE(), 0, '" + TipoIdentificacion + "', '" + TipoVinculo + "', '" + AnoRenovacion + "', '" + EstadoMatricula + "', '" + Embargado + "', '" + Liquidacion + "', '" + Afiliado + "', '" + Multas + "', '" + Proponente + "')");
                cDataBase.desconectar();*/

                parameters = new OleDbParameter[12];
                parameter = new OleDbParameter("@IdConsulta", OleDbType.Integer, 18);
                parameter.Value = Convert.ToInt32(IdConsulta);
                parameters[0] = parameter;
                parameter = new OleDbParameter("@NombreConsulta", OleDbType.VarChar);
                parameter.Value = NombreConsulta;
                parameters[1] = parameter;
                parameter = new OleDbParameter("@IdentificacionConsulta", OleDbType.VarChar);
                parameter.Value = IdentificacionConsulta;
                parameters[2] = parameter;
                parameter = new OleDbParameter("@TipoIdentificacion", OleDbType.VarChar);
                parameter.Value = TipoIdentificacion;
                parameters[3] = parameter;
                parameter = new OleDbParameter("@TipoVinculo", OleDbType.VarChar);
                parameter.Value = TipoVinculo;
                parameters[4] = parameter;
                parameter = new OleDbParameter("@AnoRenovacion", OleDbType.VarChar);
                parameter.Value = AnoRenovacion;
                parameters[5] = parameter;
                parameter = new OleDbParameter("@EstadoMatricula", OleDbType.VarChar);
                parameter.Value = EstadoMatricula;
                parameters[6] = parameter;
                parameter = new OleDbParameter("@Embargado", OleDbType.VarChar);
                parameter.Value = Embargado;
                parameters[7] = parameter;
                parameter = new OleDbParameter("@Liquidacion", OleDbType.VarChar);
                parameter.Value = Liquidacion;
                parameters[8] = parameter;
                parameter = new OleDbParameter("@Afiliado", OleDbType.VarChar);
                parameter.Value = Afiliado;
                parameters[9] = parameter;
                parameter = new OleDbParameter("@Multas", OleDbType.VarChar);
                parameter.Value = Multas;
                parameters[10] = parameter;
                parameter = new OleDbParameter("@Proponente", OleDbType.VarChar);
                parameter.Value = Proponente;
                parameters[11] = parameter;
                
                cDataBase.conectar();
                cDataBase.ejecutarSPParametros("[Procuraduria].[InsertaDetalleConsulta]", parameters);
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                WSLog.errorMessage("ERROR detalleConsultaProcuraduria: " + ex.Message + ", " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        private String IdConsultaEmpresa(String IdEmpresa)
        {
            DataTable dtInformacion = new DataTable();
            try
            {
                cDataBase.conectar();
                dtInformacion = cDataBase.ejecutarConsulta("SELECT (ISNULL(MAX(IdConsultaEmpresa), 0) + 1 ) AS IdConsultaEmpresa FROM Procuraduria.Consulta WHERE IdEmpresa = " + IdEmpresa);
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                throw new Exception(ex.Message);
            }
            return dtInformacion.Rows[0]["IdConsultaEmpresa"].ToString().Trim();
        }

        public String IdEmpresaConsulta(string IdConsulta)
        {
            DataTable dtInformacion = new DataTable();
            try
            {
                cDataBase.conectar();
                dtInformacion = cDataBase.ejecutarConsulta("SELECT IdConsultaEmpresa FROM Procuraduria.Consulta WHERE IdConsulta = " + IdConsulta);
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                throw new Exception(ex.Message);
            }
            return dtInformacion.Rows[0]["IdConsultaEmpresa"].ToString().Trim();
        }

        public DataTable ConsultaListasP(String documento, String nombre, String IdEmpresa)
        {
            DataTable dtInformacion = new DataTable();
            String condicion = string.Empty;
            nombre = nombre.Replace("'", "''");
            try
            {

                if (documento != string.Empty)
                {
                    if (condicion.ToString().Trim() == "")
                    {
                        condicion = " AND ((a.DocumentoIdentidad LIKE '" + documento.ToString().Trim() + "%') ";
                    }
                    else
                    {
                        condicion = condicion + " AND ((a.DocumentoIdentidad LIKE '" + documento.ToString().Trim() + "%') ";
                    }
                }

                if (nombre != string.Empty)
                {
                    nombre = nombre.Trim();
                    int IdentYNombre = 0;
                    string[] strSeparator = new string[] { " " };
                    string[] arrName = nombre.Split(strSeparator, StringSplitOptions.None);
                    int i = arrName.Length;
                    for (int j = 0; j < i; j++)
                    {
                        if (condicion.ToString().Trim() == "")
                        {
                            condicion = " AND ((a.NombreCompleto LIKE '%" + arrName[j] + "%') ";
                        }
                        else
                        {
                            if (j == 0)
                            {
                                condicion = condicion + " OR (";
                                condicion = condicion + " (a.NombreCompleto LIKE '%" + arrName[j] + "%') ";
                                IdentYNombre = 1;
                            }
                            else
                            {
                                condicion = condicion + " AND (a.NombreCompleto LIKE '%" + arrName[j] + "%') ";
                            }
                        }
                    }
                    if (IdentYNombre == 1)
                    {
                        condicion = condicion + ")";
                    }
                }
                string query = "SELECT a.IdListaPropia, CONVERT(varchar, a.FechaRegistro, 103) AS FechaRegistro,a.IdUsuario,"+"\n"+
                    "a.IdEmpresa, a.TipoDocumento, a.DocumentoIdentidad, a.NombreCompleto, b.NombreEmpresa as NombreTipoLista, " + "\n" +
                    "a.FuenteConsulta, a.TipoPersona, a.Alias, a.Delito, a.Zona, a.Link,a.OtraInformacion,a.IdTipoListaPropia " + "\n" +
                    "FROM Listas.ListaPropia a" + "\n" +
                    "INNER JOIN Seguridad.Empresas b on b.IdEmpresa = a.IdEmpresa " + "\n" +
                    "INNER JOIN Listas.TipoListasPropia as TLP on TLP.IdTipoListaPropia = a.IdTipoListaPropia " + "\n" + //FROM Listas.ListaPropia a, Seguridad.Empresas b
                    "where TLP.Estado = 1 and a.IdEmpresa = b.IdEmpresa " + "\n" + 
                    "AND a.IdEmpresa = " + IdEmpresa + condicion + ")";
                cDataBase.conectar();
                dtInformacion = cDataBase.ejecutarConsulta(query);
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                WSLog.errorMessage("ERROR ConsultaListasP: " + ex.Message + ", " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
            return dtInformacion;
        }

        public String IdGrupos(String IdConsulta)
        {
            DataTable dtInformacion = new DataTable();
            try
            {
                cDataBase.conectar();
                dtInformacion = cDataBase.ejecutarConsulta("DECLARE @VALORES VARCHAR(10) SELECT @VALORES = COALESCE(@VALORES + ', ', '') + CONVERT(varchar,c.IdGrupo) FROM listas.DetalleConsulta a,Listas.Lista b,Listas.TipoLista c WHERE a.IdLista = b.IdLista AND b.IdTipoLista = c.IdTipoLista AND a.IdConsulta = '" + IdConsulta + "' GROUP BY c.IdGrupo SELECT @VALORES IdGrupos ");
                cDataBase.desconectar();
            }
            catch (Exception ex)
            {
                cDataBase.desconectar();
                throw new Exception(ex.Message);
            }
            return dtInformacion.Rows[0]["IdGrupos"].ToString().Trim();
        }
        
    }
}