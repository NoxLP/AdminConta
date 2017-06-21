using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AdConta.Models;
using ModuloContabilidad.ObjModels;
using ModuloGestion.ObjModels;

namespace Repository
{
    public interface iAppRepositories
    {
        #region general
        PersonaRepository PersonaRepo { get; }
        ComunidadRepository ComunidadRepo { get; }
        #endregion

        #region contabilidad
        CuentaMayorRepository CuentaMayorRepo { get; }
        AsientoRepository AsientoRepo { get; }
        ApunteRepository ApunteRepo { get; }
        #endregion

        #region gestion
        PropietarioRepository PropietarioRepo { get; }
        #endregion
    }
}
