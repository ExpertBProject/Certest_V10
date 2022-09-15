using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;
using SAPbouiCOM;


namespace Cliente
{
    public class EXO_DocSII
    {
        public static bool lgDespuesAnadir;


        public EXO_DocSII()
        { }

        public bool ItemEvent(ItemEvent infoEvento)
        {
            switch (infoEvento.EventType)
            {
                case BoEventTypes.et_ITEM_PRESSED:
                    #region despues de añadir
                    if ( (infoEvento.ItemUID == "1" || infoEvento.ItemUID == "2349990001" ) && 
                        !infoEvento.BeforeAction)
                    {
                        if (lgDespuesAnadir)
                        {
                            SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);
                            if (oForm.Mode == BoFormMode.fm_ADD_MODE)
                            {
                                CambioTipoFac(ref oForm);
                            }
                            lgDespuesAnadir = false;
                        }
                    }
                    #endregion
                    break;

                case BoEventTypes.et_FORM_LOAD:
                    if (!infoEvento.BeforeAction)
                    {
                        SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);
                        if (oForm.Mode == BoFormMode.fm_ADD_MODE)
                        {
                            //Para que coja al empezar
                            CambioTipoFac(ref oForm);
                        }
                    }
                    break;


                case BoEventTypes.et_COMBO_SELECT:
                    #region despues de añadir
                    if ((infoEvento.ItemUID == "1" || infoEvento.ItemUID == "2349990001") &&
                        !infoEvento.BeforeAction)
                    {
                        if (lgDespuesAnadir)
                        {
                            SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);
                            if (oForm.Mode == BoFormMode.fm_ADD_MODE)
                            {
                                CambioTipoFac(ref oForm);
                            }
                            lgDespuesAnadir = false;
                        }
                    }
                    #endregion

