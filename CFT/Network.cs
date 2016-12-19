using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CerealFileTransfer {
    class Network {
        private String      portName;
        private Int32       baudrate;
        private Int32       dataBits;
        private StopBits    stopBits;  // Der Anzahl der Stoppbits (Stopbits.One, StopBits.OnePointFive, StopBits.Two)
        private Parity      parity;      // Festlegung der Parität (Parity.Even, Parity.Mark, Parity.None, Parity.Odd, Parity.Space)
        private Int32       bufferSize;
        private Int32       packageSize;
        private SerialPort  serial;
        private ProgressBar progressBar;
        private RichTextBox rtb;
        private Dispatcher  dispatcher;

        public Network(String portName, Int32 baudrate, Int32 bufferSize, Int32 packageSize, ProgressBar progressBar, RichTextBox rtb, Dispatcher dispatcher) {
            this.portName =         portName;
            this.baudrate =         baudrate;
            this.dataBits =         8;
            this.stopBits =         StopBits.One;
            this.parity =           Parity.None;
            this.bufferSize =       bufferSize;
            this.packageSize =      packageSize;
            this.progressBar =      progressBar;
            this.rtb =              rtb;
            this.dispatcher =       dispatcher;

            this.serial =   new SerialPort() {
                PortName =          this.portName,
                BaudRate =          baudrate,
                DataBits =          this.dataBits,
                StopBits =          this.stopBits,
                Parity =            this.parity,
                ReadBufferSize =    bufferSize,
                WriteBufferSize =   bufferSize / 2,
                DtrEnable =         true           
            };
        }

        public Boolean Open() {
            try {
                this.serial.Open();
                return true;
            } catch (UnauthorizedAccessException ex) {
                Debug.Print(ex.Message);
                Debug.Print("Der Zugriff auf den Anschluss wird verweigert.– oder –Der aktuelle Prozess, oder\n" +
                            "ein anderer Prozess auf dem System, lässt bereits den angegebenen COM-Port entweder\n" +
                            "durch eine System.IO.Ports.SerialPort-Instanz oder in nicht verwaltetem Code\n" +
                            "öffnen.");
            } catch (ArgumentOutOfRangeException ex) {
                Debug.Print(ex.Message);
                Debug.Print("Eine oder mehrere Eigenschaften für diese Instanz sind ungültig.Beispielsweise\n" +
                             "hat die System.IO.Ports.SerialPort.Parity-Eigenschaft, die System.IO.Ports.SerialPort.DataBits-Eigenschaft\n" +
                             "oder die System.IO.Ports.SerialPort.Handshake-Eigenschaft keinen gültigen Wert,\n" +
                             "die System.IO.Ports.SerialPort.BaudRate ist kleiner oder gleich 0, die System.IO.Ports.SerialPort.ReadTimeout-Eigenschaft\n" +
                             "oder die System.IO.Ports.SerialPort.WriteTimeout-Eigenschaft ist kleiner als\n" +
                             "0 und nicht System.IO.Ports.SerialPort.InfiniteTimeout.");
            } catch (ArgumentException ex) {
                Debug.Print(ex.Message);
                Debug.Print("Der Anschlussname fängt nicht mit \"COM\" an. – oder –Der Dateityp des Anschlusses\n" +
                             "wird nicht unterstützt.");
            } catch (System.IO.IOException ex) {
                Debug.Print(ex.Message);
                Debug.Print("Der Anschluss befindet sich in einem ungültigen Zustand. – oder – Fehler beim\n" +
                             "Versuch, den Zustand des zugrunde liegenden Anschlusses festzulegen.Beispielsweise\n" +
                             "waren die von diesem System.IO.Ports.SerialPort-Objekt übergebenen Parameter\n" +
                             "ungültig.");
            } catch (InvalidOperationException ex) {
                Debug.Print(ex.Message);
                Debug.Print("Der angegebene Port auf der aktuellen Instanz vom System.IO.Ports.SerialPort\n" +
                             "ist bereits geöffnet.");
            }
            return false;
        }
        private Double StopBitsToInt(StopBits stopBit) {
            return (stopBit == StopBits.OnePointFive) ? 1.5 : (Double)stopBit;
        }

        public Byte[][] GetPackage(Int32 count) {
            Byte[][]    package =   new Byte[count][];
            Byte[]      buffer =    new Byte[this.packageSize];

            this.dispatcher.Invoke((Action)(() => {
                this.progressBar.Maximum = this.packageSize * count;
            }));


            for (Int32 i = 0; i < count; i++) {
                while (this.serial.BytesToRead < this.packageSize) {
                    this.dispatcher.Invoke((Action)(() => {
                        this.progressBar.Value = this.serial.BytesToRead + this.packageSize * i;
                    }));
                }
                try {
                    this.serial.Read(buffer, 0, this.packageSize);
                } catch (ArgumentNullException ex) {
                    Debug.Print(ex.Message);
                    Debug.Print("Der übergebene Puffer ist null.");
                } catch (InvalidOperationException ex) {
                    Debug.Print(ex.Message);
                    Debug.Print("Der angegebene Anschluss ist nicht offen.");
                } catch (ArgumentOutOfRangeException ex) {
                    Debug.Print(ex.Message);
                    Debug.Print("Der offset-Parameter oder der count-Parameter liegt außerhalb eines gültigen\n" +
                                "Bereichs des übergebenen buffer.offset oder count ist kleiner als 0.");
                } catch (ArgumentException ex) {
                    Debug.Print(ex.Message);
                    Debug.Print("Die Summe von offset und count ist größer als die Länge von buffer.");
                } catch (TimeoutException ex) {
                    Debug.Print(ex.Message);
                    Debug.Print("Es waren keine Bytes zum Lesen verfügbar.");
                }
                this.dispatcher.Invoke((Action)(() => {
                    this.progressBar.Value = this.packageSize * i;
                    this.rtb.AppendText(Convert.ToString(count / i));
                }));
                package[i] = buffer;
            }

            this.dispatcher.Invoke((Action)(() => {
                this.progressBar.Value = 0;
            }));

            return package;
        }

        public void SendPackage(Byte[][] package) {
            this.dispatcher.Invoke((Action)(() => {
                this.progressBar.Maximum = package.Length;
            }));

            for (Int32 i = 0; i < package.Length; i++) {
                try {
                    this.serial.Write(package[i], 0, package[i].Length);
                } catch(ArgumentNullException ex) {
                    Debug.Print(ex.Message);
                    Debug.Print("Der übergebene buffer ist null.");
                } catch (InvalidOperationException ex) {
                    Debug.Print(ex.Message);
                    Debug.Print("Der angegebene Anschluss ist nicht offen.");
                } catch (ArgumentOutOfRangeException ex) {
                    Debug.Print(ex.Message);
                    Debug.Print("Der offset-Parameter oder der count-Parameter liegt außerhalb eines gültigen\n" +
                                "Bereichs des übergebenen buffer.offset oder count ist kleiner als 0.");
                } catch (ArgumentException ex) {
                    Debug.Print(ex.Message);
                    Debug.Print("Die Summe von offset und count ist größer als die Länge von buffer.");
                } catch (TimeoutException ex) {
                    Debug.Print(ex.Message);
                    Debug.Print("Der Vorgang konnte nicht vor Ablauf des Timeouts abgeschlossen werden.");
                }
                this.dispatcher.Invoke((Action)(() => {
                    this.progressBar.Value = (i + 1);
                    this.rtb.AppendText(Convert.ToString(package.Length / i));
                }));               
            }

            this.dispatcher.Invoke((Action)(() => {
                this.progressBar.Value = 0;
            }));
        }

        public bool IsDataAvailable {
            get {
                try {
                    return (this.serial.BytesToRead > 0) ? true : false;
                } catch (InvalidOperationException ex) {
                    Debug.Print(ex.Message);
                    Debug.Print("Der Anschluss ist nicht geöffnet.");
                }
                return false;
            }
        }

        public Boolean IsPartnerHappy {
            get {
                while (!this.IsDataAvailable) ;
                try {
                    return (this.serial.ReadChar() == 'Y') ? true : false;
                } catch (InvalidOperationException ex) {
                    Debug.Print(ex.Message);
                    Debug.Print("Der Anschluss ist nicht geöffnet.");
                } catch (TimeoutException ex) {
                    Debug.Print(ex.Message);
                    Debug.Print("Der Vorgang konnte nicht vor Ablauf des Timeouts abgeschlossen werden.– oder\n" +
                                "–Im zugewiesenen Timeoutzeitraum war kein Zeichen verfügbar.");
                }
                return false;
            }  
        }

        public void ImHappy(Boolean IsHappy = true) {
            try {
                this.serial.Write(new Char[] { (IsHappy) ? 'Y' : 'N' }, 0, 1);
            } catch (ArgumentNullException ex) {
                Debug.Print(ex.Message);
                Debug.Print("Der übergebene buffer ist null.");
            } catch (InvalidOperationException ex) {
                Debug.Print(ex.Message);
                Debug.Print("Der angegebene Anschluss ist nicht offen.");
            } catch (ArgumentOutOfRangeException ex) {
                Debug.Print(ex.Message);
                Debug.Print("Der offset-Parameter oder der count-Parameter liegt außerhalb eines gültigen\n" +
                            "Bereichs des übergebenen buffer.offset oder count ist kleiner als 0.");
            } catch (ArgumentException ex) {
                Debug.Print(ex.Message);
                Debug.Print("Die Summe von offset und count ist größer als die Länge von buffer.");
            } catch (TimeoutException ex) {
                Debug.Print(ex.Message);
                Debug.Print("Der Vorgang konnte nicht vor Ablauf des Timeouts abgeschlossen werden.");
            }
        }

        public Int32 PackageSize {
            get {
                return this.packageSize;
            }
        }
    }
}
