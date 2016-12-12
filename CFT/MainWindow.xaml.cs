using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace CerealFileTransfer {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const Int32 baudrate = 9600;
        private const Int32 packageSize = 4096 / 2;
        private const Int32 bufferSize = 4096;
        private Network network;
        private File file;
        private System.Threading.Timer timer;
        private String fileName;

        public MainWindow() {
            InitializeComponent();
            this.network = new Network(baudrate, bufferSize, packageSize);
            this.file = new File("%USERPROFILE%\\Desktop", packageSize);
            this.timer = new System.Threading.Timer(this.Timer_tick, null, 0, 500);
        }

        private void Timer_tick(Object o) {
            if (!this.network.IsDataAvailable())
                return;
            Byte[][] headerpackage = new Byte[1][];
            //headerpackage[1] = CreateSpecialByteArray(4069);
            headerpackage = this.network.GetPackage(1);
            String[] header = Encoding.UTF8.GetString(headerpackage[0]).Split(';').ToArray();
            String filename = header[0];
            String filesize = header[1];
            Int32 packages = Convert.ToInt32(header[3]);
            switch (MessageBox.Show("Do you want to recieve " + this.fileName + "?", "", MessageBoxButton.YesNo)) {
                case MessageBoxResult.No:
                    return;
                default:
                    break;
            }
            Byte[][] datapackage = new Byte[packages][];
            datapackage = this.network.GetPackage(packages);
            this.file.Write(this.fileName, datapackage);
        }

        private void Btn_browse_Click(Object sender, RoutedEventArgs e) {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowDialog();
            this.Txb_path.Text = fileDialog.FileName;
            this.fileName = fileDialog.FileName;
        }

        private void Btn_send_Click(Object sender, RoutedEventArgs e) {
            Byte[][] headerpackage = new Byte[1][];
            String info = this.fileName + ";" + "???B" + ";" + Convert.ToString(this.file.GetPackages(this.fileName));
            headerpackage[0] = Encoding.UTF8.GetBytes(info);
            Byte[][] package = new Byte[this.file.GetPackages(this.fileName)][];
            package = this.file.Read(this.fileName);
            this.network.SendPackage(headerpackage);
            this.network.SendPackage(package);
        }

        private void CFT_Loaded(Object sender, RoutedEventArgs e) {
            this.network.Open();
        }
        public static Byte[] CreateSpecialByteArray(Int32 length) {
            Byte[] arr = new Byte[length];
            for (Int32 i = 0; i < arr.Length; i++) {
                arr[i] = 0x20;
            }

            return arr;
        }
    }
}