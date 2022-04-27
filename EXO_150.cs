using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;

namespace Cliente
{
    public class EXO_150
    {
        public bool ItemEvent(ItemEvent infoEvento)
        {
            SAPbouiCOM.Form oForm = null;
            

            switch (infoEvento.EventType)
            {
                case BoEventTypes.et_FORM_LOAD:
                    if (!infoEvento.BeforeAction)
                    {
                        oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);

                        SAPbouiCOM.Item oItem;
                        #region Combo Lote automatico                         
                        #region Etiqueta
                        oItem = oForm.Items.Add("lblTipGes", BoFormItemTypes.it_STATIC);
                        oItem.Left = oForm.Items.Item("162").Left + oForm.Items.Item("162").Width + 2;
                        oItem.Top = oForm.Items.Item("162").Top;
                        oItem.Width = oForm.Items.Item("161").Width;
                        oItem.Height = oForm.Items.Item("161").Height;
                        oItem.FromPane = oForm.Items.Item("162").FromPane;
                        oItem.ToPane = oForm.Items.Item("162").ToPane;
                        ((SAPbouiCOM.StaticText)oItem.Specific).Caption = "Tipo Gestión";
                        #endregion

                        #region Combo
                        oItem = oForm.Items.Add("cmbAut", BoFormItemTypes.it_COMBO_BOX );
                        oItem.Left = oForm.Items.Item("lblTipGes").Left + oForm.Items.Item("lblTipGes").Width + 2;
                        oItem.Top = oForm.Items.Item("lblTipGes").Top;
                        oItem.Width = oForm.Items.Item("249").Width;
                        oItem.Height = oForm.Items.Item("249").Height;
                        oItem.FromPane = oForm.Items.Item("162").FromPane;
                        oItem.ToPane = oForm.Items.Item("162").ToPane;
                        oItem.LinkTo = "162";
                        oItem.DisplayDesc = true;
                        ((SAPbouiCOM.ComboBox)oItem.Specific).DataBind.SetBound(true, "OITM", "U_EXO_LotAut");
                        ((SAPbouiCOM.ComboBox)oItem.Specific).ExpandType = BoExpandType.et_DescriptionOnly;

                        ((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("N", "Manual");
                        ((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("Y", "Automatica");
                        #endregion

                        oForm.Items.Item("lblTipGes").LinkTo = "cmbAut";
                        #endregion

                        oForm.Items.Item("cmbAut").SetAutoManagedAttribute(BoAutoManagedAttr.ama_Visible, 4, BoModeVisualBehavior.mvb_False);
                        oForm.Items.Item("lblTipGes").SetAutoManagedAttribute(BoAutoManagedAttr.ama_Visible, 4, BoModeVisualBehavior.mvb_False);
                        try
                        {
                            string cManLot = oForm.DataSources.DBDataSources.Item("OITM").GetValue("ManBtchNum", 0).Trim();
                            CambioComboAut(cManLot, ref oForm);
                        }
                        catch (Exception ex) { }
                    }
                    break;

                case BoEventTypes.et_COMBO_SELECT:
                    #region Seleccion del combo de gestion por lote, serie,...
                    if (!infoEvento.BeforeAction && infoEvento.ItemUID == "162")
                    {
                        oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);
                        string cManLot = "";
                        try
                        {
                            cManLot = ((SAPbouiCOM.ComboBox)oForm.Items.Item("162").Specific).Selected.Value;
                        }
                        catch (Exception ex)
                        {
                        }

                        //Lo dejo a N
                        try
                        {
                            if (cManLot != "2") ((SAPbouiCOM.ComboBox)oForm.Items.Item("cmbAut").Specific).Select("N", BoSearchKey.psk_ByValue);
                        }
                        catch(Exception ex){}
                        
                        CambioComboAut(cManLot == "2" ? "Y" : "N", ref  oForm);

                    }
                    #endregion
                    break;
            }
            EXO_CleanCOM.CLiberaCOM.Form(oForm);
            return true;
        }

        public bool DataEvent(BusinessObjectInfo infoDataEvent)
        {
            bool lRetorno = true;
            
            switch (infoDataEvent.EventType)
            {
                case BoEventTypes.et_FORM_DATA_LOAD:
                    #region Para que se oculte o no el combo de tipo de gestion
                    if (!infoDataEvent.BeforeAction)
                    {
                        SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.Item(infoDataEvent.FormUID);
                        
                        string cManLot = oForm.DataSources.DBDataSources.Item("OITM").GetValue("ManBtchNum", 0).Trim();
                        CambioComboAut(cManLot, ref oForm);
                        EXO_CleanCOM.CLiberaCOM.Form(oForm);
                    }
                    #endregion
                    break;

                #region Ya que estamos, para que se quede bien al añadir y al delete
                case BoEventTypes.et_FORM_DATA_ADD:
                    if (!infoDataEvent.BeforeAction && infoDataEvent.ActionSuccess)
                    {
                        SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.Item(infoDataEvent.FormUID);
                        CambioComboAut("N", ref oForm);
                        EXO_CleanCOM.CLiberaCOM.Form(oForm);
                    }
                    break;
                case BoEventTypes.et_FORM_DATA_DELETE:
                    if (!infoDataEvent.BeforeAction && infoDataEvent.ActionSuccess)
                    {
                        SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.Item(infoDataEvent.FormUID);
                        CambioComboAut("N", ref oForm);
                        EXO_CleanCOM.CLiberaCOM.Form(oForm);
                    }
                break;
                #endregion

            }
                        
            return lRetorno;
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
                        SAPbouiCOM.Form oForm =  Matriz.gen.SBOApp.Forms.ActiveForm;
                        string cManLot = oForm.DataSources.DBDataSources.Item("OITM").GetValue("ManBtchNum", 0).Trim();
                        CambioComboAut(cManLot, ref oForm);
                        EXO_CleanCOM.CLiberaCOM.Form(oForm);
                    }

                    break;
            }

           return lRetorno;
        }

