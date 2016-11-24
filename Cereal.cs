using System;
using System.Diagnostics;
using System.IO.Ports;

namespace __Cereal__ {
    class Cereal {
        private String portName;
        private Int32 baudrate;
        private Int32 dataBits;
        private StopBits stopBits; // Der Anzahl der Stoppbits (Stopbits.One, StopBits.OnePointFive, StopBits.Two)
        private Parity parity; // Festlegung der Parität (Parity.Even, Parity.Mark, Parity.None, Parity.Odd, Parity.Space)
        private Int32 bufferSize;
        private SerialPort sp;
        
        public Cereal(String portName, Int32 baudrate, Int32 dataBits, StopBits stopBits, Parity parity, Int32 bufferSize) {
            this.portName = portName;
            this.baudrate = baudrate;
            this.dataBits = dataBits;
            this.stopBits = stopBits;
            this.parity = parity;
            this.bufferSize = bufferSize;

            this.sp = new SerialPort(){
                PortName = portName,
                BaudRate = baudrate,
                DataBits = dataBits,
                StopBits = stopBits,
                Parity = parity,
                ReadBufferSize = bufferSize,
                WriteBufferSize = bufferSize / 2
            };
        }

        public Boolean Open() {
            try { this.sp.Open(); }
            catch(Exception ex) { Debug.Print(ex.Message); return false; }
            return true;
        }       
        public void Close(){ this.sp.Close(); }
        
        public Int32 DataAvailable(){ return this.sp.BytesToRead; } // Anzahl Zeichen, die im Empfangspuffer stehen
        
        public Int32 Read()                             { return this.sp.ReadByte(); }        
        public Int32 Read(Byte[] buffer, Int32 bufSize) { return this.sp.Read(buffer, 0, bufSize); }
        public String ReadLine()                        { return this.sp.ReadLine(); }
        
        public void Write(Int32 value){
            //System.Byte[] b = new System.Byte[1];
            //b[0] = (byte)value;
            //sp.Write(b, 0, 1);
            this.sp.Write(BitConverter.GetBytes(value), 0 , 1);
        }     
        public void Write(Byte[] buffer, Int32 bytesToWrite){ this.sp.Write(buffer, 0, bytesToWrite); }
        public void Write(String s)                         { this.sp.Write(s); }    
        
        public void SetRTS(Boolean arg){ this.sp.RtsEnable = arg; }
        public void SetDTR(Boolean arg){ this.sp.DtrEnable = arg; }        
        public Boolean IsCTS(){ return this.sp.CtsHolding; }        
        public Boolean IsDSR(){ return this.sp.DsrHolding; }
        public Boolean IsDCD(){ return this.sp.CDHolding; }
        public Boolean IsDTR(){ return this.sp.DtrEnable; }
        public Boolean IsRTS(){ return this.sp.RtsEnable; }
    }
}