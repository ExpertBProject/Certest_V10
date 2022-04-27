using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;

namespace Cliente
{
    public class EXO_65211
    {
        public bool DataEvent(BusinessObjectInfo infoDataEvent)
        {
            bool lRetorno = true;

            if (infoDataEvent.BeforeAction &&                 
                ( infoDataEvent.EventType == BoEventTypes.et_FORM_DATA_UPDATE || infoDataEvent.EventType == BoEventTypes.et_FORM_DATA_ADD ) )
            {
                SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.Item(infoDataEvent.FormUID);
                string cTipo = oForm.DataSources.DBDataSources.Item("OWOR").GetValue("U_EXO_PROCESO", 0).ToString().Trim();

                if (cTipo == "")
                {
                    Matriz.gen.SBOApp.MessageBox("La orden de produccion ha de tener tipo", 1, "Ok", "","");
                    EXO_CleanCOM.CLiberaCOM.Form(oForm);
                    return false;
                }

                //A ver si puede el usuario actual - No se por Transaction Notification
                if ( oForm.DataSources.DBDataSources.Item("OWOR").GetValue("Status", 0).ToString().Trim() == "L" )
                {
                    string sql = "select TOP 1 T0.U_CodUsuario FROM [@EXO_USUPROCESOS] T0 WHERE T0.Code = '" + cTipo + "' AND T0.U_CodUsuario = " + Matriz.gen.compañia.UserSignature.ToString();
                    if ( Matriz.gen.refDi.SQL.sqlNumericaB1(sql) == 0)
                    {
                        Matriz.gen.SBOApp.MessageBox("No tiene permiso para cerrar esta orden de producción", 1, "Ok", "","");
                        EXO_CleanCOM.CLiberaCOM.Form(oForm);
                        return false;
                    }
                }
            }
           
            return lRetorno;
        }

    }
}
