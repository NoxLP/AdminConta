using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using Dapper;
using System.Data.SqlClient;
using System.Dynamic;
using Extensions;

namespace AdConta.Models
{
    public sealed partial class AutoCodigoData
    {
        private sealed class AutoCodigoConsistencyChecker
        {
            public AutoCodigoConsistencyChecker(AutoCodigoData data, ACodigoCCheckType type)
            {
                this.Data = data;
                this.CalculatedMaxs = new ConcurrentDictionary<long, int>();
                this.OwnersCdades = new ConcurrentDictionary<ACodigoCCheckType, IEnumerable<int>>();
                this.CCheckType = type;
            }

            #region data structs
            private struct MaxCorrections
            {
                public MaxCorrections(string sql, int newMax)
                {
                    this.SQL = sql;
                    this.Changes_NewMax = newMax;
                }

                public string SQL { get; private set; }
                public int Changes_NewMax { get; private set; }
            }
            private struct NoConCorrections
            {
                public NoConCorrections(string sql, HashSet<int> RNCChanges, HashSet<int> ANCChanges, HashSet<int> RDChanges)
                {
                    this.SQL = sql;
                    this.Changes_RemoveFromNoCon = RNCChanges;
                    this.Changes_AddToNoCon = ANCChanges;
                    this.Changes_RemoveFromDeleted = RDChanges;
                }

                public string SQL { get; private set; }
                public HashSet<int> Changes_RemoveFromNoCon { get; private set; }
                public HashSet<int> Changes_AddToNoCon { get; private set; }
                public HashSet<int> Changes_RemoveFromDeleted { get; private set; }
            }
            #endregion

            #region fields
            private readonly string _strCon = GlobalSettings.Properties.Settings.Default.conta1ConnectionString;
            private readonly object _LockObject = new object();
            #endregion

            #region properties
            public ACodigoCCheckType CCheckType { get; private set; }
            public AutoCodigoData Data { get; private set; }
            public ConcurrentDictionary<long, int> CalculatedMaxs { get; private set; }
            public ConcurrentDictionary<ACodigoCCheckType, IEnumerable<int>> OwnersCdades { get; private set; }
            public ConcurrentBag<Tuple<int, int>> OwnersCdadesEjer { get; private set; }
            
            public ConcurrentDictionary<long, int> Changes_NewMax { get; private set; }
            public ConcurrentDictionary<long, HashSet<int>> Changes_RemoveFromNoCon { get; private set; }
            public ConcurrentDictionary<long, HashSet<int>> Changes_AddToNoCon { get; private set; }
            public ConcurrentDictionary<long, HashSet<int>> Changes_RemoveFromDeleted { get; private set; }
            #endregion

            #region helpers
            private IEnumerable<int> GetOwnersCdades(IEnumerable<dynamic> dynamics)
            {
                IEnumerable<int> owners = dynamics
                    .DistinctBy(dyn => dyn.IdOwnerComunidad)
                    .Select(dyn => (int)dyn.IdOwnerComunidad);
                return owners;
            }
            private IEnumerable<Tuple<int, int>> GetOwnersCdadesEjer(IEnumerable<dynamic> dynamics)
            {
                IEnumerable<Tuple<int, int>> owners = dynamics
                    .DistinctBy(dyn => dyn.IdOwnerComunidad, dyn => dyn.IdOwnerEjercicio)
                    .Select(dyn => new Tuple<int, int>((int)dyn.IdOwnerComunidad, (int)dyn.IdOwnerEjercicio));
                return owners;
            }
            public async Task ExecuteAsyncSQLAsync(StringBuilder sb, ExpandoObject values)
            {
                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    await con.OpenAsync().ConfigureAwait(false);
                    var result = await con.ExecuteAsync(sb.ToString(), values).ConfigureAwait(false);
                    con.Close();
                }
            }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            public async Task ApplyDictionariesChangesToData()
            {
                foreach (KeyValuePair<long, int> kvp in this.Changes_NewMax) this.Data.AddOrUpdateMax(CCheckType, kvp.Key, kvp.Value);
                foreach (KeyValuePair<long, HashSet<int>> kvp in this.Changes_RemoveFromNoCon) this.Data.TryRemoveFromNoCon(CCheckType, kvp.Key, kvp.Value);
                foreach (KeyValuePair<long, HashSet<int>> kvp in this.Changes_AddToNoCon) this.Data.AddOrUnionNoCon(CCheckType, kvp.Key, kvp.Value);
                foreach (KeyValuePair<long, HashSet<int>> kvp in this.Changes_RemoveFromDeleted) this.Data.TryRemoveFromDeleted(CCheckType, kvp.Key, kvp.Value);
            }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            #endregion

