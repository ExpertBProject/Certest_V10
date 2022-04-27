using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;
using System.IO;

namespace Cliente
{
    public static class ModelosYNormas
    {
       
        #region Confirming Ibercaja
        public static void GeneroConfirmingIbercaja(SAPbouiCOM.Form FormPARAM)
        {
            string cFechaFich, cDatos;
            double nImpTotal = 0;
            int nRegTipo10 = 0, nRegTotales = 0;
            SAPbobsCOM.Recordset oRec = null;

            string DirectorioActual = Environment.CurrentDirectory;
            DateTime dFecha = DateTime.Now;
            cFechaFich = dFecha.ToString("d");
            string cIni = "ConfIbercaja-" + dFecha.Year.ToString("0000") + dFecha.Month.ToString("00") + dFecha.Day.ToString("00") + ".txt";
            string Fichero = TratamientoFicheros.EscojoFichero(cIni);

            if (Fichero == "")
            {
                Matriz.gen.SBOApp.MessageBox("Ha de escribir un nombre para el fichero", 1, "Ok", "", "");
                Environment.CurrentDirectory = DirectorioActual;
                return;
            }

            //Click el boton de expandir
            FormPARAM.Items.Item("146").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
            SAPbouiCOM.Matrix oMatrix = (SAPbouiCOM.Matrix)FormPARAM.Items.Item("62").Specific;

            #region Datos de banco de la empresa
            SAPbouiCOM.Matrix oMatBanco = (SAPbouiCOM.Matrix)FormPARAM.Items.Item("58").Specific;
            int nSelec = 0;
            int nCuantos = 0;
            for (int j = 1; j <= oMatBanco.VisualRowCount; j++)
            {
                if (((SAPbouiCOM.CheckBox)oMatBanco.GetCellSpecific("1", j)).Checked)
                {
                    nSelec = j;
                    nCuantos++;
                }
            }

            if (nCuantos != 1)
            {
                Matriz.gen.SBOApp.MessageBox("Seleccione un unico banco para el fichero ", 1, "Ok", "", "");
                Environment.CurrentDirectory = DirectorioActual;
                return;
            }

            #region Fecha de emision
            string cAno = ((SAPbouiCOM.EditText)FormPARAM.Items.Item("14").Specific).Value.Substring(0, 4);
            string cMes = ((SAPbouiCOM.EditText)FormPARAM.Items.Item("14").Specific).Value.Substring(4, 2);
            string cDia = ((SAPbouiCOM.EditText)FormPARAM.Items.Item("14").Specific).Value.Substring(6, 2);
            DateTime dFechaEmision = new DateTime(Convert.ToInt16(cAno), Convert.ToInt16(cMes), Convert.ToInt16(cDia));
            #endregion

            string cCuenta = ((SAPbouiCOM.EditText)oMatBanco.GetCellSpecific("1250000019", nSelec)).Value;
            string cEntidad = ((SAPbouiCOM.EditText)oMatBanco.GetCellSpecific("1250000017", nSelec)).Value;

            //Necesito contrato, sucursal, digcontrol, NumLineaConfirming                                    
            string cOficina = "", cCodOrdenante = "", cDigitosControl = "", cPagCodigos = "";
            SAPbobsCOM.Recordset oRecBanco = Matriz.gen.refDi.SQL.sqlComoRsB1(sqlDatosBancoPropio(cEntidad, cCuenta));
            if (!oRecBanco.EoF)
            {
                cOficina = oRecBanco.Fields.Item("Sucursal").Value.ToString();
                cDigitosControl = oRecBanco.Fields.Item("DigControl").Value.ToString();
                cCodOrdenante = oRecBanco.Fields.Item("Usr4").Value.ToString();
                cPagCodigos = oRecBanco.Fields.Item("PaginaCodigosCF").Value.ToString();
            }
            if (cCodOrdenante.Length != 10)
            {
                Matriz.gen.SBOApp.MessageBox("Verifique Codigo de Ordenante (Usr4) para el confirming (10 caracteres)", 1, "Ok", "", "");

                Environment.CurrentDirectory = DirectorioActual;
                oMatrix = null;
                #region Borro el fichero
                try
                {
                    File.Delete(Fichero);
                }
                catch (Exception ex)
                {
                    Matriz.gen.SBOApp.MessageBox(ex.Message, 1, "Ok", "", "");
                }
                #endregion

                oMatrix = null;
                oMatBanco = null;
                return;
            }


            oMatBanco = null;
            #endregion

            #region Datos de empresa            
            string cNifEmpresaSINPais = Matriz.gen.refDi.OADM.CIF;
            string cNombreEmpresa = Matriz.gen.refDi.OADM.razonSocial;

            if (cNifEmpresaSINPais.Length > 2) cNifEmpresaSINPais = cNifEmpresaSINPais.Substring(2);

            string cNombrecompañia = Matriz.gen.refDi.OADM.razonSocial;
            if (cNifEmpresaSINPais == "")
            {
                Matriz.gen.SBOApp.MessageBox("Vefifique el NIF de la empresa", 1, "Ok", "", "");
                oMatrix = null;
                return;
            }
            #endregion

            double nImpRemesa = Conversiones.StringSAPToDoubleSistema(((SAPbouiCOM.EditText)FormPARAM.Items.Item("215").Specific).Value, "EUR");

            Encoding oCodePage = null;
            switch (cPagCodigos)
            {
                //defecto - ¿ansi ?
                case "01":
                    oCodePage = Encoding.Default;
                    break;
                //utf8
                case "02":
                    oCodePage = Encoding.UTF8;
                    break;
            }
            StreamWriter cFicheroConfIbercaja = new StreamWriter(Fichero, false, oCodePage);

            cDatos = Cabec01ConfIbercaja(cNifEmpresaSINPais, cCodOrdenante, cOficina, cCuenta, cDigitosControl);
            cFicheroConfIbercaja.WriteLine(cDatos);
            nRegTotales++;

            cDatos = Cabec02ConfIbercaja(cNifEmpresaSINPais, cCodOrdenante, cNombreEmpresa);
            cFicheroConfIbercaja.WriteLine(cDatos);
            nRegTotales++;

            for (int j = 1; j <= oMatrix.RowCount; j++)
            {
                //Si no esta el check, salto
                if (!((SAPbouiCOM.CheckBox)oMatrix.GetCellSpecific("1", j)).Checked) continue;

                //Registros de pago, los agrupados, los del triangulo amarillo 
                //Lo hago todo con los otros                
                if (((SAPbouiCOM.EditText)oMatrix.GetCellSpecific("11", j)).Value == "")
                {
                    //18 facturas, 19 abonos
                    string cTipoDoc = ((SAPbouiCOM.EditText)oMatrix.GetCellSpecific("42", j)).Value;
                    string cNumDoc = ((SAPbouiCOM.EditText)oMatrix.GetCellSpecific("52", j)).Value;
                    double nImporte = Conversiones.StringSAPToDoubleSistema(((SAPbouiCOM.EditText)oMatrix.GetCellSpecific("32", j)).Value, "EUR");
                    DateTime dFechaVto = Conversiones.StringSAPDate(((SAPbouiCOM.EditText)oMatrix.GetCellSpecific("6", j)).Value);

                    #region Unas validaciones,...
                    if (cTipoDoc != "18" && cTipoDoc != "19")
                    {
                        Matriz.gen.SBOApp.MessageBox("Tipo de documento no permitido en Confirming. Linea " + j.ToString(), 1, "Ok", "", "");
                        cFicheroConfIbercaja.Close();
                        oMatrix = null;
                        Environment.CurrentDirectory = DirectorioActual;
                        #region Borro el fichero
                        try
                        {
                            File.Delete(Fichero);
                        }
                        catch (Exception ex)
                        {
                            Matriz.gen.SBOApp.MessageBox(ex.Message, 1, "Ok", "", "");
                        }
                        #endregion
                        return;
                    }

                    if (cTipoDoc == "18" && nImporte < 0)
                    {

                        Matriz.gen.SBOApp.MessageBox("No puede incluir en un confirming Facturas negativas. Linea " + j.ToString(), 1, "Ok", "", "");
                        cFicheroConfIbercaja.Close();
                        oMatrix = null;
                        Environment.CurrentDirectory = DirectorioActual;
                        #region Borro el fichero
                        try
                        {
                            File.Delete(Fichero);
                        }
                        catch (Exception ex)
                        {
                            Matriz.gen.SBOApp.MessageBox(ex.Message, 1, "Ok", "", "");
                        }
                        #endregion
                        return;
                    }


                    //HA DE ESTAR FIRMADA LA FACTURA
                    if (cTipoDoc == "18")
                    {
                        #region Valido consistencia fact                        
                        SAPbobsCOM.Recordset oRecValFac = Matriz.gen.refDi.SQL.sqlComoRsB1(sqlValidoDoc(cNumDoc, cTipoDoc));
                        if ((oRecValFac.Fields.Item("NomFac").Value.ToString().Trim() != oRecValFac.Fields.Item("NomIC").Value.ToString().Trim()) ||
                             (oRecValFac.Fields.Item("NIFFac").Value.ToString().Trim() != oRecValFac.Fields.Item("NIFIC").Value.ToString().Trim()))
                        {
                            Matriz.gen.SBOApp.MessageBox("Razon social / NIF  inconsistente. Linea " + j.ToString() + " (" + oRecValFac.Fields.Item("NomFac").Value.ToString().Trim()  + ")", 1, "Ok", "", "");
                            cFicheroConfIbercaja.Close();
                            oMatrix = null;
                            Environment.CurrentDirectory = DirectorioActual;
                            #region Borro el fichero
                            try
                            {
                                File.Delete(Fichero);
                            }
                            catch (Exception ex)
                            {
                                Matriz.gen.SBOApp.MessageBox(ex.Message, 1, "Ok", "", "");
                            }
                            #endregion
                            return;
                        }
                        #endregion

                        int nClaveFac = (int)oRecValFac.Fields.Item("DocEntry").Value;
                        if (nClaveFac == 0)
                        {
                            Matriz.gen.SBOApp.MessageBox("Fallo en clave Docentry. Linea " + j.ToString(), 1, "Ok", "", "");
                            cFicheroConfIbercaja.Close();
                            Environment.CurrentDirectory = DirectorioActual;
                            #region Borro el fichero
                            try
                            {
                                File.Delete(Fichero);
                            }
                            catch (Exception ex)
                            {
                                Matriz.gen.SBOApp.MessageBox(ex.Message, 1, "Ok", "", "");
                            }
                            #endregion
                            return;
                        }

                    }

                    if (cTipoDoc == "19")
                    {

                        if (nImporte > 0)
                        {
                            Matriz.gen.SBOApp.MessageBox("No puede incluir en un confirming Abonos negativos. Linea " + j.ToString(), 1, "Ok", "", "");
                            cFicheroConfIbercaja.Close();
                            Environment.CurrentDirectory = DirectorioActual;
                            oMatrix = null;
                            #region Borro el fichero
                            try
                            {
                                File.Delete(Fichero);
                            }
                            catch (Exception ex)
                            {
                                Matriz.gen.SBOApp.MessageBox(ex.Message, 1, "Ok", "", "");
                            }
                            #endregion
                            return;
                        }

                        #region Valido consistencia abono
                        SAPbobsCOM.Recordset oRecValFac = Matriz.gen.refDi.SQL.sqlComoRsB1(sqlValidoDoc(cNumDoc, cTipoDoc));
                        if ((oRecValFac.Fields.Item("NomFac").Value.ToString().Trim() != oRecValFac.Fields.Item("NomIC").Value.ToString().Trim()) ||
                             (oRecValFac.Fields.Item("NIFFac").Value.ToString().Trim() != oRecValFac.Fields.Item("NIFIC").Value.ToString().Trim()))
                        {
                            Matriz.gen.SBOApp.MessageBox("Razon social / NIF  inconsistente. Linea " + j.ToString(), 1, "Ok", "", "");
                            cFicheroConfIbercaja.Close();
                            oMatrix = null;
                            Environment.CurrentDirectory = DirectorioActual;
                            #region Borro el fichero
                            try
                            {
                                File.Delete(Fichero);
                            }
                            catch (Exception ex)
                            {
                                Matriz.gen.SBOApp.MessageBox(ex.Message, 1, "Ok", "", "");
                            }
                            #endregion
                            return;
                        }
                        #endregion

                    }
                    #endregion

                    //Datos del Provee y demas                    
                    oRec = Matriz.gen.refDi.SQL.sqlComoRsB1(sqlDatosProveedor_Y_Documento(cNumDoc, cTipoDoc));

                    cDatos = Detalle01confIbercaja(cCodOrdenante, oRec, nImporte, dFechaVto);
                    cFicheroConfIbercaja.WriteLine(cDatos);
                    nRegTipo10++;
                    nRegTotales++;
                    nImpTotal += nImporte;

                    cDatos = Detalle02confIbercaja(cCodOrdenante, oRec);
                    cFicheroConfIbercaja.WriteLine(cDatos);
                    nRegTotales++;

                    cDatos = Detalle03confIbercaja(cCodOrdenante, oRec);
                    cFicheroConfIbercaja.WriteLine(cDatos);
                    nRegTotales++;

                    cDatos = Detalle04confIbercaja(cCodOrdenante, oRec);
                    cFicheroConfIbercaja.WriteLine(cDatos);
                    nRegTotales++;

                    cDatos = Detalle05confIbercaja(cCodOrdenante, oRec);
                    cFicheroConfIbercaja.WriteLine(cDatos);
                    nRegTotales++;

                    cDatos = Detalle06confIbercaja(cCodOrdenante, oRec);
                    cFicheroConfIbercaja.WriteLine(cDatos);
                    nRegTotales++;
                }
            }

            nRegTotales++;
            cDatos = Totales01ConfIbercaja(cCodOrdenante, nImpTotal, nRegTipo10, nRegTotales);
            cFicheroConfIbercaja.WriteLine(cDatos);
            cFicheroConfIbercaja.Close();
            Environment.CurrentDirectory = DirectorioActual;

            oMatrix = null;
            Matriz.gen.SBOApp.MessageBox("Fichero Generado", 1, "Ok", "", "");
        }

