//#define SQLSERVER //BORRAME Ó COMENTAME CON -> //

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Extensions;

namespace AdConta.Models
{
    public abstract class aAutoCodigoBase
    {
        public aAutoCodigoBase(AutoCodigoData data, int currentCodigo)
        {
            this._Data = data;
            this.CodigoOnHold = -1;
            this.ChangesAppliedOrCancelled = false;
            this.ChangesErrorString = "";
            this.CurrentCodigo = currentCodigo;
        }

        #region fields
        protected AutoCodigoData _Data;
        //private readonly string _strCon = @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = e:\AdConta\PruebaDapper\PruebaDapper\PruebaDapperDB.mdf; Integrated Security = True";
        //GlobalSettings.Properties.Settings.Default.conta1ConnectionString;
        #endregion

        #region properties
        public bool ChangesAppliedOrCancelled { get; set; }
        public int CodigoOnHold { get; protected set; }
        public int CurrentCodigo { get; protected set; }
        public string ChangesErrorString { get; set; }
        public abstract ACodigoCCheckType CCheckType { get; protected set; }
        #endregion

        #region helpers
        //protected bool TypeHaveCodigo(Type type)
        //{
        //    if (type == typeof(Comunidad) ||
        //       type == typeof(Finca) ||
        //       type == typeof(Presupuesto) ||
        //       type == typeof(Asiento))
        //        return true;
        //    return false;
        //}
        protected ACodigoCCheckType GetCCheckType<T>()
            where T : IConCodigo
        {
            //Type t = typeof(T);
            //if (t == typeof(Comunidad)) return ACodigoCCheckType.Comunidad;
            //else if (t == typeof(Finca)) return ACodigoCCheckType.Fincas;
            //else if (t == typeof(Presupuesto)) return ACodigoCCheckType.Pptos;
            //else if (t == typeof(Asiento)) return ACodigoCCheckType.Asientos;

            //return ACodigoCCheckType.All;
            return this.CCheckType;
        }
        protected abstract bool TypeHaveAllIdOwnersAndSetIdOwners(int? IdComunidad = null, int? IdEjercicio = null);
        protected IEnumerable<bool> Infinite()
        {
            while (true) yield return true;
        }
        protected int ReverseFindFirstCodigoNotDeleted(int initialCodigo, ACodigoCCheckType objModelType, long lIdCdad)
        {
            int codigo = initialCodigo;

            //Parallel.ForEach(Infinite(), (ignored, loopState) =>
            //{
            //    if (!this._Data.DeletedCodigos[objModelType][lIdCdad].Contains(codigo)) codigo--;
            //    else loopState.Stop();
            //});

            while (this._Data.DeletedCodigos[objModelType][lIdCdad].Contains(codigo) && codigo != 0) codigo--;

            return codigo;
        }
        protected bool TrySetOnHoldAndGetIfConsistencyCheckNeeded<T>(ref int newCodigo, int idCdad)
            where T : IObjModelConCodigo
        {
            if (!this._Data.TrySetCodigoOnHold<T>(this, idCdad))
            {
                bool onHold = true;
                while (onHold)
                {
                    newCodigo++;
                    this.CodigoOnHold = newCodigo;
                    onHold = this._Data.TrySetCodigoOnHold<T>(this, idCdad);
                }

                return true;
            }
            return false;
        }
        protected bool TrySetOnHoldAndGetIfConsistencyCheckNeeded<T>(ref int newCodigo, int idCdad, int idEjer)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            if (!this._Data.TrySetCodigoOnHold<T>(this, idCdad, idEjer))
            {
                bool onHold = true;
                while (onHold)
                {
                    newCodigo++;
                    this.CodigoOnHold = newCodigo;
                    onHold = this._Data.TrySetCodigoOnHold<T>(this, idCdad, idEjer);
                }

                return true;
            }
            return false;
        }
        protected void TryAddSQLAndThrowExceptions<T>(
            ACodigoSQLType SQLt, 
            string cancelExceptionMsg, 
            string SQLExceptionMsg, 
            int codigo,
            int id, 
            string SQL, 
            ExpandoObject values, 
            bool max, 
            int noconCount, 
            bool nocon,
            bool prevMax = false)
            where T : IObjModelConCodigo
        {
            string msgData = $@"
Type: {typeof(T)}
SQL: {SQL}
codigo: {codigo}
Comunidad: {id}";
            cancelExceptionMsg = cancelExceptionMsg.Append(msgData);
            SQLExceptionMsg = SQLExceptionMsg.Append(msgData);

            switch (SQLt)
            {
                case ACodigoSQLType.Next:
                    if (!this._Data.TryAddNextCodigoSQLOnHold<T>(this, id, SQL, values, noconCount))
                    {
                        this.CodigoOnHold = -1;
                        if (!Task.Run(() => this._Data.TryCancelCodigoChangesAsync<T>(codigo, id, this)).ConfigureAwait(false).GetAwaiter().GetResult())
                            throw new CustomException_AutoCodigo(cancelExceptionMsg);
                        else throw new CustomException_AutoCodigo(SQLExceptionMsg);
                    }
                    break;
                case ACodigoSQLType.Deleted:
                    if (!this._Data.TryAddDeletedCodigoSQLOnHold<T>(this.CurrentCodigo, id, SQL, values, nocon, max))
                    {
                        this.CodigoOnHold = -1;
                        if (!Task.Run(() => this._Data.TryCancelCodigoChangesAsync<T>(this.CurrentCodigo, id, this)).ConfigureAwait(false).GetAwaiter().GetResult())
                            throw new CustomException_AutoCodigo(cancelExceptionMsg);
                        else throw new CustomException_AutoCodigo(SQLExceptionMsg);
                    }
                    break;
                case ACodigoSQLType.Check:
                    if (!this._Data.TryAddCheckCodigoSQLOnHold<T>(this.CurrentCodigo, id, SQL, (ExpandoObject)values, prevMax, max))
                    {
                        this.CodigoOnHold = -1;
                        if (!Task.Run(() => this._Data.TryCancelCodigoChangesAsync<T>(codigo, id, this)).ConfigureAwait(false).GetAwaiter().GetResult())
                            throw new CustomException_AutoCodigo(cancelExceptionMsg);
                        else throw new CustomException_AutoCodigo(SQLExceptionMsg);
                    }
                    break;
            }
        }
        protected void TryAddSQLAndThrowExceptions<T>(
            ACodigoSQLType SQLt,
            string cancelExceptionMsg,
            string SQLExceptionMsg,
            int codigo,
            Tuple<int,int> id,
            string SQL,
            ExpandoObject values,
            bool max,
            int noconCount,
            bool nocon,
            bool prevMax = false)
            where T : IObjModelConCodigoConComunidadYEjercicio
        {
            string msgData = $@"
Type: {typeof(T)}
SQL: {SQL}
codigo: {codigo}
Comunidad: {id.Item1}
Ejercicio: {id.Item2}";
            cancelExceptionMsg = cancelExceptionMsg.Append(msgData);
            SQLExceptionMsg = SQLExceptionMsg.Append(msgData);

            switch (SQLt)
            {
                case ACodigoSQLType.Next:
                    if (!this._Data.TryAddNextCodigoSQLOnHold<T>(this, id.Item1, id.Item2, SQL, values, noconCount))
                    {
                        this.CodigoOnHold = -1;
                        if (!Task.Run(() => this._Data.TryCancelCodigoChangesAsync<T>(codigo, id.Item1, id.Item2, this)).ConfigureAwait(false).GetAwaiter().GetResult())
                            throw new CustomException_AutoCodigo(cancelExceptionMsg);
                        else throw new CustomException_AutoCodigo(SQLExceptionMsg);
                    }
                    break;
                case ACodigoSQLType.Deleted:
                    if (!this._Data.TryAddDeletedCodigoSQLOnHold<T>(this.CurrentCodigo, id.Item1, id.Item2, SQL, values, nocon, max))
                    {
                        this.CodigoOnHold = -1;
                        if (!Task.Run(() => this._Data.TryCancelCodigoChangesAsync<T>(this.CurrentCodigo, id.Item1, id.Item2, this)).ConfigureAwait(false).GetAwaiter().GetResult())
                            throw new CustomException_AutoCodigo(cancelExceptionMsg);
                        else throw new CustomException_AutoCodigo(SQLExceptionMsg);
                    }
                    break;
                case ACodigoSQLType.Check:
                    return;
            }
        }
        #endregion

