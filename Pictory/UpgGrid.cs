using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pictory
{
    public class UpgGrid : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            Double childHeight = 0.0;
            Double childWidth = 0.0;
            Size size = new Size(0, 0);

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(availableSize.Width, availableSize.Height));
                if (child.DesiredSize.Width > childWidth)
                {
                    childWidth = child.DesiredSize.Width;   //We will be stacking vertically.
                }
                childHeight += child.DesiredSize.Height;    //Total height needs to be summed up.
            }

            size.Width = double.IsPositiveInfinity(availableSize.Width) ? childWidth : availableSize.Width;
            size.Height = double.IsPositiveInfinity(availableSize.Height) ? childHeight : availableSize.Height;

            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Boolean toggle = false;

            double yAxisHeight = 0.0;

            foreach (UIElement child in InternalChildren)
            {
                if (toggle == false)
                {
                    Rect rec = new Rect(new Point(0, yAxisHeight), child.DesiredSize);
                    child.Arrange(rec);
                    toggle = true;
                }
                else
                {
                    yAxisHeight += child.DesiredSize.Height;
                    Rect rec = new Rect(new Point(0, finalSize.Height - yAxisHeight), child.DesiredSize);
                    child.Arrange(rec);
                    toggle = false;
                }
            }
            return finalSize;
        }
    }
}