            #region max helpers
            private int CalculateMaxPerOwner(List<dynamic> dynamics)
            {
                //Rmember dynamics are ordered by codigo, so the last one consecutive is the current max
                List<int> codigosCon = new List<int>();
                int i = 0;
                foreach (dynamic dyn in dynamics)
                {
                    i++;
                    if (i == 0 || codigosCon[i - 1] == (dyn.Codigo - 1)) codigosCon.Add((int)dyn.Codigo);
                    else break;
                }
                return dynamics[i].Codigo;
            }
            private MaxCorrections DoCorrectionsForMax(
                IEnumerable<dynamic> readedMaxs,
                int calculatedMax,
                string table,
                IDictionary<string, object> values,
                string ownersClause,
                long ownersValue)
            {
                StringBuilder sb = new StringBuilder("");
                HashSet<int> maxToRemove = new HashSet<int>();
                if (readedMaxs.Count() > 1)
                {
                    IEnumerable<int> smallerThanCalculatedMax = readedMaxs //ensure all codigos are actually smaller than real max
                        .Where(dyn => (int)dyn.Codigo < calculatedMax)
                        .Select(dyn => (int)dyn.Codigo);

                    sb.Append($"UPDATE {table} SET Maximo=@cero WHERE Codigo IN ( ");
                    foreach (int possibleMax in smallerThanCalculatedMax) //add all incorrect max codigos to update to remove the max state
                    {
                        sb.Append($"@cod{possibleMax}{ownersValue},");
                        values.Add($"cod{possibleMax}{ownersValue}", possibleMax);
                    }
                    sb.Remove(sb.Length - 1, 1);//remove last comma
                    sb.Append($" ){ownersClause};{Environment.NewLine}");
                }
                else if (readedMaxs.Count() > 0)
                {
                    int possibleMax = readedMaxs        //ensure all codigos are actually smaller than real max,
                        .Select(dyn => (int)dyn.Codigo) //this time there are only one
                        .First();

                    sb.Append($"UPDATE {table} SET Maximo=@cero WHERE Codigo=@cod{possibleMax}{ownersValue}{ownersClause};{Environment.NewLine}");
                    values.Add($"cod{possibleMax}{ownersValue}", possibleMax);
                }

                sb.Append($"UPDATE {table} SET Maximo=@uno WHERE Codigo=@newmax{ownersValue}{ownersClause};{Environment.NewLine}");
                values.Add($"newmax{ownersValue}", calculatedMax);


                return new MaxCorrections(sb.ToString(), calculatedMax);
            }
            private async Task SetChangesToDictionaries(long id, MaxCorrections corrections, SemaphoreSlim sphr)
            {
                await sphr.WaitAsync();
                this.Changes_NewMax.AddOrUpdate(id, corrections.Changes_NewMax,
                    (Id, max) => corrections.Changes_NewMax);
                sphr.Release();
            }
            private async Task<string> CheckMaxNoOwnersAsync(IEnumerable<dynamic> dynamics, string table, IDictionary<string, object> values)
            {
                this.CalculatedMaxs.TryAdd(0, await Task.Run(() => CalculateMaxPerOwner(dynamics.ToList())).ConfigureAwait(false));
                bool calculatedIsActualMax = dynamics
                    .Where(dyn => dyn.Codigo == CalculatedMaxs[0]) //take only calculatedMax
                    .Select(dyn => (bool)(dyn.Maximo == 1)) //get if calculatedMax is the current DB readed max
                    .SingleOrDefault();

                if (!calculatedIsActualMax)
                {
                    SemaphoreSlim sphr = new SemaphoreSlim(1, 1);
                    StringBuilder sb = new StringBuilder();
                    IEnumerable<dynamic> readedMaxs = dynamics.Where(dyn => dyn.Maximo == 1);
                    string ownersClause = "";
                    MaxCorrections corrections = DoCorrectionsForMax(readedMaxs, CalculatedMaxs[0], table, values, ownersClause, 0);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    SetChangesToDictionaries(0, corrections, sphr).Forget().ConfigureAwait(false);
#pragma warning restore CS4014
                    sb.Append(corrections.SQL);
                    //this.CalculatedMaxs.AddOrUpdate(0, CalculatedMaxs[0], (x, y) => calculatedMax);
                    return sb.ToString();
                }

                return "";
            }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            private async Task<string> CheckMaxOwnerCdadAsync(IEnumerable<dynamic> dynamics, string table, IDictionary<string, object> values)
            {
                SemaphoreSlim sphr = new SemaphoreSlim(1, 1);
                StringBuilder sb = new StringBuilder("");
                Parallel.ForEach(this.OwnersCdades[CCheckType], async (x, loopState) =>
                {
                    this.CalculatedMaxs.TryAdd(x, await Task.Run(() => CalculateMaxPerOwner(dynamics.ToList())).ConfigureAwait(false));
                    IEnumerable<dynamic> onlyXOwner = dynamics
                        .Where(dyn => dyn.IdOwnerComunidad == x); //take those with owner = x
                    bool calculatedIsActualMax = onlyXOwner
                            .Where(dyn => dyn.Codigo == this.CalculatedMaxs[x]) //take only calculatedMax
                            .Select(dyn => (bool)(dyn.Maximo == 1)) //get if calculatedMax is the current DB readed max
                            .SingleOrDefault();

                    if (!calculatedIsActualMax)
                    {
                        IEnumerable<dynamic> readedMaxs = onlyXOwner.Where(dyn => dyn.Maximo == 1);
                        string ownersClause = $" AND IdOwnerComunidad=@idCdad";
                        values.Add("idCdad", x);
                        MaxCorrections corrections = DoCorrectionsForMax(readedMaxs, this.CalculatedMaxs[0], table, values, ownersClause, x);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        SetChangesToDictionaries(0, corrections, sphr).Forget().ConfigureAwait(false);
#pragma warning restore CS4014
                        await sphr.WaitAsync();
                        //this.CalculatedMaxs.AddOrUpdate(x, calculatedMax, (l, i) => calculatedMax);
                        sb.Append(corrections.SQL);
                        sphr.Release();
                    }
                    loopState.Stop();
                });

                return sb.ToString();
            }
            private async Task<string> CheckMaxOwnerCdEjAsync(IEnumerable<dynamic> dynamics, string table, IDictionary<string, object> values)
            {
                SemaphoreSlim sphr = new SemaphoreSlim(1, 1);
                StringBuilder sb = new StringBuilder("");
                Parallel.ForEach(this.OwnersCdadesEjer, async (x, loopState) =>
                {
                    long id = x.Item1.CantorPair(x.Item2);
                    this.CalculatedMaxs.TryAdd(id, await Task.Run(() => CalculateMaxPerOwner(dynamics.ToList())).ConfigureAwait(false));
                    IEnumerable<dynamic> onlyXOwner = dynamics
                        .Where(dyn => dyn.IdOwnerComunidad == x.Item1 && dyn.IdOwnerEjercicio == x.Item2); //take those with owner = x
                    bool calculatedIsActualMax = onlyXOwner
                            .Where(dyn => dyn.Codigo == this.CalculatedMaxs[id]) //take only calculatedMax
                            .Select(dyn => (bool)(dyn.Maximo == 1)) //get if calculatedMax is the current DB readed max
                            .SingleOrDefault();

                    if (!calculatedIsActualMax)
                    {
                        IEnumerable<dynamic> readedMaxs = onlyXOwner.Where(dyn => dyn.Maximo == 1);
                        string ownersClause = $" AND IdOwnerComunidad=@idCdad";
                        values.Add("idCdad", x);
                        long cantorId = x.Item1.CantorPair(x.Item2);
                        MaxCorrections corrections = await Task.Run(() =>
                            DoCorrectionsForMax(readedMaxs, this.CalculatedMaxs[id], table, values, ownersClause, cantorId))
                            .ConfigureAwait(false);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        SetChangesToDictionaries(0, corrections, sphr).Forget().ConfigureAwait(false);
#pragma warning restore CS4014
                        await sphr.WaitAsync();
                        //this.CalculatedMaxs.AddOrUpdate(cantorId, calculatedMax, (l, i) => calculatedMax);
                        sb.Append(corrections.SQL);
                        sphr.Release();
                    }
                    loopState.Stop();
                });

                return sb.ToString();
            }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            #endregion

