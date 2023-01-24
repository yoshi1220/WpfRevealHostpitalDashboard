using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfRevealHealthCareDashboard
{
    /// RangeからEFへのコンバーター
    /// Entity Frameworkで利用するエンティティクラスに読み込む
    /// 主に ClosedXmlのExcel からデータを読み込み、EF でデータベースに出力するときに使う
    public class ClosedXmlRangeConverter<T> where T : class, new()
    {
        protected List<System.Reflection.PropertyInfo> _Columns = new List<System.Reflection.PropertyInfo>();
        protected IXLWorksheet _sh;
        /// コンバーターの作成
        public ClosedXmlRangeConverter(IXLWorksheet sh)
        {
            _sh = sh;
            // 最初の行をコンバート先のテーブルと照合する
            var props = typeof(T).GetProperties();
            int col = 1;
            while (sh.Cell(1, col).Value.ToString() != "")
            {
                var text = sh.Cell(1, col).Value.ToString();
                var prop = props.FirstOrDefault(t => t.Name == text);
                if (prop != null)
                {
                    _Columns.Add(prop);
                }
                col++;
            }
        }

        /// 行単位でコンバート
        public T ToItem(int row)
        {
            var item = new T();
            for (int col = 0; col < this._Columns.Count; col++)
            {
                var prop = _Columns[col];
                var o = _sh.Cell(row, col + 1).ToString();
                prop.SetValue(item, o);
            }
            return item;
        }
        /// 全てのデータをコンバート
        public List<T> ToList()
        {
            var items = new List<T>();
            int r = 2;
            while (_sh.Cell(r, 1).GetValue<string>() != "")
            {
                var item = this.ToItem(r);
                items.Add(item);
                r++;
            }
            return items;
        }
    }
}