                    //cambio tipo de iva 
                    if ((infoEvento.ItemUID == "38" && infoEvento.ColUID == "18" && !infoEvento.BeforeAction) ||
                        (infoEvento.ItemUID == "39" && infoEvento.ColUID == "57" && !infoEvento.BeforeAction) ||
                        infoEvento.ItemUID == "88")
                    {
                        SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);
                        CambioTipoFac(ref oForm);
                    }

                    //cambio desplegable numeracion
                    if (infoEvento.ItemUID == "88")
                    {
                        SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);
                        CambioTipoFac(ref oForm);
                    }


                    break;

                case BoEventTypes.et_VALIDATE:
                    if (infoEvento.ItemUID == "38" && infoEvento.ColUID == "1" && !infoEvento.BeforeAction &&
                        infoEvento.InnerEvent)
                    {
                        SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);
                        CambioTipoFac(ref oForm);
                    }
                    break;
            }


            return true;
        }        

        public bool DataEvent(BusinessObjectInfo args)
        {
            //valido
            if (args.EventType == BoEventTypes.et_FORM_DATA_ADD && args.BeforeAction)
            {
                #region Valido-Aviso de las importaciones - SOLO COMPRAS
                SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.Item(args.FormUID);
                if (Matriz.gListaTiposForm.First(x => x.TipoEx == oForm.TypeEx).oTipoDoc == TipoDoc.Compras)
                {
                    string cDBPrincipal = Matriz.gListaTiposForm.First(x => x.TipoEx == oForm.TypeEx).TablaPrincipal;
                    SAPbouiCOM.Matrix oMatLin = (SAPbouiCOM.Matrix)oForm.Items.Item(oForm.DataSources.DBDataSources.Item(cDBPrincipal).GetValue("DocType", 0).Trim() == "I" ? "38" : "39").Specific;
                    if (IvasImportacion(ref oMatLin, TipoImportacion.ConDUA) &&
                        oForm.DataSources.DBDataSources.Item(cDBPrincipal).GetValue("U_B1SYS_INV_TYPE", 0).Trim() != "F5")
                    {
                        if (Matriz.gen.SBOApp.MessageBox("ATENCION!!\n La factura no es de tipo F5 aun llevando IVA's de importacion para el SII\n ¿ Continuar ? ", 1, "Si", "No", "") != 1)
                        {
                            return false;
                        }
                    }

                    if (IvasImportacion(ref oMatLin, TipoImportacion.sinDUA) &&
                        oForm.DataSources.DBDataSources.Item(cDBPrincipal).GetValue("U_B1SYS_INV_TYPE", 0).Trim() != "F6")
                    {
                        if (Matriz.gen.SBOApp.MessageBox("ATENCION!!\n La factura no es de tipo F6 aun llevando IVA de registros sin DUA\n ¿ Continuar ? ", 1, "Si", "No", "") != 1)
                        {
                            return false;
                        }
                    }
                }
                #endregion
            }

            if (args.EventType == BoEventTypes.et_FORM_DATA_ADD && !args.BeforeAction && args.ActionSuccess)
            {
                lgDespuesAnadir = true;
            }

            return true;
        }

        public bool MenuEvent(MenuEvent infoMenuEvent)
        {
            bool lRetorno = true;

            switch (infoMenuEvent.MenuUID)
            {
                //Añadir
                case "1282":
                    if (!infoMenuEvent.BeforeAction)
                    {
                        SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.ActiveForm;
                        CambioTipoFac(ref oForm);                        
                    }
                    break;
            }

            return lRetorno;
        }



        public enum TipoImportacion { ConDUA, sinDUA }


        public void CambioTipoFac(ref SAPbouiCOM.Form oForm)
        {

            try
            {
                string cTipoForm = oForm.TypeEx;
                string cTipoFacAPoner = "";
                string cDBPrincipal = Matriz.gListaTiposForm.First(x => x.TipoEx == cTipoForm).TablaPrincipal;

                //Si es de cancelación SOLO VENTAS, poner F1     
                //if (Matriz.gListaTiposForm.First(x => x.TipoEx == cTipoForm).oTipoDoc == TipoDoc.Ventas &&
                //    oForm.DataSources.DBDataSources.Item(cDBPrincipal).GetValue("CANCELED", 0) == "Y")
                //{
                //    cTipoFacAPoner = Matriz.cgTipoFacCancelaciónVENTAS;
                //}


                if (Matriz.gListaTiposForm.First(x => x.TipoEx == cTipoForm).oTipoDoc == TipoDoc.Compras)
                {
                    #region Para las importaciones
                    SAPbouiCOM.Matrix oMatLin = (SAPbouiCOM.Matrix)oForm.Items.Item(oForm.DataSources.DBDataSources.Item(cDBPrincipal).GetValue("DocType", 0).Trim() == "I" ? "38" : "39").Specific;
                    //Miro a veri si hay IVA sin DUA
                    if (cTipoFacAPoner == "")
                    {
                        if (IvasImportacion(ref oMatLin, TipoImportacion.sinDUA))
                        {
                            cTipoFacAPoner = "F6";
                        }
                    }

                    //AHora miro si F5 (importacion)
                    if (cTipoFacAPoner == "")
                    {
                        if (IvasImportacion(ref oMatLin, TipoImportacion.ConDUA))
                        {
                            cTipoFacAPoner = "F5";
                        }
                    }
                    #endregion
                }

                //Y ahora miro el tipo fac por serie
                if (cTipoFacAPoner == "")
                {
                    int nSerieAct = Convert.ToInt32(oForm.DataSources.DBDataSources.Item(cDBPrincipal).GetValue("Series", 0));
                    //Busco el TIPO FAC de la serie
                    string cIvaFact = Matriz.gen.refDi.SQL.sqlStringB1(sqlTipoFacSeries(nSerieAct));
                    if (cIvaFact != "XX") cTipoFacAPoner = cIvaFact;
                }

                string cTipoFacActual = oForm.DataSources.DBDataSources.Item(cDBPrincipal).GetValue("U_B1SYS_INV_TYPE", 0).Trim();
                oForm.Freeze(true);
                try
                {
                    #region Y lo pongo
                    if (cTipoFacAPoner != "" && cTipoFacActual != cTipoFacAPoner)
                    {
                        string cItemFoco = "";
                        int nPagAnterior = oForm.PaneLevel;
                        SAPbouiCOM.CellPosition oPosition = null;

                        if (oForm.Visible)
                        {
                            try
                            {
                                cItemFoco = oForm.ActiveItem;
                                if (cItemFoco == "38" || cItemFoco == "39") oPosition = ((SAPbouiCOM.Matrix)oForm.Items.Item(cItemFoco).Specific).GetCellFocus();
                            }
                            catch
                            { }
                        }


                        oForm.PaneLevel = 7;
                        ((SAPbouiCOM.ComboBox)oForm.Items.Item("254000669").Specific).Select(cTipoFacAPoner, BoSearchKey.psk_ByValue);

                        //
                        oForm.PaneLevel = nPagAnterior;

                        if (oForm.Visible)
                        {
                            if (cItemFoco == "38" || cItemFoco == "39")
                            {
                                ((SAPbouiCOM.Matrix)oForm.Items.Item(cItemFoco).Specific).SetCellFocus(oPosition.rowIndex, oPosition.ColumnIndex);
                            }
                            else
                            {
                                if (cItemFoco != "") oForm.ActiveItem = cItemFoco;
                            }
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                { }
            }
            catch(Exception ex)
            { }
            oForm.Freeze(false);
        }


        public bool IvasImportacion(ref SAPbouiCOM.Matrix oMatLin, TipoImportacion oModo)
        {
            bool lRetorno = true;
            string sXMLMat = "";
            System.Xml.XmlDocument oXmlMat = new System.Xml.XmlDocument();
            System.Xml.XmlNodeList oXmlNodesMat;

            //                        
            string cColumnaIVA = (oMatLin.Item.UniqueID == "38" ? "18" : "57");
            sXMLMat = oMatLin.SerializeAsXML(BoMatrixXmlSelect.mxs_All);
            oXmlMat.LoadXml(sXMLMat);


            string sql = (oModo == TipoImportacion.ConDUA ? sqlIvasImportacionDUA() : sqlIvasImportacionSinDUA());
            string cCadenaTiposIVAImportacion = Matriz.gen.refDi.SQL.sqlStringB1(sql);

            //Monto XPATH
            string cAuxXPath = "";
            bool lPrimero = true;
            List<string> ListaIVAs = cCadenaTiposIVAImportacion.Split(',').ToList();
            foreach (string x in ListaIVAs)
            {
                if (!lPrimero) cAuxXPath += " or ";
                cAuxXPath += "./ Value='" + x + "'";
                lPrimero = false;
            }
            string xPath = "/Matrix/Rows/Row/Columns/Column[./ID='" + cColumnaIVA + "' and (" + cAuxXPath + ")]";
            oXmlNodesMat = oXmlMat.SelectNodes(xPath);

            //
            lRetorno = (oXmlNodesMat != null && oXmlNodesMat.Count >= 1);
            return lRetorno;
        }


        public string sqlIvasImportacionDUA()
        {
            string sql = "";
            if (Matriz.lgHANA)
            {
                sql = "select STRING_AGG(Tabla.\"Code\", ',') FROM ( select T0.\"Code\" as \"Code\" FROM \"OVTG\" T0 WHERE T0.\"U_EXO_CambioF5\" = 'Y' ) AS Tabla";

            }
            else
            {
                sql = "SELECT STUFF(( SELECT  ',' + T0.Code FROM OVTG T0 WHERE T0.U_EXO_CambioF5 = 'Y' FOR XML PATH('') ),1,1, '')";
            }

            return sql;
        }

        public string sqlIvasImportacionSinDUA()
        {
            string sql = "";
            if (Matriz.lgHANA)
            {
                sql = "select STRING_AGG(Tabla.\"Code\", ',') FROM ( select T0.\"Code\" as \"Code\" FROM \"OVTG\" T0 WHERE T0.\"U_B1SYS_SPEC_REGIME\" = '13' ) AS Tabla";

            }
            else
            {
                sql = "SELECT STUFF(( SELECT  ',' + T0.Code FROM OVTG T0 WHERE T0.U_B1SYS_SPEC_REGIME = '13' FOR XML PATH('') ),1,1, '')";
            }

            return sql;
        }


        public string sqlTipoFacSeries(int nSerie)
        {
            string sql = "";
            if (Matriz.lgHANA)
            {
                sql = "SELECT ISNULL(T0.\"U_EXO_TipoFac\", 'XX') FROM \"NNM1\" T0  WHERE((-1 = ##SERIE) OR T0.\"Series\" = ##SERIE) LIMIT 1";
            }
            else
            {
                sql = "SELECT TOP 1 ISNULL(T0.U_EXO_TipoFac, 'XX') FROM NNM1 T0  WHERE((-1 = ##SERIE) OR T0.Series = ##SERIE) ";
            }

            sql = sql.Replace("##SERIE", nSerie.ToString());

            return sql;



        }

    }
}
