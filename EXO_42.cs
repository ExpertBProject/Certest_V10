using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;

namespace Cliente
{
    public class EXO_42
    {
        public bool ItemEvent(ItemEvent infoEvento)
        {
            SAPbouiCOM.Form oForm = null;
            oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);
           
            switch (infoEvento.EventType)
            {
                case BoEventTypes.et_ITEM_PRESSED:

                    if (infoEvento.ItemUID == "16" && infoEvento.BeforeAction)
                    {
                        #region Asigno los lotes automaticamente - Ordeno cpor la columna Fecha Vencimiento
                        OrdenoCol(false, ref oForm);
                        #endregion                                                 
                    }

                    if (infoEvento.ItemUID == "3" && !infoEvento.BeforeAction)
                    {
                        OrdenoCol(true, ref oForm);
                    }


                    break;
                case BoEventTypes.et_FORM_LOAD:
                    if (!infoEvento.BeforeAction)
                    {                                                
                        OrdenoCol(true, ref oForm);
                    }

                    break;
            }
            EXO_CleanCOM.CLiberaCOM.Form(oForm);
            return true;
        }


        public void OrdenoCol(bool lDefecto, ref SAPbouiCOM.Form oForm)
        {
            string cTipo = "", cArt = "";

            SAPbouiCOM.Matrix oMatLotDis = null;
            SAPbouiCOM.Matrix oMatArt = null;
            oMatLotDis = ((SAPbouiCOM.Matrix)oForm.Items.Item("4").Specific);
            oMatArt = ((SAPbouiCOM.Matrix)oForm.Items.Item("3").Specific);


            int nLin = oMatArt.GetNextSelectedRow(0, BoOrderType.ot_RowOrder);
            if (nLin != 0 && nLin != -1) cArt = ((SAPbouiCOM.EditText)oMatArt.GetCellSpecific("1", nLin)).Value.Trim();

            if (cArt != "")
            {
                string sql = "SELECT isnull(T0.U_EXO_LotAut, 'N') FROM OITM T0 WHERE T0.ItemCode = '" + cArt + "'";
                cTipo = Matriz.gen.refDi.SQL.sqlStringB1(sql);
            }

            if (lDefecto)
            {
                string cColumna = (cTipo == "Y") ? "15" : "22";
                ((SAPbouiCOM.Column)oMatLotDis.Columns.Item(cColumna)).TitleObject.Sort(BoGridSortType.gst_Ascending);
            }
            else
            {
                if (cTipo == "Y")
                {
                    ((SAPbouiCOM.Column)oMatLotDis.Columns.Item("15")).TitleObject.Sort(BoGridSortType.gst_Ascending);
                }
            }                
        }
              
    }
}
