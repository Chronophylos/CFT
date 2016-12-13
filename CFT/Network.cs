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
        private Dispatcher  dispatcher;

        public Network(String portName, Int32 baudrate, Int32 bufferSize, Int32 packageSize, ProgressBar progressBar, Dispatcher dispatcher) {
            this.portName =         portName;
            this.baudrate =         baudrate;
            this.dataBits =         8;
            this.stopBits =         StopBits.One;
            this.parity =           Parity.None;
            this.bufferSize =       bufferSize;
            this.packageSize =      packageSize;
            this.progressBar =      progressBar;
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

        public void Open() {
            try {
                this.serial.Open();
            } catch (Exception ex) {
                Debug.Print(ex.Message);
            }
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
                this.serial.Read(buffer, 0, this.packageSize);
                this.dispatcher.Invoke((Action)(() => {
                    this.progressBar.Value = this.packageSize * i;
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
                this.progressBar.Maximum = this.packageSize * package.Length;
            }));

            for (Int32 i = 0; i < package.Length; i++) {
                this.dispatcher.Invoke((Action)(() => {
                    this.progressBar.Value = this.serial.BytesToWrite + this.packageSize * i;
                }));

                this.serial.Write(package[i], 0, package[i].Length);
            }

            this.dispatcher.Invoke((Action)(() => {
                this.progressBar.Value = 0;
            }));
        }

        public bool IsDataAvailable {
            get {
                try {
                    if (this.serial.BytesToRead > 0) 
                        return true;
                    else 
                        return false;
                } catch (InvalidOperationException ex) {
                    Debug.Print(ex.Message);
                }

                return false;
            }
        }

        public Boolean IsPartnerHappy {
            get {
                while (!this.IsDataAvailable)
                    ;
                if (this.serial.ReadChar() == 'Y')
                    return true;
                else
                    return false;
            }
        }

        public void ImHappy(Boolean IsHappy = true) {
            if (IsHappy)
                this.serial.Write(new Char[] { 'Y' }, 0, 1);
            else
                this.serial.Write(new Char[] { 'N' }, 0, 1);
        }

        public Int32 PackageSize {
            get {
                return this.packageSize;
            }
        }
    }
}