            #region nocon helpers
            private NoConCorrections GetCorrectionsForNoCon(
                IEnumerable<dynamic> smallerThanMax,
                IEnumerable<dynamic> deletedGreaterThanMax,
                IEnumerable<dynamic> noDeletedGreaterThanMax,
                string table,
                IDictionary<string, object> values,
                string ownersClause,
                long ownersValue)
            {
                HashSet<int> noConToRemove = new HashSet<int>();
                HashSet<int> noConToAdd = new HashSet<int>();
                HashSet<int> deletedToRemove = new HashSet<int>();
                var sb = new StringBuilder("");
                if (smallerThanMax.Count() > 0)
                {
                    foreach (dynamic dyn in smallerThanMax)
                    {
                        sb.Append($"UPDATE {table} SET NoConsecutivo=@cero WHERE Codigo=@cod{dyn.Codigo}{ownersValue}{ownersClause};{Environment.NewLine}");
                        values.Add($"cod{dyn.Codigo}{ownersValue}", dyn.Codigo);
                        noConToRemove.Add(dyn.Codigo);
                    }
                }
                if (deletedGreaterThanMax.Count() > 0)
                {
                    foreach (dynamic dyn in deletedGreaterThanMax)
                    {
                        sb.Append($"DELETE FROM {table} WHERE Codigo=@cod{dyn.Codigo}{ownersValue}{ownersClause};{Environment.NewLine}");
                        values.Add($"cod{dyn.Codigo}{ownersValue}", dyn.Codigo);
                        noConToRemove.Add(dyn.Codigo);
                        deletedToRemove.Add(dyn.Codigo);
                    }
                }
                if (noDeletedGreaterThanMax.Count() > 0)
                {
                    foreach (dynamic dyn in noDeletedGreaterThanMax)
                    {
                        sb.Append($"UPDATE {table} SET NoConsecutivo=@uno WHERE Codigo=@cod{dyn.Codigo}{ownersValue}{ownersClause};{Environment.NewLine}");
                        values.Add($"cod{dyn.Codigo}{ownersValue}", dyn.Codigo);
                        noConToAdd.Add(dyn.Codigo);
                    }
                }

                return new NoConCorrections(sb.ToString(), noConToRemove, noConToAdd, deletedToRemove);
            }
            private async Task SetChangesToDictionaries(long id, NoConCorrections corrections, SemaphoreSlim sphr)
            {
                await sphr.WaitAsync();
                this.Changes_RemoveFromNoCon.AddOrUpdate(id, corrections.Changes_RemoveFromNoCon, 
                    (Id,set) => (HashSet<int>)set.Union(corrections.Changes_RemoveFromNoCon));
                this.Changes_AddToNoCon.AddOrUpdate(id, corrections.Changes_AddToNoCon,
                    (Id, set) => (HashSet<int>)set.Union(corrections.Changes_AddToNoCon));
                this.Changes_RemoveFromDeleted.AddOrUpdate(id, corrections.Changes_RemoveFromDeleted,
                    (Id, set) => (HashSet<int>)set.Union(corrections.Changes_RemoveFromDeleted));
                sphr.Release();
            }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            private async Task<string> CheckNoConNoOwnersAsync(IEnumerable<dynamic> dynamics, string table, IDictionary<string, object> values)
            {
                IEnumerable<dynamic> noConSmallerThanMax = dynamics.Where(dyn => dyn.Codigo < this.CalculatedMaxs[0]);
                IEnumerable<dynamic> deletedNoConGreaterThanMax = dynamics
                    .Except(noConSmallerThanMax)
                    .Where(dyn => dyn.Deleted == 1);
                IEnumerable<dynamic> noDeletedNoConGreaterThanMax = dynamics
                    .Where(dyn => dyn.Codigo > this.CalculatedMaxs[0] && dyn.Deleted == 0 && dyn.NoConsecutivo == 0);
                
                NoConCorrections corrections = GetCorrectionsForNoCon(
                    noConSmallerThanMax, deletedNoConGreaterThanMax, noDeletedNoConGreaterThanMax, table, values, "", 0);
                SemaphoreSlim sphr = new SemaphoreSlim(1, 1);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                SetChangesToDictionaries(0, corrections, sphr).Forget().ConfigureAwait(false);
#pragma warning restore CS4014

                return corrections.SQL;
            }
            private async Task<string> CheckNoConOwnersCdadAsync(IEnumerable<dynamic> dynamics, string table, IDictionary<string, object> values)
            {
                SemaphoreSlim sphr = new SemaphoreSlim(1, 1);
                StringBuilder sb = new StringBuilder("");
                Parallel.ForEach(this.OwnersCdades[CCheckType], x =>
                {
                    IEnumerable<dynamic> noConSmallerThanMax = dynamics.Where(dyn => dyn.Codigo < this.CalculatedMaxs[0] && dyn.IdOwnerComunidad == x);
                    IEnumerable<dynamic> deletedNoConGreaterThanMax = dynamics
                        .Except(noConSmallerThanMax)
                        .Where(dyn => dyn.Deleted == 1 && dyn.IdOwnerComunidad == x);
                    IEnumerable<dynamic> noDeletedNoConGreaterThanMax = dynamics
                        .Where(dyn => dyn.Codigo > this.CalculatedMaxs[0] && dyn.Deleted == 0 && dyn.NoConsecutivo == 0 && dyn.IdOwnerComunidad == x);

                    string ownerClause = $" AND IdOwnerComunidad=@cdad{x}";
                    values.Add($"cdad{x}", x);

                    NoConCorrections corrections = GetCorrectionsForNoCon(
                        noConSmallerThanMax, deletedNoConGreaterThanMax, noDeletedNoConGreaterThanMax, table, values, ownerClause, x);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    SetChangesToDictionaries(x, corrections, sphr).Forget().ConfigureAwait(false);
#pragma warning restore CS4014
                    sphr.WaitAsync();
                    sb.Append(corrections.SQL);
                    sphr.Release();
                });
                return sb.ToString();
            }
            private async Task<string> CheckNoConOwnersCdEjAsync(IEnumerable<dynamic> dynamics, string table, IDictionary<string, object> values)
            {
                SemaphoreSlim sphr = new SemaphoreSlim(1, 1);
                StringBuilder sb = new StringBuilder("");
                Parallel.ForEach(this.OwnersCdadesEjer, async x =>
                {
                    IEnumerable<dynamic> noConSmallerThanMax = dynamics
                        .Where(dyn =>
                            dyn.Codigo < this.CalculatedMaxs[0]
                            && dyn.IdOwnerComunidad == x.Item1
                            && dyn.IdOwnerEjercicio == x.Item2);
                    IEnumerable<dynamic> deletedNoConGreaterThanMax = dynamics
                        .Except(noConSmallerThanMax)
                        .Where(dyn =>
                            dyn.Deleted == 1
                            && dyn.IdOwnerComunidad == x.Item1
                            && dyn.IdOwnerEjercicio == x.Item2);
                    IEnumerable<dynamic> noDeletedNoConGreaterThanMax = dynamics
                        .Where(dyn =>
                            dyn.Codigo > this.CalculatedMaxs[0]
                            && dyn.Deleted == 0
                            && dyn.NoConsecutivo == 0
                            && dyn.IdOwnerComunidad == x.Item1
                            && dyn.IdOwnerEjercicio == x.Item2);

                    long id = x.Item1.CantorPair(x.Item2);
                    string ownerClause = $" AND IdOwnerComunidad=@cdad{id} AND IdOwnerEjercicio=@ejer{id}";
                    values.Add($"cdad{id}", x.Item1);
                    values.Add($"ejer{id}", x.Item2);

                    NoConCorrections corrections = await Task.Run(() => GetCorrectionsForNoCon(
                        noConSmallerThanMax, deletedNoConGreaterThanMax, noDeletedNoConGreaterThanMax, table, values, ownerClause, id))
                        .ConfigureAwait(false);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    SetChangesToDictionaries(id, corrections, sphr).Forget().ConfigureAwait(false);
#pragma warning restore CS4014
                    await sphr.WaitAsync();
                    sb.Append(corrections.SQL);
                    sphr.Release();
                });
                return sb.ToString();
            }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            #endregion

