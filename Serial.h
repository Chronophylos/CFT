/******************************************************************************
 *                                                                            *
 *   Klasse Serial                                                           *
 *                                                                            *
 *   Klasse zur Datenübertragung über die serielle Schnittstelle,             *
 *                                                                            *
 *   (c) 2006 Michael Zimmer, HTS                                             *
 *                                                                            *
 *   Borland C++Builder 4.0                                                       *
 *                                                                            *
 ****V2.0**********************************************************************/

#ifndef __SERIAL_H__
#define __SERIAL_H__

#include <windows.h>
#include <string>

using namespace std;

class Serial
{
private:
  // Der Port-Name (COM1, COM2)
 string portName;
  // Die Datenübertragungsrate
  int baudrate;
  // Die Anzahl der Datenbits (5 .. 8)
  int dataBits;
  // Der Anzahl der Stoppbits (ONESTOPBIT, ONE5STOPBITS, TWOSTOPBITS)
  int stopBits;
  // Festlegung der Parität (EVENPARITY, NOPARITY, ODDPARITY)
  int parity;

  /** Handle für den Com-Port */
  HANDLE handle;
public:
  Serial(string portName, int baudrate, int dataBits, int stopBits, int parity);
  ~Serial();

  bool open();
  void close();

  int dataAvailable(); // Anzahl Zeichen, die im Empfangspuffer stehen
  int read();
  int read(char *buffer, int bufSize);
  string readLine();
  void write(int value);
  void write(const char *buffer, int bytesToWrite);
  void write(string s);
  void setRTS(bool arg);
  void setDTR(bool arg);
  bool isCTS();
  bool isDSR();
  bool isRI();
  bool isDCD();
};

#endif