        static string Cabec01ConfIbercaja(string cNifEmpresaSINPais, string cCodOrdenante, string cSucursal, string cCuenta, string cDigitoControl)
        {
            string cDatos = "", cAux = "";

            //cod registro
            cDatos = "04";

            //cod operacion
            cDatos += "56";

            //cod ordenante
            cDatos += cCodOrdenante.Trim().PadRight(10).Substring(0, 10);

            //NIF
            cAux = cNifEmpresaSINPais.PadRight(12).Substring(0, 12);
            cDatos += cAux;

            //nº dato (fijo)
            cDatos += "001";

            //fecha creacion fichero            
            cDatos += DateTime.Today.ToString("ddMMyy");

            //fecha emision (fijo 000000)
            cDatos += "000000";

            //banco destino - fijo, ibercaja
            cDatos += "2085";

            //oficina - 
            cDatos += cSucursal;

            //num cuenta
            cDatos += cCuenta;

            //detalle (fijo 0)
            cDatos += "0";

            //libre
            cDatos += "".PadRight(3);

            //DIGITO DE CONTROL
            cDatos += cDigitoControl;

            //libre            
            cDatos += "".PadRight(7);

            return cDatos;
        }

        static string Cabec02ConfIbercaja(string cNifEmpresaSINPais, string cCodOrdenante, string cNombreEmpresa)
        {
            string cDatos = "", cAux = "";

            //cod registro
            cDatos = "04";

            //cod operacion
            cDatos += "56";

            //cod ordenante
            cDatos += cCodOrdenante.Trim().PadRight(10).Substring(0, 10);

            //NIF
            cAux = cNifEmpresaSINPais.PadRight(12).Substring(0, 12);
            cDatos += cAux;

            //nº dato (fijo)
            cDatos += "002";

            //Nombre empresa
            cDatos += cNombreEmpresa.Trim().PadRight(36).Substring(0, 36);

            //libre
            cDatos += "".PadRight(7);


            return cDatos;
        }


