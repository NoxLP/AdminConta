using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace AdConta
{
    public class Date
    {
        public Date(DateTime dateT)
        {
            this._Year = dateT.Year;
            this._Month = dateT.Month;
            this._Day = dateT.Day;
        }
        public Date(int year, int month, int day)
        {
            this._Year = year;

            if (month < 1 || month > 12)
                throw new Exception("Invalid month creating Date object");
            this._Month = month;

            if(day < 1 || day > DateTime.DaysInMonth(year, month))
                throw new Exception("Invalid day creating Date object");
            this._Day = day;
        }
        public Date(int month, int day)
        {
            this._Year = DateTime.Today.Year;

            if (month < 1 || month > 12)
                throw new Exception("Invalid month creating Date object");
            this._Month = month;

            if (day < 1 || day > DateTime.DaysInMonth(this._Year, month))
                throw new Exception("Invalid day creating Date object");
            this._Day = day;
        }
        public Date(int day)
        {
            this._Year = DateTime.Today.Year;
            this._Month = DateTime.Today.Month;

            if (day < 1 || day > DateTime.DaysInMonth(this._Year, this._Month))
                throw new Exception("Invalid day creating Date object");
            this._Day = day;
        }
        public Date() { }

        #region fields
        private int _Year;
        private int _Month;
        private int _Day;
        #endregion

        #region properties
        public int Year
        {
            get { return this._Year; }
            set { this._Year = value; }
        }
        public int Month
        {
            get { return this._Month; }
            set { this._Month = value; }
        }
        public int Day
        {
            get { return this._Day; }
            set { this._Day = value; }
        }

        public static Date Today
        {
            get
            {
                DateTime t = DateTime.Today;
                return new Date(t);
            }
        }
        #endregion

        #region public methods
        public DateTime GetDateTime()
        {
            return new DateTime(this.Year, this.Month, this.Day);
        }
        public void CopyDateTime(DateTime dateT)
        {
            this.Year = dateT.Year;
            this.Month = dateT.Month;
            this.Day = dateT.Day;
        }
        public override string ToString()
        {
            string paddedDay = this.Day.ToString().PadLeft(2);
            string paddedMonth = this.Month.ToString().PadLeft(2);
            return string.Format("{0}/{1}/{2}", paddedDay, paddedMonth, this.Year);
        }
        public string ToString(string param)
        {
            switch(param)
            {
                case "d":
                    return this.ToString();
                case "D":
                    return string.Format("{0} de {1} del {3}", 
                        this.Day, 
                        CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(this.Month),
                        this.Year);
                default:
                    return this.GetDateTime().ToString();
            }
        }
        public void AddDays(int days)
        {
            DateTime dateT = new DateTime(this.Year, this.Month, this.Day);
            dateT.AddDays(days);
            CopyDateTime(dateT);
        }
        public void AddMonths(int months)
        {
            DateTime dateT = new DateTime(this.Year, this.Month, this.Day);
            dateT.AddMonths(months);
            CopyDateTime(dateT);
        }
        public void AddYears(int years)
        {
            DateTime dateT = new DateTime(this.Year, this.Month, this.Day);
            dateT.AddYears(years);
            CopyDateTime(dateT);
        }
        public ReadOnlyDate AsReadOnly()
        {
            return new ReadOnlyDate(this);
        }
        #endregion

        #region operators
        public static bool operator <(Date dateL, Date dateR)
        {
            if (dateL.Year < dateR.Year) return true;
            else if (dateL.Year > dateR.Year) return false;

            if (dateL.Month < dateL.Month) return true;
            else if (dateL.Month > dateL.Month) return false;

            if (dateL.Day < dateL.Day) return true;
            else if (dateL.Day > dateL.Day) return false;

            return false;
        }
        public static bool operator >(Date dateL, Date dateR)
        {
            if (dateL.Year < dateR.Year) return true;
            else if (dateL.Year > dateR.Year) return false;

            if (dateL.Month < dateL.Month) return true;
            else if (dateL.Month > dateL.Month) return false;

            if (dateL.Day < dateL.Day) return true;
            else if (dateL.Day > dateL.Day) return false;

            return false;
        }
        public static bool operator ==(Date dateL, Date dateR)
        {
            if (dateL.Year == dateR.Year && dateL.Month == dateR.Month && dateL.Day == dateR.Day)
                return true;
            return false;
        }
        public static bool operator !=(Date dateL, Date dateR)
        {
            if (!(dateL == dateR)) return true;
            return false;
        }
        public static bool operator <=(Date dateL, Date dateR)
        {
            if (dateL < dateR || dateL == dateR) return true;

            return false;
        }
        public static bool operator >=(Date dateL, Date dateR)
        {
            if (dateL > dateR || dateL == dateR) return true;

            return false;
        }
        #endregion

        public static implicit operator Date (DateTime dt)
        {
            Date d = new Date(dt);
            return d;
        }
        public static implicit operator DateTime(Date d)
        {
            DateTime dt = d.GetDateTime();
            return dt;
        }

        public override bool Equals(object obj)
        {
            if (obj is Date) return (Date)obj == this;
            else return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class ReadOnlyDate
    {
        public ReadOnlyDate(Date date)
        {
            this._Year = date.Year;
            this._Month = date.Month;
            this._Day = date.Day;
        }

        #region fields
        private int _Year;
        private int _Month;
        private int _Day;
        #endregion

        #region properties
        public int Year { get { return this._Year; } }
        public int Month { get { return this._Month; } }
        public int Day { get { return this._Day; } }
        #endregion

        #region public methods
        public DateTime GetDateTime()
        {
            return new DateTime(this.Year, this.Month, this.Day);
        }
        public Date GetDate()
        {
            return new Date(this.Year, this.Month, this.Day);
        }
        public override string ToString()
        {
            string paddedDay = this.Day.ToString().PadLeft(2);
            string paddedMonth = this.Month.ToString().PadLeft(2);
            return string.Format("{0}/{1}/{2}", paddedDay, paddedMonth, this.Year);
        }
        public string ToString(string param)
        {
            switch (param)
            {
                case "d":
                    return this.ToString();
                case "D":
                    return string.Format("{0} de {1} del {3}",
                        this.Day,
                        CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(this.Month),
                        this.Year);
                default:
                    return this.GetDateTime().ToString();
            }
        }
        #endregion
    }

}
