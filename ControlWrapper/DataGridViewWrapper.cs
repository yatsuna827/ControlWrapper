using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ControlWrapper
{
    public class DataGridViewWrapper<TData>
    {
        private readonly DataGridView _dataGridView;
        private readonly Dictionary<string, DataGridViewTextBoxColumn> _columnMap;

        public DataGridViewWrapper(DataGridView dataGridView)
        {
            _dataGridView = dataGridView;
            _columnMap = new Dictionary<string, DataGridViewTextBoxColumn>();

            var dgvPropertyInfo = typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            dgvPropertyInfo.SetValue(dataGridView, true, null);

            var headers = new List<DataGridViewTextBoxColumn>();
            foreach (var prop in typeof(TData).GetProperties())
            {
                foreach (var attr in prop.GetCustomAttributes(true))
                {
                    if (attr is DataGridViewRowHeaderAttribute dgvHeaderAttr)
                    {
                        var col = new DataGridViewTextBoxColumn()
                        {
                            DataPropertyName = prop.Name,
                            HeaderText = dgvHeaderAttr.HeaderText ?? prop.Name,
                            ReadOnly = true,
                            Resizable = DataGridViewTriState.False,
                            Width = dgvHeaderAttr.Width,
                            MinimumWidth = dgvHeaderAttr.Width,
                            SortMode = DataGridViewColumnSortMode.Programmatic,
                        };
                        if (dgvHeaderAttr.Resizable)
                            col.Resizable = DataGridViewTriState.True;
                        if (dgvHeaderAttr.Font != null)
                            col.DefaultCellStyle.Font = new Font(dgvHeaderAttr.Font, 9);

                        headers.Add(col);
                        _columnMap.Add(prop.Name, col);
                    }
                }
            }

            _dataGridView.Columns.AddRange(headers.ToArray());
            _dataGridView.AutoGenerateColumns = false;

            _dataGridView.AllowUserToAddRows = false;
            _dataGridView.AllowUserToDeleteRows = false;
            _dataGridView.AllowUserToResizeColumns = false;
            _dataGridView.AllowUserToResizeRows = false;

            _dataGridView.ReadOnly = true;
        }

        public void SetData(IEnumerable<TData> data)
        {
            _dataGridView.DataSource = data.ToArray();
        }
        public void SetData(IList<TData> data)
        {
            _dataGridView.DataSource = data;
        }
        public void SetColumnVisibility(string key, bool value)
            => _columnMap[key].Visible = value;

    }

    public class DataGridViewRowHeaderAttribute : Attribute
    {
        public int Width { get; }
        public string HeaderText { get; }
        public bool Resizable { get; }
        public string Font { get; }

        public DataGridViewRowHeaderAttribute(int width, string headerText, bool resizable = false, string font = null)
            => (Width, HeaderText, Resizable, Font) = (width, headerText, resizable, font);
    }
}
