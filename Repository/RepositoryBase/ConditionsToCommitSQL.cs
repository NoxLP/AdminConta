using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta;
using System.Threading;
using Dapper;

namespace Repository
{
    public interface IConditionToCommit
    {
        bool GetIfMatchCondition(dynamic singleResult);
    }
    public interface IConditionToCommit<T> : IConditionToCommit, IEqualityComparer<IConditionToCommit<T>> where T : IComparable<T>
    {
        ConditionTCType Type { get; }
        T ScalarRightValue { get; }
    }

    public class ConditionToCommitScalar<T> : IConditionToCommit<T> where T : IComparable<T>
    {
        public ConditionToCommitScalar(ConditionTCType condType, T value)
        {
            this.Type = condType;
            this.ScalarRightValue = value;
        }

        #region properties
        public ConditionTCType Type { get; private set; }
        public T ScalarRightValue { get; private set; }
        #endregion

        #region public methods
        public bool GetIfMatchCondition(dynamic singleResult)
        {
            T result = (T)singleResult;
            if (result == null) return false;
            switch(this.Type)
            {
                case ConditionTCType.equal:
                    return ScalarRightValue.Equals(result);
                case ConditionTCType.diff:
                    return !ScalarRightValue.Equals(result);
                case ConditionTCType.greater:
                    return result.CompareTo(ScalarRightValue) > 0;
                case ConditionTCType.lesser:
                    return result.CompareTo(ScalarRightValue) < 0;
                case ConditionTCType.greatOrEq:
                    return result.CompareTo(ScalarRightValue) >= 0;
                case ConditionTCType.lessOrEq:
                    return result.CompareTo(ScalarRightValue) <= 0;
            }

            return false;
        }

        public bool Equals(IConditionToCommit<T> x, IConditionToCommit<T> y)
        {
            return x.Type.Equals(y.Type) && x.ScalarRightValue.Equals(y.ScalarRightValue);
        }

        public int GetHashCode(IConditionToCommit<T> obj)
        {
            int hash = 13;
            hash = (hash * 7) + this.Type.GetHashCode();
            hash = (hash * 7) + this.ScalarRightValue.GetHashCode();
            return hash;
        }
        #endregion
    }

    public class ConditionsToCommitSQL
    {
        public ConditionsToCommitSQL()
        {
            this.Conditions = new List<IConditionToCommit>();
            this._ConditionsNoMatched = new List<int>();
        }

        #region fields
        private List<int> _ConditionsNoMatched;
        #endregion

        #region properties
        public List<IConditionToCommit> Conditions { get; private set; }
        public IReadOnlyList<int> ConditionsNoMatched { get { return this._ConditionsNoMatched.AsReadOnly(); } }
        #endregion

        #region public methods
        public void Add(IConditionToCommit condition)
        {
            this.Conditions.Add(condition);
        }
        public void RemoveAt(int index)
        {
            this.Conditions.RemoveAt(index);
        }
        public void Remove(IConditionToCommit condition)
        {
            this.Conditions.Remove(condition);
        }
        public void Clear()
        {
            this.Conditions.Clear();
            this._ConditionsNoMatched.Clear();
        }
        public bool GetIfMatchAllConditions(List<dynamic> dapperResult)
        {
            this._ConditionsNoMatched.Clear();
            if (dapperResult.Count() != this.Conditions.Count) return false;
            
            Parallel.For(0, this.Conditions.Count, i =>
            {
                if (!this.Conditions[i].GetIfMatchCondition(dapperResult[i])) this._ConditionsNoMatched.Add(i);
            });

            return this._ConditionsNoMatched.Count == 0;
        }
        public bool GetIfMatchAllConditions(SqlMapper.GridReader dapperMultiGrid)
        {
            this._ConditionsNoMatched.Clear();

            List<dynamic> dapperResult = new List<dynamic>();
            while (!dapperMultiGrid.IsConsumed) dapperResult.Add(dapperMultiGrid.Read());

            if (dapperResult.Count() != this.Conditions.Count) return false;

            Parallel.For(0, this.Conditions.Count, i =>
            {
                if (!this.Conditions[i].GetIfMatchCondition(dapperResult[i])) this._ConditionsNoMatched.Add(i);
            });

            return this._ConditionsNoMatched.Count == 0;
        }
        #endregion
    }

}
