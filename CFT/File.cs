using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CerealFileTransfer {
    class File {
        private Int32 packageSize;

        public File(Int32 packageSize) {
            this.packageSize = packageSize;
        }

        public Byte[][] Read(String fileName) {
            FileStream  fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            FileInfo    fileInfo =   new FileInfo(fileName);
            Byte[][]    package =    new Byte[this.GetPackages(fileInfo)][];
            Byte[]      buffer =     new Byte[this.packageSize];

            for (Int32 i = 0; i < this.GetPackages(fileInfo); i++) {
                fileStream.Read(buffer, 0, buffer.Length);
                package[i] = buffer;
            }

            return package;
        }

        public Int32 GetPackages(String fileName) {
            FileInfo    fileInfo =   new FileInfo(fileName);
            Int32       packages =   Convert.ToInt32(fileInfo.Length / this.packageSize);

            return (fileInfo.Length % this.packageSize <= this.packageSize) ? packages + 1 : packages;
        }

        public Int32 GetPackages(FileInfo fileInfo) {
            Int32       packages =   Convert.ToInt32(fileInfo.Length / this.packageSize);

            return (fileInfo.Length % this.packageSize <= this.packageSize) ? packages + 1 : packages;
        }

        public void Write(String fileName, Byte[][] package) {
            try {
                using (
                StreamWriter streamWriter = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write), Encoding.UTF8)) {
                    for (Int32 i = 0; i < package.Length; i++) {
                        streamWriter.Write(Encoding.UTF8.GetString(package[i]).Replace("\0", String.Empty));
                    }
                }
            } catch (ObjectDisposedException ex) {
                Debug.Print(ex.Message);
                Debug.Print("System.IO.StreamWriter.AutoFlush ist true, oder der System.IO.StreamWriter-Puffer\n" +
                            "ist voll, und der aktuelle Writer ist geschlossen.");
            } catch (NotSupportedException ex) {
                Debug.Print(ex.Message);
                Debug.Print("System.IO.StreamWriter.AutoFlush ist true, oder der System.IO.StreamWriter-Puffer\n" +
                            "ist voll, und der Inhalt des Puffers kann nicht in den zugrunde liegenden Stream\n" +
                            "fester Größe geschrieben werden, da der System.IO.StreamWriter sich am Ende des\n" +
                            "Streams befindet.");
            } catch (IOException ex) {
                Debug.Print(ex.Message);
                Debug.Print("E/A-Fehler.");
            }
        }
    }
}