            #region public methods
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            public async Task CheckConsistencyOfTypeAsync(ACodigoCCheckType type)
            {
                switch (type)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    case ACodigoCCheckType.Comunidad:
                        ComunidadCheckConsistencyAsync().Forget().ConfigureAwait(false);
                        break;
                    case ACodigoCCheckType.Fincas:
                        FincaCheckConsistencyAsync().Forget().ConfigureAwait(false);
                        break;
                    case ACodigoCCheckType.Pptos:
                        PptoCheckConsistencyAsync().Forget().ConfigureAwait(false);
                        break;
                    case ACodigoCCheckType.Asientos:
                        AsientoCheckConsistencyAsync().Forget().ConfigureAwait(false);
                        break;
                    default: return;
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            public async Task ComunidadCheckConsistencyAsync()
            {
                IEnumerable<dynamic> comunidades;
                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    await con.OpenAsync().ConfigureAwait(false);

                    comunidades = await con.QueryAsync(
                        $@"SELECT Codigo,Deleted,NoConsecutivo FROM {this.Data.TableNames[CCheckType]} ORDER BY Codigo;")
                        .ConfigureAwait(false);
                    con.Close();
                }
                
                StringBuilder sb = new StringBuilder();
                var values = new ExpandoObject() as IDictionary<string, object>;
                values.Add("cero", 0);
                values.Add("uno", 1);

                sb.Append(await CheckMaxNoOwnersAsync(comunidades, this.Data.TableNames[CCheckType], values).ConfigureAwait(false));
                sb.Append(await CheckNoConNoOwnersAsync(comunidades.Where(dyn => dyn.NoConsecutivo == 1), this.Data.TableNames[CCheckType], values)
                .ConfigureAwait(false));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ApplyDictionariesChangesToData().Forget().ConfigureAwait(false);
                ExecuteAsyncSQLAsync(sb, (ExpandoObject)values).Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            public async Task FincaCheckConsistencyAsync()
            {
                IEnumerable<dynamic> fincas;
                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    await con.OpenAsync().ConfigureAwait(false);

                    fincas = await con.QueryAsync(
                        $@"SELECT Codigo,Maximo,Deleted,IdOwnerComunidad FROM {this.Data.TableNames[CCheckType]} ORDER BY Codigo;")
                        .ConfigureAwait(false);
                    con.Close();
                }
                
                this.OwnersCdades.TryAdd(CCheckType, GetOwnersCdades(fincas));
                StringBuilder sb = new StringBuilder();
                var values = new ExpandoObject() as IDictionary<string, object>;
                values.Add("cero", 0);
                values.Add("uno", 1);

                sb.Append(await CheckMaxOwnerCdadAsync(fincas, this.Data.TableNames[CCheckType], values).ConfigureAwait(false));
                sb.Append(await CheckNoConOwnersCdadAsync(fincas.Where(dyn => dyn.NoConsecutivo == 1), this.Data.TableNames[CCheckType], values)
                    .ConfigureAwait(false));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ApplyDictionariesChangesToData().Forget().ConfigureAwait(false);
                ExecuteAsyncSQLAsync(sb, (ExpandoObject)values).Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            public async Task PptoCheckConsistencyAsync()
            {
                IEnumerable<dynamic> pptos;
                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    await con.OpenAsync().ConfigureAwait(false);

                    pptos = await con.QueryAsync(
                        $@"SELECT Codigo,Maximo,Deleted,IdOwnerComunidad FROM {this.Data.TableNames[CCheckType]} ORDER BY Codigo;")
                        .ConfigureAwait(false);
                    con.Close();
                }
                
                this.OwnersCdades.TryAdd(CCheckType, GetOwnersCdades(pptos));
                StringBuilder sb = new StringBuilder();
                var values = new ExpandoObject() as IDictionary<string, object>;
                values.Add("cero", 0);
                values.Add("uno", 1);                

                sb.Append(await CheckMaxOwnerCdadAsync(pptos, this.Data.TableNames[CCheckType], values).ConfigureAwait(false));
                sb.Append(await CheckNoConOwnersCdadAsync(pptos.Where(dyn => dyn.NoConsecutivo == 1), this.Data.TableNames[CCheckType], values)
                    .ConfigureAwait(false));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ApplyDictionariesChangesToData().Forget().ConfigureAwait(false);
                ExecuteAsyncSQLAsync(sb, (ExpandoObject)values).Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            public async Task AsientoCheckConsistencyAsync()
            {
                IEnumerable<dynamic> asientos;
                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    await con.OpenAsync().ConfigureAwait(false);

                    asientos = await con.QueryAsync(
                        $@"SELECT Codigo,Maximo,Deleted,IdOwnerComunidad,IdOwnerEjercicio FROM {this.Data.TableNames[CCheckType]} ORDER BY Codigo;")
                        .ConfigureAwait(false);
                    con.Close();
                }
                
                this.OwnersCdadesEjer = new ConcurrentBag<Tuple<int, int>>(await Task.Run(() => GetOwnersCdadesEjer(asientos)).ConfigureAwait(false));
                StringBuilder sb = new StringBuilder();
                var values = new ExpandoObject() as IDictionary<string, object>;
                values.Add("cero", 0);
                values.Add("uno", 1);

                sb.Append(await CheckMaxOwnerCdEjAsync(asientos, this.Data.TableNames[CCheckType], values).ConfigureAwait(false));
                sb.Append(await CheckNoConOwnersCdEjAsync(asientos.Where(dyn => dyn.NoConsecutivo == 1), this.Data.TableNames[CCheckType], values)
                    .ConfigureAwait(false));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ApplyDictionariesChangesToData().Forget().ConfigureAwait(false);
                ExecuteAsyncSQLAsync(sb, (ExpandoObject)values).Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            public async Task ReArrangeAsientosAsync<T>(bool byDate)
                where T : IObjModelConCodigoConComunidadYEjercicio
            {
                //TODO
            }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            #endregion
        }
    }
}
