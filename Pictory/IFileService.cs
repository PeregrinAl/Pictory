using System.Drawing;

namespace Pictory
{
    interface IFileService
    {
        void Save(System.Windows.FrameworkElement visual, string fileName);
    }
}
