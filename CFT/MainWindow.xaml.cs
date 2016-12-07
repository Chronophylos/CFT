using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CerealFileTransfer {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const Int32 baudrate = 9600;
        private const Int32 packageSize = 4096 * 2;
        private const Int32 bufferSize = 4096;
        private Network network;
        private File file;
        private System.Threading.Timer timer;
        private String fileName;

        public MainWindow() {
            InitializeComponent();
            network = new Network(baudrate, bufferSize, packageSize);
            file = new File("%USERPROFILE%\\Desktop", packageSize);
            timer = new System.Threading.Timer(timer_tick, null, 0, 500);
        }

        private void timer_tick(Object o) {
            if (!network.isDataAvailable()) return;
            Byte[][] headerpackage = new Byte[1][];
            headerpackage = network.GetPackage(1);
            String[] header = Convert.ToString(headerpackage[0]).Split(':').ToArray();
            String filename = header[0];
            String filesize = header[1];
            Int32 packages = Convert.ToInt32(header[3]);
            switch (MessageBox.Show("Do you want to recieve " + fileName + "?", "", MessageBoxButton.YesNo)) {
                case MessageBoxResult.No:
                    return;
                default:
                    break;
            }
            Byte[][] datapackage = new Byte[packages][];
            datapackage = network.GetPackage(packages);
            file.Write(fileName, datapackage);
        }

        private void Btn_browse_Click(Object sender, RoutedEventArgs e) {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowDialog();
            this.Txb_path.Text = fileDialog.FileName;
            this.fileName = fileDialog.FileName;
        }

        private void Btn_send_Click(Object sender, RoutedEventArgs e) {
            Byte[][] headerpackage = new Byte[1][];
            headerpackage[0] = Encoding.UTF8.GetBytes(this.fileName + ":" + "???B" + ":" + Convert.ToString(file.getPackages(this.fileName)));
            Byte[][] package = new Byte[file.getPackages(this.fileName)][];
            package = file.Read(this.fileName);
            network.SendPackage(headerpackage);
            network.SendPackage(package);
        }

        private void CFT_Loaded(object sender, RoutedEventArgs e) {
            network.Open();
        }
    }
}
