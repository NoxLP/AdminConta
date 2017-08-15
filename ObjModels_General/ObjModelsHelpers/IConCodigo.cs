using System;

namespace AdConta.Models
{
    /// <summary>
    /// Interfaz para todos los objetos con auto código.
    /// </summary>
    public interface IConCodigo { aAutoCodigoBase Codigo { get; } }
    /// <summary>
    /// Interfaz únicamente para el tipo Comunidad. Con código, sin idOwners.
    /// </summary>
    public interface IObjModelConCodigo : IConCodigo { }
    /// <summary>
    /// Interfaz para los tipos que tengan auto código Y IdOwnerComunidad.
    /// </summary>
    public interface IObjModelConCodigoConComunidad : IOwnerComunidad, IObjModelConCodigo { int GetOwnerId(); }
    /// <summary>
    /// Interfaz para los objetos que tengan auto código, IdOwnerComunidad e IdOwnerEjercicio.
    /// </summary>
    public interface IObjModelConCodigoConComunidadYEjercicio : IConCodigo, IOwnerComunidad, IOwnerEjercicio { Tuple<int, int> GetOwnersIds(); }
}