        static string Detalle01confIbercaja(string cCodOrdenante, SAPbobsCOM.Recordset oRec, double nImpFactura, DateTime dFechaVto)
        {
            string cDatos = "", cAux = "";

            //Cod registro
            cDatos = "06";

            //Cod operacion
            cDatos += "60";

            //ordenante 
            cDatos += cCodOrdenante.Trim().PadRight(10).Substring(0, 10);

            //Beneficiario - NIF proveedor
            cAux = oRec.Fields.Item("NIF").Value.ToString().Trim();
            if (cAux.Length > 2) cAux = cAux.Substring(2).Trim();
            cDatos += cAux.PadRight(12).ToUpper();

            //numero de dato
            cDatos += "010";

            //importe (en centimos, 12 caracteres)            
            decimal nAux = Math.Abs((decimal)nImpFactura);
            nAux = Math.Round(nAux, Matriz.SumDec, MidpointRounding.AwayFromZero);
            int nAux1 = (int)(nAux * 100);
            string cSigno = nImpFactura < 0 ? "-" : "0";

            cDatos += (cSigno + nAux1.ToString("00000000000"));

            //banco beneficiaria
            cDatos += ((string)oRec.Fields.Item("Banco").Value).Trim().PadRight(4).Substring(0, 4);

            //sucursal beneficiaria            
            cDatos += ((string)oRec.Fields.Item("Sucursal").Value).Trim().PadRight(4).Substring(0, 4);

            //cuenta beneficiaria            
            cDatos += ((string)oRec.Fields.Item("Cuenta").Value).Trim().PadRight(10).Substring(0, 10);

            //gastos - fijo 1
            cDatos += "1";

            //concepto - fijo 9
            cDatos += "9";

            //libre
            cDatos += "".PadRight(2);

            //control beneficiaria            
            cDatos += ((string)oRec.Fields.Item("IDControl").Value).Trim().PadRight(2).Substring(0, 2);

            //vto
            cDatos += "0" + dFechaVto.ToString("ddMMyy");

            return cDatos;
        }

