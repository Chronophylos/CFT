using System;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using __Cereal__;

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
        private Cereal rs232;

        private Boolean isConnectionOK;

        public MainWindow() {
            InitializeComponent();

            this.portName = "COM1";
            this.baudrate = 9600;
            this.dataBits = 8;
            this.stopBits = StopBits.One;
            this.parity = Parity.None;
            this.rs232 = new Cereal(this.portName,
                                    this.baudrate,
                                    this.dataBits,
                                    this.stopBits,
                                    this.parity);
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
            while(true) {
                if(!this.rs232.Open()) {
                    switch(System.Windows.MessageBox.Show("ERROR: Can't open " + this.portName + ".\n" +
                                            "Maybe " + this.portName + " is in use or does not exist\n" +
                                            "Retry opening " + this.portName + "?",
                                            "Error opening " + this.portName,
                                            MessageBoxButton.OKCancel,
                                            MessageBoxImage.Error,
                                            MessageBoxResult.OK)) {
                        case MessageBoxResult.OK:
                            break;
                        case MessageBoxResult.Cancel:
                            return false;
                        case MessageBoxResult.None:
                            break;
                        case MessageBoxResult.Yes:
                            break;
                        case MessageBoxResult.No:
                            break;
                    }
                } else { break; }
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
                    case MessageBoxResult.OK:
                        break;
                    case MessageBoxResult.Cancel:
                        return false;
                    default:
                        break;
                } // we're waiting
            }
            this.Rtb_Log.AppendText("[  OK  ] Partner ready\n");
            return true;
        }

        private void CFT_Loaded(Object sender, RoutedEventArgs e) {
            this.isConnectionOK = OpenCerealPort();
        }

        private void Btn_browse_Click(Object sender, RoutedEventArgs e) {
            OpenFileDialog fileDialog = new OpenFileDialog() {
                Multiselect = false
            };
            switch(fileDialog.ShowDialog()) {
                case System.Windows.Forms.DialogResult.OK:
                    break;
                default:
                    return;
            }
            this.Txb_path.Text = fileDialog.FileName;
        }

        private void Btn_send_Click(Object sender, RoutedEventArgs e) {

        }
    }
}
