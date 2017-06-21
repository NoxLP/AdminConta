using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace AdConta.Models
{
    public class Concepto : IObjModelBase
    {
        public Concepto(int id)
        {
            this.Id = id;
            this.Nombre = "";
            this.NombreReducido = "";
        }
        public Concepto(int id, string nombre, string nombreReducido)
        {
            this.Id = id;
            this.Nombre = nombre;
            this.NombreReducido = nombreReducido;
        }

        public int Id { get; private set; }
        public string Nombre { get; set; }
        public string NombreReducido { get; set; }
    }
}
