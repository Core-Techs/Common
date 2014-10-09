using System;
using System.Data;
using System.Dynamic;
using System.Linq;

namespace CoreTechs.Common.Database
{
    public class DynamicDataRow : DynamicObject
    {
        private readonly DataRow _row;

        public DynamicDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException("row");
            _row = row;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var found = _row.Table.Columns.Contains(binder.Name);
            result = found ? _row[binder.Name] : null;

            if (result == DBNull.Value)
                result = null;

            return found;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var found = _row.Table.Columns.Contains(binder.Name);

            if (!found)
                return false;

            _row[binder.Name] = value ?? DBNull.Value;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = null;
            var index = indexes.FirstOrDefault();

            if (index == null)
                return false;

            Attempt<object> attempt;
            var s = index as string;
            if (s != null)
            {
                attempt = _row.AttemptGet(r => r[s]);
            }
            else
            {
                var isInt = index.AttemptGet(Convert.ToInt32);
                if (isInt.Succeeded)
                    attempt = isInt.Value.AttemptGet(i => _row[i]);
                else return false;
            }

            result = attempt.Value;
            return attempt.Succeeded;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            var index = indexes.FirstOrDefault();

            if (index == null)
                return false;

            Attempt attempt;
            var s = index as string;
            if (s != null)
            {
                attempt = Attempt.Do(() => _row[s] = value);
            }
            else
            {
                var isInt = index.AttemptGet(Convert.ToInt32);
                if (isInt.Succeeded)
                    attempt = Attempt.Do(() => _row[isInt.Value] = value);
                else return false;
            }
            
            return attempt.Succeeded;

        }
    }
}
