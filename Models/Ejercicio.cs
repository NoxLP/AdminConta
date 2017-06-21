using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using AdConta;

namespace AdConta.Models
{
    public class Ejercicio : IObjModelBase, IOwnerComunidad, IObjWithDLO<EjercicioDLO>
    {
        #region constructors
        private Ejercicio() { }
        public Ejercicio(int id, Date fechaComienzo, Date fechaFinal, int idOwnerComunidad, bool cerrado = false)
        {
            if (id < 0 || idOwnerComunidad < 0) throw new CustomException_ObjModels("sEjercicio's Id and IdOwnerComunidad have to be > 0");
            else
            {
                this.Id = id;
                this.IdOwnerComunidad = idOwnerComunidad;
            }

            this.FechaComienzo = fechaComienzo;
            this.FechaFinal = fechaFinal;
            this.Cerrado = cerrado;
        }
        /*/// <summary>
        /// Usar este constructor para nuevos ejercicios.
        /// </summary>
        /// <param name="idCdad"></param>
        public Ejercicio(int idOwnerComunidad)
        {
            if (idOwnerComunidad < 0) throw new CustomException_ObjModels("sEjercicio's IdOwnerComunidad have to be > 0");
            else this.IdOwnerComunidad = idOwnerComunidad;

            this.Id = -1;
            this.FechaComienzo = new Date();
            this.FechaFinal = new Date();
            this.Cerrado = false;
        }*/
        #endregion

        #region properties
        public int Id { get; private set; }
        public Date FechaComienzo { get; private set; }
        public Date FechaFinal { get; private set; }
        //TODO: ¡¡¡OJO!!! ESTE OWNER NO SE SUMA AL NOMBRE DE LA TABLA
        public int IdOwnerComunidad { get; private set; }
        public bool Cerrado { get; private set; }
        #endregion

        #region public methods
        public bool Contains(Date date)
        {
            if (date > this.FechaComienzo && date < this.FechaFinal)
                return true;

            return false;
        }
        #endregion

        #region DLO
        public EjercicioDLO GetDLO()
        {
            return new EjercicioDLO(
                this.Id,
                this.FechaComienzo,
                this.FechaFinal,
                this.IdOwnerComunidad,
                this.Cerrado);
        }
        #endregion
    }

    public class EjercicioDLO : IObjModelBase, IOwnerComunidad, IDataListObject
    {
        public EjercicioDLO() { }
        public EjercicioDLO(int id, Date fechaComienzo, Date fechaFinal, int idOwnerComunidad, bool cerrado)
        {
            this.Id = id;
            this.FechaComienzo = fechaComienzo.ToString();
            this.FechaFinal = fechaFinal.ToString();
            this.IdOwnerComunidad = idOwnerComunidad;
            this.Cerrado = cerrado;
        }

        public int Id { get; private set; }
        public string FechaComienzo { get; private set; }
        public string FechaFinal { get; private set; }
        public int IdOwnerComunidad { get; private set; }
        public bool Cerrado { get; private set; }
    }
}
