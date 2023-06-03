using System;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace AnySoftMobile.Controls;

public class UniformGrid : Grid
{
    public UniformGrid()
    {
    }

    /// <summary>
    ///  Gets the number of columns that are in the grid.
    /// </summary>
    public int Columns { get; set; }

    /// <summary>
    /// Gets  the number of rows that are in the grid.
    /// </summary>
    public int Rows { get; set; }


    protected override void LayoutChildren(double x, double y, double width, double height)
    {
        base.LayoutChildren(x, y, width, height);
    }

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        RefreshLayout();
    }

    protected override void OnChildRemoved(Element child)
    {
        base.OnChildRemoved(child);
        RefreshLayout();
    }

    private void RefreshLayout()
    {
        int ColsRows = Columns;

        int row = Rows;
        int column = 0;

        for (int i = 0; i < Children.Count; i++)
        {
            if (column >= ColsRows && Children.Count > 3)
            {
                row++;
                column = 0;
            }

            SetColumn(Children[i], column);
            SetRow(Children[i], row);
            column++;
        }

        InvalidateLayout();
        /*this.Rows =
            this.Columns = ColsRows;*/
    }
}