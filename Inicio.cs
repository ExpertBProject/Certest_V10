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
                        this.aplicacionB1.MessageBox("Actualizacion de campos realizada",1, "Ok", "", "");
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
            Matriz.SumDec = Convert.ToInt32(oRec.Fields.Item("SumDec").Value);
            Matriz.PriceDec = Convert.ToInt32(oRec.Fields.Item("PriceDec").Value);
            Matriz.RateDec = Convert.ToInt32(oRec.Fields.Item("RateDec").Value);
            Matriz.QtyDec = Convert.ToInt32(oRec.Fields.Item("QtyDec").Value);
            Matriz.PercentDec = Convert.ToInt32(oRec.Fields.Item("PercentDec").Value);
            Matriz.MeasureDec = Convert.ToInt32(oRec.Fields.Item("MeasureDec").Value);
            Matriz.SepMill = Convert.ToString(oRec.Fields.Item("ThousSep").Value);
            Matriz.SepDec = Convert.ToString(oRec.Fields.Item("DecSep").Value);

            System.Runtime.InteropServices.Marshal.ReleaseComObject(oRec);
            oRec = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            #endregion

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

            if (infoEvento.FormTypeEx == "40005" && !infoEvento.BeforeAction)
            {
                SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.ActiveForm;

                //int yellowForeColor = Color.Yellow.R | (Color.Yellow.G << 8) | (Color.Yellow.B << 16);

                //setting.SetCellFontColor(2, 9, yellowForeColor);


                SAPbouiCOM.Matrix oMatrix = (SAPbouiCOM.Matrix)oForm.Items.Item("10").Specific;

                oMatrix.CommonSetting.SetCellBackColor(23, 72, System.Drawing.Color.Red.R);
                oMatrix.CommonSetting.SetCellBackColor(22, 72, System.Drawing.Color.Red.R);

                oMatrix.CommonSetting.SetCellFontSize(22, 72, 14);
                oMatrix.CommonSetting.SetCellFontColor(22, 72, System.Drawing.Color.Red.R);

                //   string xx = ((SAPbouiCOM.EditText)oMatrix.GetCellSpecific("72", 22)).Value;
                // ((SAPbouiCOM.EditText)oMatrix.GetCellSpecific("72", 22)).Value;

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

            //if (infoEvento.FormTypeEx == "138")
            //{
            //    EXO_138 f138 = new EXO_138();
            //    lRetorno = f138.ItemEvent(infoEvento, ref this.Company, ref this.SboApp);
            //    f138 = null;
            //}

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

            return lRetorno;            
        }

        public override bool SBOApp_FormDataEvent(BusinessObjectInfo infoDataEvent)
        {
            bool lRetorno = true;

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

            return lRetorno;
        }

        public override bool SBOApp_MenuEvent(MenuEvent infoMenuEvent)
        {
            bool lRetorno = true;

            switch(infoMenuEvent.MenuUID)
                {
                    case "1282":
                        //Articulos
                        if (!infoMenuEvent.BeforeAction)
                        {
                        string cTypeEx = Matriz.gen.SBOApp.Forms.ActiveForm.TypeEx;
                            switch (cTypeEx)
                            {
                                case "150":
                                EXO_150 f150;
                                f150 = new EXO_150();
                                lRetorno = f150.MenuEvent(infoMenuEvent);
                                f150 = null;
                                break;
                            }  
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
                cRetorno += "T0.\"ThousSep\" ThousSep, T0.\"DecSep\" DecSep FROM \"OADM\" T0";
            }
            else
            {

                cRetorno = "SELECT T0.SumDec as 'SumDec', T0.PriceDec as 'PriceDec', T0.RateDec as 'RateDec', T0.QtyDec as 'QtyDec', T0.PercentDec as 'PercentDec', T0.MeasureDec as 'MeasureDec', ";
                cRetorno += " T0.ThousSep as 'ThousSep', T0.DecSep as 'DecSep' FROM OADM T0";
            }
            

            return cRetorno;

        }

    }
}
