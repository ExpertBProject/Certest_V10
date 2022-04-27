using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;

namespace Cliente
{
    public class EXO_504
    {
        public bool ItemEvent(SAPbouiCOM.ItemEvent infoEvento)
        {

            switch (infoEvento.EventType)
            {
                case BoEventTypes.et_FORM_LOAD:

                    if (infoEvento.BeforeAction == false)
                    {
                        SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);

                        #region  Creo el combo con los bancos 
                        SAPbouiCOM.Item oItem;
                        oItem = (SAPbouiCOM.Item)oForm.Items.Add("cmbConf", BoFormItemTypes.it_COMBO_BOX);
                        oItem.Width = oForm.Items.Item("1320002144").Width;
                        oItem.Top = oForm.Items.Item("144").Top;
                        oItem.Left = oForm.Items.Item("1250002053").Left + oForm.Items.Item("1250002053").Width + 200;
                        oItem.Height = oForm.Items.Item("144").Height;
                        oItem.FromPane = 5;
                        oItem.DisplayDesc = true;
                        oItem.ToPane = 5;
                        oItem.LinkTo = "144";
                        //((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("0049", "Santander");
                        //((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("2100", "La Caixa");
                        //((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("0128", "Bankinter");
                        //((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("0182", "BBVA");
                        //((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("0019", "Deutsche Bank");
                        //((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("0081", "Banco de Sabadell");
                        //((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("2080", "Abanca");
                        //((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("3191", "Bantierra");
                        //((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("2095", "Kutxa Bank");
                        //((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("0075", "Banco Popular");
                        ((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("2085", "Ibercaja");
                        //((SAPbouiCOM.ComboBox)oItem.Specific).ValidValues.Add("3035", "Caja Laboral");

                        oItem = (SAPbouiCOM.Item)oForm.Items.Add("lblConf", BoFormItemTypes.it_STATIC);
                        oItem.Width = 70;
                        oItem.Top = oForm.Items.Item("cmbConf").Top;
                        oItem.Left = oForm.Items.Item("cmbConf").Left - 75;
                        oItem.Height = oForm.Items.Item("cmbConf").Height;
                        oItem.FromPane = 5;
                        oItem.ToPane = 5;
                        oItem.LinkTo = "cmbConf";

                        ((SAPbouiCOM.StaticText)oItem.Specific).Caption = "Confirming";


                        //Boton                        
                        oItem = (SAPbouiCOM.Item)oForm.Items.Add("btnConf", BoFormItemTypes.it_BUTTON);
                        oItem.Width = oForm.Items.Item("cmbConf").Width;
                        oItem.Top = oForm.Items.Item("cmbConf").Top;
                        oItem.Left = oForm.Items.Item("cmbConf").Left + oForm.Items.Item("cmbConf").Width + 5;
                        oItem.Height = oForm.Items.Item("cmbConf").Height;
                        oItem.FromPane = oForm.Items.Item("cmbConf").FromPane;
                        oItem.ToPane = oForm.Items.Item("cmbConf").ToPane;
                        oItem.LinkTo = "cmbConf";
                        ((SAPbouiCOM.Button)oItem.Specific).Caption = "Generar Fichero";
                        #endregion
                    }
                    break;

                case BoEventTypes.et_ITEM_PRESSED:

                    #region Desmarcada por defecto la casilla de asientos manuales
                    if (infoEvento.ItemUID == "4" && !infoEvento.BeforeAction && infoEvento.ActionSuccess)
                    {

                        SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);
                        if (oForm.PaneLevel == 3 && ((SAPbouiCOM.OptionBtn)oForm.Items.Item("131").Specific).Selected)
                        {
                            ((SAPbouiCOM.CheckBox)oForm.Items.Item("480002043").Specific).Checked = false;
                        }


                        if (oForm.PaneLevel == 5)
                        {
                            SAPbouiCOM.Matrix oMatBancos = (SAPbouiCOM.Matrix)oForm.Items.Item("58").Specific;
                            string cBanco = "";
                            for (int j = 0; j <= oMatBancos.VisualRowCount; j++)
                            {
                                if (((SAPbouiCOM.CheckBox)oMatBancos.GetCellSpecific("1", j)).Checked)
                                {
                                    cBanco = ((SAPbouiCOM.EditText)oMatBancos.GetCellSpecific("1250000017", j)).Value;
                                    break;
                                }
                            }


                            try
                            {
                                ((SAPbouiCOM.ComboBox)oForm.Items.Item("cmbConf").Specific).Select(cBanco, BoSearchKey.psk_ByValue);
                            }
                            catch
                            {
                                ((SAPbouiCOM.ComboBox)oForm.Items.Item("cmbConf").Specific).Select(0, BoSearchKey.psk_Index);
                            }

                        }

                    }
                    #endregion


                    if (infoEvento.ItemUID == "btnConf" && !infoEvento.BeforeAction)
                    {
                        SAPbouiCOM.Form oForm = Matriz.gen.SBOApp.Forms.GetForm(infoEvento.FormTypeEx, infoEvento.FormTypeCount);
                        string cSeleccionado = ((SAPbouiCOM.ComboBox)oForm.Items.Item("cmbConf").Specific).Selected.Value;


                        switch (cSeleccionado)
                        {
                            ////case "0049": //Santander
                            ////    ModelosYNormas.GeneroConfirmingSantander(oForm);
                            ////    break;

                            //case "2100": //La caixa
                            //    ModelosYNormas.GeneroConfirmingCaixa(oForm);
                            //    break;
                            //case "0128": //Bankinter
                            //    ModelosYNormas.GeneroConfirmingBankinter(oForm);
                            //    break;
                            //case "0182": //BBVA
                            //    ModelosYNormas.GeneroConfirmingBBVA(oForm);
                            //    break;
                            //case "0019": //Deutsche bank
                            //    ModelosYNormas.GeneroConfirmingDeutsche(oForm);
                            //    break;
                            //case "0081": //Sabadell
                            //    ModelosYNormas.GeneroConfirmingSabadell(oForm);
                            //    break;
                            //case "2080": //Abanca
                            //    ModelosYNormas.GeneroConfirmingAbanca(oForm);
                            //    break;
                            //case "3191": //Bantierra
                            //    ModelosYNormas.GeneroConfirmingBantierra(oForm);
                            //    break;
                            //case "2095": //Kutxabank
                            //    ModelosYNormas.GeneroConfirmingKutxa(oForm);
                            //    break;
                            //case "0075": //Popular
                            //    ModelosYNormas.GeneroConfirmingPopular(oForm);
                            //    break;
                            case "2085": //Ibercaja
                                ModelosYNormas.GeneroConfirmingIbercaja(oForm);
                                break;
                            //case "3035": //Caja Laboral
                            //    ModelosYNormas.GeneroConfirmingCajaLaboral(oForm);
                            //    break;
                        }

                    }
                    break;

            }

            return true;
        }

    }
}
