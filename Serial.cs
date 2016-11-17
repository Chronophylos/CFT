using System;
using System.IO.Ports;

namespace __Serial__ {
    class Serial {
        private string portName;
        private int baudrate;
        private int dataBits;
        private StopBits stopBits; // Der Anzahl der Stoppbits (Stopbits.One, StopBits.OnePointFive, StopBits.Two)
        private Parity parity; // Festlegung der Parität (Parity.Even, Parity.Mark, Parity.None, Parity.Odd, Parity.Space)
        private SerialPort sp;
        
        public Serial(string portName, int baudrate, int dataBits, StopBits stopBits, Parity parity) {
            this.portName = portName;
            this.baudrate = baudrate;
            this.dataBits = dataBits;
            this.stopBits = stopBits;
            this.parity = parity;

            sp = new SerialPort();
            sp.PortName = portName;
            sp.BaudRate = baudrate;
            sp.DataBits = dataBits;
            sp.StopBits = stopBits;
            sp.Parity = parity;
        }
        
        public bool open() {
            try { sp.Open(); }
            catch(UnauthorizedAccessException ex){ return false; }
            catch (ArgumentOutOfRangeException ex){ return false; }
            catch (ArgumentException ex){ return false; }
            catch (InvalidOperationException ex){ return false; }
            return true;
        }       
        public void close(){ sp.Close(); }
        
        public int dataAvailable(){ return sp.BytesToRead; } // Anzahl Zeichen, die im Empfangspuffer stehen
        
        public int read(){ return sp.ReadByte(); }        
        public int read(System.Byte[] buffer, int bufSize){ return sp.Read(buffer, 0, bufSize); }
        public string readLine(){ return sp.ReadLine(); }
        
        public void write(int value){
            System.Byte[] b = new System.Byte[1];
            b[0] = (byte)value;
            sp.Write(b, 0, 1);
        }     
        public void write(System.Byte[] buffer, int bytesToWrite){ sp.Write(buffer, 0, bytesToWrite); }
        public void write(string s){ sp.Write(s); }    
        
        public void setRTS(bool arg){ sp.RtsEnable = arg; }
        public void setDTR(bool arg){ sp.DtrEnable = arg; }        
        public bool isCTS(){ return sp.CtsHolding; }        
        public bool isDSR(){ return sp.DsrHolding; }
        public bool isDCD(){ return sp.CDHolding; }
        public bool isDTR(){ return sp.DtrEnable; }
        public bool isRTS(){ return sp.RtsEnable; }
    }
}
