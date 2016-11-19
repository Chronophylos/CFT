using System;
using System.IO.Ports;

namespace __Cereal__ {
    class Cereal {
        private String portName;
        private Int32 baudrate;
        private Int32 dataBits;
        private StopBits stopBits; // Der Anzahl der Stoppbits (Stopbits.One, StopBits.OnePointFive, StopBits.Two)
        private Parity parity; // Festlegung der Parität (Parity.Even, Parity.Mark, Parity.None, Parity.Odd, Parity.Space)
        private SerialPort sp;
        
        public Cereal(String portName, Int32 baudrate, Int32 dataBits, StopBits stopBits, Parity parity) {
            this.portName = portName;
            this.baudrate = baudrate;
            this.dataBits = dataBits;
            this.stopBits = stopBits;
            this.parity = parity;

            sp = new SerialPort()
            {
                PortName = portName,
                BaudRate = baudrate,
                DataBits = dataBits,
                StopBits = stopBits,
                Parity = parity
            };
        }

        public Boolean Open() {
            try { sp.Open(); }
            catch(UnauthorizedAccessException){ return false; }
            catch (ArgumentOutOfRangeException){ return false; }
            catch (ArgumentException){ return false; }
            catch (InvalidOperationException){ return false; }
            return true;
        }       
        public void Close(){ sp.Close(); }
        
        public Int32 DataAvailable(){ return sp.BytesToRead; } // Anzahl Zeichen, die im Empfangspuffer stehen
        
        public Int32 Read(){ return sp.ReadByte(); }        
        public Int32 Read(Byte[] buffer, Int32 bufSize){ return sp.Read(buffer, 0, bufSize); }
        public String ReadLine(){ return sp.ReadLine(); }
        
        public void Write(Int32 value){
            //System.Byte[] b = new System.Byte[1];
            //b[0] = (byte)value;
            //sp.Write(b, 0, 1);
            sp.Write(BitConverter.GetBytes(value), 0 , 1);
        }     
        public void Write(Byte[] buffer, Int32 bytesToWrite){ sp.Write(buffer, 0, bytesToWrite); }
        public void Write(String s){ sp.Write(s); }    
        
        public void SetRTS(Boolean arg){ sp.RtsEnable = arg; }
        public void SetDTR(Boolean arg){ sp.DtrEnable = arg; }        
        public Boolean IsCTS(){ return sp.CtsHolding; }        
        public Boolean IsDSR(){ return sp.DsrHolding; }
        public Boolean IsDCD(){ return sp.CDHolding; }
        public Boolean IsDTR(){ return sp.DtrEnable; }
        public Boolean IsRTS(){ return sp.RtsEnable; }
    }
}