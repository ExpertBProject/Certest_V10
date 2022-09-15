using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;

namespace Cliente
{

    public class Matriz : EXO_UIAPI.EXO_DLLBase
    {
        public static EXO_UIAPI.EXO_UIAPI gen;
        public static EXO_DIAPI.EXO_DIAPI conexionSAP;

        public static Type TypeMatriz;
        public static int SumDec;
        public static int PriceDec;
        public static int RateDec;
        public static int QtyDec;
        public static int PercentDec;
        public static int MeasureDec;
        public static string SepMill;
        public static string SepDec;
        public static string cgTipoFacCancelaciónVENTAS;
        public static List<TiposForm> gListaTiposForm;

        public static bool lgHANA;

        public Matriz(EXO_UIAPI.EXO_UIAPI gen, Boolean act, Boolean usalicencia, int idAddon)
                     : base(gen, act, usalicencia, idAddon)
        {
            Matriz.gen = this.objGlobal;
            TypeMatriz = this.GetType();
            lgHANA = (Matriz.gen.compañia.DbServerType == BoDataServerTypes.dst_HANADB);
            if (act)
            {
                #region Creo campo en OITM
                string cMen = "";


                string fXML = Utilidades.LeoQueryFich("xBDArticulos.xml");

                if (objGlobal.refDi.comunes.esAdministrador())
                {
                    if (!objGlobal.refDi.comunes.LoadBDFromXML(fXML, cMen))
                    {
                        this.aplicacionB1.MessageBox(cMen, 1, "Ok", "", "");
                        this.aplicacionB1.MessageBox("Error en creacion de campos xBDArticulos", 1, "Ok", "", "");
                    }
                    else
                    {
                        this.aplicacionB1.MessageBox("Actualizacion de campos realizada", 1, "Ok", "", "");
                        //Casca
                        //this.SboApp.ActivateMenuItem("3329");
                    }
                }
                else
                {
                    this.aplicacionB1.MessageBox("Necesita permisos de administrador para actualizar la base de datos.\nCampos no creados", 1, "Ok", "", "");
                }

                #endregion
            }

            #region Decimales de la aplicacion y provisionar o no art no inve            
            SAPbobsCOM.Recordset oRec = Matriz.gen.refDi.SQL.sqlComoRsB1(sqlInicio1());
            Matriz.SumDec = oRec.Fields.Item("SumDec").Value;
            Matriz.PriceDec = oRec.Fields.Item("PriceDec").Value;
            Matriz.RateDec = oRec.Fields.Item("RateDec").Value;
            Matriz.QtyDec = oRec.Fields.Item("QtyDec").Value;
            Matriz.PercentDec = oRec.Fields.Item("PercentDec").Value;
            Matriz.MeasureDec = oRec.Fields.Item("MeasureDec").Value;
            Matriz.SepMill = oRec.Fields.Item("ThousSep").Value;
            Matriz.SepDec = oRec.Fields.Item("DecSep").Value;
            Matriz.cgTipoFacCancelaciónVENTAS = oRec.Fields.Item("U_EXO_TipoFacCancel").Value;

            System.Runtime.InteropServices.Marshal.ReleaseComObject(oRec);
            oRec = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            #endregion

            gListaTiposForm = LlenoListaDocs();


        }

        public override SAPbouiCOM.EventFilters filtros()
        {
            SAPbouiCOM.EventFilters oFilter = new SAPbouiCOM.EventFilters();

            #region Mando filtros
            try
            {
                Type Tipo = this.GetType();
                string fXML = Utilidades.LeoFichEmbebido("xFiltros.xml");
                oFilter.LoadFromXML(fXML);
            }
            catch (Exception ex)
            {
                //this.SboApp.MessageBox("Error en carga de filtros Frenos\n" + ex.Message, 1, "Ok", "", "");
                oFilter = null;
            }
            #endregion
            return oFilter;
        }

        public override System.Xml.XmlDocument menus()
        {
            //Type Tipo = this.GetType();
            //string mXML = Utilidades.LeoQueryFich("xMenuTimacFlotasyTelf.xml");
            //System.Xml.XmlDocument menu = new System.Xml.XmlDocument();
            //menu.LoadXml(mXML);
            //return menu;

            return null;
        }


