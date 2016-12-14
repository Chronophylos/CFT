using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace CerealFileTransfer {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const Int32 baudrate    = 9600;//128000;
        private const Int32 packageSize = 4096 / 2;
        private const Int32 bufferSize  = 4096;
        private Network     network;
        private File        file;
        private Timer       networkTimer;
        private String      fileName;

        public MainWindow() {
            InitializeComponent();
            String portName = "";
            while (!Array.Exists(SerialPort.GetPortNames(), element => element == portName)) { // while not this.portName in SerialPort.GetPortNames()
                if (SerialPort.GetPortNames().Length == 1) {
                    portName = SerialPort.GetPortNames()[0];
                    break;
                }
                if (InputBox.Show("Cereal File Transfer",
                                  "Please choose a Port: (" + String.Join(", ", SerialPort.GetPortNames()) + ")",
                                  ref portName) == System.Windows.Forms.DialogResult.Cancel) {
                    Environment.Exit(2);
                }
                portName = portName.ToUpper();
            }
            this.Rtb_Log.AppendText("Using Port: " + portName + '\n');

            this.network = new Network(portName, baudrate, bufferSize, packageSize, this.Pb_progress, Application.Current.Dispatcher);
            this.file = new File(packageSize);
            this.networkTimer = new Timer(this.NetworkTimer_tick, null, 0, 500);
        }

        static String BytesToString(long byteCount) {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        private void NetworkTimer_tick(Object o) {
            if (!this.network.IsDataAvailable) {
                return;
            }
            Application.Current.Dispatcher.Invoke((Action)(() => {
                this.Rtb_Log.AppendText("File Transfer incoming\n");
            }));

            this.networkTimer.Change(Timeout.Infinite, Timeout.Infinite); // stop timer

            Byte[][]    headerpackage = this.network.GetPackage(1);
            String[]    header =        Encoding.UTF8.GetString(headerpackage[0]).Split(';').ToArray();
            String      fileName =      header[0];
            String      fileSize =      header[1];
            Int32       packages =      Int32.Parse(header[3]);

            switch (MessageBox.Show("Do you want to recieve " + fileName + "? " + fileSize, "", MessageBoxButton.YesNo)) {
                case MessageBoxResult.No:
                    this.network.ImHappy(false);
                    this.networkTimer.Change(0, 500); // resume timer
                    return;
            }
            /* OpenFileDialog fileDialog = new OpenFileDialog() {
                FileName = System.IO.Path.GetFileName(fileName),
                CheckFileExists = false,
                ShowReadOnly = true,
                Multiselect = false,
                Title = "Save..."
            };*/
            SaveFileDialog fileDialog = new SaveFileDialog() {
                FileName = System.IO.Path.GetFileName(fileName),
                CheckFileExists = false,
                OverwritePrompt = true,
                CreatePrompt = false,
                Title = "Save..."
            };
            fileDialog.ShowDialog();

            this.network.ImHappy();

            Byte[][] datapackage = this.network.GetPackage(packages);
            this.file.Write(fileDialog.FileName, datapackage);

            Application.Current.Dispatcher.Invoke((Action)(() => {
                this.Rtb_Log.AppendText("File Transfer Successful\n");
            }));
            this.networkTimer.Change(0, 500); // resume timer
        }

        private void Btn_browse_Click(Object sender, RoutedEventArgs e) {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowDialog();
            this.Txb_path.Text = fileDialog.FileName;
            this.fileName = fileDialog.FileName;
        }

        private void Btn_send_Click(Object sender, RoutedEventArgs e) {
            try {
                if (this.fileName.Length == 0) {
                    return;
                }
            } catch(Exception ex) {
                Debug.Print(ex.Message);
                return;
            }
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(this.fileName);

            Byte[][] headerpackage = new Byte[1][];
            String info = this.fileName + ';' + BytesToString(fileInfo.Length) + ';' + "\0" + ';' + Convert.ToString(this.file.GetPackages(this.fileName)) + ';' + "\0";
            while (this.network.PackageSize != info.Length) {
                info += '\0';
            }

            headerpackage[0] = Encoding.UTF8.GetBytes(info);
            Byte[][] package = new Byte[this.file.GetPackages(this.fileName)][];

            package = this.file.Read(this.fileName);

            this.Rtb_Log.AppendText("Sending " + this.fileName + '\n');

            this.network.SendPackage(headerpackage);

            if (!this.network.IsPartnerHappy) {
                this.Rtb_Log.AppendText("File Transfer Rejected");
                return;
            }

            this.Rtb_Log.AppendText("File Transfer Accepted\n");
            this.network.SendPackage(package);
            this.Rtb_Log.AppendText("Finished sending file\n");
        }

        private void CFT_Loaded(Object sender, RoutedEventArgs e) {
            while (!this.network.Open()) ;
        }
    }
}