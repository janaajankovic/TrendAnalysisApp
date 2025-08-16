using TrendAnalysis.Contracts;
using System.Windows.Forms;
using System.Windows;


namespace TrendAnalysis.UI.Services 
{
    public class WpfFileService : IFileService
    {
        public string SaveFile(string defaultFileName, string filter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = filter;
            saveFileDialog.FileName = defaultFileName;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog.FileName;
            }
            return null; 
        }
    }
}