        public override bool SBOApp_ItemEvent(ItemEvent infoEvento)
        {
            bool lRetorno = true;

            #region comentado
            //if (infoEvento.FormTypeEx == "40005" && !infoEvento.BeforeAction)
            //{
            //    SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.ActiveForm;

            //    SAPbouiCOM.Matrix oMatrix = (SAPbouiCOM.Matrix)oForm.Items.Item("10").Specific;

            //    oMatrix.CommonSetting.SetCellBackColor(23, 72, System.Drawing.Color.Red.R);
            //    oMatrix.CommonSetting.SetCellBackColor(22, 72, System.Drawing.Color.Red.R);

            //    oMatrix.CommonSetting.SetCellFontSize(22, 72, 14);
            //    oMatrix.CommonSetting.SetCellFontColor(22, 72, System.Drawing.Color.Red.R);

            //    //   string xx = ((SAPbouiCOM.EditText)oMatrix.GetCellSpecific("72", 22)).Value;
            //    // ((SAPbouiCOM.EditText)oMatrix.GetCellSpecific("72", 22)).Value;

            //}
            #endregion


            //Doc ventas
            if (infoEvento.FormTypeEx == "133" || infoEvento.FormTypeEx == "179" ||  //Facturas, Abono
                infoEvento.FormTypeEx == "65300" ||  //Anticipos
                infoEvento.FormTypeEx == "60091" || infoEvento.FormTypeEx == "90090")  //Factura reserva 60091, factura cliente  + pago
            {
                EXO_DocSII fDocsSII = new EXO_DocSII();
                lRetorno = fDocsSII.ItemEvent(infoEvento);
            }

            //Doc compras
            if (infoEvento.FormTypeEx == "141" || infoEvento.FormTypeEx == "181" ||  //Facturas, Abono
                infoEvento.FormTypeEx == "65301" ||  //Anticipos
                infoEvento.FormTypeEx == "90092")  //Factura reserva 60091, factura cliente  + pago
            {
                EXO_DocSII fDocsSII = new EXO_DocSII();
                lRetorno = fDocsSII.ItemEvent(infoEvento);
            }



            if (infoEvento.FormTypeEx == "150")
            {
                EXO_150 f150 = new EXO_150();
                lRetorno = f150.ItemEvent(infoEvento);
                f150 = null;
            }

            if (infoEvento.FormTypeEx == "42")
            {
                EXO_42 f42 = new EXO_42();
                lRetorno = f42.ItemEvent(infoEvento);
                f42 = null;
            }

            if (infoEvento.FormTypeEx == "41")
            {
                EXO_41 f41 = new EXO_41();
                lRetorno = f41.ItemEvent(infoEvento);
                f41 = null;
            }

            if (infoEvento.FormTypeEx == "62")
            {
                EXO_62 f62 = new EXO_62();
                lRetorno = f62.ItemEvent(infoEvento);
                f62 = null;
            }

            if (infoEvento.FormTypeEx == "504")
            {
                EXO_504 f504 = new EXO_504();
                lRetorno = f504.ItemEvent(infoEvento);
                f504 = null;
            }

            //if (infoEvento.FormTypeEx == "136")
            //{
            //    EXO_136 f136 = new EXO_136();
            //    lRetorno = f136.ItemEvent(infoEvento);
            //    f136 = null;
            //}

            return lRetorno;
        }

        public override bool SBOApp_FormDataEvent(BusinessObjectInfo infoDataEvent)
        {
            bool lRetorno = true;

            //Doc ventas
            if (infoDataEvent.FormTypeEx == "133" || infoDataEvent.FormTypeEx == "179" ||  //Facturas, Abono
                infoDataEvent.FormTypeEx == "65300" ||  //Anticipos
                infoDataEvent.FormTypeEx == "60091" || infoDataEvent.FormTypeEx == "60090")  //Factura reserva, factura cliente  + pago
            {
                EXO_DocSII fDocsSII = new EXO_DocSII();
                lRetorno = fDocsSII.DataEvent(infoDataEvent);
            }

            //Doc compras
            if (infoDataEvent.FormTypeEx == "141" || infoDataEvent.FormTypeEx == "181" ||  //Facturas, Abono
                infoDataEvent.FormTypeEx == "65301" ||  //Anticipos
                infoDataEvent.FormTypeEx == "60092")  //Factura reserva
            {
                EXO_DocSII fDocsSII = new EXO_DocSII();
                lRetorno = fDocsSII.DataEvent(infoDataEvent);
            }


            if (infoDataEvent.FormTypeEx == "150")
            {
                EXO_150 f150 = new EXO_150();
                lRetorno = f150.DataEvent(infoDataEvent);
                f150 = null;
            }

            if (infoDataEvent.FormTypeEx == "65211")
            {
                EXO_65211 f65211 = new EXO_65211();
                lRetorno = f65211.DataEvent(infoDataEvent);
                f65211 = null;
            }

            //if (infoDataEvent.FormTypeEx == "136")
            //{
            //    EXO_136 f136 = new EXO_136();
            //    lRetorno = f136.DataEvent(infoDataEvent);
            //    f136 = null;
            //}

            return lRetorno;
        }

