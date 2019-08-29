using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace BMC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewModel viewModel = new ViewModel();
        public static RoutedCommand OpenFolder = new RoutedCommand();

        public MainWindow()
        {
            InitializeComponent();
            this.Title += " v" + GetAppVersion();

            DataContext = viewModel;
        }

        private string GetAppVersion()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            return String.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Revision);
        }

        private void OpenCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = viewModel.Model.NotConverting;
        }

        private void OpenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (e.Source == sourceButton) viewModel.UpdateSourcePath(dialog.SelectedPath);
                else if (e.Source == outputButton) viewModel.UpdateOutputPath(dialog.SelectedPath);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/sivu22/BMC");
        }
    }
}
