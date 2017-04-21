using System;
using System.Collections.Generic;

namespace FEM_NET
{
    internal struct Matrix
    {
        private readonly Dictionary<int, Dictionary<int, double>> rows;

        public int RowCount { get; }
        public int ColumnCount { get; }

        public Matrix(int rowCount, int columnCount)
        {
            rows = new Dictionary<int, Dictionary<int, double>>();
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public double this[int row, int col]
        {
            get
            {
                return GetAt(row, col);
            }
            set
            {
                SetAt(row, col, value);
            }
        }

        private double GetAt(int row, int column)
        {
            Validate(row, column);

            Dictionary<int, double> cells;
            bool rowExists = rows.TryGetValue(row, out cells);
            if (!rowExists)
                return default(double);

            double value = default(double);
            cells.TryGetValue(column, out value);
            return value;
        }

        private void Validate(int row, int column)
        {
            if (row < 0 || row >= RowCount || column < 0 || column >= ColumnCount)
                throw new IndexOutOfRangeException();
        }

        private void SetAt(int row, int column, double value)
        {
            Validate(row, column);

            bool valueIsDefault = EqualityComparer<double>.Default.Equals(value, default(double));
            if (valueIsDefault)
            {
                RemoveAt(row, column);
                return;
            }

            Dictionary<int, double> cells;
            var rowExists = rows.TryGetValue(row, out cells);
            if (!rowExists)
            {
                cells = new Dictionary<int, double>();
                rows.Add(row, cells);
            }
            cells[column] = value;
        }

        private void RemoveAt(int row, int column)
        {
            Dictionary<int, double> cells;
            var rowExists = rows.TryGetValue(row, out cells);
            if (!rowExists)
                return;

            cells.Remove(column);
            if (cells.Count == 0)
                rows.Remove(row);
        }

        public static Vector operator *(Matrix matrix, Vector vector)
        {
            if (vector.Length != matrix.ColumnCount)
                throw new ArgumentException("Dimension mismatched.");

            int length = matrix.RowCount;

            var output = new double[length];
            for (int i = 0; i < length; i++)
            {
                output[i] = 0;
                foreach (var p in matrix.GetRowData(i))
                    output[i] += p.Value * vector[p.Key];
            }
            return new Vector(output);
        }

        private IEnumerable<KeyValuePair<int, double>> GetRowData(int row)
        {
            Dictionary<int, double> cells;
            var rowExists = rows.TryGetValue(row, out cells);
            if (!rowExists)
                yield break;

            foreach (KeyValuePair<int, double> pair in cells)
                yield return pair;
        }
    }
}
