using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using SAPbobsCOM;
using SAPbouiCOM;
using System.Reflection;
using Microsoft.VisualBasic.CompilerServices;

namespace Cliente
{
//    public class VarGlobal
//    {
//        public static int SumDec;
//        public static int PriceDec;
//        public static int RateDec;
//        public static int QtyDec;
//        public static int PercentDec;
//        public static int MeasureDec;
//        public static string SepMill;
//        public static string SepDec;       
//    }


    public class Conversiones
    {

      public static double ValueSAPToDoubleSistema(string Texto)
            {
                 if (Texto == "") Texto = "0";

                string Cadena = Texto;

                double Valor = 0.0;
                System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
                string SepDecSistema = nfi.NumberGroupSeparator;
                //string SepMilSistema = nfi.NumberDecimalSeparator;

                //En pantalla el separador decimal es .
                if (SepDecSistema != ".")
                {
                    Cadena = Cadena.Replace('.', ',');
                }
                double.TryParse(Cadena, out Valor);
                return Valor;
            }
      
      public static double StringSAPToDoubleSistema(string Texto)
      {
          double nRetorno = 0;
          if (Texto == "") Texto = "0";

          //Quito la moneda y el sep miles
          string Cadena = Texto;
          Cadena = Cadena.Replace(Matriz.SepMill, "");

          System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
          string SepDecSistema = nfi.NumberGroupSeparator;
          Cadena = Cadena.Replace(Matriz.SepDec, SepDecSistema);
          double.TryParse(Cadena, out nRetorno);          

          return nRetorno;
      }

      public static double StringSAPToDoubleSistema(string Texto, string cMoneda)
      {
          double nRetorno = 0;          
          string  Cadena = Texto.Replace((cMoneda != "") ? cMoneda : "EUR", ""); 
          
          nRetorno = StringSAPToDoubleSistema(Cadena);

          return nRetorno;
      }

      public static string DoubleStringSAP(double Valor, BoFldSubTypes BoTipo)
            {

                string cRetorno = "";

                switch (BoTipo)
                {
                    case BoFldSubTypes.st_Quantity:
                        Valor = Math.Round(Valor, Matriz.QtyDec);
                        break;
                    case BoFldSubTypes.st_Sum:
                        Valor = Math.Round(Valor, Matriz.SumDec);
                        break;
                    case BoFldSubTypes.st_Percentage:
                        Valor = Math.Round(Valor, Matriz.PercentDec);
                        break;
                    case BoFldSubTypes.st_Price:
                        Valor = Math.Round(Valor, Matriz.PriceDec);
                        break;
                    case BoFldSubTypes.st_Measurement:
                        Valor = Math.Round(Valor, Matriz.MeasureDec);
                        break;
                    case BoFldSubTypes.st_Rate:
                        Valor = Math.Round(Valor, Matriz.RateDec);
                        break;
                    default:
                        Valor = Math.Round(Valor, 2);
                        break;
                }

                cRetorno = Valor.ToString();
                cRetorno = cRetorno.Replace(',', '.');
                return cRetorno;

                //string cRetorno = "";
                //string cAux = Valor.ToString();

                //cRetorno = cAux.Replace(',', csVariablesGlobales.cSepDecimal);

                ////System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();

                ////string hh = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                ////string hh1 = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

                ////string SepDecSAP = DevuelveValor("OADM", "DecSep", "");
                ////string SepMilSAP = DevuelveValor("OADM", "ThousSep", ""); 
                ////string SepDecSistema = nfi.NumberGroupSeparator;
                ////string SepMilSistema = nfi.NumberDecimalSeparator;
                ////string ValorDevuelto = Valor.ToString();
                ////if (SepDecSAP != SepDecSistema)
                ////{
                ////    ValorDevuelto = ValorDevuelto.Replace(SepDecSistema, SepDecSAP);
                ////}


            }

      public static string DateStringSAP(DateTime dFecha)
      {
          string cRetorno = dFecha.Year.ToString("0000") + dFecha.Month.ToString("00") + dFecha.Day.ToString("00");

          return cRetorno;
      }

      public static DateTime StringSAPDate(string cTexto)
      {
         DateTime dRetorno = Matriz.gen.funciones.ReturnISOtoDate(cTexto);

         return dRetorno;
      }

    }
      

    public class Utilidades
    {
        //public static string LeoQueryFich(string cNomFichLargo)
        //{
        //    //string cFich = System.IO.Path.Combine(System.IO.Path.Combine(EXO_BasicDLL.EXO_Globales.PathServer, "09.Consultas"), "InfSigre1.sql");            
        //    string sql = "", cAux = "";
        //    System.IO.StreamReader Fichero = new System.IO.StreamReader(cNomFichLargo);
        //    while (Fichero.Peek() != -1)
        //    {
        //        cAux = Fichero.ReadLine();
        //        if (cAux.Length > 2 && cAux.Substring(0, 2) == "--") continue;

