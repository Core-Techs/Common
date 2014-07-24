using System;
using System.Data;
using System.Dynamic;

namespace CoreTechs.Common
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
    }
}
