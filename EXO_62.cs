using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;

namespace Cliente
{
    public class EXO_62
    {
        public bool ItemEvent(ItemEvent infoEvento)
        {
            

            //switch (infoEvento.EventType)
            //{
            //    case BoEventTypes.et_FORM_LOAD:
            //        if (!infoEvento.BeforeAction)
            //        {
            //            SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);
            //            #region Boton reemplazar almacen defecto
            //            SAPbouiCOM.Item oItem;
            //            oItem = oForm.Items.Add("btnReemp", BoFormItemTypes.it_BUTTON);
            //            oItem.Left = oForm.Items.Item("1470002017").Left;
            //            oItem.Top = oForm.Items.Item("73").Top;
            //            oItem.Width = oForm.Items.Item("1").Width + 100;
            //            oItem.Height = oForm.Items.Item("1").Height;
            //            oItem.FromPane = oForm.Items.Item("1470002017").FromPane;
            //            oItem.ToPane = oForm.Items.Item("1470002017").ToPane;
            //            oItem.LinkTo = "73";
            //            ((SAPbouiCOM.Button)oItem.Specific).Caption = "Reemplazar almacén por defecto";
            //            #endregion

            //            EXO_CleanCOM.CLiberaCOM.Form(oForm);
            //        }
            //        break;

            //    case BoEventTypes.et_ITEM_PRESSED:
            //        #region Seleccion del combo de gestion por lote, serie,...
            //        if (!infoEvento.BeforeAction && infoEvento.ItemUID == "btnReemp")
            //        {
            //            SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);
            //            string cAlmaDefec = ((SAPbouiCOM.EditText)oForm.Items.Item("1470002038").Specific).Value;
            //            if (cAlmaDefec == "") cAlmaDefec = "01";

            //            if (Matriz.gen.SBOApp.MessageBox("Será asignado el almacén '" + cAlmaDefec + "' como Almacén por defecto.\n ¿ Desea continuar ?", 2, "Sí", "No", "") == 1)
            //            {
            //                string cWH = ((SAPbouiCOM.EditText)oForm.Items.Item("5").Specific).Value;
            //                string sql = "SELECT T0.ItemCode FROM OITM T0 WHERE T0.DfltWH = '" + cWH + "' ORDER BY T0.ItemCode";

            //                SAPbobsCOM.Recordset oRec = Matriz.gen.refDi.SQL.sqlComoRsB1(sql);
            //                SAPbobsCOM.Items oItem = (SAPbobsCOM.Items) Matriz.gen.compañia.GetBusinessObject(BoObjectTypes.oItems);

            //                while (!oRec.EoF)
            //                {
            //                    oItem.GetByKey(oRec.Fields.Item(0).Value);
            //                    oItem.DefaultWarehouse = cAlmaDefec;
            //                    oItem.Update();
            //                    Matriz.gen.SBOApp.SetStatusBarMessage("Actualizado " + oRec.Fields.Item(0).Value, BoMessageTime.bmt_Short, false);

            //                    oRec.MoveNext();
            //                }

            //                Matriz.gen.SBOApp.MessageBox("Proceso terminado", 1, "Ok", "", "");
            //            }

            //            EXO_CleanCOM.CLiberaCOM.Form(oForm);

            //        }
            //        #endregion
            //        break;
            //}
            
            return true;
        }

    }
}
