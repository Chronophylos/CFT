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
            Cereal rs232 = new Cereal("COM1", 9600, 8, StopBits.One, Parity.None);

            bool retryCOM1 = true;
            while (retryCOM1) {
                if (!rs232.open()) {
                    switch (MessageBox.Show("ERROR: Can't open COM1\nRetry to open COM1?",
                                            "Error opening COM1",
                                            MessageBoxButton.OKCancel,
                                            MessageBoxImage.Asterisk,
                                            MessageBoxResult.OK)) {
                        case MessageBoxResult.OK: break;
                        case MessageBoxResult.Cancel: return;
                    } 
                } else { retryCOM1 = false; }
            }
            rtb_Log.AppendText("[  OK  ] Port COM1 opened");
        }
    }
}