        #region public methods
        public virtual int GetNextCodigo(int? idComunidad = null)
        {
            throw new NotImplementedException();
        }
        public virtual int GetNextCodigo(Tuple<int,int> ids)
        {
            throw new NotImplementedException();
        }
        public virtual void DeleteCodigo(int? idComunidad = null)
        {
            throw new NotImplementedException();
        }
        public virtual void DeleteCodigo(Tuple<int, int> ids)
        {
            throw new NotImplementedException();
        }
        public virtual bool CheckCodigoIsAvailableAndTakeIt(int codigo, int? idComunidad = null)
        {
            throw new NotImplementedException();
        }

        public virtual bool ApplyChanges(int idCdad)
        {
            throw new NotImplementedException();
        }
        public virtual bool ApplyChanges(Tuple<int, int> ids)
        {
            throw new NotImplementedException();
        }
        public virtual bool CancelChanges(int idCdad)
        {
            throw new NotImplementedException();
        }
        public virtual bool CancelChanges(Tuple<int, int> ids)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public sealed class AutoCodigoNoOwner<T> : aAutoCodigoBase
        where T : IObjModelConCodigo
    {
        public AutoCodigoNoOwner(AutoCodigoData data, int currentCodigo = 0) : base(data, currentCodigo)
        {
            this.CCheckType = ACodigoCCheckType.Comunidad;
        }

        #region properties
        public int IdOwnerComunidad { get; private set; }
        public override ACodigoCCheckType CCheckType { get; protected set; }
        #endregion

        #region helpers
        protected override bool TypeHaveAllIdOwnersAndSetIdOwners(int? IdComunidad = null, int? IdEjercicio = null)
        {
            this.IdOwnerComunidad = 0;
            return true;
        }
        #endregion

        #region public methods
        public override int GetNextCodigo(int? idComunidad = null)
        {
            if (!TypeHaveAllIdOwnersAndSetIdOwners(idComunidad))
                throw new CustomException_AutoCodigo($@"El tipo {CCheckType} no ha sido acompañado de los correspondientes IdOwners: 
IdComunidad = 0
IdEjercicio = {null}");

            if (this.CodigoOnHold != -1)
                throw new CustomException_AutoCodigo("Este código ya está siendo modificado, cancele o termine los cambios anteriores.");

            var values = new ExpandoObject() as IDictionary<string, object>;
            values.Add("cero", 0);
            values.Add("uno", 1);

            string table = base._Data.TableNames[CCheckType];
            bool deletedCodigoIsOnHold = false;
            bool consistencyCheckNeeded = false;
            //If there are some codigos deleted
            if (base._Data.DeletedCodigos[CCheckType][0].Count > 0) //base._Data.DeletedCodigos[objModelType].ContainsKey(lIdCdad))
            {
                //Take the minimum one
                //int codigo = base._Data.DeletedCodigos[objModelType][lIdCdad].Min();
                //Check if the same codigo have been taken by other user
                //this.CodigoOnHold = codigo;
                this.CodigoOnHold = base._Data.DeletedCodigos[CCheckType][0].Min();
                if (!base._Data.TrySetCodigoOnHold<T>(this, 0))
                {
                    List<int> deletedCodigos = new List<int>(base._Data.DeletedCodigos[CCheckType][0]).OrderBy(x => x).ToList();
                    deletedCodigoIsOnHold = !base._Data.TrySetCodigoOnHold<T>(this, 0);
                    int i = 0;
                    while (deletedCodigoIsOnHold && i <= deletedCodigos.Count)
                    {
                        i++;
                        this.CodigoOnHold = deletedCodigos[i];
                        deletedCodigoIsOnHold = !base._Data.TrySetCodigoOnHold<T>(this, 0);
                    }
                }
                if (!deletedCodigoIsOnHold)
                {
                    //The codigo is taken, so it's no more deleted    
                    //Update dictionary
                    base._Data.TryRemoveFromDeletedOnHold<T>(this.CurrentCodigo, 0, this.CodigoOnHold, CCheckType);
                    //Update DB
                    string SQL = $"UPDATE {table} SET Deleted = @cero WHERE Codigo=@cod;{Environment.NewLine}";
                    values.Add("cod", this.CodigoOnHold);

                    string cancelException = $@"AutoCodigo.GetNextCodigo: Error al intentar cancelar cambios por no ser posible ocupar nuevo código:
No se pudo ocupar el codigo por error al intentar añadir SQLOnHold.";
                    string SQLException = $"AutoCodigo.GetNextCodigo: Error al intentar añadir SQLOnHold:";
                    int codigo = this.CodigoOnHold;
                    TryAddSQLAndThrowExceptions<T>(
                        ACodigoSQLType.Next, cancelException, SQLException, codigo, 0, SQL, (ExpandoObject)values, false, 0, false);

                    //return taken codigo
                    return this.CodigoOnHold;
                }
            }
            //else
            if (base._Data.DeletedCodigos[CCheckType][0].Count == 0 || deletedCodigoIsOnHold)
            {
                var sb = new StringBuilder();//$"START TRANSACTION;{Environment.NewLine}");
                int maxCodigo;
                List<int> noConToRemove = new List<int>();

                //If there isn't any max codigo for this Comunidad, it means there is no codigo at all, so the next one is the first one and the max
                if (!base._Data.MaxCodigos[CCheckType].ContainsKey(0))
                {
                    maxCodigo = 1;
                    this.CodigoOnHold = maxCodigo;
                    consistencyCheckNeeded = TrySetOnHoldAndGetIfConsistencyCheckNeeded<T>(ref maxCodigo, 0);
                }
                else
                {
                    int previous = base._Data.MaxCodigos[CCheckType][0];
                    maxCodigo = previous + 1;

                    //Find next available not consecutive codigo IF there are any.
                    //First sum one to the max codigo and check if there's a not consecutive codigo occupied, keep summing until one free is found.
                    //Update both dictionary and DB setting codigos as not consecutive
                    if (base._Data.LargerNotConsecutiveCodigos[CCheckType].ContainsKey(0))
                    {
                        SemaphoreSlim sphr = new SemaphoreSlim(1, 1);
                        maxCodigo = Task.Run(() =>
                        {
                            int newcodigo = maxCodigo;
                            this.CodigoOnHold = maxCodigo;
                            bool newCodigoFound = !base._Data.LargerNotConsecutiveCodigos[CCheckType][0].Contains(maxCodigo)
                                && base._Data.TrySetCodigoOnHold<T>(this, 0);

                            while (!newCodigoFound)
                            {
                                if (base._Data.LargerNotConsecutiveCodigos[CCheckType][0].Contains(maxCodigo))
                                {
                                    sphr.WaitAsync();
                                    //Since we have added 1 and found it, this codigo isn't anymore not consecutive, therefore remove not consecutive state...
                                    sb.Append($"UPDATE {table} SET NoConsecutivo=@cero WHERE Codigo=@cod{maxCodigo};{Environment.NewLine}");
                                    values.Add($"cod{maxCodigo}", maxCodigo);
                                    //...and add codigo to list of to-be-removed-from-NotConsecutive
                                    noConToRemove.Add(maxCodigo);

                                    newcodigo++;
                                    this.CodigoOnHold = newcodigo;
                                    sphr.Release();
                                }
                                else if (base._Data.TrySetCodigoOnHold<T>(this, 0)) newCodigoFound = true;
                                else newcodigo++;
                            }

                            return newcodigo;
                        })
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    else
                        consistencyCheckNeeded = TrySetOnHoldAndGetIfConsistencyCheckNeeded<T>(ref maxCodigo, 0);

                    //Update dictionary with all codigos in the list to-be-removed
                    if (noConToRemove.Count > 0)
                        noConToRemove.ForEach(x => base._Data.TryRemoveFromNoConOnHold<T>(this.CurrentCodigo, 0, x, CCheckType));

                    //Previous maximum isn't the maximum anymore, update DB
                    sb.Append($"UPDATE {table} SET maximo=@cero WHERE codigo=@previous;{Environment.NewLine}");
                    values.Add("previous", previous);
                }
                //New codigo is the new maximum, update dictionary
                base._Data.TrySetNewMaxOnHold<T>(this.CurrentCodigo, 0, maxCodigo);

                //Insert new codigo into DB
                sb.Append($"INSERT INTO {table} (codigo,deleted,noconsecutivo,maximo) VALUES (@codfinal,@cero,@cero,@uno);{Environment.NewLine}");
#if SQLSERVER
                sb.Append($"SELECT MAX(Codigo) AS LastId FROM {table};");//BORRAME
#endif
                values.Add("codfinal", maxCodigo);
                
                string cancelException = $@"AutoCodigo.GetNextCodigo: Error al intentar cancelar cambios por no ser posible ocupar nuevo código:
No se pudo ocupar el codigo por Error al intentar añadir SQLOnHold.";
                string SQLException = $@"AutoCodigo.GetNextCodigo: Error al intentar añadir SQLOnHold:";
                TryAddSQLAndThrowExceptions<T>(
                    ACodigoSQLType.Next, cancelException, SQLException, maxCodigo, 0, sb.ToString(), (ExpandoObject)values, false, noConToRemove.Count, false);

                //Do consistency check if needed
                if (consistencyCheckNeeded) base._Data.CheckConsistencyOfType(this._Data.UsuarioLogueado.Id, GetCCheckType<T>());
                //return taken codigo
                return maxCodigo;
            }
            return 0;
        }
        public override void DeleteCodigo(int? idComunidad = null)
        {
            //if (!TypeHaveCodigo(objModelType))
            //    throw new CustomException_AutoCodigo($"El tipo {objModelType.Name} no usa codigo. AutoCodigo.DeleteCodigo(Type objModelType, int codigo).");
            if (!TypeHaveAllIdOwnersAndSetIdOwners(idComunidad))
                throw new CustomException_AutoCodigo($@"El tipo {CCheckType} no ha sido acompañado de los correspondientes IdOwners: 
IdComunidad = 0
IdEjercicio = {null}");

            if (this.CodigoOnHold != -1)
                throw new CustomException_AutoCodigo("Este código ya está siendo modificado o ha sido borrado, cancele o termine los cambios.");

            this.CodigoOnHold = 0;
            if (!base._Data.TrySetCodigoOnHold<T>(this, (int)this.IdOwnerComunidad))
            {
                this.CodigoOnHold = -1;
                throw new CustomException_AutoCodigo("Este código ya está siendo modificado o ha sido borrado, cancele o termine los cambios.");
            }

            //If DeletedCodigos contains codigo, codigo is already deleted so no more to do
            if (base._Data.DeletedCodigos[CCheckType][(int)this.IdOwnerComunidad].Contains(this.CurrentCodigo)) return;

            var sb = new StringBuilder();
            var values = new ExpandoObject() as IDictionary<string, object>;
            values.Add("cero", 0);
            values.Add("uno", 1);

            string table = base._Data.TableNames[CCheckType];
            
            int currentMax = base._Data.MaxCodigos[CCheckType][0];
            //If codigo is greater than max it's not taken so it can't be deleted, except if it is one of the not consecutive codigos already taken
            if (this.CurrentCodigo > currentMax && base._Data.LargerNotConsecutiveCodigos[CCheckType][0].Contains(this.CurrentCodigo))
            {
                //Codigo is taken and is one of the not consecutive codigos
                //Don't set not consecutive codigos as deleted because they don't interfere with the rest of the codigos, just remove them
                sb.Append($"DELETE FROM {table} WHERE Codigo=@cod;{Environment.NewLine}");
                values.Add("cod", this.CurrentCodigo);
                base._Data.TryRemoveFromNoConOnHold<T>(this.CurrentCodigo, 0, this.CurrentCodigo, CCheckType);
                
                string cancelException = $@"AutoCodigo.DeleteCodigo: Error al intentar cancelar cambios por no ser posible borrar código:
No se pudo borrar el codigo por error al intentar añadir SQLOnHold.";
                string SQLException = $@"AutoCodigo.DeleteCodigo: Error al intentar añadir SQLOnHold:";
                TryAddSQLAndThrowExceptions<T>(
                    ACodigoSQLType.Deleted, cancelException, SQLException, this.CurrentCodigo, 0, sb.ToString(), (ExpandoObject)values, false, 0, true);

                return;
            }
            else
            {
                if (this.CurrentCodigo == currentMax)
                {
                    //The current max is about to be deleted, so the previous one have to be the max except it is deleted too
                    //if that's the case go to the previous one, and so on until find the new max not deleted
                    int newMax = ReverseFindFirstCodigoNotDeleted(this.CurrentCodigo - 1, CCheckType, 0);
                    
                    sb.Append($"UPDATE {table} SET Maximo=@uno WHERE Codigo=@newMax;{Environment.NewLine}");
                    values.Add("newMax", newMax);
                    base._Data.TrySetNewMaxOnHold<T>(this.CurrentCodigo, 0, newMax);

                    //Now the old codigo is not consecutive, since it's greater than max, and have to be deleted, so remove it
                    sb.Append($"DELETE FROM {table} WHERE Codigo=@cod;{Environment.NewLine}");
                    values.Add("cod", this.CurrentCodigo);

                    string cEx = $@"AutoCodigo.DeleteCodigo: Error al intentar cancelar cambios por no ser posible borrar código:
No se pudo borrar el codigo por error al intentar añadir SQLOnHold.";
                    string SQLEx = "AutoCodigo.DeleteCodigo: Error al intentar añadir SQLOnHold:";
                    TryAddSQLAndThrowExceptions<T>(
                        ACodigoSQLType.Deleted, cEx, SQLEx, this.CurrentCodigo, 0, sb.ToString(), (ExpandoObject)values, true, 0, false);

                    return;
                }
                //Codigo is lesser than max, just update deleted
                base._Data.TryAddToDeletedOnHold<T>(this.CurrentCodigo, 0, this.CurrentCodigo, CCheckType);
                sb.Append($"UPDATE {table} SET Deleted=@uno,Maximo=@cero WHERE Codigo=@cod;");
                values.Add("cod", this.CurrentCodigo);

                string cancelException = $@"AutoCodigo.DeleteCodigo: Error al intentar cancelar cambios por no ser posible borrar código:
No se pudo borrar el codigo por error al intentar añadir SQLOnHold.";
                string SQLException = "AutoCodigo.DeleteCodigo: Error al intentar añadir SQLOnHold:";
                TryAddSQLAndThrowExceptions<T>(
                    ACodigoSQLType.Deleted, cancelException, SQLException, this.CurrentCodigo, 0, sb.ToString(), (ExpandoObject)values, false, 0, false);
            }
        }
        public override bool CheckCodigoIsAvailableAndTakeIt(int codigo, int? idComunidad = null)
        {
            if (!TypeHaveAllIdOwnersAndSetIdOwners(idComunidad))
                throw new CustomException_AutoCodigo($@"El tipo {CCheckType} no ha sido acompañado de los correspondientes IdOwners: 
IdComunidad = 0
IdEjercicio = {null}");

            if (this.CodigoOnHold != -1)
                throw new CustomException_AutoCodigo("Este código ya está siendo modificado, cancele o termine los cambios anteriores.");
            this.CodigoOnHold = codigo;
            if (!base._Data.TrySetCodigoOnHold<T>(this, (int)this.IdOwnerComunidad))
            {
                this.CodigoOnHold = -1;
                return false;
            }

            var sb = new StringBuilder();//$"START TRANSACTION;{Environment.NewLine}");
            var values = new ExpandoObject() as IDictionary<string, object>;
            values.Add("cero", 0);
            values.Add("uno", 1);

            string table = base._Data.TableNames[CCheckType];

            bool isMax = false;
            int currentMax = base._Data.MaxCodigos[CCheckType][0];
            if (codigo > currentMax)
            {
                if (base._Data.LargerNotConsecutiveCodigos[CCheckType][0].Contains(codigo))
                {
                    this.CodigoOnHold = -1;
                    if (!Task.Run(() => base._Data.TryCancelCodigoChangesAsync<T>(codigo, 0, this)).ConfigureAwait(false).GetAwaiter().GetResult())
                        throw new CustomException_AutoCodigo(
                            $@"AutoCodigo.CheckCodigoIsAvailableAndTakeIt: Error al intentar cancelar cambios por no ser posible ocupar nuevo código:
No se pudo ocupar el codigo por ya estar ocupado por otro codigo no consecutivo.
Type: {CCheckType}
SQL: {sb.ToString()}
codigo: {codigo}
Comunidad: 0");

                    return false;
                }
                else if (codigo == currentMax + 1)
                {
                    sb.Append($"UPDATE {table} SET Maximo=@cero WHERE Codigo=@prevMax;{Environment.NewLine}");
                    values.Add("prevMax", currentMax);
                    base._Data.TrySetNewMaxOnHold<T>(codigo, 0, codigo);
                    isMax = true;
                }
                else base._Data.TryAddToNoConOnHold<T>(this.CurrentCodigo, 0, codigo, CCheckType);

                sb.Append($"INSERT INTO {table} (Codigo,Deleted,NoConsecutivo,Maximo) VALUES (@cod,@cero,@uno,@max);");
                values.Add("max", Convert.ToInt32(isMax));
                values.Add("cod", codigo);

                string cancelException = $@"AutoCodigo.CheckCodigoIsAvailableAndTakeIt: Error al intentar cancelar cambios por no ser posible ocupar nuevo código:
No se pudo ocupar el código por error al intentar añadir SQLOnHold.";
                string SQLException = $@"AutoCodigo.CheckCodigoIsAvailableAndTakeIt: Error al intentar añadir SQLOnHold:";
                TryAddSQLAndThrowExceptions<T>(
                    ACodigoSQLType.Check, cancelException, SQLException, codigo, 0, sb.ToString(), (ExpandoObject)values, isMax, 0, false, true);

                return true;
            }
            else
            {
                if (!base._Data.DeletedCodigos[CCheckType][0].Contains(codigo))
                {
                    this.CodigoOnHold = -1;
                    if (!Task.Run(() => base._Data.TryCancelCodigoChangesAsync<T>(codigo, 0, this)).ConfigureAwait(false).GetAwaiter().GetResult())
                        throw new CustomException_AutoCodigo(
                            $@"AutoCodigo.CheckCodigoIsAvailableAndTakeIt: Error al intentar cancelar cambios por no ser posible ocupar nuevo código:
No se pudo ocupar el codigo por ya estar ocupado por otro codigo.
Type: {CCheckType}
SQL: {sb.ToString()}
codigo: {codigo}
Comunidad: 0");

                    return false;
                }

                sb.Append($"UPDATE codigo SET deleted=0 UPDATE {table} SET Deleted=@cero WHERE Codigo=@cod;");
                values.Add("cod", codigo);
                base._Data.TryRemoveFromDeletedOnHold<T>(this.CurrentCodigo, 0, codigo, CCheckType);

                string cancelException = $@"AutoCodigo.CheckCodigoIsAvailableAndTakeIt: Error al intentar cancelar cambios por no ser posible ocupar nuevo código:
No se pudo ocupar el codigo por error al intentar añadir SQLOnHold.";
                string SQLException = $"AutoCodigo.CheckCodigoIsAvailableAndTakeIt: Error al intentar añadir SQLOnHold:";
                TryAddSQLAndThrowExceptions<T>(
                    ACodigoSQLType.Check, cancelException, SQLException, codigo, 0, sb.ToString(), (ExpandoObject)values, false, 0, false);

                return true;
            }
        }

        public override bool ApplyChanges(int idCdad)
        {
            //No codigo on hold to apply changes or changes already applied
            if (this.CodigoOnHold == -1 || this.ChangesAppliedOrCancelled) return false;

            //Task<bool> apply = Task.Run(() => base._Data.ApplyCodigoChanges<T>(codigo, idCdad, this));
            this.ChangesAppliedOrCancelled = Task.Run(() => base._Data.TryApplyCodigoChangesAsync<T>(idCdad, this))
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (!this.ChangesAppliedOrCancelled) return false;
            else
            {
                this.ChangesAppliedOrCancelled = false;
                this.CurrentCodigo = CodigoOnHold;
                this.CodigoOnHold = -1;
                return true;
            }
        }
        public override bool CancelChanges(int idCdad)
        {
            //No codigo on hold to apply changes or changes already applied
            if (this.CodigoOnHold == -1 || this.ChangesAppliedOrCancelled) return false;

            this.ChangesAppliedOrCancelled = Task.Run(() => base._Data.TryCancelCodigoChangesAsync<T>(null, idCdad, this))
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (!this.ChangesAppliedOrCancelled) return false;
            else
            {
                this.ChangesAppliedOrCancelled = false;
                this.CodigoOnHold = -1;
                return true;
            }
        }        
        #endregion
    }

    public sealed class AutoCodigoOwnerCdad<T> : aAutoCodigoBase
        where T : IObjModelConCodigoConComunidad
    {
        public AutoCodigoOwnerCdad(AutoCodigoData data, ACodigoCCheckType type, int currentCodigo = 0) : base(data, currentCodigo)
        {
            if (type != ACodigoCCheckType.Fincas || type != ACodigoCCheckType.Pptos)
                throw new CustomException_AutoCodigo(
                    "AutoCodigoOwnerCdad.Constructor: Esta clase debe tener un ACodigoCCheckType tipo Fincas o Pptos.");
            this.CCheckType = type;
        }

        #region properties
        public int IdOwnerComunidad { get; private set; }
        public override ACodigoCCheckType CCheckType { get; protected set; }
        #endregion

        #region helpers
        protected override bool TypeHaveAllIdOwnersAndSetIdOwners(int? IdComunidad = null, int? IdEjercicio = null)
        {
            if (IdComunidad == null) return false;

            this.IdOwnerComunidad = (int)IdComunidad;
            return true;
        }
        #endregion

        #region public methods
        public override int GetNextCodigo(int? idComunidad = null)
        {
            if (!TypeHaveAllIdOwnersAndSetIdOwners(idComunidad))
                throw new CustomException_AutoCodigo($@"El tipo {CCheckType} no ha sido acompañado de los correspondientes IdOwners: 
IdComunidad = {idComunidad}
IdEjercicio = {null}");

            if (this.CodigoOnHold != -1)
                throw new CustomException_AutoCodigo("Este código ya está siendo modificado, cancele o termine los cambios anteriores.");

            string ownersClause = "";
            string ownersColumns = "";
            string ownersValues = "";
            var values = new ExpandoObject() as IDictionary<string, object>;
            if (idComunidad != null)
            {
                values.Add("idCdad", idComunidad);
                ownersClause = " AND IdOwnerComunidad=@idCdad";
                ownersColumns = ",IdOwnerComunidad";
                ownersValues = ",@idCdad";
            }
            values.Add("cero", 0);
            values.Add("uno", 1);

            long lIdCdad = (long)this.IdOwnerComunidad;
            int iIdCdad = (int)this.IdOwnerComunidad;
            string table = base._Data.TableNames[CCheckType];
            bool deletedCodigoIsOnHold = false;
            bool consistencyCheckNeeded = false;
            //If there are some codigos deleted
            if (base._Data.DeletedCodigos[CCheckType][lIdCdad].Count > 0) //base._Data.DeletedCodigos[objModelType].ContainsKey(lIdCdad))
            {
                //Take the minimum one
                int codigo = base._Data.DeletedCodigos[CCheckType][lIdCdad].Min();
                //Check if the same codigo have been taken by other user
                this.CodigoOnHold = codigo;
                if (!base._Data.TrySetCodigoOnHold<T>(this, iIdCdad))
                {
                    List<int> deletedCodigos = new List<int>(base._Data.DeletedCodigos[CCheckType][lIdCdad]).OrderBy(x => x).ToList();
                    deletedCodigoIsOnHold = !base._Data.TrySetCodigoOnHold<T>(this, iIdCdad);
                    int i = 0;
                    while (deletedCodigoIsOnHold && i <= deletedCodigos.Count)
                    {
                        i++;
                        this.CodigoOnHold = deletedCodigos[i];
                        deletedCodigoIsOnHold = !base._Data.TrySetCodigoOnHold<T>(this, iIdCdad);
                    }
                }
                if (!deletedCodigoIsOnHold)
                {
                    //The codigo is taken, so it's no more deleted    
                    //Update dictionary
                    base._Data.TryRemoveFromDeletedOnHold<T>(this.CurrentCodigo, iIdCdad, codigo, CCheckType);
                    //Update DB
                    string SQL = $"UPDATE {table} SET Deleted = @cero WHERE Codigo=@cod{ownersClause};{Environment.NewLine}";
                    values.Add("cod", codigo);

                    string cancelException = $@"AutoCodigo.GetNextCodigo: Error al intentar cancelar cambios por no ser posible ocupar nuevo código:
No se pudo ocupar el codigo por error al intentar añadir SQLOnHold.";
                    string SQLException = $"AutoCodigo.GetNextCodigo: Error al intentar añadir SQLOnHold:";
                    TryAddSQLAndThrowExceptions<T>(
                        ACodigoSQLType.Next, cancelException, SQLException, codigo, iIdCdad, SQL, (ExpandoObject)values, false, 0, false);

                    //return taken codigo
                    return codigo;
                }
            }
            //else
            if (base._Data.DeletedCodigos[CCheckType][lIdCdad].Count == 0 || deletedCodigoIsOnHold)
            {
                var sb = new StringBuilder();//$"START TRANSACTION;{Environment.NewLine}");
                int maxCodigo;
                List<int> noConToRemove = new List<int>();

                //If there isn't any max codigo for this Comunidad, it means there is no codigo at all, so the next one is the first one and the max
                if (!base._Data.MaxCodigos[CCheckType].ContainsKey(lIdCdad))
                {
                    maxCodigo = 1;
                    this.CodigoOnHold = maxCodigo;
                    consistencyCheckNeeded = TrySetOnHoldAndGetIfConsistencyCheckNeeded<T>(ref maxCodigo, iIdCdad);
                }
                else
                {
                    int previous = base._Data.MaxCodigos[CCheckType][lIdCdad];
                    maxCodigo = previous + 1;
                    this.CodigoOnHold = maxCodigo;

                    //Find next available not consecutive codigo IF there are any.
                    //First sum one to the max codigo and check if there's a not consecutive codigo occupied, keep summing until one free is found.
                    //Update both dictionary and DB setting codigos as not consecutive
                    if (base._Data.LargerNotConsecutiveCodigos[CCheckType].ContainsKey(lIdCdad))
                    {
                        SemaphoreSlim sphr = new SemaphoreSlim(1, 1);
                        maxCodigo = Task.Run(() =>
                        {
                            int newcodigo = maxCodigo;
                            bool newCodigoFound = !base._Data.LargerNotConsecutiveCodigos[CCheckType][lIdCdad].Contains(maxCodigo)
                                && base._Data.TrySetCodigoOnHold<T>(this, iIdCdad);

                            while (!newCodigoFound)
                            {
                                if (base._Data.LargerNotConsecutiveCodigos[CCheckType][lIdCdad].Contains(maxCodigo))
                                {
                                    sphr.WaitAsync();
                                    //Since we have added 1 and found it, this codigo isn't anymore not consecutive, therefore remove not consecutive state...
                                    sb.Append($"UPDATE {table} SET NoConsecutivo=@cero WHERE Codigo=@cod{maxCodigo}{ownersClause};{Environment.NewLine}");
                                    values.Add($"cod{maxCodigo}", maxCodigo);
                                    //...and add codigo to list of to-be-removed-from-NotConsecutive
                                    noConToRemove.Add(maxCodigo);

                                    newcodigo++;
                                    this.CodigoOnHold = newcodigo;
                                    sphr.Release();
                                }
                                else if (base._Data.TrySetCodigoOnHold<T>(this, iIdCdad)) newCodigoFound = true;
                                else newcodigo++;
                            }

                            return newcodigo;
                        })
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    else
                        consistencyCheckNeeded = TrySetOnHoldAndGetIfConsistencyCheckNeeded<T>(ref maxCodigo, iIdCdad);

                    //Update dictionary with all codigos in the list to-be-removed
                    if (noConToRemove.Count > 0)
                        noConToRemove.ForEach(x => base._Data.TryRemoveFromNoConOnHold<T>(this.CurrentCodigo, iIdCdad, x, CCheckType));

                    //Previous maximum isn't the maximum anymore, update DB
                    sb.Append($"UPDATE {table} SET maximo=@cero WHERE codigo=@previous{ownersClause};{Environment.NewLine}");
                    values.Add("previous", previous);
                }
                //New codigo is the new maximum, update dictionary
                base._Data.TrySetNewMaxOnHold<T>(this.CurrentCodigo, iIdCdad, maxCodigo);

                //Insert new codigo into DB
                sb.Append($"INSERT INTO {table} (codigo,deleted,noconsecutivo,maximo{ownersColumns}) VALUES (@codfinal,@cero,@cero,@uno{ownersValues});{Environment.NewLine}");
#if SQLSERVER
                sb.Append($"SELECT MAX(Codigo) AS LastId FROM {table};");//BORRAME
#endif
                values.Add("codfinal", maxCodigo);

                string cancelException = $@"AutoCodigo.GetNextCodigo: Error al intentar cancelar cambios por no ser posible ocupar nuevo código:
No se pudo ocupar el codigo por Error al intentar añadir SQLOnHold.";
                string SQLException = $@"AutoCodigo.GetNextCodigo: Error al intentar añadir SQLOnHold:";
                TryAddSQLAndThrowExceptions<T>(
                    ACodigoSQLType.Next, cancelException, SQLException, maxCodigo, iIdCdad, sb.ToString(), (ExpandoObject)values, false, noConToRemove.Count, false);

                //Do consistency check if needed
                if (consistencyCheckNeeded) base._Data.CheckConsistencyOfType(this._Data.UsuarioLogueado.Id, GetCCheckType<T>());
                //return taken codigo
                return maxCodigo;
            }
            return 0;
        }
        public override void DeleteCodigo(int? idComunidad = null)
        {
            if (!TypeHaveAllIdOwnersAndSetIdOwners(idComunidad))
                throw new CustomException_AutoCodigo($@"El tipo {CCheckType} no ha sido acompañado de los correspondientes IdOwners: 
IdComunidad = {idComunidad}
IdEjercicio = {null}");

            if (this.CodigoOnHold != -1)
                throw new CustomException_AutoCodigo("Este código ya está siendo modificado o ha sido borrado, cancele o termine los cambios.");

            this.CodigoOnHold = 0;
            if (!base._Data.TrySetCodigoOnHold<T>(this, (int)this.IdOwnerComunidad))
            {
                this.CodigoOnHold = -1;
                throw new CustomException_AutoCodigo("Este código ya está siendo modificado o ha sido borrado, cancele o termine los cambios.");
            }

            //If DeletedCodigos contains codigo, codigo is already deleted so no more to do
            if (base._Data.DeletedCodigos[CCheckType][(int)this.IdOwnerComunidad].Contains(this.CurrentCodigo)) return;

            var sb = new StringBuilder();//$"START TRANSACTION;{Environment.NewLine}");
            string ownersClause = "";
            var values = new ExpandoObject() as IDictionary<string, object>;
            if (idComunidad != null)
            {
                values.Add("idCdad", idComunidad);
                ownersClause = " AND IdOwnerComunidad=@idCdad";
            }
            values.Add("cero", 0);
            values.Add("uno", 1);

            long lIdCdad = (long)this.IdOwnerComunidad;
            int iIdCdad = (int)this.IdOwnerComunidad;
            string table = base._Data.TableNames[CCheckType];
            
            int currentMax = base._Data.MaxCodigos[CCheckType][lIdCdad];
            //If codigo is greater than max it's not taken so it can't be deleted, except if it is one of the not consecutive codigos already taken
            if (this.CurrentCodigo > currentMax && base._Data.LargerNotConsecutiveCodigos[CCheckType][lIdCdad].Contains(this.CurrentCodigo))
            {
                //Codigo is taken and is one of the not consecutive codigos
                //Don't set not consecutive codigos as deleted because they don't interfere with the rest of the codigos, just remove them
                sb.Append($"DELETE FROM {table} WHERE Codigo=@cod{ownersClause};{Environment.NewLine}");
                values.Add("cod", this.CurrentCodigo);
                base._Data.TryRemoveFromNoConOnHold<T>(this.CurrentCodigo, iIdCdad, this.CurrentCodigo, CCheckType);

                string cancelException = $@"AutoCodigo.DeleteCodigo: Error al intentar cancelar cambios por no ser posible borrar código:
No se pudo borrar el codigo por error al intentar añadir SQLOnHold.";
                string SQLException = $@"AutoCodigo.DeleteCodigo: Error al intentar añadir SQLOnHold:";
                TryAddSQLAndThrowExceptions<T>(
                    ACodigoSQLType.Deleted, cancelException, SQLException, this.CurrentCodigo, iIdCdad, sb.ToString(), (ExpandoObject)values, false, 0, true);

                return;
            }
            else
            {
                if (this.CurrentCodigo == currentMax)
                {
                    //The current max is about to be deleted, so the previous one have to be the max except it is deleted
                    //if that's the case go to the previous one, and so on until find the new max not deleted
                    int newMax = ReverseFindFirstCodigoNotDeleted(this.CurrentCodigo - 1, CCheckType, lIdCdad);

                    sb.Append($"UPDATE {table} SET Maximo=@uno WHERE Codigo=@newMax{ownersClause};{Environment.NewLine}");
                    values.Add("newMax", newMax);
                    base._Data.TrySetNewMaxOnHold<T>(this.CurrentCodigo, iIdCdad, newMax);

                    //Now the old codigo is not consecutive, since it's greater than max, and have to be deleted, so remove it
                    sb.Append($"DELETE FROM {table} WHERE Codigo=@cod{ownersClause};{Environment.NewLine}");
                    values.Add("cod", this.CurrentCodigo);

                    string cEx = $@"AutoCodigo.DeleteCodigo: Error al intentar cancelar cambios por no ser posible borrar código:
No se pudo borrar el codigo por error al intentar añadir SQLOnHold.";
                    string SQLEx = "AutoCodigo.DeleteCodigo: Error al intentar añadir SQLOnHold:";
                    TryAddSQLAndThrowExceptions<T>(
                        ACodigoSQLType.Deleted, cEx, SQLEx, this.CurrentCodigo, iIdCdad, sb.ToString(), (ExpandoObject)values, true, 0, false);

                    return;
                }
                //Codigo is lesser than max, just update deleted
                base._Data.TryAddToDeletedOnHold<T>(this.CurrentCodigo, iIdCdad, this.CurrentCodigo, CCheckType);
                sb.Append($"UPDATE {table} SET Deleted=@uno,Maximo=@cero WHERE Codigo=@cod{ownersClause};");
                values.Add("cod", this.CurrentCodigo);

                string cancelException = $@"AutoCodigo.DeleteCodigo: Error al intentar cancelar cambios por no ser posible borrar código:
No se pudo borrar el codigo por error al intentar añadir SQLOnHold.";
                string SQLException = $"AutoCodigo.DeleteCodigo: Error al intentar añadir SQLOnHold:";
                TryAddSQLAndThrowExceptions<T>(
                    ACodigoSQLType.Deleted, cancelException, SQLException, this.CurrentCodigo, iIdCdad, sb.ToString(), (ExpandoObject)values, false, 0, false);
            }
        }
        public override bool CheckCodigoIsAvailableAndTakeIt(int codigo, int? idComunidad = null)
        {
            if (!TypeHaveAllIdOwnersAndSetIdOwners(idComunidad))
                throw new CustomException_AutoCodigo($@"El tipo {CCheckType} no ha sido acompañado de los correspondientes IdOwners: 
IdComunidad = {idComunidad}
IdEjercicio = {null}");

            if (this.CodigoOnHold != -1)
                throw new CustomException_AutoCodigo("Este código ya está siendo modificado, cancele o termine los cambios anteriores.");
            this.CodigoOnHold = codigo;
            if (!base._Data.TrySetCodigoOnHold<T>(this, (int)this.IdOwnerComunidad))
            {
                this.CodigoOnHold = -1;
                return false;
            }

            var sb = new StringBuilder();//$"START TRANSACTION;{Environment.NewLine}");
            string ownersClause = "";
            string ownersColumns = "";
            string ownersValues = "";
            var values = new ExpandoObject() as IDictionary<string, object>;
            if (idComunidad != null)
            {
                values.Add("idCdad", idComunidad);
                ownersClause = " AND IdOwnerComunidad=@idCdad";
                ownersColumns = ",IdOwnerComunidad";
                ownersValues = ",@idCdad";
            }
            values.Add("cero", 0);
            values.Add("uno", 1);

            long lIdCdad = (long)this.IdOwnerComunidad;
            int iIdCdad = (int)this.IdOwnerComunidad;
            string table = base._Data.TableNames[CCheckType];

            bool isMax = false;
            int currentMax = base._Data.MaxCodigos[CCheckType][lIdCdad];
            if (codigo > currentMax)
            {
                if (base._Data.LargerNotConsecutiveCodigos[CCheckType][lIdCdad].Contains(codigo))
                {
                    this.CodigoOnHold = -1;
                    if (!Task.Run(() => base._Data.TryCancelCodigoChangesAsync<T>(codigo, iIdCdad, this)).ConfigureAwait(false).GetAwaiter().GetResult())
                        throw new CustomException_AutoCodigo(
                            $@"AutoCodigo.CheckCodigoIsAvailableAndTakeIt: Error al intentar cancelar cambios por no ser posible ocupar nuevo código:
No se pudo ocupar el codigo por ya estar ocupado por otro codigo no consecutivo.
Type: {CCheckType}
SQL: {sb.ToString()}
codigo: {codigo}
Comunidad: {iIdCdad}");

                    return false;
                }
                else if (codigo == currentMax + 1)
                {
                    sb.Append($"UPDATE {table} SET Maximo=@cero WHERE Codigo=@prevMax{ownersClause};{Environment.NewLine}");
                    values.Add("prevMax", currentMax);
                    base._Data.TrySetNewMaxOnHold<T>(this.CurrentCodigo, iIdCdad, codigo);
                    isMax = true;
                }
                else base._Data.TryAddToNoConOnHold<T>(this.CurrentCodigo, iIdCdad, codigo, CCheckType);

                sb.Append($"INSERT INTO {table} (Codigo,Deleted,NoConsecutivo,Maximo{ownersColumns}) VALUES (@cod,@cero,@uno,@max{ownersValues});");
                values.Add("max", Convert.ToInt32(isMax));
                values.Add("cod", codigo);

                string cancelException = $@"AutoCodigo.CheckCodigoIsAvailableAndTakeIt: Error al intentar cancelar cambios por no ser posible ocupar nuevo código:
No se pudo ocupar el código por error al intentar añadir SQLOnHold.";
                string SQLException = $@"AutoCodigo.CheckCodigoIsAvailableAndTakeIt: Error al intentar añadir SQLOnHold:";
                TryAddSQLAndThrowExceptions<T>(
                    ACodigoSQLType.Check, cancelException, SQLException, codigo, iIdCdad, sb.ToString(), (ExpandoObject)values, isMax, 0, false, true);

                return true;
            }
            else
            {
                if (!base._Data.DeletedCodigos[CCheckType][lIdCdad].Contains(codigo))
                {
                    this.CodigoOnHold = -1;
                    if (!Task.Run(() => base._Data.TryCancelCodigoChangesAsync<T>(codigo, iIdCdad, this)).ConfigureAwait(false).GetAwaiter().GetResult())
                        throw new CustomException_AutoCodigo(
                            $@"AutoCodigo.CheckCodigoIsAvailableAndTakeIt: Error al intentar cancelar cambios por no ser posible ocupar nuevo código:
No se pudo ocupar el codigo por ya estar ocupado por otro codigo.
Type: {CCheckType}
SQL: {sb.ToString()}
codigo: {codigo}
Comunidad: {iIdCdad}");

                    return false;
                }

                sb.Append($"UPDATE codigo SET deleted=0 UPDATE {table} SET Deleted=@cero WHERE Codigo=@cod{ownersClause};");
                values.Add("cod", codigo);
                base._Data.TryRemoveFromDeletedOnHold<T>(this.CurrentCodigo, iIdCdad, codigo, CCheckType);

                string cancelException = $@"AutoCodigo.CheckCodigoIsAvailableAndTakeIt: Error al intentar cancelar cambios por no ser posible ocupar nuevo código:
No se pudo ocupar el codigo por error al intentar añadir SQLOnHold.";
                string SQLException = $"AutoCodigo.CheckCodigoIsAvailableAndTakeIt: Error al intentar añadir SQLOnHold:";
                TryAddSQLAndThrowExceptions<T>(
                    ACodigoSQLType.Check, cancelException, SQLException, codigo, iIdCdad, sb.ToString(), (ExpandoObject)values, false, 0, false);

                return true;
            }
        }

        public override bool ApplyChanges(int idCdad)
        {
            //No codigo on hold to apply changes or changes already applied
            if (this.CodigoOnHold == -1 || this.ChangesAppliedOrCancelled) return false;

            //Task<bool> apply = Task.Run(() => base._Data.ApplyCodigoChanges<T>(codigo, idCdad, this));
            this.ChangesAppliedOrCancelled = Task.Run(() => base._Data.TryApplyCodigoChangesAsync<T>(idCdad, this))
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (!this.ChangesAppliedOrCancelled) return false;
            else
            {
                this.ChangesAppliedOrCancelled = false;
                this.CurrentCodigo = CodigoOnHold;
                this.CodigoOnHold = -1;
                return true;
            }
        }
        public override bool CancelChanges(int idCdad)
        {
            //No codigo on hold to apply changes or changes already applied
            if (this.CodigoOnHold == -1 || this.ChangesAppliedOrCancelled) return false;

            this.ChangesAppliedOrCancelled = Task.Run(() => base._Data.TryCancelCodigoChangesAsync<T>(null, idCdad, this))
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (!this.ChangesAppliedOrCancelled) return false;
            else
            {
                this.ChangesAppliedOrCancelled = false;
                this.CodigoOnHold = -1;
                return true;
            }
        }
        #endregion
    }

    public sealed class AutoCodigoOwnerCdEj<T> : aAutoCodigoBase
        where T : IObjModelConCodigoConComunidadYEjercicio
    {
        public AutoCodigoOwnerCdEj(AutoCodigoData data, int currentCodigo = 0) : base(data, currentCodigo)
        {
            this.CCheckType = ACodigoCCheckType.Asientos;
        }

        #region properties
        public long CantorId { get; private set; }
        public override ACodigoCCheckType CCheckType { get; protected set; }
        #endregion

        #region helpers
        protected override bool TypeHaveAllIdOwnersAndSetIdOwners(int? IdComunidad = null, int? IdEjercicio = null)
        {
            if (IdComunidad == null || IdEjercicio == null) return false;

            this.CantorId = ((int)IdComunidad).CantorPair((int)IdEjercicio);
            return true;
        }
        #endregion

        #region public methods
        public override int GetNextCodigo(Tuple<int, int> ids)
        {
            int idComunidad = ids.Item1;
            int idEjercicio = ids.Item2;
            //if (!TypeHaveCodigo(objModelType))
            //    throw new CustomException_AutoCodigo($"El tipo {objModelType.Name} no usa codigo. AutoCodigo.GetNextCodigo(Type objModelType).");
            if (!TypeHaveAllIdOwnersAndSetIdOwners(idComunidad, idEjercicio))
                throw new CustomException_AutoCodigo($@"El tipo {CCheckType} no ha sido acompañado de los correspondientes IdOwners: 
IdComunidad = {idComunidad}
IdEjercicio = {idEjercicio}");

            if (this.CodigoOnHold != -1)
                throw new CustomException_AutoCodigo("Este código ya está siendo modificado, cancele o termine los cambios anteriores.");

            string ownersClause = "AND IdOwnerComunidad=@idCdad AND IdOwnerEjercicio=@idEjer";
            string ownersColumns = ",IdOwnerComunidad,IdOwnerEjercicio";
            string ownersValues = ",@idCdad,@idEjer";
            var values = new ExpandoObject() as IDictionary<string, object>;
            values.Add("idCdad", idComunidad);
            values.Add("idEjer", idEjercicio);
            values.Add("cero", 0);
            values.Add("uno", 1);

            string table = base._Data.TableNames[CCheckType];
            var sb = new StringBuilder();
            int maxCodigo;
            List<int> noConToRemove = new List<int>();
            bool consistencyCheckNeeded = false;
            //If there isn't any max codigo for this Comunidad, it means there is no codigo at all, so the next one is the first one and the max
            if (!base._Data.MaxCodigos[CCheckType].ContainsKey(this.CantorId))
            {
                maxCodigo = 1;
                this.CodigoOnHold = maxCodigo;
                consistencyCheckNeeded = TrySetOnHoldAndGetIfConsistencyCheckNeeded<T>(ref maxCodigo, idComunidad, idEjercicio);
            }
            else
            {
                int previous = base._Data.MaxCodigos[CCheckType][this.CantorId];
                maxCodigo = previous + 1;

                //Find next available not consecutive codigo IF there are any.
                //First sum one to the max codigo and check if there's a not consecutive codigo occupied, keep summing until one free is found.
                //Update both dictionary and DB setting codigos as not consecutive
                if (base._Data.LargerNotConsecutiveCodigos[CCheckType].ContainsKey(this.CantorId))
                {
                    SemaphoreSlim sphr = new SemaphoreSlim(1, 1);
                    maxCodigo = Task.Run(() =>
                    {
                        int newcodigo = maxCodigo;
                        this.CodigoOnHold = maxCodigo;
                        bool newCodigoFound = !base._Data.LargerNotConsecutiveCodigos[CCheckType][this.CantorId].Contains(maxCodigo)
                            && base._Data.TrySetCodigoOnHold<T>(this, idComunidad, idEjercicio);

                        while (!newCodigoFound)
                        {
                            if (base._Data.LargerNotConsecutiveCodigos[CCheckType][this.CantorId].Contains(maxCodigo))
                            {
                                sphr.WaitAsync();
                                //Since we have added 1 and found it, this codigo isn't anymore not consecutive, therefore remove not consecutive state...
                                sb.Append($"UPDATE {table} SET NoConsecutivo=@cero WHERE Codigo=@cod{maxCodigo}{ownersClause};{Environment.NewLine}");
                                values.Add($"cod{maxCodigo}", maxCodigo);
                                //...and add codigo to list of to-be-removed-from-NotConsecutive
                                noConToRemove.Add(maxCodigo);

                                newcodigo++;
                                this.CodigoOnHold = newcodigo;
                                sphr.Release();
                            }
                            else if (base._Data.TrySetCodigoOnHold<T>(this, idComunidad, idEjercicio)) newCodigoFound = true;
                            else newcodigo++;
                        }

                        return newcodigo;
                    })
                    .ConfigureAwait(false).GetAwaiter().GetResult();
                }
                else
                    consistencyCheckNeeded = TrySetOnHoldAndGetIfConsistencyCheckNeeded<T>(ref maxCodigo, idComunidad, idEjercicio);

                //Update dictionary with all codigos in the list to-be-removed
                if (noConToRemove.Count > 0)
                    noConToRemove.ForEach(x => base._Data.TryRemoveFromNoConOnHold<T>(this.CurrentCodigo, idComunidad, idEjercicio, x, CCheckType));

                //Previous maximum isn't the maximum anymore, update DB
                sb.Append($"UPDATE {table} SET maximo=@cero WHERE codigo=@previous{ownersClause};{Environment.NewLine}");
                values.Add("previous", previous);
            }

            //New codigo is the new maximum, update dictionary
            base._Data.TrySetNewMaxOnHold<T>(this.CurrentCodigo, idComunidad, idEjercicio, maxCodigo);

            //Insert new codigo into DB
            sb.Append($"INSERT INTO {table} (codigo,deleted,noconsecutivo,maximo{ownersColumns}) VALUES (@codfinal,@cero,@cero,@uno{ownersValues});{Environment.NewLine}");
#if SQLSERVER
            sb.Append($"SELECT MAX(Codigo) AS LastId FROM {table};");//BORRAME
#endif
            values.Add("codfinal", maxCodigo);

            string cancelException = $@"AutoCodigo.GetNextCodigo: Error al intentar cancelar cambios por no ser posible ocupar nuevo código:
No se pudo ocupar el codigo por error al intentar añadir SQLOnHold.";
            string SQLException = $"AutoCodigo.GetNextCodigo: Error al intentar añadir SQLOnHold:";
            TryAddSQLAndThrowExceptions<T>(
                ACodigoSQLType.Next, cancelException, SQLException, maxCodigo, ids, sb.ToString(), 
                (ExpandoObject)values, false, noConToRemove.Count, false);

            //Do consistency check if needed
            if (consistencyCheckNeeded) base._Data.CheckConsistencyOfType(this._Data.UsuarioLogueado.Id, GetCCheckType<T>());
            //return taken codigo
            return maxCodigo;
        }
        public override void DeleteCodigo(Tuple<int, int> ids)
        {
            int idComunidad = ids.Item1;
            int idEjercicio = ids.Item2;
            //if (!TypeHaveCodigo(objModelType))
            //    throw new CustomException_AutoCodigo($"El tipo {objModelType.Name} no usa codigo. AutoCodigo.DeleteCodigo(Type objModelType, int codigo).");
            if (!TypeHaveAllIdOwnersAndSetIdOwners(idComunidad, idEjercicio))
                throw new CustomException_AutoCodigo($@"El tipo {CCheckType} no ha sido acompañado de los correspondientes IdOwners: 
IdComunidad = {idComunidad}
IdEjercicio = {idEjercicio}");

            if (this.CodigoOnHold != -1)
                throw new CustomException_AutoCodigo("Este código ya está siendo modificado, cancele o termine los cambios anteriores.");

            this.CodigoOnHold = 0;
            if (!base._Data.TrySetCodigoOnHold<T>(this, idComunidad, idEjercicio))
            {
                this.CodigoOnHold = -1;
                throw new CustomException_AutoCodigo("Este código ya está siendo modificado o ha sido borrado, cancele o termine los cambios.");
            }

            //If DeletedCodigos contains codigo, codigo is already deleted so no more to do
            if (base._Data.DeletedCodigos[CCheckType][this.CantorId].Contains(this.CurrentCodigo)) return;

            string ownersClause = "AND IdOwnerComunidad=@idCdad AND IdOwnerEjercicio=@idEjer";
            var values = new ExpandoObject() as IDictionary<string, object>;
            values.Add("idCdad", idComunidad);
            values.Add("idEjer", idEjercicio);
            values.Add("cero", 0);
            values.Add("uno", 1);

            var sb = new StringBuilder();//$"START TRANSACTION;{Environment.NewLine}");
            string table = base._Data.TableNames[CCheckType];
            
            int currentMax = base._Data.MaxCodigos[CCheckType][this.CantorId];
            if (this.CurrentCodigo > currentMax && base._Data.LargerNotConsecutiveCodigos[CCheckType][this.CantorId].Contains(this.CurrentCodigo))
            {
                sb.Append($"DELETE FROM {table} WHERE Codigo=@cod{ownersClause};{Environment.NewLine}");
                values.Add("cod", this.CurrentCodigo);
                base._Data.TryRemoveFromNoConOnHold<T>(this.CurrentCodigo, idComunidad, idEjercicio, this.CurrentCodigo, CCheckType);

                string cEx = @"AutoCodigo.DeleteCodigo: Error al intentar cancelar cambios por no ser posible borrar código:
No se pudo borrar el codigo por error al intentar añadir SQLOnHold.";
                string SQLEx = "AutoCodigo.DeleteCodigo: Error al intentar añadir SQLOnHold:";
                TryAddSQLAndThrowExceptions<T>(
                    ACodigoSQLType.Deleted, cEx, SQLEx, this.CurrentCodigo, ids, sb.ToString(), 
                    (ExpandoObject)values, false, 0, true);
                
                return;
            }
            else if (this.CurrentCodigo == currentMax)
            {
                //The current max is about to be deleted, so the previous one have to be the max except it is deleted
                //if that's the case go to the previous one, and so on until find the new max not deleted
                int newMax = ReverseFindFirstCodigoNotDeleted(this.CurrentCodigo - 1, CCheckType, this.CantorId);

                sb.Append($"UPDATE {table} SET Maximo=@uno WHERE Codigo=@newMax{ownersClause};{Environment.NewLine}");
                values.Add("newMax", newMax);
                base._Data.TrySetNewMaxOnHold<T>(this.CurrentCodigo, idComunidad, idEjercicio, newMax);

                //Now the old codigo is not consecutive, since it's greater than max, and have to be deleted, so remove it
                sb.Append($"DELETE FROM {table} WHERE Codigo=@cod{ownersClause};{Environment.NewLine}");
                values.Add("cod", this.CurrentCodigo);

                string cEx = $@"AutoCodigo.DeleteCodigo: Error al intentar cancelar cambios por no ser posible borrar código:
No se pudo borrar el codigo por error al intentar añadir SQLOnHold.";
                string SQLEx = "AutoCodigo.DeleteCodigo: Error al intentar añadir SQLOnHold:";
                TryAddSQLAndThrowExceptions<T>(
                    ACodigoSQLType.Deleted, cEx, SQLEx, this.CurrentCodigo, ids, sb.ToString(), 
                    (ExpandoObject)values, true, 0, false);

                return;
            }
            //Codigo is lesser than max, just update deleted
            base._Data.TryAddToDeletedOnHold<T>(this.CurrentCodigo, idComunidad, idEjercicio, this.CurrentCodigo, CCheckType);
            sb.Append($"UPDATE {table} SET Deleted=@uno,Maximo=@cero WHERE Codigo=@cod{ownersClause};");
            values.Add("cod", this.CurrentCodigo);

            string cancelException = @"AutoCodigo.DeleteCodigo: Error al intentar cancelar cambios por no ser posible borrar código:
No se pudo borrar el codigo por error al intentar añadir SQLOnHold.";
            string SQLException = "AutoCodigo.DeleteCodigo: Error al intentar añadir SQLOnHold:";
            TryAddSQLAndThrowExceptions<T>(
                ACodigoSQLType.Deleted, cancelException, SQLException, this.CurrentCodigo, ids, sb.ToString(), 
                (ExpandoObject)values, false, 0, false);
        }
        public override bool ApplyChanges(Tuple<int, int> ids)
        {
            //No codigo on hold to apply changes or changes already applied
            if (this.CodigoOnHold == -1 || this.ChangesAppliedOrCancelled) return false;
            
            this.ChangesAppliedOrCancelled = Task.Run(() => base._Data.TryApplyCodigoChangesAsync<T>(ids.Item1, ids.Item2, this))
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (!this.ChangesAppliedOrCancelled) return false;
            else
            {
                this.ChangesAppliedOrCancelled = false;
                this.CurrentCodigo = CodigoOnHold;
                this.CodigoOnHold = -1;
                return true;
            }
        }
        public override bool CancelChanges(Tuple<int, int> ids)
        {
            //No codigo on hold to apply changes or changes already applied
            if (this.CodigoOnHold == -1 || this.ChangesAppliedOrCancelled) return false;

            this.ChangesAppliedOrCancelled = Task.Run(() => base._Data.TryCancelCodigoChangesAsync<T>(null, ids.Item1, ids.Item2, this))
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (!this.ChangesAppliedOrCancelled) return false;
            else
            {
                this.ChangesAppliedOrCancelled = false;
                this.CodigoOnHold = -1;
                return true;
            }
        }
        #endregion
    }
}