        private bool CambioComboAut(string cManLot, ref SAPbouiCOM.Form oForm)
        {
            // -1 All modes 
            //afm_Ok 1 OK mode 
            //afm_Add 2 Add mode 
            //afm_Find 4 Find mode 
            //afm_View 8 View mode 
            //oForm = oSBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);
            //string gg = "";
            try
            {            
                if (cManLot == "Y")
                {
                    oForm.Items.Item("cmbAut").SetAutoManagedAttribute(BoAutoManagedAttr.ama_Visible, 1, BoModeVisualBehavior.mvb_True);
                    oForm.Items.Item("lblTipGes").SetAutoManagedAttribute(BoAutoManagedAttr.ama_Visible, 1, BoModeVisualBehavior.mvb_True);
                    oForm.Items.Item("cmbAut").SetAutoManagedAttribute(BoAutoManagedAttr.ama_Visible, 2, BoModeVisualBehavior.mvb_True);
                    oForm.Items.Item("lblTipGes").SetAutoManagedAttribute(BoAutoManagedAttr.ama_Visible, 2, BoModeVisualBehavior.mvb_True);
                }
                else
                {
                    oForm.Items.Item("cmbAut").SetAutoManagedAttribute(BoAutoManagedAttr.ama_Visible, 1, BoModeVisualBehavior.mvb_False);
                    oForm.Items.Item("lblTipGes").SetAutoManagedAttribute(BoAutoManagedAttr.ama_Visible, 1, BoModeVisualBehavior.mvb_False);
                    oForm.Items.Item("cmbAut").SetAutoManagedAttribute(BoAutoManagedAttr.ama_Visible, 2, BoModeVisualBehavior.mvb_False);
                    oForm.Items.Item("lblTipGes").SetAutoManagedAttribute(BoAutoManagedAttr.ama_Visible, 2, BoModeVisualBehavior.mvb_False);
                }
            }
            catch (Exception EX)
            {
            }

            return true;
        }
    }
}
