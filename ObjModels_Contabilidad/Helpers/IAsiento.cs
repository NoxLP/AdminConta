using AdConta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuloContabilidad.ObjModels
{
    public interface IAsiento : IOwnerComunidad, IObjModelBase
    {
        ObservableApuntesList Apuntes { get; }
        DateTime FechaValor { get; }
        decimal Saldo { get; }
        Apunte this[int i] { get; }
        bool Abierto { get; }

        void CalculaSaldo();
        void SetApuntesList(IEnumerable<Apunte> apuntes);
    }
}
