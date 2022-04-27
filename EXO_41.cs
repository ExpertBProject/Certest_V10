using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;

namespace Cliente
{
    public class EXO_41
    {
        public bool ItemEvent(ItemEvent infoEvento)
        {
            switch (infoEvento.EventType)
            {
                case BoEventTypes.et_ITEM_PRESSED:
                    if (infoEvento.ItemUID == "1")
                    {
                        SAPbouiCOM.Form oForm;
                        oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);

                        if (oForm.Mode == BoFormMode.fm_UPDATE_MODE || oForm.Mode == BoFormMode.fm_ADD_MODE)
                        {
                            #region Recorro y valido
                            SAPbouiCOM.Matrix oMatLin;
                            oMatLin = (SAPbouiCOM.Matrix)oForm.Items.Item("3").Specific;
                            for (int i = 1; i <= oMatLin.RowCount; i++)
                            {
                                string cAux = ((SAPbouiCOM.EditText)oMatLin.GetCellSpecific("10", i)).Value.Trim();
                                if (cAux == "")
                                {
                                    Matriz.gen.SBOApp.MessageBox("No ha introducido Fecha de vencimiento para el lote en linea " + i.ToString(), 1, "Ok", "", "");
                                    return false;
                                }
                            }
                            #endregion
                        }

                        EXO_CleanCOM.CLiberaCOM.Form(oForm);
                    }

                    break;
            }

            return true;
        }
    }
}
