namespace Repository
{
    public interface IAppRepositories
    {
        #region general
        PersonaRepository PersonaRepo { get; }
        ComunidadRepository ComunidadRepo { get; }
        EjercicioRepository EjercicioRepo { get; }
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
