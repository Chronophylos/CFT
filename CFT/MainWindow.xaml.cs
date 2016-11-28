using __Cereal__;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace CerealFileTransfer {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private String portName;
        private Int32 baudrate;
        private Int32 dataBits;
        private StopBits stopBits;
        private Parity parity;
        private Int32 bufferSize;
        private Cereal rs232;
        //private DispatcherTimer Timer;

        private Boolean isConnectionOK;

        private List<String> fileNames;

        private const Int16 MAX_PACKAGE_SIZE = 4096 - 4 - 4; //Int16.MaxValue - 4 - 4;

        public MainWindow() {
            InitializeComponent();
            Debug.WriteLine("Aviable Serial Ports: '" + String.Join("', '", SerialPort.GetPortNames()) + "'");
            this.portName = SerialPort.GetPortNames()[0];
            this.baudrate = 9600;
            this.dataBits = 8;
            this.stopBits = StopBits.One;
            this.parity = Parity.None;
            this.bufferSize = MAX_PACKAGE_SIZE;
            this.rs232 = new Cereal(this.portName,
                                    this.baudrate,
                                    this.dataBits,
                                    this.stopBits,
                                    this.parity,
                                    this.bufferSize);
        }

        // convert control and data to a package ready to send
        Byte[] StringToPackage(String control, String data) {
            Byte[] controlBytes = new Byte[4];
            System.Buffer.BlockCopy(control.ToCharArray(), 0,
                                    controlBytes, 0, 4);
            Int32 dataSize = data.Length * sizeof(Char);
            Byte[] dataSizeBytes = new Byte[4];
            dataSizeBytes = BitConverter.GetBytes(dataSize);
            Byte[] dataBytes = new Byte[dataSize];
            System.Buffer.BlockCopy(data.ToCharArray(), 0,
                                    dataBytes, 0, 4);

            Byte[] package = new Byte[4 + 4 + dataSize];
            package = controlBytes.Concat(dataSizeBytes.Concat(dataBytes)
                                                       .ToArray()).ToArray();
            return package;
        }

        Boolean OpenCerealPort() {
            /* open COM1
            Display a Error MessageBox when open() return false
                OK to retry opening COM1
                CANCEL to close the program
            */
            Boolean isOpen = false;
            while(!isOpen) {
                if(!this.rs232.Open()){
                    Debug.Write("Can't open SerialPort");
                    switch(System.Windows.MessageBox.Show("ERROR: Can't open " + this.portName + ".\n" +
                                                          "Maybe " + this.portName + " is in use or does not exist\n" +
                                                          "Retry opening " + this.portName + "?",
                                                          "Error opening " + this.portName,
                                                          MessageBoxButton.OKCancel,
                                                          MessageBoxImage.Error,
                                                          MessageBoxResult.OK)) {
                        case MessageBoxResult.Cancel:
                            return false;
                        default:
                            //isOpen = true;
                            break;
                    }
                } else {
                    Debug.Write("SerialPort opened");
                    isOpen = true;
                }
            }
            this.Rtb_Log.AppendText("[  OK  ] Port " + this.portName + " opened\n");

            this.rs232.SetDTR(true); // we're ready to work
            this.Rtb_Log.AppendText("[ INFO ] Terminal ready\n");
            // are you ready too?
            while(!this.rs232.IsDCD()) {
                switch(System.Windows.MessageBox.Show("ERROR: Cannot connect with Partner\n" +
                                                      "Maybe the cable is not connected correctly?",
                                                      "Error connecting to Partner",
                                                      MessageBoxButton.OKCancel,
                                                      MessageBoxImage.Error,
                                                      MessageBoxResult.OK)) {
                    case MessageBoxResult.Cancel:
                        return false;
                    default:
                        break;
                } // we're waiting
            }
            this.isConnectionOK = true;
            this.Rtb_Log.AppendText("[  OK  ] Partner ready\n");
            return true;
        }

        private Boolean ConnectToPort() {
            if (!isConnectionOK) { return true; }

            while (!this.rs232.IsDCD()) {
                switch (System.Windows.MessageBox.Show("ERROR: Cannot connect with Partner\n" +
                                                      "Maybe the cable is not connected correctly?",
                                                      "Error connecting to Partner",
                                                      MessageBoxButton.OKCancel,
                                                      MessageBoxImage.Error,
                                                      MessageBoxResult.OK)) {
                    case MessageBoxResult.Cancel:
                        return false;
                    default:
                        break;
                }
            }
            isConnectionOK = true;
            return true;
        }

        private void CFT_Loaded(Object sender, RoutedEventArgs e) {
            try { this.isConnectionOK = OpenCerealPort(); }
            catch (Exception ex) { Debug.Print(ex.Message); }
        }

        private void Btn_browse_Click(Object sender, RoutedEventArgs e) {
            OpenFileDialog fileDialog = new OpenFileDialog() {
                Multiselect = true
            };
            switch(fileDialog.ShowDialog()) {
                case System.Windows.Forms.DialogResult.OK:
                    break;
                default:
                    return;
            }
            this.Txb_path.Text = String.Join(";", fileDialog.FileNames);
            this.fileNames = fileDialog.FileNames.ToList();
        }

        private void Btn_send_Click(Object sender, RoutedEventArgs e) {
            this.Btn_send.IsEnabled = false; // disable button

            // check isConnectionOK
            if (!isConnectionOK) {
                if (!ConnectToPort()) {
                    this.Btn_send.IsEnabled = true;
                    return;
                };                
            }

            // checkPath not null
            if (this.Txb_path.Text == "") {
                this.Btn_send.IsEnabled = true;
                return;
            }

            // check if path exist
            try {
                this.fileNames.Concat(this.Txb_path.Text.Split(new Char[] { ';' }).ToList());
                foreach (String file in this.fileNames) {
                    if (!System.IO.File.Exists(file)) { this.fileNames.Remove(file); } // if path doesn't exit remove it
                }
            } catch (ArgumentNullException ex) {
                Debug.Print(ex.Message);
                return;
            }

            // are you ready for tranfer?
            this.rs232.SetRTS(true);
            while (this.rs232.IsCTS()); 
            this.Rtb_Log.AppendText("[  OK  ] Partner is clear to send\n");

            foreach (String file in this.fileNames) {
                // how many packages do we need?
                Int32 fileSize = (Int32)new System.IO.FileInfo(file).Length; // get fileSize
                Int32 packageNumber = fileSize / MAX_PACKAGE_SIZE;
                if (fileSize % MAX_PACKAGE_SIZE > 0) { packageNumber++; }

                this.Rtb_Log.AppendText("[ INFO ] Send " + file + " (" + fileSize + "Byte) in " + Convert.ToString(packageNumber) + " Packages");

                this.Rtb_Log.AppendText("[  OK  ] Send INFO Package");

                // make info package
                Byte[] infopackage = this.StringToPackage("INFO", "PackageNumber:" + Convert.ToString(packageNumber) + "\n" +
                                                                  "FileName:" + file + "\n");
                // send info package
                this.rs232.Write(infopackage, infopackage.Count());
                //StartProgressbar(infopackage.Count());

                this.Rtb_Log.AppendText("[  OK  ] Send DATA Packages");

                // send packages
                try {
                    // open file as stream
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read)) {
                        Byte[] buffer = new Byte[MAX_PACKAGE_SIZE];
                        // send data package
                        for (Int16 currentPackage = 0; currentPackage < packageNumber; currentPackage++) {
                            // get data from file
                            // read MAX_PACKAGE_SIZE byte into buffer starting from currentPackage * MAX_PACKAGE_SIZE
                            fs.Read(buffer, currentPackage * MAX_PACKAGE_SIZE, buffer.Length);

                            // make package
                            Byte[] datapackage = this.StringToPackage("DATA", System.Text.Encoding.Default.GetString(buffer));

                            // send package
                            rs232.Write(datapackage, datapackage.Count());
                        }
                        fs.Close();
                    }
                } catch (System.UnauthorizedAccessException ex) { Debug.Print(ex.Message); }
            }
            this.Btn_send.IsEnabled = true; // enable buttton
        }
        
        private void StartPB(Int32 Filesize, Int32 PackageCounter){
            Int32 StartTime = (Filesize * 8) * (1 / this.baudrate);
            this.Pb_progress.Value = ( 1 / (Filesize / (MAX_PACKAGE_SIZE * PackageCounter) ) ) * 100;
            Int32 CurrentEstTime = (Int32)(StartTime * (this.Pb_progress.Value / 100 ) );
            if (this.Pb_progress.Value > 99)
            {
                ResetPB();
            }
        }

        private void ResetPB(){
            this.Pb_progress.Value = 0;
        }
    }
}
