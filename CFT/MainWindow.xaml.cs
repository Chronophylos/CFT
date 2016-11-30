using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private System.Threading.Timer timer;
        private String fileName;

        public MainWindow() {
            InitializeComponent();
            network = new Network(baudrate, bufferSize, packageSize);
            timer = new System.Threading.Timer(timer_tick, null, 0, 500);
        }

        private void timer_tick(Object o) {
            Byte[][] headerpackage = new Byte[1][];
            headerpackage = network.GetPackage(1);
            List <String> header = Convert.ToString(headerpackage[1]).Split(':').ToList();
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
            // datapackage to file
        }

        private void Btn_browse_Click(Object sender, RoutedEventArgs e) {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowDialog();
            this.Txb_path.Text = fileDialog.FileName;
            this.fileName = fileDialog.FileName;
        }

        private void Btn_send_Click(Object sender, RoutedEventArgs e) {
            // read file
            // calc number of packages
            // send file
        }
    }
}
