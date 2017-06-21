using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuloContabilidad.ObjModels
{
    public struct sGrupoContable
    {
        public int Digits { get; private set; }
        /// <summary>
        /// Este sirve tanto con el número de cuenta completo como con el número reducido del grupo (Ej.: 410)
        /// </summary>
        /// <param name="accountNumber"></param>
        public sGrupoContable(string accountNumber)
        {
            this.Digits = int.Parse(accountNumber.Substring(0, 1)) * 100;
        }
        public sGrupoContable(int accountNumber)
        {
            //Get total default digits
            int digits = GlobalSettings.Properties.Settings.Default.DIGITOSCUENTAS - 1;
            //Get first digit
            this.Digits = (int)Math.Truncate(accountNumber / Math.Pow(10, digits)) * 100;
        }

        public void SetGrupoByAccNumber(string accountNumber)
        {
            this.Digits = int.Parse(accountNumber.Substring(0, 1)) * 100;
        }
        public void SetGrupoByAccNumber(int accountNumber)
        {
            //Get total default digits
            int digits = GlobalSettings.Properties.Settings.Default.DIGITOSCUENTAS - 1;
            //Get first digit
            this.Digits = (int)Math.Truncate(accountNumber / Math.Pow(10, digits)) * 100;
        }

        /// <summary>
        /// Carefull, this method don't check if the string provided is a correct ledge account number
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int GetGrupoDigitsFromString(ref string s)
        {
            return int.Parse(s.Substring(0, 1)) * 100;
        }
    }
    public struct sSubgrupoContable
    {
        public int Digits { get; private set; }

        /// <summary>
        /// Este sirve tanto con el número de cuenta completo como con el número reducido del grupo (Ej.: 410 - ¡¡OJO!! minimo TRES digitos)
        /// </summary>
        /// <param name="accountNumber"></param>
        public sSubgrupoContable(string accountNumber)
        {
            this.Digits = int.Parse(accountNumber.Substring(1, 2));
        }
        public sSubgrupoContable(int accountNumber)
        {
            //Get total default digits
            int digits = GlobalSettings.Properties.Settings.Default.DIGITOSCUENTAS - 1;
            //Get second and third digit
            this.Digits = (int)Math.Truncate(accountNumber / Math.Pow(10, digits - 2)) % 100;
        }

        public void SetSubgrupoByAccNumber(string accountNumber)
        {
            this.Digits = int.Parse(accountNumber.Substring(1, 2));
        }
        public void SetSubgrupoByAccNumber(int accountNumber)
        {
            //Get total default digits
            int digits = GlobalSettings.Properties.Settings.Default.DIGITOSCUENTAS - 1;
            //Get second and third digit
            this.Digits = (int)Math.Truncate(accountNumber / Math.Pow(10, digits - 2)) % 100;
        }

        /// <summary>
        /// Carefull, this method don't check if the string provided is a correct ledge account number
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int GetSubgrupoDigitsFromString(ref string s)
        {
            return int.Parse(s.Substring(1, 2));
        }
    }
}
