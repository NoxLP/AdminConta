//#define SQLSERVER //BORRAME Ó COMENTAME CON -> //
#define NOBD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Threading;
using System.Dynamic;
using System.Data.SqlClient;
using System.Diagnostics;
using Extensions;
using Dapper;

namespace AdConta.Models
{
    public sealed partial class AutoCodigoData
    {
        public AutoCodigoData(Usuario usuario)
        {
            this.UsuarioLogueado = usuario;
#if !NOBD
            InitDictionaries();
            FillDictionaries();
#endif
        }

        #region fields
        private readonly string _strCon = GlobalSettings.Properties.Settings.Default.conta1ConnectionString;
        private readonly object _LockObject = new object();
        private bool _DoingConsistencyCheck = false;
        private Dictionary<ACodigoCCheckType, string> _TableNames;
        private Dictionary<ACodigoCCheckType, Dictionary<long, int>> _MaxCodigos;
        private Dictionary<ACodigoCCheckType, Dictionary<long, List<int>>> _DeletedCodigos;
        private Dictionary<ACodigoCCheckType, Dictionary<long, List<int>>> _LargerNotConsecutiveCodigos;

        private Dictionary<ACodigoCCheckType, Dictionary<long, List<int>>> _CodigosOnHold;
        private ConcurrentDictionary<Tuple<int, long>, Func<Task<string>>> _SQL_OnHold;
        private ConcurrentDictionary<Tuple<int, long>, int> _Max_OnHold;
        private ConcurrentDictionary<Tuple<int, long>, Action> _Deleted_OnHold;
        private ConcurrentDictionary<Tuple<int, long>, List<Func<Task>>> _LargerNotConsecutive_OnHold;
        #endregion

        #region properties
        public Usuario UsuarioLogueado { get; private set; }
        public ReadOnlyDictionary<ACodigoCCheckType, string> TableNames { get { return new ReadOnlyDictionary<ACodigoCCheckType, string>(this._TableNames); } }
        public ReadOnlyDictionary<ACodigoCCheckType, Dictionary<long, int>> MaxCodigos
        { get { return new ReadOnlyDictionary<ACodigoCCheckType, Dictionary<long, int>>(this._MaxCodigos); } }
        public ReadOnlyDictionary<ACodigoCCheckType, Dictionary<long, List<int>>> DeletedCodigos
        { get { return new ReadOnlyDictionary<ACodigoCCheckType, Dictionary<long, List<int>>>(this._DeletedCodigos); } }
        public ReadOnlyDictionary<ACodigoCCheckType, Dictionary<long, List<int>>> LargerNotConsecutiveCodigos
        { get { return new ReadOnlyDictionary<ACodigoCCheckType, Dictionary<long, List<int>>>(this._LargerNotConsecutiveCodigos); } }
        public ReadOnlyDictionary<ACodigoCCheckType, Dictionary<long, List<int>>> CodigosOnHold
        { get { return new ReadOnlyDictionary<ACodigoCCheckType, Dictionary<long, List<int>>>(this._CodigosOnHold); } }
        #endregion

