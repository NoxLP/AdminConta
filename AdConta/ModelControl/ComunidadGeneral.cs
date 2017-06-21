using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using AdConta.Models;

namespace AdConta.ModelControl
{
    public partial class Comunidad
    {
        public Comunidad(int id)
        {
            this._Id = id;
            this._Ejercicios = new Dictionary<int, Ejercicio>();
            this.Ejercicios = new ReadOnlyDictionary<int, Ejercicio>(this._Ejercicios);
/*
#if (MCONTABILIDAD)
            InitContabilidad();
#endif
#if (MGESTION)
            InitGestion();
#endif
            /*
            MethodInfo initConta = typeof(Comunidad).GetMethod("InitContabilidad");
                initConta.Invoke(this, null);
            }

            if(Properties.Settings.Default.ModuloGestion)
            {
                MethodInfo initGestion = typeof(Comunidad).GetMethod("InitGestion");
                initGestion.Invoke(this, null);
            }*/
        }

        #region fields
        private int _Id;
        internal Dictionary<int, Ejercicio> _Ejercicios;
        //internal Dictionary<int, Presupuesto> _Presupuestos;
        //internal List<int> prueba;
        #endregion

        #region properties
        public int Id { get { return this._Id; } }
        public ReadOnlyDictionary<int, Ejercicio> Ejercicios { get; set; }
        #endregion

        #region helpers
        #endregion

        #region public methods
        #endregion
    }

}