        static string Detalle02confIbercaja(string cCodOrdenante, SAPbobsCOM.Recordset oRec)
        {
            string cDatos = "", cAux = "";

            //Cod registro
            cDatos = "06";

            //Cod operacion
            cDatos += "60";

            //ordenante 
            cDatos += cCodOrdenante.Trim().PadRight(10).Substring(0, 10);

            //Beneficiario - NIF proveedor
            cAux = oRec.Fields.Item("NIF").Value.ToString().Trim();
            if (cAux.Length > 2) cAux = cAux.Substring(2).Trim();
            cDatos += cAux.PadRight(12).ToUpper();

            //numero de dato
            cDatos += "011";

            //nombreproveedor 
            cDatos += ((string)oRec.Fields.Item("NomProvee").Value).Trim().PadRight(36).Substring(0, 36);

            //libre 7
            cDatos += "".PadRight(7);


            return cDatos;
        }

        static string Detalle03confIbercaja(string cCodOrdenante, SAPbobsCOM.Recordset oRec)
        {
            string cDatos = "", cAux = "";

            //Cod registro
            cDatos = "06";

            //Cod operacion
            cDatos += "60";

            //ordenante 
            cDatos += cCodOrdenante.Trim().PadRight(10).Substring(0, 10);

            //Beneficiario - NIF proveedor
            cAux = oRec.Fields.Item("NIF").Value.ToString().Trim();
            if (cAux.Length > 2) cAux = cAux.Substring(2).Trim();
            cDatos += cAux.PadRight(12).ToUpper();

            //numero de dato
            cDatos += "012";

            //calle            
            cDatos += ((string)oRec.Fields.Item("Calle").Value).Trim().PadRight(36).Substring(0, 36);

            //libre 7
            cDatos += "".PadRight(7);

            return cDatos;
        }

        static string Detalle04confIbercaja(string cCodOrdenante, SAPbobsCOM.Recordset oRec)
        {
            string cDatos = "", cAux = "";

            //Cod registro
            cDatos = "06";

            //Cod operacion
            cDatos += "60";

            //ordenante 
            cDatos += cCodOrdenante.Trim().PadRight(10).Substring(0, 10);

            //Beneficiario - NIF proveedor
            cAux = oRec.Fields.Item("NIF").Value.ToString().Trim();
            if (cAux.Length > 2) cAux = cAux.Substring(2).Trim();
            cDatos += cAux.PadRight(12).ToUpper();

            //numero de dato
            cDatos += "014";

            //cp ciudad              
            cAux = oRec.Fields.Item("CP").Value + " " + oRec.Fields.Item("Ciudad").Value;
            cDatos += cAux.Trim().PadRight(36).Substring(0, 36);

            //libre 7
            cDatos += "".PadRight(7);

            return cDatos;
        }

