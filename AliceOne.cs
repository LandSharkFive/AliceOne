/*
 * ===========================================================================
 * MODULE:          AliceOne CSV Engine
 * AUTHOR:          Koivu
 * DATE:            2026-04-13
 * VERSION:         1.0.0
 * LICENSE:         MIT License
 * ===========================================================================
 *
 * ABSTRACT:
 * A high-performance, in-memory CLI CSV editor. Designed for O(log n) 
 * search/updates via binary search on sorted record identifiers.
 *
 * KEY FEATURES:
 * - Hybrid Sort: Handles numerical and alphabetical data types automatically.
 * - Dual-Predicate Querying: Supports complex CLI-based filtering logic.
 * - Conflict Resolution: Auto-updates existing records by ID during insertion.
 * * ---------------------------------------------------------------------------
 * Copyright (c) 2026 Koivu.
 * Licensed under the MIT License.
 * ---------------------------------------------------------------------------
 */


namespace AliceOne
{

    /// <summary>
    /// Provides a robust, in-memory CSV (Comma-Separated Values) editor and data management engine.
    /// </summary>
    public class AliceOne
    {
        public bool IsSorted = true;

        private List<string[]> Rows = new List<string[]>();

        private List<string> Columns = new List<string>();

        private IComparer<string[]> comparer = new IdentifierComparer();

        private const int PageSize = 10;

        private int CurrentRow = 0;


        // ----- Initialization ----------

        public AliceOne()
        {
            IsSorted = true;
        }

        /// <summary>
        /// Loads a CSV file into the Rows list, clearing any existing data.
        /// </summary>
        public void Load(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"File not found: {path}");  
                return;
            }

            Rows.Clear();
            Columns.Clear();

