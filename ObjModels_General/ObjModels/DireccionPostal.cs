using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdConta.Models
{
    public class DireccionPostal
    {
        public string TipoVia { get; set; }
        public string Direccion { get; set; }
        public int CP { get; set; }
        public string Localidad { get; set; }
        public string Provincia { get; set; }

        public DireccionPostal(string tipoVia, string direccion, int cp, string localidad, string provincia)
        {
            this.TipoVia = tipoVia;
            this.Direccion = direccion;
            this.CP = cp;
            this.Localidad = localidad;
            this.Provincia = provincia;
        }

        public string GetDireccionSinCP()
        {
            return string.Concat(TipoVia, " ", Direccion);
        }
    }

    public class DireccionPostalCompleta
    {
        public string TipoVia { get; set; }
        public string NombreVia { get; set; }
        public string NumeroVia { get; set; }
        public string Portal { get; set; }
        public string Piso { get; set; }
        public string Puerta { get; set; }
        public int CP { get; set; }
        public string Localidad { get; set; }
        public string Provincia { get; set; }

        public void GetDireccionPostalSimple(out DireccionPostal direccion)
        {
            direccion = new DireccionPostal(
                this.TipoVia,
                string.Format("{0}, {1}{2}{3}{4}", this.NombreVia, this.NumeroVia, this.Portal, this.Piso, this.Puerta),
                this.CP,
                this.Localidad,
                this.Provincia);
        }

        public string GetDireccionSinCP()
        {
            throw new NotImplementedException();
        }
    }
}