        static string Detalle05confIbercaja(string cCodOrdenante, SAPbobsCOM.Recordset oRec)
        {
            string cDatos = "", cAux = "";

            //Cod registro
            cDatos = "06";

            //Cod operacion
            cDatos += "60";

            //ordenante 
            cDatos += cCodOrdenante.Trim().PadRight(10).Substring(0, 10);

            //Beneficiario - NIF proveedor
            cAux = oRec.Fields.Item("NIF").Value.ToString().Trim();
            if (cAux.Length > 2) cAux = cAux.Substring(2).Trim();
            cDatos += cAux.PadRight(12).ToUpper();

            //numero de dato
            cDatos += "015";

            //provincia            
            cDatos += ((string)oRec.Fields.Item("Provincia").Value).Trim().PadRight(36).Substring(0, 36);

            //libre 7
            cDatos += "".PadRight(7);

            return cDatos;
        }

        static string Detalle06confIbercaja(string cCodOrdenante, SAPbobsCOM.Recordset oRec)
        {
            string cDatos = "", cAux = "";

            //Cod registro
            cDatos = "06";

            //Cod operacion
            cDatos += "60";

            //ordenante 
            cDatos += cCodOrdenante.Trim().PadRight(10).Substring(0, 10);

            //Beneficiario - NIF proveedor
            cAux = oRec.Fields.Item("NIF").Value.ToString().Trim();
            if (cAux.Length > 2) cAux = cAux.Substring(2).Trim();
            cDatos += cAux.PadRight(12).ToUpper();

            //numero de dato
            cDatos += "016";

            //num fact prov    
            cDatos += ((string)oRec.Fields.Item("RefDoc").Value).Trim().PadRight(36).Substring(0, 36);

            //fecha fac
            cDatos += ("0" + oRec.Fields.Item("FacFecha").Value.ToString("ddMMyy"));


            return cDatos;
        }

        static string Totales01ConfIbercaja(string cCodOrdenante, double nTotalRemesa, int nRegistros010, int nTotRegConCabecera)
        {
            string cDatos = "", cAux = "";

            //Cod registro
            cDatos = "08";

            //Cod operacion
            cDatos += "56";

            //ordenante
            cAux = cCodOrdenante.PadLeft(10).Substring(0, 10);
            cDatos += cAux;

            //libre
            cDatos += "".PadRight(12);

            //libre
            cDatos += "".PadRight(3);


            //suma importes            
            decimal nAux = (decimal)nTotalRemesa;
            nAux = Math.Round(nAux, Matriz.SumDec);
            int nAux1 = (int)(nAux * 100);
            cDatos += nAux1.ToString("000000000000");

            //registros de tipo 10
            cDatos += nRegistros010.ToString("00000000");

            //total registros
            cDatos += nTotRegConCabecera.ToString("0000000000");

            //libre
            cDatos += "".PadRight(6);

            //libre
            cDatos += "".PadRight(7);

            return cDatos;
        }
        #endregion
        

        public static string sqlDatosBancoPropio(string cEntidad, string cCuenta)
        {
            string csql = "";
            if (Matriz.lgHANA)
            {
                csql = "SELECT T0.\"Branch\" AS \"Sucursal\", T0.\"UsrNumber4\" as \"Usr4\", T0.\"UsrNumber1\" as \"Usr1\", T0.\"UsrNumber3\" as \"Usr3\", T0.\"ControlKey\" as \"DigControl\", T0.\"IBAN\" AS \"IBAN\", ";
                csql += " T0.\"U_EXO_SufijoN68\" AS \"SufijoN68\", T1.\"U_EXO_CodePageCF\" as \"PaginaCodigosCF\", T1.\"U_EXO_CodePageN68\" as \"PaginaCodigosN68\" ";
                csql += " FROM \"DSC1\" T0 ";
                csql += " INNER JOIN \"ODSC\" T1 ON T0.\"BankCode\" = T1.\"BankCode\" and T0.\"Country\" = T1.\"CountryCod\" ";
                csql += "WHERE T0.\"BankCode\" = '##ENTIDAD' AND T0.\"Account\" = '##CUENTA'";
            }
            else
            {
                csql = "SELECT T0.Branch AS 'Sucursal', T0.UsrNumber4 as 'Usr4', T0.UsrNumber1 as 'Usr1', T0.UsrNumber3 as 'Usr3', ";
                csql += " T0.ControlKey as 'DigControl', T0.IBAN AS 'IBAN', T0.U_EXO_SufijoN68 AS 'SufijoN68', ";
                csql += " T1.U_EXO_CodePageCF as PaginaCodigosCF, T1.U_EXO_CodePageN68 as PaginaCodigosN68 ";
                csql += " FROM DSC1 T0 ";
                csql += " INNER JOIN ODSC T1 ON T0.BankCode = T1.BankCode and T0.Country = T1.CountryCod ";
                csql += " WHERE T0.BankCode = '##ENTIDAD' AND T0.Account = '##CUENTA'";
            }

            csql = csql.Replace("##ENTIDAD", cEntidad).Replace("##CUENTA", cCuenta);
            return csql;

        }