        #region init
        private void InitDictionaries()
        {
            this._TableNames = new Dictionary<ACodigoCCheckType, string>();
            this._TableNames.Add(ACodigoCCheckType.Comunidad, "codigoscomunidades");
            this._TableNames.Add(ACodigoCCheckType.Fincas, "codigosfincas");
            this._TableNames.Add(ACodigoCCheckType.Pptos, "codigospresupuestos");
            this._TableNames.Add(ACodigoCCheckType.Asientos, "codigosasientos");

            this._MaxCodigos = new Dictionary<ACodigoCCheckType, Dictionary<long, int>>();
            this._DeletedCodigos = new Dictionary<ACodigoCCheckType, Dictionary<long, List<int>>>();
            this._LargerNotConsecutiveCodigos = new Dictionary<ACodigoCCheckType, Dictionary<long, List<int>>>();
            this._CodigosOnHold = new Dictionary<ACodigoCCheckType, Dictionary<long, List<int>>>();

            this._SQL_OnHold = new ConcurrentDictionary<Tuple<int, long>, Func<Task<string>>>();
            this._Max_OnHold = new ConcurrentDictionary<Tuple<int, long>, int>();
            this._Deleted_OnHold = new ConcurrentDictionary<Tuple<int, long>, Action>();
            this._LargerNotConsecutive_OnHold = new ConcurrentDictionary<Tuple<int, long>, List<Func<Task>>>();

            //Type tCdad = typeof(Comunidad);
            //Type tFinca = typeof(Finca);
            //Type tPpto = typeof(Presupuesto);
            //Type tAsiento = typeof(Asiento);

            this._DeletedCodigos.Add(ACodigoCCheckType.Comunidad, new Dictionary<long, List<int>>());
            this._DeletedCodigos.Add(ACodigoCCheckType.Fincas, new Dictionary<long, List<int>>());
            this._DeletedCodigos.Add(ACodigoCCheckType.Pptos, new Dictionary<long, List<int>>());
            this._DeletedCodigos.Add(ACodigoCCheckType.Asientos, new Dictionary<long, List<int>>());
            this._LargerNotConsecutiveCodigos.Add(ACodigoCCheckType.Comunidad, new Dictionary<long, List<int>>());
            this._LargerNotConsecutiveCodigos.Add(ACodigoCCheckType.Fincas, new Dictionary<long, List<int>>());
            this._LargerNotConsecutiveCodigos.Add(ACodigoCCheckType.Pptos, new Dictionary<long, List<int>>());
            this._LargerNotConsecutiveCodigos.Add(ACodigoCCheckType.Asientos, new Dictionary<long, List<int>>());
            this._MaxCodigos.Add(ACodigoCCheckType.Comunidad, new Dictionary<long, int>());
            this._MaxCodigos.Add(ACodigoCCheckType.Fincas, new Dictionary<long, int>());
            this._MaxCodigos.Add(ACodigoCCheckType.Pptos, new Dictionary<long, int>());
            this._MaxCodigos.Add(ACodigoCCheckType.Asientos, new Dictionary<long, int>());
            this._CodigosOnHold.Add(ACodigoCCheckType.Comunidad, new Dictionary<long, List<int>>());
            this._CodigosOnHold.Add(ACodigoCCheckType.Fincas, new Dictionary<long, List<int>>());
            this._CodigosOnHold.Add(ACodigoCCheckType.Pptos, new Dictionary<long, List<int>>());
            this._CodigosOnHold.Add(ACodigoCCheckType.Asientos, new Dictionary<long, List<int>>());
        }
        private void SetDeletedByIds(ACodigoCCheckType t, SqlMapper.GridReader reader)
        {
            int id = -1;
            List<int> lista = new List<int>();
            IEnumerable<dynamic> read = reader.Read();
            foreach (dynamic dyn in read)
            {
                if (id == -1)
                {
                    lista.Add(dyn.codigo);
                    id = dyn.IdOwnerComunidad;
                }
                else if (dyn.IdOwnerComunidad == id) lista.Add(dyn.codigo);
                else //if(dyn.IdOwnerComunidad != id)
                {
                    _DeletedCodigos[t].Add(id, lista);
                    lista.Clear();
                    lista.Add(dyn.codigo);
                    id = dyn.IdOwnerComunidad;
                }
            }
        }
        private void SetNoConByIds(ACodigoCCheckType t, SqlMapper.GridReader reader)
        {
            int id = -1;
            List<int> lista = new List<int>();
            IEnumerable<dynamic> read = reader.Read();
            foreach (dynamic dyn in read)
            {
                if (id == -1)
                {
                    lista.Add(dyn.codigo);
                    id = dyn.IdOwnerComunidad;
                }
                else if (dyn.IdOwnerComunidad == id) lista.Add(dyn.codigo);
                else //if(dyn.IdOwnerComunidad != id)
                {
                    _LargerNotConsecutiveCodigos[t].Add(id, lista);
                    lista.Clear();
                    lista.Add(dyn.codigo);
                    id = dyn.IdOwnerComunidad;
                }
            }
        }
        private void SetAsientoDeletedListByIds(SqlMapper.GridReader reader)
        {
            long id = -1;
            List<int> lista = new List<int>();
            ACodigoCCheckType t = ACodigoCCheckType.Asientos;
            IEnumerable<dynamic> read = reader.Read();
            foreach (dynamic dyn in read)
            {
                int cdad = dyn.IdOwnerComunidad;
                int ejer = dyn.IdOwnerEjercicio;
                long dynId = cdad.CantorPair(ejer);

                if (id == -1)
                {
                    lista.Add(dyn.codigo);
                    id = dynId; //new Tuple<int, int>(dyn.IdOwnerComunidad, dyn.IdOwnerEjercicio);
                }

                if (dynId == id) lista.Add(dyn.codigo);
                else //if(dyn.IdOwnerComunidad != id)
                {
                    _DeletedCodigos[t].Add(id, lista);
                    lista.Clear();
                    lista.Add(dyn.codigo);
                    id = dynId;
                }
            }
        }
        private void SetAsientoNoConListByIds(SqlMapper.GridReader reader)
        {
            long id = -1;
            List<int> lista = new List<int>();
            ACodigoCCheckType t = ACodigoCCheckType.Asientos;
            IEnumerable<dynamic> read = reader.Read();
            foreach (dynamic dyn in read)
            {
                int cdad = dyn.IdOwnerComunidad;
                int ejer = dyn.IdOwnerEjercicio;
                long dynId = cdad.CantorPair(ejer);

                if (id == -1)
                {
                    lista.Add(dyn.codigo);
                    id = dynId; //new Tuple<int, int>(dyn.IdOwnerComunidad, dyn.IdOwnerEjercicio);
                }

                if (dynId == id) lista.Add(dyn.codigo);
                else //if(dyn.IdOwnerComunidad != id)
                {
                    _LargerNotConsecutiveCodigos[t].Add(id, lista);
                    lista.Clear();
                    lista.Add(dyn.codigo);
                    id = dynId;
                }
            }
        }
        private void SetMaxByIds(ACodigoCCheckType t, SqlMapper.GridReader reader)
        {
            //TODO: MAL. Recuerda que puede haber fallos: ahora esto solo coje el primer registro de cada Id y lo guarda como máximo, 
            //eso funcionaría si no hubieran fallos, como puede haber fallos cada Id puede tener más de un código seteado como máximo.
            //Hay que elegir uno: ¿?¿?¿?¿?? => recuerda que no están ordenados
            List<int> ids = new List<int>();
            IEnumerable<dynamic> read = reader.Read();
            foreach (dynamic dyn in read)
            {
                if (!ids.Contains(dyn.IdOwnerComunidad))
                {
                    this._MaxCodigos[t].Add(dyn.IdOwnerComunidad, dyn.codigo);
                    ids.Add(dyn.IdOwnerComunidad);
                }
            }
        }
        private void SetAsientoMaxByIds(SqlMapper.GridReader reader)
        {
            //TODO: MAL. Recuerda que puede haber fallos: ahora esto solo coje el primer registro de cada Id y lo guarda como máximo, 
            //eso funcionaría si no hubieran fallos, como puede haber fallos cada Id puede tener más de un código seteado como máximo.
            //Hay que elegir uno: ¿?¿?¿?¿?? => recuerda que no están ordenados
            List<long> ids = new List<long>();
            ACodigoCCheckType t = ACodigoCCheckType.Asientos;
            IEnumerable<dynamic> read = reader.Read();
            foreach (dynamic dyn in read)
            {
                int cdad = dyn.IdOwnerComunidad;
                int ejer = dyn.IdOwnerEjercicio;
                long id = cdad.CantorPair(ejer);

                if (!ids.Contains(id))
                {
                    this._MaxCodigos[t][id] = dyn.codigo;
                    ids.Add(id);
                }
            }
        }
        private void FillDictionaries()
        {
            SqlMapper.GridReader result;

            //Type tCdad = typeof(Comunidad);
            //Type tFinca = typeof(Finca);
            //Type tPpto = typeof(Presupuesto);
            //Type tAsiento = typeof(Asiento);

            using (SqlConnection con = new SqlConnection(_strCon))
            {
                con.Open();

                result = con.QueryMultiple(
@"SELECT IdOwnerComunidad,codigo FROM codigosfincas WHERE deleted=@del;
SELECT IdOwnerComunidad,codigo FROM codigosfincas WHERE noconsecutivo=@nocons;
SELECT IdOwnerComunidad,codigo FROM codigosfincas WHERE maximo=@max;
SELECT IdOwnerComunidad,codigo FROM codigospresupuestos WHERE deleted=@del;
SELECT IdOwnerComunidad,codigo FROM codigospresupuestos WHERE noconsecutivo=@nocons;
SELECT IdOwnerComunidad,codigo FROM codigospresupuestos WHERE maximo=@max;
SELECT IdOwnerComunidad,IdOwnerEjercicio,codigo FROM codigosasientos WHERE deleted=@del;
SELECT IdOwnerComunidad,IdOwnerEjercicio,codigo FROM codigosasientos WHERE noconsecutivo=@nocons;
SELECT IdOwnerComunidad,IdOwnerEjercicio,codigo FROM codigosasientos WHERE maximo=@max;
SELECT codigo FROM codigoscomunidades WHERE deleted=@del;
SELECT codigo FROM codigoscomunidades WHERE noconsecutivo=@nocons;
SELECT codigo FROM codigoscomunidades WHERE maximo=@max;"
                    , new { del = 1, nocons = 1, max = 1 });

                SetDeletedByIds(ACodigoCCheckType.Fincas, result);
                SetNoConByIds(ACodigoCCheckType.Fincas, result);
                SetMaxByIds(ACodigoCCheckType.Fincas, result);

                SetDeletedByIds(ACodigoCCheckType.Pptos, result);
                SetNoConByIds(ACodigoCCheckType.Pptos, result);
                SetMaxByIds(ACodigoCCheckType.Pptos, result);

                SetAsientoDeletedListByIds(result);
                SetAsientoNoConListByIds(result);
                SetAsientoMaxByIds(result);

                this._DeletedCodigos[ACodigoCCheckType.Comunidad].Add(0, result.Read<int>().ToList());
                this._LargerNotConsecutiveCodigos[ACodigoCCheckType.Comunidad].Add(0, result.Read<int>().ToList());
                this._MaxCodigos[ACodigoCCheckType.Comunidad].Add(0, result.Read<int>().SingleOrDefault());

                con.Close();
            }
        }
        #endregion

