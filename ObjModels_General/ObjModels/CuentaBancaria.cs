using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdConta.Models
{
    #region bank account class
    public class CuentaBancaria
    {
        public CuentaBancaria() { }
        public CuentaBancaria(string cuenta)
        {
            AccountNumber = cuenta;
        }

        #region fields
        private string _account;
        private readonly int[] _InitIndex = new int[] { 0, 4, 8, 12, 14 };
        private readonly int[] _Count = new int[] { 4, 4, 4, 2, 10 };
        /*switch (part)
            {
                case AccountPart.Bank:
                    i = 4;
                    n = 4;
                    break;
                case AccountPart.Office:
                    i = 8;
                    n = 4;
                    break;
                case AccountPart.DC:
                    i = 12;
                    n = 2;
                    break;
                case AccountPart.Account:
                    i = 14;
                    n = 10;
                    break;
            }*/
        #endregion

        #region properties
        public string AccountNumber
        {
            get { return this._account; }
            set
            {
                if (value == this._account)
                    return;

                string acc = "";

                if (IsAnAccount(value, out acc))
                    this._account = acc;
                else
                    throw new CustomException_ObjModels(string.Format("La cuenta \"{0}\" no es una cuenta de banco correcta y no se ha podido guardar.",
                        value));
                /*throw new Exception(string.Format("String \"{0}\" is NOT an account number, can not set it as BankAccount.AccountNumber property",
                    value));*/
            }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Get account part of bank account
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public int GetAccountPart(AccountPart part)
        {
            if (part == AccountPart.IBAN) throw new Exception("Can not get IBAN as an integer value. Use AccountString.GetIBAN instead");

            return int.Parse(this.AccountNumber.Substring(this._InitIndex[(int)part], this._Count[(int)part]));
        }
        /// <summary>
        /// Get IBAN of bank account
        /// </summary>
        /// <returns></returns>
        public string GetIBAN()
        {
            return this.AccountNumber.Substring(this._InitIndex[(int)AccountPart.IBAN], this._Count[(int)AccountPart.IBAN]);
        }
        /// <summary>
        /// Set bank account part by enum IF final string is a correct IBAN bank account. Returns if final string is a correct IBAN bank account.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetNewAccountByPart(AccountPart part, int value)
        {
            if (part == AccountPart.IBAN)
                throw new Exception("Can not set IBAN as an integer value. Use AccountString.SetNewAccountIBAN instead");
            /*if (this.GetAccountPart(part) == value)
                return true;*/

            string newStr = string.Copy(this.AccountNumber);
            newStr.Remove(this._InitIndex[(int)part], this._Count[(int)part]);
            newStr.Insert(this._InitIndex[(int)part], value.ToString());

            if (IsAnAccount(newStr, out newStr))
            {
                this.AccountNumber = newStr;
                return true;
            }
            else return false;
        }
        /// <summary>
        /// Set IBAN part of bank account IF final string is a correct IBAN bank account. Returns if final string is a correct IBAN bank account.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetNewAccountIBAN(string value)
        {
            /*if (this.GetIBAN() == value)
                return true;*/

            string newStr = string.Copy(this.AccountNumber);
            newStr.Remove(this._InitIndex[(int)AccountPart.IBAN], this._Count[(int)AccountPart.IBAN]);
            newStr.Insert(this._InitIndex[(int)AccountPart.IBAN], value);

            if (IsAnAccount(newStr, out newStr))
            {
                this.AccountNumber = newStr;
                return true;
            }
            else return false;
        }
        /// <summary>
        /// Get if string bankAccount is a correct IBAN bank account, and set cleanAccount to a trimmed(and clean of typical separators
        /// '/' and '-') version of bankAccount.
        /// Both parameters can be any account strings, not necessarily the one of BankAccount class.
        /// </summary>
        /// <param name="bankAccount"></param>
        /// <param name="cleanAccount"></param>
        /// <returns></returns>
        public bool IsAnAccount(string bankAccount, out string cleanAccount) //http://www.codeproject.com/Tips/775696/IBAN-Validator modified
        {
            bankAccount = bankAccount.ToUpper(); //IN ORDER TO COPE WITH THE REGEX BELOW
            if (String.IsNullOrEmpty(bankAccount))
            {
                cleanAccount = string.Empty;
                return false;
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(bankAccount, "^[A-Z0-9]"))
            {
                bankAccount = bankAccount.Replace(" ", String.Empty).Replace("-", String.Empty).Replace("/", String.Empty);
                cleanAccount = bankAccount;

                string bank = bankAccount.Substring(4, bankAccount.Length - 4) + bankAccount.Substring(0, 4);
                int asciiShift = 55;
                StringBuilder sb = new StringBuilder();
                foreach (char c in bank)
                {
                    int v;
                    if (Char.IsLetter(c)) v = c - asciiShift;
                    else v = int.Parse(c.ToString()); //tryparse
                    sb.Append(v);
                }
                string checkSumString = sb.ToString();
                int checksum = int.Parse(checkSumString.Substring(0, 1));
                for (int i = 1; i < checkSumString.Length; i++)
                {
                    int v = int.Parse(checkSumString.Substring(i, 1));
                    checksum *= 10;
                    checksum += v;
                    checksum %= 97;
                }
                return checksum == 1;
            }
            else
            {
                cleanAccount = string.Empty;
                return false;
            }
        }
        #endregion
    }
    #endregion
}