        public static string sqlValidoDoc(string cNumDoc, string cTipoDoc)
        {
            string csql = "";
            if (Matriz.lgHANA)
            {
                csql = "SELECT T0.\"DocEntry\" AS \"DocEntry\", T0.\"CardName\" as \"NomFac\", T0.\"LicTradNum\" as \"NIFFac\", ";
                csql += " T1.\"CardName\" as \"NomIC\", T1.\"LicTradNum\" AS \"NIFIC\" FROM \"##TABLA\" T0 INNER JOIN \"OCRD\" T1 ON T0.\"CardCode\" = T1.\"CardCode\" WHERE T0.\"DocNum\" = ##DOCNUM";
            }
            else
            {
                csql = "SELECT T0.DocEntry AS 'DocEntry', T0.CardName as 'NomFac', T0.LicTradNum as 'NIFFac', T1.CardName as 'NomIC', T1.LicTradNum AS 'NIFIC' FROM \"##TABLA\" T0 INNER JOIN OCRD T1 ON T0.CardCode = T1.CardCode WHERE T0.DocNum = ##DOCNUM";
            }

            csql = csql.Replace("##DOCNUM", cNumDoc).Replace("##TABLA", cTipoDoc == "18" ? "OPCH" : "ORPC");
            return csql;

        }

        public static string sqlDatosProveedor_Y_Documento(string cNumDoc, string cTipoDoc)
        {
            string sql = "";

            if (Matriz.lgHANA)
            {
                sql = "SELECT T0.\"CardCode\" as \"Proveedor\",  T0.\"CardName\" as \"NomProvee\", T0.\"LicTradNum\" as \"NIF\", T0.\"CmpPrivate\" AS \"TipoPersona\", T0.\"Address\" AS \"Calle\", ";
                sql += " T0.\"BillToDef\" AS \"BillToDef\", T0.\"City\" AS \"Ciudad\", T0.\"ZipCode\" AS \"CP\", IFNULL(T1.\"Name\", '') AS \"Provincia\", ";
                sql += " T0.\"Phone1\" AS \"Telefono\", T0.\"Fax\" AS \"FAX\", T0.\"E_Mail\" AS \"email\", ";
                sql += " T0.\"BankCode\" as \"Banco\", T0.\"DflBranch\" as \"Sucursal\", T0.\"BankCtlKey\" as \"IDControl\", T0.\"DflAccount\" as \"Cuenta\",  T0.\"DflIBAN\" as \"IBAN\", T0.\"DflSwift\" as \"Swift\", ";
                sql += " TDoc.\"TaxDate\" AS \"FacFecha\", T2.\"Code\" AS \"CodPais\", ";
                sql += " T2.\"Name\" as \"NomPais\", T0.\"StreetNo\" as \"NumCalleFact\", TDoc.\"NumAtCard\" AS \"RefDoc\", TDoc.\"DocNum\" as \"NumFact\", ";
                sql += " (CASE TDoc.\"ObjType\" WHEN '18' THEN 'F' WHEN '19' THEN 'A' WHEN '204' THEN 'J' END) AS \"TipoDoc\", ";
                sql += " (CASE T0.\"LangCode\" WHEN 23 THEN 'E' ELSE 'I' END) AS \"Idioma\", ";
                sql += " ifnull((SELECT TBanco.\"BankName\" FROM \"ODSC\" TBanco WHERE TBanco.\"AbsEntry\" = T0.\"DflBankKey\"), '') AS \"NombreBancoProveedor\", ";
                sql += " ifnull((SELECT TBanco.\"CountryCod\" FROM \"ODSC\" TBanco WHERE TBanco.\"AbsEntry\" = T0.\"DflBankKey\"), '') AS \"PaisBanco\" ";
                sql += " FROM \"##TABLA\" TDoc INNER JOIN \"OCRD\" T0 ON TDoc.\"CardCode\" = T0.\"CardCode\" ";
                sql += " LEFT OUTER JOIN \"OCST\" T1 ON T0.\"Country\" = T1.\"Country\" AND T0.\"State1\" = T1.\"Code\" ";
                sql += " LEFT OUTER JOIN \"OCRY\" T2 ON T0.\"Country\" = T2.\"Code\" ";
                sql += " WHERE TDoc.\"DocNum\" = ##DOCNUM";
            }
            else
            {
                sql = "SELECT T0.CardCode as 'Proveedor',  T0.CardName as'NomProvee', T0.LicTradNum as 'NIF', CmpPrivate AS 'TipoPersona', T0.Address AS 'Calle', ";
                sql += " T0.BillToDef AS 'BillToDef', T0.City AS 'Ciudad', T0.ZipCode AS 'CP', ISNULL(T1.Name, '') AS 'Provincia', ";
                sql += " T0.Phone1 AS 'Telefono', T0.Fax AS 'FAX', T0.E_Mail AS 'email', ";
                sql += " T0.BankCode as 'Banco', T0.DflBranch as 'Sucursal', T0.BankCtlKey as 'IDControl', T0.DflAccount as 'Cuenta',  T0.DflIBAN as 'IBAN', T0.DflSwift as 'Swift', ";
                sql += " TDoc.TaxDate AS FacFecha, T2.Code AS CodPais, ";
                sql += " T2.Name as 'NomPais', T0.StreetNo as 'NumCalleFact', TDoc.NumAtCard AS 'RefDoc', TDoc.DocNum as NumFact, ";
                sql += " (CASE TDoc.ObjType WHEN '18' THEN 'F' WHEN '19' THEN 'A' WHEN '204' THEN 'J' END) AS 'TipoDoc', ";
                sql += " (CASE T0.LangCode WHEN 23 THEN 'E' ELSE 'I' END) AS 'Idioma', ";
                sql += " isnull((SELECT TBanco.BankName FROM ODSC TBanco WHERE TBanco.AbsEntry = T0.DflBankKey), '') AS 'NombreBancoProveedor', ";
                sql += " isnull((SELECT TBanco.CountryCod FROM ODSC TBanco WHERE TBanco.AbsEntry = T0.DflBankKey), '') AS 'PaisBanco' ";
                sql += " FROM ##TABLA TDoc INNER JOIN OCRD T0 ON TDoc.CardCode = T0.CardCode ";
                sql += " LEFT OUTER JOIN OCST T1 ON T0.Country = T1.Country AND T0.State1 = T1.Code ";
                sql += " LEFT OUTER JOIN OCRY T2 ON T0.Country = T2.Code ";
                sql += " WHERE TDoc.DocNum = ##DOCNUM";
            }

            string cTabla = "";
            switch (cTipoDoc)
            {
                case "18":
                    cTabla = "OPCH";
                    break;
                case "19":
                    cTabla = "ORPC";
                    break;
                case "204":
                    cTabla = "ODPO";
                    break;

            }
            sql = sql.Replace("##DOCNUM", cNumDoc).Replace("##TABLA", cTabla);
            return sql;
        }

