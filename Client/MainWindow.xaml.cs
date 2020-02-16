using Client.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace Client {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new PhoneViewModel();
        }

        private void ShowFilePicker_OnClick(object sender, RoutedEventArgs e) {
            var phoneViewModel = DataContext as PhoneViewModel;
            OpenFileDialog dlg = new OpenFileDialog {
                DefaultExt = ".jpg",
                Filter = "JPG Files (*.jpg)|*.jpg"
            };
            bool? result = dlg.ShowDialog();
            if ((bool)result && phoneViewModel.OpenFileCommand.CanExecute(dlg.FileName)) { 
                phoneViewModel.OpenFileCommand.Execute(dlg.FileName);
            }
        }
    }
}
