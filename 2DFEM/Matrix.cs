using System;
using System.Collections.Generic;

namespace _2DFEM
{
    struct Matrix
    {
        private readonly int rowsCount, colsCount;          // doesnt actually do anything
        private Dictionary<int, Dictionary<int, double>> _rows;

        public Matrix(int rowsCount, int colsCount)
        {
            _rows = new Dictionary<int, Dictionary<int, double>>();
            this.rowsCount = rowsCount;
            this.colsCount = colsCount;
        }

        public double this[int row, int col]
        {
            get
            {
                if (row < 0 || row >= RowsCount || col < 0 || col >= ColsCount)
                    throw new ArgumentOutOfRangeException();

                return GetAt(row, col);
            }
            set
            {
                if (row < 0 || row >= RowsCount || col < 0 || col >= ColsCount)
                    throw new ArgumentOutOfRangeException();

                SetAt(row, col, value);
            }
        }

        public int RowsCount
        {
            get
            {
                return rowsCount;
            }
        }

        public int ColsCount
        {
            get
            {
                return colsCount;
            }
        }

        public double GetAt(int row, int col)
        {
            Dictionary<int, double> cols;
            if (_rows.TryGetValue(row, out cols))
            {
                double value = default(double);
                if (cols.TryGetValue(col, out value))
                    return value;
            }
            return default(double);
        }

        public void SetAt(int row, int col, double value)
        {
            if (EqualityComparer<double>.Default.Equals(value, default(double)))
            {
                RemoveAt(row, col);
            }
            else
            {
                Dictionary<int, double> cols;
                if (!_rows.TryGetValue(row, out cols))
                {
                    cols = new Dictionary<int, double>();
                    _rows.Add(row, cols);
                }
                cols[col] = value;
            }
        }

        public void RemoveAt(int row, int col)
        {
            Dictionary<int, double> cols;
            if (_rows.TryGetValue(row, out cols))
            {
                cols.Remove(col);
                if (cols.Count == 0)
                    _rows.Remove(row);
            }
        }

        public IEnumerable<KeyValuePair<int, double>> GetRowData(int row)
        {
            Dictionary<int, double> cols;
            if (_rows.TryGetValue(row, out cols))
            {
                foreach (KeyValuePair<int, double> pair in cols)
                {
                    yield return pair;
                }
            }
        }

        public int GetRowDataCount(int row)
        {
            Dictionary<int, double> cols;
            if (_rows.TryGetValue(row, out cols))
            {
                return cols.Count;
            }
            return 0;
        }

        public IEnumerable<double> GetColumnData(int col)
        {
            foreach (KeyValuePair<int, Dictionary<int, double>> rowdata in _rows)
            {
                double result;
                if (rowdata.Value.TryGetValue(col, out result))
                    yield return result;
            }
        }

        public int GetColumnDataCount(int col)
        {
            int result = 0;

            foreach (KeyValuePair<int, Dictionary<int, double>> cols in _rows)
            {
                if (cols.Value.ContainsKey(col))
                    result++;
            }
            return result;
        }
        
        public static Vector operator *(Matrix m, Vector v)
        {
            if (v.length != m.ColsCount)
                throw new ArgumentException("Matrix and vector size must match.");

            int length = m.RowsCount;

            double[] output = new double[length];
            for (int i = 0; i < length; i++)
            {
                output[i] = 0;
                foreach (var p in m.GetRowData(i))
                    output[i] += p.Value * v[p.Key];
            }

            return new Vector(output);
        }
    }
}