        public static string sqlDatProveedor(string cProveedor)
        {
            string sql = "";

            if (Matriz.lgHANA)
            {
                sql = "SELECT T0.\"CardCode\" as \"Proveedor\",  T0.\"CardName\" as \"NomProvee\", T0.\"LicTradNum\" as \"NIF\", ";
                sql += " T0.\"Address\" AS \"Calle\", T0.\"City\" AS \"Ciudad\", T0.\"ZipCode\" AS \"CP\", T0.\"Country\" AS \"Pais\", ";
                sql += " T0.\"Phone1\" AS \"Telefono\", T0.\"Fax\" AS \"FAX\", T0.\"E_Mail\" AS \"email\", ifnull(T1.\"Name\", '') as \"Provincia\", ";
                sql += " T0.\"BankCode\" as \"Banco\", T0.\"DflBranch\" as \"Sucursal\", T0.\"BankCtlKey\" as \"IDControl\", T0.\"DflAccount\" as \"Cuenta\",  T0.\"DflIBAN\" as \"IBAN\", T0.\"DflSwift\" as \"Swift\" ";
                sql += " FROM \"OCRD\" T0 ";
                sql += " LEFT OUTER JOIN \"OCST\" T1 ON T0.\"Country\" = T1.\"Country\" AND T0.\"State1\" = T1.\"Code\" ";
                sql += " WHERE T0.\"CardCode\" = '##PROVEEDOR'";
            }
            else
            {
                sql = "SELECT T0.CardCode as 'Proveedor',  T0.CardName as'NomProvee', T0.LicTradNum as 'NIF', ";
                sql += " T0.Address AS 'Calle', T0.City AS 'Ciudad', T0.ZipCode AS 'CP', T0.Country AS 'Pais', ";
                sql += " T0.Phone1 AS 'Telefono', T0.Fax AS 'FAX', T0.E_Mail AS 'email', isnull(T1.Name, '') as 'Provincia', ";
                sql += " T0.BankCode as 'Banco', T0.DflBranch as 'Sucursal', T0.BankCtlKey as 'IDControl', T0.DflAccount as 'Cuenta',  T0.DflIBAN as 'IBAN', T0.DflSwift as 'Swift' ";
                sql += " FROM OCRD T0 ";
                sql += " LEFT OUTER JOIN OCST T1 ON T0.Country = T1.Country AND T0.State1 = T1.Code ";
                sql += " WHERE T0.CardCode = '##PROVEEDOR'";
            }

            sql = sql.Replace("##PROVEEDOR", cProveedor);
            return sql;
        }
        

