// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;

using ConsoleTables;

namespace FrameworkDetector.CLI;

internal static class ConsoleTableExtensions
{
    /// <summary>
    /// Sets the <see cref="ConsoleTable"/>'s <see cref="ConsoleTable.MaxWidth"/> to fit the largest contents of the given column index.
    /// </summary>
    /// <param name="table">The <see cref="ConsoleTable"/>.</param>
    /// <param name="columnIndex">The column index.</param>
    /// <exception cref="ArgumentOutOfRangeException">The column index is out of range.</exception>
    public static void SetMaxWidthBasedOnColumn(this ConsoleTable table, int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= table.Columns.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(columnIndex));
        }

        table.MaxWidth = table.Rows.Max(r => (r[columnIndex] ?? table.Columns[columnIndex])?.ToString()?.Length ?? MinimumColumnWidth);
    }

    /// <summary>
    /// The absolute minimum <see cref="ConsoleTable.MaxWidth"/> set when calling <see cref="SetMaxWidthBasedOnColumn(ConsoleTable, int)"/>.
    /// </summary>
    public const int MinimumColumnWidth = 3;
}