            using (var reader = new StreamReader(path))
            {
                string headerLine = reader.ReadLine();
                if (headerLine != null)
                {
                    Columns = headerLine.Split(',').Select(h => h.Trim()).ToList();
                }

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        Rows.Add(line.Split(',').Select(c => c.Trim()).ToArray());
                    }
                }
            }

            Rows.Sort(comparer);
            IsSorted = true;
        }


        /// <summary>
        /// Clears all data and resets the state. 
        /// </summary>
        public void Clear()
        {
            Rows.Clear();
            Columns.Clear();
            IsSorted = true;
            CurrentRow = 0;
        }

        // ------ Read and Write Files -----------

        /// <summary>
        /// Loads a file into memory. The file is expected to be in CSV format. Each line represents a row and columns are separated by commas.
        /// </summary>
        public void HandleLoad(string line)
        {
            string[] parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
            {
                Load(parts[1]);
            }
            else 
            {
                Console.WriteLine("Error: Invalid arguments. Usage: L [file_path]");
            }
        }

        /// <summary>
        /// Show help text to the console.  Discus all available commands and their expected formats.
        /// </summary>
        /// 
        public void ShowHelp()
        {
            Console.WriteLine("\n--- CSV Editor Commands ---");
            Console.WriteLine("\n[ VIEWING ]");
            Console.WriteLine("  A              : Show table status");
            Console.WriteLine("  L [path]       : Load a CSV file");
            Console.WriteLine("  W [path]       : Write (Save) current data to file");
            Console.WriteLine("\n[ NAVIGATION ]");
            Console.WriteLine("  N              : Next page");
            Console.WriteLine("  P              : Previous page");
            Console.WriteLine("  T              : Top of file");
            Console.WriteLine("  B              : Bottom of file");
            Console.WriteLine("\n[ SEARCHING ]");
            Console.WriteLine("  S              : Select all");
            Console.WriteLine("  S [col] [op] [val] : Search (ex: S Age > 30) ");
            Console.WriteLine("  S [c1] [op] [v1] [c2] [op] [v2] : Search (ex: S Age > 20 City == NY)");
            Console.WriteLine("  Operators: ==, !=, >, <, >=, <=");
            Console.WriteLine("\n[ EDITING ]");
            Console.WriteLine("  I [v1,v2...]   : Insert new row (comma-separated)");
            Console.WriteLine("  D [col] == [id] : Delete row by ID");
            Console.WriteLine("  X              : Remove duplicate rows");
            Console.WriteLine("\n[ SORTING ]");
            Console.WriteLine("  O              : Sort by id");
            Console.WriteLine("\n  Q              : Quit");
            Console.WriteLine("---------------------------\n");
        }


        /// <summary>
        /// Writes the current in-memory data to a CSV file. Columns are separated by commas.
        /// </summary>
        public void HandleWrite(string line)
        {
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                string path = parts[1];
                Write(path);
            }
            else
            {
                Console.WriteLine("Invalid command format. Use: W [path]");
            }
        }

        // <summary>
        /// Serializes the current in-memory Rows back into a CSV file.
        /// </summary>
        public void Write(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine(string.Join(",", Columns));

                for (int i = 0; i < Rows.Count; i++)
                {
                    string line = string.Join(",", Rows[i]);
                    writer.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Gets the index of a column by its name.
        /// </summary>
        /// <returns>The index of the column, or -1 if not found.</returns>
        private int GetColumnIndex(string input)
        {
            return Columns.FindIndex(h => h.Equals(input, StringComparison.OrdinalIgnoreCase));
        }

        // -------- Insert ---------

        /// <summary>
        /// Inserts a new row into the data grid. The input is expected to be in the format: I [v1,v2,...].
        /// </summary>
        public void HandleInsert(string line)
        {
            // 1. Find the first space to separate the command (e.g., 'I') from the data
            int firstSpace = line.IndexOf(' ');
            if (firstSpace == -1)
            {
                Console.WriteLine("Invalid command format. Use: I v1, v2, v3, ...]");
                return;
            }
            // 2. Extract the data portion and clean up.
            string dataPart = line.Substring(firstSpace + 1).Trim();

            // 2. Split by comma and trim whitespace.    
            string[] newRow = dataPart.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (newRow.Length != Columns.Count)
            {
                Console.WriteLine($"Warning: Column mismatch. Expected {Columns.Count} values, received {newRow.Length}.");
            }

            // 3. Add the new row.
            Insert(newRow);
        }

        /// <summary>
        /// Performs an Insert or Update operation: Updates an existing row if the ID (index 0) 
        /// is found, otherwise inserts a new row while maintaining sort order.
        /// This prevents duplicate entries and eliminates the need for manual de-duping.
        /// </summary>
        private void Insert(string[] newRow)
        {
            if (newRow == null || newRow.Length == 0) return;

            string id = newRow[0];
            if (IsSorted)
            {
                // Binary Search for the ID. 
                int index = Rows.BinarySearch(new[] { id }, comparer);
                if (index >= 0)
                {
                    Rows[index] = newRow;
                }
                else
                {
                    Rows.Insert(~index, newRow);
                }
            }
            else
            {
                // Linear Search for the ID.
                int index = Rows.FindIndex(r => r[0] == id);
                if (index >= 0)
                {
                    // Update existing row. Prevent duplicates.
                    Rows[index] = newRow;
                }
                else
                {
                    Rows.Add(newRow);
                }
            }
        }


        // --------- Compare ---------


        /// <summary>
        /// Helper class to tell BinarySearch to compare column 0.
        /// </summary>
        private class IdentifierComparer : IComparer<string[]>
        {
            public int Compare(string[] x, string[] y)
            {
                // 1. Basic Null/Length Checks
                if (x == null || y == null || x.Length == 0 || y.Length == 0) return 0;

                string xId = x[0];
                string yId = y[0];
                // 2. Try Numeric Comparison first
                bool xIsInt = int.TryParse(xId, out int xVal);
                bool yIsInt = int.TryParse(yId, out int yVal);

                // 3. Compare integers.
                if (xIsInt && yIsInt)
                {
                    return xVal.CompareTo(yVal);
                }

                // 4. Handle Mixed Cases. One is a number, one is a string.
                // Usually, we want numbers to come before strings.
                if (xIsInt) return -1; // x is a number, y is a string -> x comes first
                if (yIsInt) return 1;  // y is a number, x is a string -> y comes first

                // 4. Both are strings.
                return string.Compare(xId, yId, StringComparison.OrdinalIgnoreCase);
            }
        }


        /// <summary>
        /// A flexible comparer that allows sorting the data by any column index.
        /// Uses Hybrid Logic to sort numbers numerically and everything else alphabetical.
        /// </summary>
        private class ColumnComparer : IComparer<string[]>
        {
            // The column to compare.
            private readonly int colIndex;

            /// <summary>
            /// Initializes the comparer to target a specific column.
            /// </summary>
            public ColumnComparer(int index)
            {
                colIndex = index;
            }

            /// <summary>
            /// Compares two rows based on the value at the target column index.
            /// </summary>
            public int Compare(string[] x, string[] y)
            {
                // 1. Handle nulls first.  Convert nulls to strings.
                if (x == null || y == null) return 0;
                string valX = colIndex < x.Length ? x[colIndex] : string.Empty;
                string valY = colIndex < y.Length ? y[colIndex] : string.Empty;

                // 2. Hybrid Logic: Try Numbers first.
                if (double.TryParse(valX, out double nX) && double.TryParse(valY, out double nY))
                {
                    return nX.CompareTo(nY);
                }

                // 3. Compare two string.
                return string.Compare(valX, valY, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Performs a high-speed search if sorted, or a linear scan if not sorted.
        /// </summary>
        private int FindRowIndex(string target)
        {
            if (string.IsNullOrEmpty(target)) return -1;

            // 1. Trim whitespace.  If sorted, use binary search.
            target = target.Trim();
            if (IsSorted)
            {
                return Rows.BinarySearch(new[] { target }, comparer);
            }

            // 2. Linear Search.
            return Rows.FindIndex(r => r.Length > 0 && r[0] == target);
        }


        // ---------- Delete ----------


        /// <summary>
        /// Parses the delete command (e.g., "D index == 123") and removes the first matching row.
        /// </summary>
        public void HandleDelete(string line)
        {
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
            {
                Console.WriteLine("Syntax Error.  Use: D index == [id]");
                return;
            }

            // Delete command syntax:
            // parts[0] is 'D'
            // parts[1] is 'index' (the column name)
            // parts[2] is '=='
            // Check that the operator is '=='.

            int colIndex = GetColumnIndex(parts[1]);
            if (colIndex == -1)
            {
                Console.WriteLine($"Error: Column not found: {parts[1]}");
                return;
            }

            if (parts[2] != "==")
            {
                Console.WriteLine($"Error: Operator '{parts[2]}' not supported for delete. Use '=='.");
                return;
            }

            Delete(colIndex, parts[3]);
        }

        /// <summary>
        /// Locates and removes a row based on its column index and value.
        /// </summary>
        private void Delete(int colIndex, string value)
        {
            int rowIndex = -1;

            // Use Binary Search ONLY if searching by the sorted ID column
            if (colIndex == 0)
            {
                rowIndex = Rows.BinarySearch(new[] { value }, comparer);
            }
            else
            {
                // Linear scan lookup for other columns
                rowIndex = Rows.FindIndex(r => r.Length > colIndex && string.Equals(r[colIndex], value, StringComparison.OrdinalIgnoreCase));
            }

            if (rowIndex >= 0)
            {
                Rows.RemoveAt(rowIndex);
            }
            else
            {
                Console.WriteLine("Record not found.");
            }
        }


        // ---------- Search ----------


        /// <summary>
        /// Routes the 'SELECT' command to specific search logic based on argument count and format.
        /// </summary>
        public void HandleSelect(string line)
        {
            var p = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            switch (p.Length)
            {
                case 1:
                    SelectAll();
                    break;
                case 4:
                    int idx = GetColumnIndex(p[1]);
                    if (idx == -1) Console.WriteLine($"Error: Unknown column: {p[1]}");
                    else SelectWhere(idx, p[2], p[3]);
                    break;
                case 7:
                    int c1 = GetColumnIndex(p[1]);
                    int c2 = GetColumnIndex(p[4]);
                    if (c1 == -1 || c2 == -1) Console.WriteLine("Error: One or more columns not found.");
                    else SelectWhereDual(c1, p[2], p[3], c2, p[5], p[6]);
                    break;
                default:
                    Console.WriteLine("Search Usage:");
                    Console.WriteLine("  S (Select All)");
                    Console.WriteLine("  S [col] [op] [val] (Single Filter)");
                    Console.WriteLine("  S [c1] [o1] [v1] [c2] [o2] [v2] (Dual Filter)");
                    break;
            }
        }



        /// <summary>
        /// Displays the first 10 rows and a row count to the console.
        /// Useful for a quick heads-up view of the dataset.
        /// </summary>
        private void SelectAll()
        {
            Console.WriteLine(string.Join(",", Columns));

            int count = 0;
            foreach (var row in Rows)
            {
                count++;
                if (count > 10) break;
                Console.WriteLine(string.Join(",", row));
            }
            Console.WriteLine($"Rows: {count}");
        }

        /// <summary>
        /// Filters and displays rows based on a single column condition.
        /// Prints the headers, matching rows, and the total count of matches found.
        /// </summary>
        private void SelectWhere(int c1, string o1, string v1)
        {
            Console.WriteLine(string.Join(",", Columns));
            // Parse the filter value ONCE upfront
            bool isNumericFilter = double.TryParse(v1, out double valNum);
            int matchCount = 0;

            foreach (var row in Rows)
            {
                if (IsMatch(row, c1, o1, v1, isNumericFilter, valNum))
                {
                    Console.WriteLine(string.Join(",", row));
                    matchCount++;
                }
            }

            Console.WriteLine($"Rows: {matchCount}");
        }

        /// <summary>
        /// Evaluates a single cell within a row against a provided value using a specific operator.
        /// Supports both numeric (double) and case-insensitive string comparisons.
        /// </summary>
        private bool IsMatch(string[] row, int col, string op, string val, bool isNumericFilter, double valNum)
        {
            if (col < 0 || col >= row.Length) return false;
            string cell = row[col];

            // If the filter is numeric and the cell is numeric, use high-speed double comparison
            if (isNumericFilter && double.TryParse(cell, out double cellNum))
            {
                return op switch
                {
                    "==" => cellNum == valNum,
                    "!=" => cellNum != valNum,
                    ">" => cellNum > valNum,
                    "<" => cellNum < valNum,
                    ">=" => cellNum >= valNum,
                    "<=" => cellNum <= valNum,
                    _ => false
                };
            }

            // Otherwise fallback to case-insensitive string comparison
            int cmp = string.Compare(cell, val, StringComparison.OrdinalIgnoreCase);
            return op switch
            {
                "==" => cmp == 0,
                "!=" => cmp != 0,
                ">" => cmp > 0,
                "<" => cmp < 0,
                ">=" => cmp >= 0,
                "<=" => cmp <= 0,
                _ => false
            };
        }

        /// <summary>
        /// Filters and displays rows that satisfy two different conditions simultaneously with logical AND.
        /// </summary>
        private void SelectWhereDual(int c1, string o1, string v1, int c2, string o2, string v2)
        {
            Console.WriteLine(string.Join(",", Columns));
            bool isNumericFilterOne = double.TryParse(v1, out double valOne);
            bool isNumericFilterTwo = double.TryParse(v2, out double valTwo);

            int matchCount = 0;
            foreach (var row in Rows)
            {
                if (IsMatch(row, c1, o1, v1, isNumericFilterOne, valOne) 
                    && IsMatch(row, c2, o2, v2, isNumericFilterTwo, valTwo))
                {
                    Console.WriteLine(string.Join(",", row));
                    matchCount++;
                }
            }
            Console.WriteLine($"Rows: {matchCount}");
        }


        // ---------- Paging ----------

        /// <summary>
        /// Advances the view to the next page of data.
        /// </summary>
        public void NextPage()
        {
            if (CurrentRow + PageSize < Rows.Count)
            {
                CurrentRow += PageSize;
            }
            else
            {
                CurrentRow = 0;
            }
            ShowPage();
        }

        /// <summary>
        /// Moves the view to the previous page of data.
        /// </summary>
        public void PreviousPage()
        {
            if (CurrentRow - PageSize >= 0)
            {
                CurrentRow -= PageSize;
            }
            else
            {
                CurrentRow = 0; 
            }
            ShowPage();
        }

        /// <summary>
        /// Displays the current page of data.
        /// </summary>
        void ShowPage()
        {
            Console.Clear(); 

            // Calculate last row.
            int lastRow = Math.Min(CurrentRow + PageSize, Rows.Count);

            Console.WriteLine(string.Join(",", Columns));
            for (int i = CurrentRow; i < lastRow; i++)
            {
                Console.WriteLine(string.Join(",", Rows[i]));
            }
        }

        /// <summary>
        /// Displays the first page.
        /// </summary>
        public void ShowTop()
        {
            CurrentRow = 0;
            ShowPage();
        }

        /// <summary>
        /// Displays the last page.
        /// </summary>
        public void ShowBottom()
        {
            CurrentRow = Math.Max(0, Rows.Count - PageSize);
            ShowPage();
        }

        // --------- Sorting ------------

        /// <summary>
        /// Routes the 'Sort' command. Can perform a default primary sort 
        /// or a specialized sort on a specific column.
        /// </summary>
        public void HandleSort(string line)
        {
            // 1. If no column index is provided, sort by the default comparer (Column 0).
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                Rows.Sort(comparer);
                IsSorted = true;
            }
        }


        // ------- Helper Methods -----------

        /// <summary>
        /// Outputs a diagnostic summary of the current data state to the console.
        /// Displays the source file, record count, and the 'IsSorted' flag—which 
        /// determines if the system can use high-speed Binary Search (O(log n)).
        /// </summary>
        public void ShowStatus()
        {
            Console.WriteLine($"Rows: {Rows.Count}");
            Console.WriteLine($"Columns: {Columns.Count}");
            Console.WriteLine($"Sorted: {IsSorted}");
        }


        /// <summary>
        /// Removes duplicate rows by ID and sorts the collection.
        /// </summary>
        public void FixDuplicates()
        {
            RemoveDuplicateById();
            Rows.Sort(comparer);
            IsSorted = true;
        }

        /// <summary>
        /// Removes duplicate rows based on the value in Column 0 (ID). 
        /// </summary>
        private void RemoveDuplicateById()
        {
            if (Rows.Count == 0) return;

            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var uniqueRows = new List<string[]>(Rows.Count);

            foreach (var row in Rows)
            {
                if (row.Length > 0 && seenIds.Add(row[0]))
                {
                    uniqueRows.Add(row);
                }
            }
            Rows = uniqueRows;
        }

    }
}