        public static string sqlDatosEfecto(string cEfecto)
        {
            string sql = "";
            if (Matriz.lgHANA)
            {
                sql = " SELECT T3.\"LicTradNum\" AS \"NIF\", T0.\"CardName\" AS \"NombreBeneficiario\", ";
                sql += " T3.\"Address\" AS \"Direccion\", T3.\"City\" AS \"Ciudad\", T3.\"ZipCode\" AS \"CP\", ";
                sql += " T3.\"Country\" AS \"Pais\", IFNULL((SELECT TPais.\"Name\" FROM \"OCRY\" TPais WHERE TPais.\"Code\" = T3.\"Country\" ), '') AS \"NomPais\", ";
                sql += " IFNULL((SELECT TPro.\"Name\" FROM \"OCST\" TPro WHERE TPro.\"Country\" = T3.\"Country\" AND TPro.\"Code\" = T3.\"State1\"), '') AS \"Provincia\", ";
                sql += " TO_VARCHAR(T0.\"DueDate\", 'YYYYMMDD') AS \"FechaVto\", ";
                sql += "  T0.\"BoeSum\" AS \"ImporteEfecto\", ";
                sql += " T2.\"InvType\", T1.\"PayNoDoc\" AS \"Acuenta\", ";
                sql += " (CASE T2.\"InvType\" ";
                sql += " WHEN '18' THEN(SELECT TFac.\"NumAtCard\" from \"OPCH\" TFac WHERE TFac.\"DocEntry\" = T2.\"DocEntry\") ";
                sql += " WHEN '19' THEN(SELECT TAbo.\"NumAtCard\" from \"ORPC\" TAbo WHERE TAbo.\"DocEntry\" = T2.\"DocEntry\") ";
                sql += " END) AS \"NumFactProv\", ";
                sql += " (CASE T2.\"InvType\" ";
                sql += " WHEN '18' THEN(SELECT TO_VARCHAR(TFac.\"TaxDate\", 'YYYYMMDD') from \"OPCH\" TFac WHERE TFac.\"DocEntry\" = T2.\"DocEntry\") ";
                sql += " WHEN '19' THEN(SELECT TO_VARCHAR(TAbo.\"TaxDate\", 'YYYYMMDD') from \"ORPC\" TAbo WHERE TAbo.\"DocEntry\" = T2.\"DocEntry\") ";
                sql += " END) AS \"FechaFactura\", ";
                sql += " T2.\"AppliedSys\" AS \"Pagado\", ";
                //sql += " (CASE T2.\"InvType\" WHEN '18' THEN  'H' WHEN '19' THEN  'D' ELSE 'X' END) AS \"DoH\", ";
                sql += " T3.\"BankCode\" as \"Banco\", T3.\"DflBranch\" as \"Sucursal\", T3.\"BankCtlKey\" as \"IDControl\", T3.\"DflAccount\" as \"Cuenta\",  T3.\"DflIBAN\" as \"IBAN\", T3.\"DflSwift\" as \"Swift\", ";
                sql += " T3.\"Phone1\" as \"Telefono\", T3.\"Fax\" ";
                sql += " FROM \"OBOE\" T0 ";
                sql += " INNER JOIN \"OVPM\" T1 ON T1.\"BoeAbs\" = T0.\"BoeKey\" ";
                sql += " INNER JOIN \"VPM2\" T2 ON T1.\"DocEntry\" = T2.\"DocNum\" ";
                sql += " INNER JOIN \"OCRD\" T3 ON T3.\"CardCode\" = T0.\"CardCode\" ";
                sql += " WHERE T0.\"BoeType\" = 'O' AND T0.\"BoeNum\" = " + cEfecto;
            }
            else
            {
                sql = " SELECT T3.LicTradNum AS 'NIF', T0.CardName AS 'NombreBeneficiario', ";
                sql += " T3.Address AS 'Direccion', T3.City AS 'Ciudad', T3.ZipCode AS 'CP', ";
                sql += " T3.Country AS 'Pais', ISNULL((SELECT TPais.Name FROM OCRY TPais WHERE TPais.Code = T3.Country ), '') AS 'NomPais', ";
                sql += " ISNULL((SELECT TPro.Name FROM OCST TPro WHERE TPro.Country = T3.Country AND TPro.Code = T3.State1), '') AS 'Provincia', ";
                sql += " CONVERT(NVARCHAR(10), T0.DueDate, 112) AS 'FechaVto', ";
                sql += " T0.BoeSum as 'ImporteEfecto', ";
                sql += " T2.InvType, T1.PayNoDoc AS 'Acuenta', ";
                sql += " (CASE T2.InvType ";
                sql += " WHEN '18' THEN(SELECT TFac.NumAtCard from OPCH TFac WHERE TFac.DocEntry = T2.DocEntry) ";
                sql += " WHEN '19' THEN(SELECT TAbo.NumAtCard from ORPC TAbo WHERE TAbo.DocEntry = T2.DocEntry) ";
                sql += " END) AS 'NumFactura', ";
                sql += " (CASE T2.InvType ";
                sql += " WHEN '18' THEN(SELECT CONVERT(NVARCHAR(10), TFac.TaxDate, 112) from OPCH TFac WHERE TFac.DocEntry = T2.DocEntry) ";
                sql += " WHEN '19' THEN(SELECT CONVERT(NVARCHAR(10), TAbo.TaxDate, 112) from ORPC TAbo WHERE TAbo.DocEntry = T2.DocEntry) ";
                sql += " END) AS 'FechaFactura', ";
                sql += " T2.AppliedSys AS 'Pagado', ";
                //sql += " (CASE T2.InvType WHEN '18' THEN  'H' WHEN '19' THEN  'D' ELSE 'X' END) AS 'DoH', ";
                sql += " T3.BankCode as Banco, T3.DflBranch as Sucursal, T3.BankCtlKey as IDControl, T3.DflAccount as Cuenta,  T3.DflIBAN as IBAN, T3.DflSwift as Swift, ";
                sql += " T3.Phone1 as Telefono, T3.Fax ";
                sql += " FROM OBOE T0 ";
                sql += " INNER JOIN OVPM T1 ON T1.BoeAbs = T0.BoeKey ";
                sql += " INNER JOIN VPM2 T2 ON T1.DocEntry = T2.DocNum ";
                sql += " INNER JOIN OCRD T3 ON T3.CardCode = T0.CardCode ";
                sql += " WHERE T0.BoeType = 'O' AND T0.BoeNum = " + cEfecto;
            }
            return sql;
        }
    }

}



