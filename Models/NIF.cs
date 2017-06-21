using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdConta.Models
{
    public class NIFModel
    {
        public NIFModel(string nif)
        {
            string newnif = nif.Trim().ToUpper();

            SetTipoNif(ref this._tipoNIF, ref newnif);
            this._IsValid = IsValidNIF(ref this._tipoNIF, ref newnif, ref this._DC);

            if (this._IsValid) this._NIF = newnif;
            else this._NIF = null;
        }

        #region fields
        private TipoNIF _tipoNIF;
        private string _NIF = "";
        private bool _IsValid = false;
        private string _DC = "";
        #endregion

        #region properties
        public string NIF
        {
            get { return this._NIF; }
            set
            {
                TipoNIF tipo = new TipoNIF();
                string dc = "";
                string nif = value.Trim().ToUpper();
                SetTipoNif(ref tipo, ref nif);

                if (IsValidNIF(ref tipo, ref nif, ref dc))
                {
                    this._NIF = nif;
                    this._DC = dc;
                    this._IsValid = true;
                }
            }
        }
        public bool IsValid { get { return this._IsValid; } }
        public string DC { get { return this._DC; } }
        public string InvalidMessage { get; set; }
        #endregion

        #region helpers
        private void SetTipoNif(ref TipoNIF tipo, ref string nif)
        {
            char firstChar = nif.Substring(0, 1).ToUpper().ToCharArray()[0];

            if (char.IsDigit(firstChar)) tipo = TipoNIF.DNI;
            //Si la primera letra es x, y ó z, es NIE
            else if (firstChar > 0x57 && firstChar < 0x5B) tipo = TipoNIF.NIE;
            else tipo = TipoNIF.CIF;
        }
        private bool IsValidNIF(ref TipoNIF tipo, ref string nif, ref string dc)
        {
            if (string.IsNullOrEmpty(nif))
            {
                this.InvalidMessage = "No se ha introducido ningún N.I.F.";
                dc = "";
                return false;
            }

            //Debe tener una longitud igual a 9 caracteres;
            if (nif.Length != 9)
            {
                //TODO: AÑADIR CEROS DELANTE
                this.InvalidMessage = "Un N.I.F. debe tener 9 caracteres en total incluyendo números y letras. ¿Quiere añadir céros hasta completar los 9 caracteres?";
                dc = "";
                return false;
            }

            switch (tipo)
            {
                case TipoNIF.DNI:
                    return IsValid_DNI(ref nif, ref dc);
                case TipoNIF.NIE:
                    return IsValid_NIE(ref nif, ref dc);
                case TipoNIF.CIF:
                    return IsValid_CIF(ref nif, ref dc);
                default:
                    return false;
            }
        }
        private bool IsValid_DNI(ref string nif, ref string dc)
        {
            int sincontrol;

            if(!int.TryParse(nif.Substring(0, 8), out sincontrol))
            {
                this.InvalidMessage = "Un D.N.I. no puede contener letras excepto en la última posición.";
                return false;
            }

            return CheckDNI_NIE_DC(ref nif, ref sincontrol, ref dc);
        }
        private bool IsValid_NIE(ref string nif, ref string dc)
        {
            int sincontrol;

            if (!int.TryParse(nif.Substring(1, 8), out sincontrol))
            {
                this.InvalidMessage = "Un N.I.E. no puede contener letras excepto en la primera y última posición.";
                return false;
            }

            string firstChar = nif.Substring(0, 1);
            switch (firstChar)
            {
                case "X":
                    break;
                case "Y":
                    sincontrol += 10000000;
                    break;
                case "Z":
                    sincontrol += 20000000;
                    break;
            }

            return CheckDNI_NIE_DC(ref nif, ref sincontrol, ref dc);
        }
        private bool CheckDNI_NIE_DC(ref string nif, ref int sincontrol, ref string dc)
        {
            string letras = "TRWAGMYFPDXBNJZSQVHLCKET";
            string letter = nif.Substring(8, 1);
            int modulo = sincontrol % 23;

            dc = letras[modulo].ToString();
            if (letter == dc)
            {
                this.InvalidMessage = null;
                return true;
            }
            else
            {
                this.InvalidMessage = string.Format("La letra introducida no es correcta, se ha calculado que la letra correcta es: {0}." +
                    " /n ¿Quiere usar esta letra?", letter);
                return false;
            }
        }
        //https://sites.google.com/site/lagaterainformatica/home/-net/-net-c-/-generico/-calculo-de-un-cif-con-c
        private bool IsValid_CIF(ref string nif, ref string dc)
        {               
            // ... y debe comenzar por una letra, la cual pasamos a mayúscula, ... 
            // 
            string firstChar = nif.Substring(0, 1);
            // ...que necesariamente deberá de estar comprendida en 
            // el siguiente intervalo: ABCDEFGHJNPQRSUVW 
            // 
            string cadena = "ABCDEFGHJNPQRSUVW";
            if (cadena.IndexOf(firstChar) == -1)
            {
                this.InvalidMessage = "La letra inicial no es correcta.";
                dc = "";
                return false;
            }
            try
            {
                this.InvalidMessage = null;
                Int32 sumaPar = default(Int32);
                Int32 sumaImpar = default(Int32);
                // A continuación, la cadena debe tener 7 dígitos + el dígito de control. 
                // 
                string _sinControl = nif.Substring(0, 8);
                string digits = _sinControl.Substring(1, 7);
                for (Int32 n = 0; n <= digits.Length - 1; n += 2)
                {
                    if (n < 6)
                    {
                        // Sumo las nifras pares del número que se corresponderá 
                        // con los caracteres 1, 3 y 5 de la variable «digits». 
                        // 
                        sumaPar += Convert.ToInt32(digits[n + 1].ToString());
                    }
                    // Multiplico por dos cada nifra impar (caracteres 0, 2, 4 y 6). 
                    // 
                    Int32 dobleImpar = 2 * Convert.ToInt32(digits[n].ToString());
                    // Acumulo la suma del doble de números impares. 
                    // 
                    sumaImpar += (dobleImpar % 10) + (dobleImpar / 10);
                }
                // Sumo las nifras pares e impares. 
                // 
                Int32 sumaTotal = sumaPar + sumaImpar;
                // Me quedo con la nifra de las unidades y se la resto a 10, siempre 
                // y cuando la nifra de las unidades sea distinta de cero 
                // 
                sumaTotal = (10 - (sumaTotal % 10)) % 10;
                // Devuelvo el Dígito de Control dependiendo del primer carácter 
                // del NIF pasado a la función. 
                //
                //string digitoControl = "";
                //En vez de eso seteo variable dc
                switch (firstChar)
                {
                    case "N":
                    case "P":
                    case "Q":
                    case "R":
                    case "S":
                    case "W":
                        // NIF de entidades cuyo dígito de control se corresponde 
                        // con una letra. 
                        // 
                        // Al estar los índices de los arrays en base cero, el primer 
                        // elemento del array se corresponderá con la unidad del número 
                        // 10, es decir, el número cero. 
                        // 
                        char[] characters = { 'J', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I' };
                        dc = characters[sumaTotal].ToString();
                        break;
                    default:
                        // NIF de las restantes entidades, cuyo dígito de control es un número. 
                        // 
                        dc = sumaTotal.ToString();
                        break;
                }

                if (!dc.Equals(nif.Substring(8, 1)))
                {
                    this.InvalidMessage = string.Format("El N.I.F. introducido no es correcto, " +
                        "el dígito de control (último carácter) correcto para el número introducido es: {0}. /n ¿Quiere sustituir este carácter?"
                        , dc.ToString());
                    return false;
                }
                else
                {
                    this.InvalidMessage = null;
                    return true;
                }
            }
            catch (Exception)
            {
                // Cualquier excepción producida, devolverá false.
                return false;
            }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Force the class to set an invalid NIF by explicit user request.
        /// </summary>
        /// <param name="NIF"></param>
        public void ForceInvalidNIF(ref string NIF)
        {
            this._NIF = NIF;
            this._IsValid = false;
            this._tipoNIF = TipoNIF.NULL;
            this._DC = null;
            this.InvalidMessage = "N.I.F. invalido forzado por el usuario.";
        }
        /// <summary>
        /// Add zeroes and validate nif. Usually called after a not enough length invalid message were recieved.
        /// </summary>
        /// <param name="nif"></param>
        /// <returns></returns>
        public bool AddZeroesAndValidateNIF(string nif)
        {
            if (string.IsNullOrEmpty(nif))
            {
                this.InvalidMessage = "No se ha introducido ningún N.I.F.";
                return false;
            }

            int digits;
            int totalDigits;
            string first;
            string second = nif.Substring(0);

            if (char.IsLetter(nif[0]))
            {
                digits = nif.Length - 2;
                totalDigits = 7;
                first = nif.Substring(0, 1);
                second = nif.Substring(1);
            }
            else
            {
                digits = nif.Length - 1;
                totalDigits = 8;
                first = "";
                second = nif.Substring(0);
            }

            int numberOfZeroes = totalDigits - digits;
            string zeroes = "";

            for (int i = 0; i < numberOfZeroes; i++)
                zeroes += "0";

            this.NIF = first + zeroes + second;
            return true;
        }
        /// <summary>
        /// Instead of setting the NIF number by the property, try to set it and return a string with an invalid message 
        /// if the number is invalid, and null if the number is valid.
        /// </summary>
        /// <param name="nif"></param>
        /// <returns>Invalid message if number is invalid, null if number is valid.</returns>
        public string TryModifyNIF(ref string nif)
        {
            TipoNIF tipo = new TipoNIF();
            string dc = "";
            string newNif = nif.Trim().ToUpper();
            SetTipoNif(ref tipo, ref newNif);

            if (IsValidNIF(ref tipo, ref newNif, ref dc))
            {
                this._NIF = nif;
                this._DC = dc;
                this._IsValid = true;
                return null;
            }
            else return this.InvalidMessage;
        }
        #endregion
    }

}