        #region error builders
        private string BuildNextCodigoErrorString(int err)
        {
            if (err < 1) return "";

            string error = $"Error en la BD al crear siguiente codigo en AutoCodigo.GetNextCodigo(Type objModelType):{Environment.NewLine}";

            if ((err & 1) == 1)
                error = error.Append(
                    "Error en actualización de códigos no consecutivos: UPDATE {table} SET NoConsecutivo=@cero WHERE Codigo=@cod{maxCodigo}{ownersClause};",
                    Environment.NewLine);
            if ((err & 10) == 10)
                error = error.Append(
                    "Error en actualización de máximo: UPDATE {table} SET maximo=@cero WHERE codigo=@previous{ownersClause};",
                    Environment.NewLine);
            if ((err & 100) == 100)
                error = error.Append(
                    "Error al insertar nuevo código: INSERT INTO {table} (codigo,noconsecutivo,maximo{ownersColumns}) VALUES (@codfinal,@cero,@uno{ownersValues});");

            return error;
        }
        private string BuildDeleteCodigoErrorString(int err, bool noConChanges, bool maxChanges)
        {
            if (err < 1) return "";

            string error = $"Error en la BD al borrar codigo en AutoCodigo.DeleteCodigo:{Environment.NewLine}";
            if (maxChanges)
            {
                if ((err & 1) == 1)
                    error = error.Append(
                        "Error al intentar setear nuevo máximo: UPDATE {table} SET Maximo=@uno WHERE Codigo=@PrevCod{ownersClause};",
                        Environment.NewLine);
                if ((err & 10) == 10)
                    error = error.Append(
                        "Error al intentar borrar codigo de BD: DELETE FROM {table} WHERE Codigo=@cod{ownersClause};",
                        Environment.NewLine);
            }
            else if (noConChanges)
                error = error.Append(
                    "Error al intentar borrar codigo de BD: DELETE FROM {table} WHERE Codigo=@cod{ownersClause};");
            else
                error = error.Append(
                    "Error al intentar setear codigo como deleted: UPDATE {table} SET Deleted=@uno WHERE Codigo=@cod{ownersClause};");

            return error;
        }
        private string BuildCheckCodigoErrorString(int err, bool previousMax, bool maxChanges)
        {
            if (err < 1) return "";

            string error = $"Error en la BD al chequear codigo en AutoCodigo.CheckCodigoIsAvailableAndTakeIt:{Environment.NewLine}";
            if (previousMax)
            {
                if (maxChanges)
                {
                    if ((err & 1) == 1)
                        error = error.Append(
                            "Error al intentar cambiar su estado de máximo al antigo código máximo: UPDATE {table} SET Maximo=@cero WHERE Codigo=@prevMax{ownersClause};",
                            Environment.NewLine);
                    if ((err & 10) == 10)
                        error = error.Append(
                            "Error al insertar nuevo código chequeado: INSERT INTO {table} (Codigo,Deleted,NoConsecutivo,Maximo{ownersColumns}) VALUES (@cod,@cero,@uno,@max{ownersValues});",
                            Environment.NewLine);
                }
                else
                    error = error.Append(
                            "Error al insertar nuevo código chequeado: INSERT INTO {table} (Codigo,Deleted,NoConsecutivo,Maximo{ownersColumns}) VALUES (@cod,@cero,@uno,@max{ownersValues});",
                            Environment.NewLine);
            }
            else
                error = error.Append(
                        "Error al intentar setear código como NO deleted: UPDATE codigo SET deleted=0 UPDATE {table} SET Deleted=@cero WHERE Codigo=@cod{ownersClause};",
                        Environment.NewLine);

            return error;
        }
        #endregion

        #region SQL tasks
        public async Task<bool> UnsetCodigoAsOnHoldAsync(int codigo, string table, long id)
        {
            int result;
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                await con.OpenAsync().ConfigureAwait(false);

                result = await con.ExecuteAsync($"DELETE FROM {table} WHERE Codigo=@cod AND IdOwners=@Id", new { cod = codigo, Id = id }).ConfigureAwait(false);

                con.Close();
            }
            return result == 1;
        }
        public async Task<string> NextCodigoQueryMultipleAsync(string SQL, ExpandoObject values, int codigo, int noConToRemoveCount)
        {
            int err = 0;
#if !SQLSERVER
            SQL = SQL.PutAhead("START TRANSACTION;", Environment.NewLine);
#endif
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                await con.OpenAsync().ConfigureAwait(false);

                var result = await con.QueryMultipleAsync(SQL, values).ConfigureAwait(false);

                if (noConToRemoveCount > 0)
                {
                    for (int i = 0; i < noConToRemoveCount; i++)
                        err = err | (Convert.ToInt32(result.Read<int>().SingleOrDefault() != noConToRemoveCount) * 1);

                    err = err | (Convert.ToInt32(result.Read<int>().SingleOrDefault() != 1) * 10);
                }
                //var r1 = result.Read();
#if SQLSERVER
                var ie = result.Read<int>().SingleOrDefault();
                err = err | (Convert.ToInt32(ie != codigo) * 100);
#else
                err = err | (Convert.ToInt32(result.Read<int>().SingleOrDefault() != 1) * 100);
                string postQuery = "";
                if (err > 0) postQuery = "ROLLBACK;" + Environment.NewLine;
                postQuery = postQuery + "COMMIT;";
                con.Execute(postQuery);
#endif
                con.Close();
            }