        public override bool SBOApp_MenuEvent(MenuEvent infoMenuEvent)
        {
            bool lRetorno = true;

            switch (infoMenuEvent.MenuUID)
            {
                case "1282":
                    //Articulos
                    string cTypeEx = Matriz.gen.SBOApp.Forms.ActiveForm.TypeEx;
                    switch (cTypeEx)
                    {
                        case "150":
                            EXO_150 f150 = new EXO_150();
                            lRetorno = f150.MenuEvent(infoMenuEvent);
                            f150 = null;
                            break;
                        case "133":
                        case "179":
                        case "65300":
                        case "60091":
                        case "60090":
                        case "141":
                        case "181":
                        case "65301":
                        case "60092":
                            EXO_DocSII fDocSII = new EXO_DocSII();
                            lRetorno = fDocSII.MenuEvent(infoMenuEvent);
                            f150 = null;
                            break;
                    }

                    break;
            }

            return lRetorno;
        }

        private string sqlInicio1()
        {
            string cRetorno;

            if (Matriz.lgHANA)
            {
                cRetorno = "SELECT T0.\"SumDec\" SumDec, T0.\"PriceDec\" PriceDec, T0.\"RateDec\" RateDec, T0.\"QtyDec\" QtyDec, ";
                cRetorno += "T0.\"PercentDec\" PercentDec, T0.\"MeasureDec\" MeasureDec, ";
                cRetorno += "T0.\"ThousSep\" ThousSep, T0.\"DecSep\" DecSep, ";
                cRetorno += " \"U_EXO_TipoFacCancel\" \"U_EXO_TipoFacCancel\" ";
                cRetorno += " FROM \"OADM\" T0";
            }
            else
            {

                cRetorno = "SELECT T0.SumDec as 'SumDec', T0.PriceDec as 'PriceDec', T0.RateDec as 'RateDec', T0.QtyDec as 'QtyDec', T0.PercentDec as 'PercentDec', T0.MeasureDec as 'MeasureDec', ";
                cRetorno += " T0.ThousSep as 'ThousSep', T0.DecSep as 'DecSep', ";
                cRetorno += " T0.U_EXO_TipoFacCancel AS 'U_EXO_TipoFacCancel' ";
                cRetorno += " FROM OADM T0";
            }


            return cRetorno;

        }

        private List<TiposForm> LlenoListaDocs()
        {
            List<TiposForm> ListaAux = new List<TiposForm>();
            TiposForm Aux;
            #region Lleno los tipos
            //
            Aux.TipoEx = "133";
            Aux.Descripcion = "Factura de ventas";
            Aux.oTipoDoc = TipoDoc.Ventas;
            Aux.TablaPrincipal = "OINV";
            ListaAux.Add(Aux);

            //
            Aux.TipoEx = "179";
            Aux.Descripcion = "Abono de ventas";
            Aux.oTipoDoc = TipoDoc.Ventas;
            Aux.TablaPrincipal = "ORIN";
            ListaAux.Add(Aux);

            //
            Aux.TipoEx = "65300";
            Aux.Descripcion = "Anticipo de ventas";
            Aux.oTipoDoc = TipoDoc.Ventas;
            Aux.TablaPrincipal = "ODPI";
            ListaAux.Add(Aux);

            //
            Aux.TipoEx = "60091";
            Aux.Descripcion = "Factura de reserva (ventas)";
            Aux.oTipoDoc = TipoDoc.Ventas;
            Aux.TablaPrincipal = "OINV";
            ListaAux.Add(Aux);

            //
            Aux.TipoEx = "60090";
            Aux.Descripcion = "Factura + pago (ventas)";
            Aux.oTipoDoc = TipoDoc.Ventas;
            Aux.TablaPrincipal = "OINV";
            ListaAux.Add(Aux);

            //
            Aux.TipoEx = "141";
            Aux.Descripcion = "Factura de compras";
            Aux.oTipoDoc = TipoDoc.Compras;
            Aux.TablaPrincipal = "OPCH";
            ListaAux.Add(Aux);

            //
            Aux.TipoEx = "181";
            Aux.Descripcion = "Abono de compras";
            Aux.oTipoDoc = TipoDoc.Compras;
            Aux.TablaPrincipal = "ORPC";
            ListaAux.Add(Aux);

            //
            Aux.TipoEx = "65301";
            Aux.Descripcion = "Anticipo de compras";
            Aux.oTipoDoc = TipoDoc.Compras;
            Aux.TablaPrincipal = "ODPO";
            ListaAux.Add(Aux);

            //
            Aux.TipoEx = "60092";
            Aux.Descripcion = "Factura de reserva (compras)";
            Aux.oTipoDoc = TipoDoc.Compras;
            Aux.TablaPrincipal = "OPCH";
            ListaAux.Add(Aux);
            #endregion

            return ListaAux;
        }



    }
}