        //        sql += cAux.Replace("\t", " ") + " ";
        //    }
        //    Fichero.Close();

        //    return sql;
        //}

        public static string LeoFichEmbebido(string cFichEmbebido)
        {
            string result = "";
            try
            {
                Type tipo = Matriz.TypeMatriz;
                Assembly assembly = tipo.Assembly;
                StreamReader streamReader = new StreamReader(tipo.Assembly.GetManifestResourceStream(tipo.Namespace + "." + cFichEmbebido));
                result = streamReader.ReadToEnd();
                result = result.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ");
                streamReader.Close();
            }
            catch (Exception expr_40)
            {
                ProjectData.SetProjectError(expr_40);
                ProjectData.ClearProjectError();
            }

            return result;
        }

        public static string LeoQueryFich(string cFichEmbebido)
        {
            string result = "";
            try
            {
                Type tipo = Matriz.TypeMatriz;
                Assembly assembly = tipo.Assembly;
                StreamReader streamReader = new StreamReader(tipo.Assembly.GetManifestResourceStream(tipo.Namespace + "." + cFichEmbebido));
                result = streamReader.ReadToEnd();
                result = result.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ");
                streamReader.Close();
            }
            catch (Exception expr_40)
            {
              //  ProjectData.SetProjectError(expr_40);
               // ProjectData.ClearProjectError();
            }

            return result;


            //string cQuery = Matriz.oGlobal.Functions.leerEmbebido(ref Tipo, cNomQueryIncrustada);
            //cQuery = cQuery.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ");

            //return cQuery;
        }
        
    }


    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        private IntPtr _hwnd;

        // Property
        public virtual IntPtr Handle
        {
            get { return _hwnd; }
        }

        // Constructor
        public WindowWrapper(IntPtr handle)
        {
            _hwnd = handle;
        }
    }

    public class TratamientoFicheros
    {
        public static string EscojoFichero(string cCadenaDefect)
        {
            string Ruta = "";

            EXO_SaveFileDialog oFichero = new EXO_SaveFileDialog();
            oFichero.Filter = "All Files (*)|*|Dat (*.dat)|*.dat|Text Files (*.txt)|*.txt";
            oFichero.FileName = cCadenaDefect;
            string DirectorioActual = Environment.CurrentDirectory;
            Thread threadGetFile = new Thread(new ThreadStart(oFichero.GetFileName));
            threadGetFile.TrySetApartmentState(ApartmentState.STA);
            threadGetFile.Start();
            try
            {
                while (!threadGetFile.IsAlive) ; // Wait for thread to get started
                Thread.Sleep(1);  // Wait a sec more
                threadGetFile.Join();    // Wait for thread to end

                Ruta = oFichero.FileName;
            }
            catch (Exception ex)
            {
                Matriz.gen.SBOApp.MessageBox(ex.Message, 1, "OK", "", "");
            }
            threadGetFile = null;
            oFichero = null;

            return Ruta;
        }

        public static string SeleccionoFichero(string cCadenaDefect)
        {
            string cRetorno = "";

            EXO_OpenFileDialog OpenFileDialog = new EXO_OpenFileDialog();
            OpenFileDialog.Filter = "Todos los ficheros|*.*";
            OpenFileDialog.InitialDirectory = "";
            Thread threadGetFile = new Thread(new ThreadStart(OpenFileDialog.GetFileName));
            threadGetFile.TrySetApartmentState(ApartmentState.STA);
            try
            {

                threadGetFile.Start();
                while (!threadGetFile.IsAlive) ; // Wait for thread to get started
                Thread.Sleep(1);  // Wait a sec more
                threadGetFile.Join();    // Wait for thread to end

                // Use file name as you will here
                cRetorno = OpenFileDialog.FileName;
                threadGetFile.Abort();
                threadGetFile = null;
                OpenFileDialog.InitialDirectory = "";
                OpenFileDialog = null;
            }
            catch (Exception ex)
            {
                Matriz.gen.SBOApp.MessageBox(ex.Message, 1, "OK", "", "");
                threadGetFile.Abort();
                threadGetFile = null;
                OpenFileDialog.InitialDirectory = "";
                OpenFileDialog = null;

            }

            return cRetorno;
        }

        public static bool IsDirectoryWritable(string dirPath, bool throwIfFails = false)
        {
            try
            {
                using (FileStream fs = File.Create(Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose)
                )
                { }
                return true;
            }
            catch
            {
                if (throwIfFails)
                    throw;
                else
                    return false;
            }
        }

    }



}