            return BuildNextCodigoErrorString(err);
        }
        public async Task<string> DeleteCodigoQueryMultipleAsync(string SQL, ExpandoObject values, bool noConChanges, bool maxChanges)
        {
            int err = 0;
#if !SQLSERVER
            SQL = SQL.PutAhead("START TRANSACTION;", Environment.NewLine);
#endif
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                await con.OpenAsync().ConfigureAwait(false);

                if (maxChanges)
                {
#if SQLSERVER
                    string[] split = SQL.Split(';');

                    var result = await con.ExecuteAsync(split[0], values).ConfigureAwait(false);
                    err = Convert.ToInt32(result != 1);

                    result = await con.ExecuteAsync(split[1], values).ConfigureAwait(false);
                    err = err | (Convert.ToInt32(result != 1) * 10);
#else
                    var result = await con.QueryMultipleAsync(SQL, values).ConfigureAwait(false);

                    err = Convert.ToInt32(result.Read<int>().SingleOrDefault() == 1);
                    err = err | (Convert.ToInt32(result.Read<int>().SingleOrDefault() == 1) * 10);
#endif
                }
                else
                {
                    var result = await con.ExecuteAsync(SQL, values).ConfigureAwait(false);

                    err = Convert.ToInt32(result != 1);
                }
#if !SQLSERVER
                string postQuery = "";
                if (err > 0) postQuery = "ROLLBACK;" + Environment.NewLine;
                postQuery = postQuery + "COMMIT;";
                con.Execute(postQuery);
#endif
                con.Close();
            }

            return BuildDeleteCodigoErrorString(err, noConChanges, maxChanges);
        }
        public async Task<string> CheckCodigoQueryMultipleAsync(string SQL, ExpandoObject values, bool previousMax, bool maxChanges)
        {
            int err = 0;
#if !SQLSERVER
            SQL = SQL.PutAhead("START TRANSACTION;", Environment.NewLine);
#endif
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                await con.OpenAsync().ConfigureAwait(false);

                var result = await con.QueryMultipleAsync(SQL, values).ConfigureAwait(false);

                err = Convert.ToInt32(result.Read<int>().SingleOrDefault() == 1);
                if (maxChanges) err = err | (Convert.ToInt32(result.Read<int>().SingleOrDefault() == 1) * 10);
#if !SQLSERVER
                string postQuery = "";
                if (err > 0) postQuery = "ROLLBACK;" + Environment.NewLine;
                postQuery = postQuery + "COMMIT;";
                con.Execute(postQuery);
#endif
                con.Close();
            }

            return BuildCheckCodigoErrorString(err, previousMax, maxChanges);
        }
        #endregion

        #region do
        public async Task<bool> TryApplyCodigoChangesAsync<T>(int idCdad, aAutoCodigoBase caller)
            where T : IObjModelConCodigo
        {
            Tuple<int, long> id = new Tuple<int, long>(caller.CurrentCodigo, idCdad);
            ACodigoCCheckType objModelType = caller.CCheckType;

            if (!_SQL_OnHold.ContainsKey(id))
            {
                caller.ChangesErrorString = $"El código {id.Item1} de la comunidad {id.Item2} no existe en la lista de codigos por cambiar.";
                return false;
            }

            Task<string> SQL = _SQL_OnHold[id]();

            int nuevoMax = -1;
            Action changesToDeleted = null;
            List<Func<Task>> changesToNoCon = null;
            _Max_OnHold.TryGetValue(id, out nuevoMax);
            _Deleted_OnHold.TryGetValue(id, out changesToDeleted);
            _LargerNotConsecutive_OnHold.TryGetValue(id, out changesToNoCon);

            //Espera respuesta base de datos 
            //Wait DB response
            string error = await SQL.ConfigureAwait(false);

            //Si hay un error devuelve el error y finaliza => TODO: ¿Hay que borrar los ONHOLD? => NO, usuario cancelará
            //If there is an error return the error string
            if (error != "")
            {
                caller.ChangesErrorString = error;
                return false;
            }

            //Los cambios han sido guardados en base de datos sin errores => guarda cambios en diccionarios
            //Changes have been stored in DB without errors => store changes in dictionaries
            if (nuevoMax > 0)
            {
                if (_MaxCodigos[objModelType].ContainsKey(idCdad))
                    _MaxCodigos[objModelType][idCdad] = nuevoMax;
                else
                    _MaxCodigos[objModelType].Add(idCdad, nuevoMax);
                    //_MaxCodigos.Add(objModelType, new Dictionary<long, int>() { { idCdad, nuevoMax } });

                _Max_OnHold.TryRemove(id, out nuevoMax);
            }
            if (changesToDeleted != null)
            {
                changesToDeleted();
                _Deleted_OnHold.TryRemove(id, out changesToDeleted);
            }
            if (changesToNoCon != null)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                foreach (Func<Task> func in changesToNoCon) Task.Run(() => func()).Forget().ConfigureAwait(false); //fire and forget
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                _LargerNotConsecutive_OnHold.TryRemove(id, out changesToNoCon);
            }
            //Remove SQL task that was on hold
            Func<Task<string>> dump;
            _SQL_OnHold.TryRemove(id, out dump);
            //Unset the codigo as OnHold
            bool unsetted = await UnsetCodigoAsOnHoldAsync(caller.CodigoOnHold, this._TableNames[objModelType].Append("onhold"), idCdad).ConfigureAwait(false);
            if (!unsetted)
                throw new CustomException_AutoCodigo(
                    $"AutoCodigoData.ApplyCodigoChanges: Error al intentar desetear el código {caller.CodigoOnHold} de la comunidad {idCdad} en la BD.");
            this._CodigosOnHold[objModelType][idCdad].Remove(caller.CodigoOnHold);

            caller.ChangesErrorString = "";

            return true;
        }
        public async Task<bool> TryApplyCodigoChangesAsync<T>(int idCdad, int idEjer, aAutoCodigoBase caller)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            long cantorId = idCdad.CantorPair(idEjer);
            Tuple<int, long> id = new Tuple<int, long>(caller.CurrentCodigo, cantorId);
            ACodigoCCheckType objModelType = caller.CCheckType;

            if (!_SQL_OnHold.ContainsKey(id))
            {
                caller.ChangesErrorString = $"El código {id.Item1} de la comunidad {id.Item2} no existe en la lista de codigos por cambiar.";
                return false;
            }

            Task<string> SQL = _SQL_OnHold[id]();

            int nuevoMax = -1;
            Action changesToDeleted = null;
            List<Func<Task>> changesToNoCon = null;
            _Max_OnHold.TryGetValue(id, out nuevoMax);
            _Deleted_OnHold.TryGetValue(id, out changesToDeleted);
            _LargerNotConsecutive_OnHold.TryGetValue(id, out changesToNoCon);

            //Espera respuesta base de datos 
            //Wait DB response
            string error = await SQL.ConfigureAwait(false);

            //Si hay un error devuelve el error y finaliza => TODO: ¿Hay que borrar los ONHOLD? => NO, usuario cancelará
            //If there is an error return the error string
            if (error != "")
            {
                caller.ChangesErrorString = error;
                return false;
            }

            //Los cambios han sido guardados en base de datos sin errores => guarda cambios en diccionarios
            //Changes have been stored in DB without errors => store changes in dictionaries
            if (nuevoMax > 0)
            {
                if (_MaxCodigos[objModelType].ContainsKey(cantorId))
                    _MaxCodigos[objModelType][cantorId] = nuevoMax;
                else
                    _MaxCodigos[objModelType].Add(cantorId, nuevoMax);

                _Max_OnHold.TryRemove(id, out nuevoMax);
            }
            if (changesToDeleted != null)
            {
                changesToDeleted();
                _Deleted_OnHold.TryRemove(id, out changesToDeleted);
            }
            if (changesToNoCon != null)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                foreach (Func<Task> func in changesToNoCon) Task.Run(() => func()).Forget().ConfigureAwait(false); //fire and forget
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                _LargerNotConsecutive_OnHold.TryRemove(id, out changesToNoCon);
            }
            //Remove SQL task that was on hold
            Func<Task<string>> dump;
            _SQL_OnHold.TryRemove(id, out dump);
            //Unset the codigo as OnHold
            bool unsetted = await UnsetCodigoAsOnHoldAsync(caller.CodigoOnHold, this._TableNames[objModelType].Append("onhold"), cantorId).ConfigureAwait(false);
            if (!unsetted)
                throw new CustomException_AutoCodigo(
                    $"AutoCodigoData.ApplyCodigoChanges: Error al intentar desetear el código {caller.CodigoOnHold} de comunidad {idCdad} y ejercicio {idEjer} en la BD.");
            this._CodigosOnHold[objModelType][cantorId].Remove(caller.CodigoOnHold);

            caller.ChangesErrorString = "";
            return true;
        }
        public async Task<bool> TryCancelCodigoChangesAsync<T>(int? codigoOnHold, int idCdad, aAutoCodigoBase caller)
            where T : IObjModelConCodigo
        {
            ACodigoCCheckType objModelType = caller.CCheckType;
            int codigo = codigoOnHold ?? caller.CodigoOnHold;
            //One can only cancel owned codigos, and owned codigos on hold are always in the dictionary
            if (!this._CodigosOnHold[objModelType][idCdad].Contains(codigo))
            {
                caller.ChangesErrorString = $"El código {codigo} de la comunidad {idCdad} no está pendiente de cambios.";
                return false;
            }

            string table = this._TableNames[objModelType].Append("onhold");

            bool onHold = await UnsetCodigoAsOnHoldAsync(codigo, table, idCdad).ConfigureAwait(false);

            if (!onHold)
            {
                caller.ChangesErrorString = $"Error al intentar cancelar cambios en código {codigo} de comunidad {idCdad} en la base de datos, que produjo error en: DELETE FROM {table} WHERE Codigo=@cod AND Id=@Id";
                return false;
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() =>
            {
                this._CodigosOnHold[objModelType][idCdad].Remove(codigo);

                Tuple<int, long> id = new Tuple<int, long>(caller.CurrentCodigo, idCdad);

                Action dump;
                this._Deleted_OnHold.TryRemove(id, out dump);

                List<Func<Task>> dump2;
                this._LargerNotConsecutive_OnHold.TryRemove(id, out dump2);

                int dump3;
                this._Max_OnHold.TryRemove(id, out dump3);

                Func<Task<string>> dump4;
                this._SQL_OnHold.TryRemove(id, out dump4);
            })
            .Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            caller.ChangesErrorString = "";
            return true;
        }
        public async Task<bool> TryCancelCodigoChangesAsync<T>(int? codigoOnHold, int idCdad, int idEjer, aAutoCodigoBase caller)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            ACodigoCCheckType objModelType = caller.CCheckType;
            int codigo = codigoOnHold ?? caller.CodigoOnHold;
            long id = idCdad.CantorPair(idEjer);
            //One can only cancel owned codigos, and owned codigos on hold are always in the dictionary
            if (!this._CodigosOnHold[objModelType][id].Contains(codigo))
            {
                caller.ChangesErrorString = $"El código {codigo} de la comunidad {idCdad} y ejercicio {idEjer} no está pendiente de cambios.";
                return false;
            }

            string table = this._TableNames[objModelType].Append("onhold");

            bool onHold = await UnsetCodigoAsOnHoldAsync(codigo, table, id).ConfigureAwait(false);

            if (!onHold)
            {
                caller.ChangesErrorString = $"Error al intentar cancelar cambios en código {codigo} de comunidad {idCdad} y ejercicio {idEjer} en la base de datos, que produjo error en: DELETE FROM {table} WHERE Codigo=@cod AND Id=@Id";
                return false;
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() =>
            {
                this._CodigosOnHold[objModelType][id].Remove(codigo);

                Tuple<int, long> tid = new Tuple<int, long>(caller.CurrentCodigo, id);

                Action dump;
                this._Deleted_OnHold.TryRemove(tid, out dump);

                List<Func<Task>> dump2;
                this._LargerNotConsecutive_OnHold.TryRemove(tid, out dump2);

                int dump3;
                this._Max_OnHold.TryRemove(tid, out dump3);

                Func<Task<string>> dump4;
                this._SQL_OnHold.TryRemove(tid, out dump4);
            })
            .Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            caller.ChangesErrorString = "";
            return true;
        }
        #endregion

        #region add/remove on hold
        public bool TrySetCodigoOnHold<T>(aAutoCodigoBase caller, int idCdad)
            where T : IObjModelConCodigo
        {
            ACodigoCCheckType objModelType = caller.CCheckType;
            //If codigo have been set on hold previously by this user (in another tab f.i.), return false
            if (this._CodigosOnHold[objModelType].ContainsKey(idCdad) && this._CodigosOnHold[objModelType][idCdad].Contains(caller.CodigoOnHold)) return false;
            //Check if codigo have been setted on hold in the DB by other user
            string table = this._TableNames[objModelType].Append("onhold");
            bool onHold = false;
            var manualReset = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(x =>
            {
                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    con.Open();

                    string SQL = caller.CodigoOnHold != 0 ? 
                        $"SELECT count(*) FROM {table} WHERE ( Codigo=@ohcod OR Curr=@ccod ) AND IdOwners=@idCdad" :
                        $"SELECT count(*) FROM {table} WHERE Curr=@ccod AND IdOwners=@idCdad"; 
                    var values = new ExpandoObject() as IDictionary<string, object>;
                    values.Add("ccod", caller.CurrentCodigo);
                    values.Add("ohcod", caller.CodigoOnHold);
                    values.Add("idCdad", idCdad);
                    var result = con.Query<bool>(SQL, values); //direct cast to bool because there can't be more than one
                    onHold = result.SingleOrDefault();
                    con.Close();
                }
                manualReset.Set();
            });
            manualReset.WaitOne();
            if (onHold) return false;
            //If method is here, codigo isn't on hold on DB, set it as on hold on DB... AND dictionary since this user requested it.
            onHold = true;
            manualReset.Reset();
            ThreadPool.QueueUserWorkItem(x =>
            {
                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    con.Open();

                    var result = con.Execute($"INSERT INTO {table} (IdOwners,Codigo,Curr,OnHold) VALUES (@cdad,@ohcod,@ccod,@hold)",// UPDATE {table} SET OnHold=@hold WHERE Codigo=@cod AND Id=@cdad",
                        new { hold = 1, ohcod = caller.CodigoOnHold, ccod = caller.CurrentCodigo, cdad = idCdad });
                    onHold = result == 1;
                    con.Close();
                }
                manualReset.Set();
            });
            manualReset.WaitOne();
            if (!onHold) return false;

            if (!this._CodigosOnHold[objModelType].ContainsKey(idCdad))
                lock (_LockObject) this._CodigosOnHold[objModelType].Add(idCdad, new List<int>() { caller.CodigoOnHold });
            else
                lock (_LockObject) this._CodigosOnHold[objModelType][idCdad].Add(caller.CodigoOnHold);
            return true;
        }
        public bool TrySetCodigoOnHold<T>(aAutoCodigoBase caller, int idCdad, int idEjer)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            ACodigoCCheckType objModelType = caller.CCheckType;
            long id = idCdad.CantorPair(idEjer);
            //If codigo have been set on hold previously by this user (in another tab f.i.), return false
            if (this._CodigosOnHold[objModelType].ContainsKey(id) && this._CodigosOnHold[objModelType][id].Contains(caller.CodigoOnHold)) return false;
            //Check if codigo have been setted on hold in the DB by other user
            string table = this._TableNames[objModelType].Append("onhold");
            bool onHold = false;
            var manualReset = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(x =>
            {
                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    con.Open();

                    string SQL = caller.CodigoOnHold != 0 ?
                        $"SELECT count(*) FROM {table} WHERE ( Codigo=@ohcod OR Curr=@ccod ) AND IdOwners=@Id" :
                        $"SELECT count(*) FROM {table} WHERE Curr=@ccod AND IdOwners=@Id";
                    var values = new ExpandoObject() as IDictionary<string, object>;
                    values.Add("ccod", caller.CurrentCodigo);
                    values.Add("ohcod", caller.CodigoOnHold);
                    values.Add("Id", id);
                    var result = con.Query<bool>(SQL, values); //direct cast to bool because there can't be more than one
                    onHold = result.SingleOrDefault();
                    con.Close();
                }
                manualReset.Set();
            });
            manualReset.WaitOne();
            if (onHold) return false;
            //If method is here, codigo isn't on hold on DB, set it as on hold on DB... AND dictionary since this user requested it.
            onHold = true;
            manualReset.Reset();
            ThreadPool.QueueUserWorkItem(x =>
            {
                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    con.Open();

                    var result = con.Execute($"INSERT INTO {table} (IdOwners,Codigo,Curr,OnHold) VALUES (@Id,@ohcod,@ccod,@hold)",// UPDATE {table} SET OnHold=@hold WHERE Codigo=@cod AND Id=@Id",
                        new { hold = 1, ohcod = caller.CodigoOnHold, ccod = caller.CurrentCodigo, Id = id });
                    onHold = result == 1;
                    con.Close();
                }
                manualReset.Set();
            });
            manualReset.WaitOne();
            if (!onHold) return false;

            if (!this._CodigosOnHold[objModelType].ContainsKey(id))
                lock (_LockObject) this._CodigosOnHold[objModelType].Add(id, new List<int>() { caller.CodigoOnHold });
            else
                lock (_LockObject) this._CodigosOnHold[objModelType][id].Add(caller.CodigoOnHold);
            return true;
        }
        public bool IsCodigoOnHold<T>(int codigo, long id, ACodigoCCheckType t)
        {
            string table = this._TableNames[t].Append("onhold");
            bool onHold;
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                con.Open();

                string SQL = $"SELECT OnHold FROM {table} WHERE IdOwners=@idCdad AND (Codigo=@cod OR Curr=@cod)";
                var values = new ExpandoObject() as IDictionary<string, object>;
                values.Add("cod", codigo);
                values.Add("idCdad", id);
                var result = con.Query<bool>(SQL, values);
                onHold = result.SingleOrDefault();
                con.Close();
            }
            return onHold;
        }
        public async Task<bool> IsCodigoOnHoldAsync<T>(int codigo, long id, ACodigoCCheckType t)
        {
            string table = this._TableNames[t].Append("onhold");
            bool onHold;
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                await con.OpenAsync().ConfigureAwait(false);

                string SQL = $"SELECT OnHold FROM {table} WHERE IdOwners=@idCdad AND (Codigo=@cod OR Curr=@cod)";
                var values = new ExpandoObject() as IDictionary<string, object>;
                values.Add("cod", codigo);
                values.Add("idCdad", id);
                var result = await con.QueryAsync<bool>(SQL, values).ConfigureAwait(false);
                onHold = result.SingleOrDefault();
                con.Close();
            }
            return onHold;
        }

        public bool TryAddNextCodigoSQLOnHold<T>(aAutoCodigoBase caller, int idCdad, string SQL, ExpandoObject values, int noConToRemoveCount = 0)
            where T : IObjModelConCodigo
        {
            Tuple<int, long> id = new Tuple<int, long>(caller.CurrentCodigo, idCdad);
            if (!_SQL_OnHold.TryAdd(id, () => NextCodigoQueryMultipleAsync(SQL, values, caller.CodigoOnHold, noConToRemoveCount)))
                return false;
            return true;
        }
        public bool TryAddNextCodigoSQLOnHold<T>(aAutoCodigoBase caller, int idCdad, int idEjer, string SQL, ExpandoObject values, int noConToRemoveCount = 0)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            Tuple<int, long> id = new Tuple<int, long>(caller.CurrentCodigo, idCdad.CantorPair(idEjer));
            if (!_SQL_OnHold.TryAdd(id, () => NextCodigoQueryMultipleAsync(SQL, values, caller.CodigoOnHold, noConToRemoveCount)))
                return false;
            return true;
        }
        public bool TryAddDeletedCodigoSQLOnHold<T>(int codigo, int idCdad, string SQL, ExpandoObject values, bool noConChanges, bool maxChanges)
            where T : IObjModelConCodigo
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad);
            if (!_SQL_OnHold.TryAdd(id, () => DeleteCodigoQueryMultipleAsync(SQL, values, noConChanges, maxChanges)))
                return false;
            return true;
        }
        public bool TryAddDeletedCodigoSQLOnHold<T>(int codigo, int idCdad, int idEjer, string SQL, ExpandoObject values, bool noConChanges, bool maxChanges)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad.CantorPair(idEjer));
            if (!_SQL_OnHold.TryAdd(id, () => DeleteCodigoQueryMultipleAsync(SQL, values, noConChanges, maxChanges)))
                return false;
            return true;
        }
        public bool TryAddCheckCodigoSQLOnHold<T>(int codigo, int idCdad, string SQL, ExpandoObject values, bool previousMax, bool maxChanges)
            where T : IObjModelConCodigo
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad);
            if (!_SQL_OnHold.TryAdd(id, () => CheckCodigoQueryMultipleAsync(SQL, values, previousMax, maxChanges)))
                return false;
            return true;
        }
        public bool TryAddCheckCodigoSQLOnHold<T>(int codigo, int idCdad, int idEjer, string SQL, ExpandoObject values, bool previousMax, bool maxChanges)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad.CantorPair(idEjer));
            if (!_SQL_OnHold.TryAdd(id, () => CheckCodigoQueryMultipleAsync(SQL, values, previousMax, maxChanges)))
                return false;
            return true;
        }

        public bool TrySetNewMaxOnHold<T>(int codigo, int idCdad, int nuevoMax)
            where T : IObjModelConCodigo
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad);
            if (!_Max_OnHold.TryAdd(id, nuevoMax)) return false;
            return true;
        }
        public bool TrySetNewMaxOnHold<T>(int codigo, int idCdad, int idEjer, int nuevoMax)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad.CantorPair(idEjer));
            if (!_Max_OnHold.TryAdd(id, nuevoMax)) return false;
            return true;
        }
        public bool TryAddToDeletedOnHold<T>(int codigo, int idCdad, int codigoToAdd, ACodigoCCheckType t)
            where T : IObjModelConCodigo
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad);
            Action newAction = () =>
            {
                if (!_DeletedCodigos[t][id.Item2].Contains(codigoToAdd))
                    _DeletedCodigos[t][id.Item2].Add(codigoToAdd);
            };

            if (!_Deleted_OnHold.TryAdd(id, newAction)) return false;

            return true;
        }
        public bool TryAddToDeletedOnHold<T>(int codigo, int idCdad, int idEjer, int codigoToAdd, ACodigoCCheckType t)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad.CantorPair(idEjer));
            Action newAction = () =>
            {
                if (!_DeletedCodigos[t][id.Item2].Contains(codigoToAdd))
                    _DeletedCodigos[t][id.Item2].Add(codigoToAdd);
            };

            if (!_Deleted_OnHold.TryAdd(id, newAction)) return false;

            return true;
        }
        public bool TryRemoveFromDeletedOnHold<T>(int codigo, int idCdad, int codigoToRemove, ACodigoCCheckType t)
            where T : IObjModelConCodigo
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad);
            Action newAction = () =>
            {
                if (_DeletedCodigos[t][id.Item2].Contains(codigoToRemove))
                    _DeletedCodigos[t][id.Item2].Remove(codigoToRemove);
            };

            if (!_Deleted_OnHold.TryAdd(id, newAction)) return false;

            return true;
        }
        public bool TryRemoveFromDeletedOnHold<T>(int codigo, int idCdad, int idEjer, int codigoToRemove, ACodigoCCheckType t)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad.CantorPair(idEjer));
            Action newAction = () =>
            {
                if (_DeletedCodigos[t][id.Item2].Contains(codigoToRemove))
                    _DeletedCodigos[t][id.Item2].Remove(codigoToRemove);
            };

            if (!_Deleted_OnHold.TryAdd(id, newAction)) return false;

            return true;
        }
        public bool TryAddToNoConOnHold<T>(int codigo, int idCdad, int codigoToAdd, ACodigoCCheckType t)
            where T : IObjModelConCodigo
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad);
            List<Func<Task>> old;
            Func<Task> newTask = () => new Task(() =>
            {
                if (!_LargerNotConsecutiveCodigos[t][id.Item2].Contains(codigoToAdd))
                    _LargerNotConsecutiveCodigos[t][id.Item2].Add(codigoToAdd);
            });

            if (_LargerNotConsecutive_OnHold.TryGetValue(id, out old))
            {
                lock (_LockObject)
                {
                    if (_LargerNotConsecutive_OnHold.TryGetValue(id, out old) && !_LargerNotConsecutive_OnHold[id].Contains(newTask))
                        _LargerNotConsecutive_OnHold[id].Add(newTask);
                }
            }
            else if (!_LargerNotConsecutive_OnHold.TryAdd(id, new List<Func<Task>>() { newTask })) return false;

            return true;
        }
        public bool TryAddToNoConOnHold<T>(int codigo, int idCdad, int idEjer, int codigoToAdd, ACodigoCCheckType t)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad.CantorPair(idEjer));
            List<Func<Task>> old;
            Func<Task> newTask = () => new Task(() =>
            {
                if (!_LargerNotConsecutiveCodigos[t][id.Item2].Contains(codigoToAdd))
                    _LargerNotConsecutiveCodigos[t][id.Item2].Add(codigoToAdd);
            });

            if (_LargerNotConsecutive_OnHold.TryGetValue(id, out old))
            {
                lock (_LockObject)
                {
                    if (_LargerNotConsecutive_OnHold.TryGetValue(id, out old) && !_LargerNotConsecutive_OnHold[id].Contains(newTask))
                        _LargerNotConsecutive_OnHold[id].Add(newTask);
                }
            }
            else if (!_LargerNotConsecutive_OnHold.TryAdd(id, new List<Func<Task>>() { newTask })) return false;

            return true;
        }
        public bool TryRemoveFromNoConOnHold<T>(int codigo, int idCdad, int codigoToRemove, ACodigoCCheckType t)
            where T : IObjModelConCodigo
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad);
            List<Func<Task>> old;
            Func<Task> newTask = () => new Task(() =>
            {
                if (_LargerNotConsecutiveCodigos[t][id.Item2].Contains(codigoToRemove))
                    _LargerNotConsecutiveCodigos[t][id.Item2].Remove(codigoToRemove);
            });

            if (_LargerNotConsecutive_OnHold.TryGetValue(id, out old))
            {
                lock (_LockObject)
                {
                    if (_LargerNotConsecutive_OnHold.TryGetValue(id, out old) && _LargerNotConsecutive_OnHold[id].Contains(newTask))
                        _LargerNotConsecutive_OnHold[id].Remove(newTask);
                }
            }
            else if (!_LargerNotConsecutive_OnHold.TryAdd(id, new List<Func<Task>>() { newTask })) return false;

            return true;
        }
        public bool TryRemoveFromNoConOnHold<T>(int codigo, int idCdad, int idEjer, int codigoToRemove, ACodigoCCheckType t)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            Tuple<int, long> id = new Tuple<int, long>(codigo, idCdad.CantorPair(idEjer));
            List<Func<Task>> old;
            Func<Task> newTask = () => new Task(() =>
            {
                if (_LargerNotConsecutiveCodigos[t][id.Item2].Contains(codigoToRemove))
                    _LargerNotConsecutiveCodigos[t][id.Item2].Remove(codigoToRemove);
            });

            if (_LargerNotConsecutive_OnHold.TryGetValue(id, out old))
            {
                lock (_LockObject)
                {
                    if (_LargerNotConsecutive_OnHold.TryGetValue(id, out old) && _LargerNotConsecutive_OnHold[id].Contains(newTask))
                        _LargerNotConsecutive_OnHold[id].Remove(newTask);
                }
            }
            else if (!_LargerNotConsecutive_OnHold.TryAdd(id, new List<Func<Task>>() { newTask })) return false;

            return true;
        }
        #endregion

        #region helpers
        private bool GetIfOtherUserBegunCCheckOfType(ACodigoCCheckType type)
        {
            int checkType = (int)type;
            bool onGoing = false;
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                con.Open();
                onGoing = con.Query<bool>("SELECT COUNT(*) FROM usuarios WHERE CodigoCCheck=@uno AND TipoCCheck=@t",
                    new { uno = 1, t = checkType}).SingleOrDefault();
                con.Close();
            }

            return onGoing;
        }
        private async Task SetThisUserBegunCCheckOfTypeAsync(int userId, ACodigoCCheckType type)
        {
            int checkType = (int)type;
            this._DoingConsistencyCheck = true;
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                await con.OpenAsync().ConfigureAwait(false);
                await con.ExecuteAsync("UPDATE usuarios SET CodigoCCheck=@uno,TipoCCheck=@t WHERE Id=@id",
                    new { uno = 1, t = checkType })
                    .ConfigureAwait(false);
                con.Close();
            }
        }
