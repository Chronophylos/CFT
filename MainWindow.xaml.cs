using System;
using System.IO.Ports;
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
using __Cereal__;

namespace CerealFileTransfer {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            string portName = "COM1";
            int baudrate = 9600
            int dataBits = 8;
            StopBits stopBits = StopBits.One;
            Parity parity = Parity.None
            Cereal rs232 = new Cereal(portName, baudrate, dataBits, stopBits, parity);
            rtb_Log.AppendText("[ INFO ] Created Serial Port Driver on " + portName +
                               " with " + dataBits " databits, " + stopBits " and " + 
                               stopBits + " at a speed of " + baudrate " bps");

            /* open COM1
            Display a Error MessageBox when open() return false
                OK to retry opening COM1
                CANCEL to close the program
            */
            //bool retryCOM1 = true;
            //while (retryCOM1) {
            while(true) {
                if (!rs232.open()) {
                    switch (MessageBox.Show("ERROR: Can't open " + portName + ".\n" +
                                            "Maybe " + portName + " is in use or does not exist\n" +
                                            "Retry opening " + portName + "?",
                                            "Error opening " + portName,
                                            MessageBoxButton.OKCancel,
                                            MessageBoxImage.Asterisk,
                                            MessageBoxResult.OK)) {
                        case MessageBoxResult.OK: break;
                        case MessageBoxResult.Cancel: return;
                    } 
                } else { break; //retryCOM1 = false; }
            }
            rtb_Log.AppendText("[  OK  ] Port " + portName + " opened");

            rs232.setDTR(true); // we're ready to work
            rtb_Log.AppendText("[ INFO ] Terminal ready");
            // are you ready too?
            while (!rs232.isDCD()); // we're waiting
            rtb_Log.AppendText("[  OK  ] Partner ready");
            // now it is the GUIs turn, we're done here
        }
    }
}
