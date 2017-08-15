using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdConta.Models
{
    public interface IOwnerComunidad
    {
        int IdOwnerComunidad { get; }
    }

    public interface IOwnerComunidadNullable
    {
        int? IdOwnerComunidad { get; }
    }

    public interface IOwnerEjercicio
    {
        int IdOwnerEjercicio { get; }
    }

    public interface IOwnerPersona
    {
        int IdOwnerPersona { get; }
    }

    public interface IOwnerFinca
    {
        int IdOwnerFinca { get; }
    }

    public interface IOwnerProveedor
    {
        int? IdOwnerProveedor { get; }
    }

    public interface IOwnerPropietario
    {
        int IdOwnerPropietario { get; }
    }

    public interface IOwnerCuota
    {
        int IdOwnerCuota { get; }
    }

    public interface IOwnerRecibo
    {
        int IdOwnerRecibo { get; }
    }

    public interface IOwnerFactura
    {
        int? IdOwnerFactura { get; }
    }

    public interface IOwnerDevolucion
    {
        int IdOwnerDevolucion { get; }
    }

    public interface IOwnerPresupuesto
    {
        int IdOwnerPresupuesto { get; }
    }

    public interface IOwnerGrupoGasto
    {
        int IdOwnerGrupoGasto { get; }
    }
}