#pragma warning disable CS0628 // New protected member declared in sealed class
        //Protected members for use with AutoCodigoData.AutoCodigoConsistencyCheker
        protected void AddOrUpdateMax(ACodigoCCheckType t, long id, int newMax)
        {
            if (!this._MaxCodigos.ContainsKey(t))
                this._MaxCodigos.Add(t, new Dictionary<long, int>() { { id, newMax } });
            else if (!this.MaxCodigos[t].ContainsKey(id))
                this._MaxCodigos[t].Add(id, newMax);
            else this._MaxCodigos[t][id] = newMax;
        }
        protected void AddOrUnionNoCon(ACodigoCCheckType t, long id, IEnumerable<int> noCon)
        {
            if (!this._LargerNotConsecutiveCodigos.ContainsKey(t))
                this._LargerNotConsecutiveCodigos.Add(t, new Dictionary<long, List<int>>() { { id, noCon.ToList() } });
            else if (!this._LargerNotConsecutiveCodigos[t].ContainsKey(id))
                this._LargerNotConsecutiveCodigos[t].Add(id, noCon.ToList());
            else this._LargerNotConsecutiveCodigos[t][id] = this._LargerNotConsecutiveCodigos[t][id].Union(noCon).ToList();
        }
        protected void TryRemoveFromNoCon(ACodigoCCheckType t, long id, IEnumerable<int> noCon)
        {
            if (!this._LargerNotConsecutiveCodigos.ContainsKey(t) || !this._LargerNotConsecutiveCodigos[t].ContainsKey(id)) return;
            else this._LargerNotConsecutiveCodigos[t][id] = this._LargerNotConsecutiveCodigos[t][id].Except(noCon).ToList();
        }
        protected void TryRemoveFromDeleted(ACodigoCCheckType t, long id, IEnumerable<int> deleted)
        {
            if (!this._DeletedCodigos.ContainsKey(t) || !this._DeletedCodigos[t].ContainsKey(id)) return;
            else this._DeletedCodigos[t][id] = this._DeletedCodigos[t][id].Except(deleted).ToList();
        }
