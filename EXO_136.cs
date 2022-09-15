using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;

namespace Cliente
{
    //Detalles de empresa
    public class EXO_136
    {
        public bool ItemEvent(ItemEvent infoEvento)
        {

            SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);

            switch (infoEvento.EventType)
            {
                //En la pestaña de Inventario
                case BoEventTypes.et_FORM_LOAD:
                    if (!infoEvento.BeforeAction)
                    {
                        #region Comentado
                        //SAPbouiCOM.Item oItem;
                        //string cVersion = Matriz.gen.compañia.Version.ToString();

                        //#region Combo
                        //oItem = (SAPbouiCOM.Item)oForm.Items.Add("cmbTipCan", BoFormItemTypes.it_COMBO_BOX);
                        
                        //if (cVersion.Substring(0, 1) == "9")
                        //{
                        //    //1980000467 = edittext id acreedor sepa
                        //    oItem.Width = oForm.Items.Item("1980000467").Width + 20;
                        //    oItem.Top = oForm.Items.Item("1980000467").Top + oForm.Items.Item("1980000467").Height + 20;
                        //    oItem.Left = oForm.Items.Item("1980000467").Left;
                        //    oItem.Height = oForm.Items.Item("1980000467").Height;                            
                        //}
                        //else if (cVersion.Substring(0, 2) == "10")
                        //{

                        //    oItem.Width = oForm.Items.Item("1980000467").Width + 20;
                        //    //Casilla = Permitir c&álculo externo de impuesto en documentos de cliente
                        //    oItem.Top = oForm.Items.Item("256000641").Top + oForm.Items.Item("256000641").Height + 20;
                        //    oItem.Left = oForm.Items.Item("1980000467").Left;
                        //    oItem.Height = oForm.Items.Item("1980000467").Height;                            
                        //}
                        //oItem.LinkTo = "1980000467";
                        //oItem.FromPane = oForm.Items.Item("1980000467").FromPane;
                        //oItem.ToPane = oForm.Items.Item("1980000467").ToPane;                        
                        //((SAPbouiCOM.ComboBox)oItem.Specific).ExpandType = BoExpandType.et_ValueDescription;

                        ////
                        //SAPbouiCOM.ValidValues oValid = ((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues;
                        //Matriz.gen.funcionesUI.cargaCombo(oValid, Utilidades.sqlCUFD("OADM", "EXO_TipoFacCancel"));

                        ////
                        //((SAPbouiCOM.ComboBox)oItem.Specific).DataBind.SetBound(true, "OADM", "U_EXO_TipoFacCancel");
                        //#endregion

                        //#region Etiqueta
                        //oItem = (SAPbouiCOM.Item)oForm.Items.Add("lblTipCan", BoFormItemTypes.it_STATIC);                                                
                        //if (cVersion.Substring(0, 1) == "9")
                        //{
                        //    //1980000466 = label id acreedor sepa
                        //    oItem.Width = oForm.Items.Item("1980000466").Width + 20;
                        //    oItem.Top = oForm.Items.Item("cmbTipCan").Top;
                        //    oItem.Left = oForm.Items.Item("1980000466").Left;
                        //    oItem.Height = oForm.Items.Item("1980000466").Height;                            
                        //}
                        //else if (cVersion.Substring(0, 2) == "10")
                        //{

                        //    oItem.Width = oForm.Items.Item("1980000466").Width + 20;
                        //    //Casilla = Permitir c&álculo externo de impuesto en documentos de cliente
                        //    oItem.Top = oForm.Items.Item("cmbTipCan").Top;
                        //    oItem.Left = oForm.Items.Item("1980000466").Left;
                        //    oItem.Height = oForm.Items.Item("1980000466").Height;                            
                        //}
                        //oItem.LinkTo = "cmbTipCan";
                        //oItem.FromPane = oForm.Items.Item("cmbTipCan").FromPane;
                        //oItem.ToPane = oForm.Items.Item("cmbTipCan").ToPane;
                        //((SAPbouiCOM.StaticText)oItem.Specific).Caption = "Tipo Factura Cancelación Ventas";
                        //#endregion

                        #endregion
                    }
                    break;
            }
            return true;
        }

        public bool DataEvent(BusinessObjectInfo args)
        {
            ////valido
            //if (args.EventType == BoEventTypes.et_FORM_DATA_UPDATE && !args.BeforeAction && args.ActionSuccess)
            //{
            //    SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.Item(args.FormUID);                
            //    Matriz.cgTipoFacCancelaciónVENTAS = oForm.DataSources.DBDataSources.Item("OADM").GetValue("U_EXO_TipoFacCancel", 0);
            //}

            return true;
        }


    }
}