#pragma warning restore CS0628 // New protected member declared in sealed class
        #endregion

        #region consistency and others
        public void CheckConsistencyOfType(int userId, ACodigoCCheckType type)
        {
            if (type == ACodigoCCheckType.All) throw new NotImplementedException();

            if (!this._DoingConsistencyCheck && !GetIfOtherUserBegunCCheckOfType(type))
            {
                Task.Run(() => SetThisUserBegunCCheckOfTypeAsync(userId, type)).Forget().ConfigureAwait(false);
                AutoCodigoConsistencyChecker checker = new AutoCodigoConsistencyChecker(this, type);
                Task.Run(() => checker.CheckConsistencyOfTypeAsync(type)).Forget().ConfigureAwait(false);
            }
            else
            {
                Task.Run(() =>
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

                    bool onGoingCheck = true;
                    while (onGoingCheck && watch.ElapsedMilliseconds < 120000/*2 minutes*/)
                    {
                        Task delay = Task.Run(() => Task.Delay(5000));
                        delay.Wait();
                        onGoingCheck = this._DoingConsistencyCheck || GetIfOtherUserBegunCCheckOfType(type);
                    }

                    //TODO: reescribir mensaje de fallo => ¿relacionado con la progress bar?
                    if (onGoingCheck)
                        throw new CustomException_AutoCodigo(
                            "AutoCodigoData.CheckConsistency: El sistema ha estado ocupado más de 2 minutos ejecutando un chequeo en los códigos de la base de datos.");
                    else
                    {
                        Task.Run(() => SetThisUserBegunCCheckOfTypeAsync(userId, type)).Forget().ConfigureAwait(false);
                        AutoCodigoConsistencyChecker checker = new AutoCodigoConsistencyChecker(this, type);
                        Task.Run(() => checker.CheckConsistencyOfTypeAsync(type)).Forget().ConfigureAwait(false);
                    }
                })
                .Forget().ConfigureAwait(false);
            }
            //TODO: Progress bar?
        }
        public void ReArrangeAsientos<T>(bool byDate)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            //TODO
        }
        #endregion
    }
}
