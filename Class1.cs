using System;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace FTD2XX_NET
{
    public class FTDI
    {
        public enum FT_STATUS
        {
            FT_OK,
            FT_INVALID_HANDLE,
            FT_DEVICE_NOT_FOUND,
            FT_DEVICE_NOT_OPENED,
            FT_IO_ERROR,
            FT_INSUFFICIENT_RESOURCES,
            FT_INVALID_PARAMETER,
            FT_INVALID_BAUD_RATE,
            FT_DEVICE_NOT_OPENED_FOR_ERASE,
            FT_DEVICE_NOT_OPENED_FOR_WRITE,
            FT_FAILED_TO_WRITE_DEVICE,
            FT_EEPROM_READ_FAILED,
            FT_EEPROM_WRITE_FAILED,
            FT_EEPROM_ERASE_FAILED,
            FT_EEPROM_NOT_PRESENT,
            FT_EEPROM_NOT_PROGRAMMED,
            FT_INVALID_ARGS,
            FT_OTHER_ERROR
        }

        public class FT_DEVICE_INFO_NODE
        {
            // public uint Flags;
            // public FT_DEVICE Type;
            // public uint ID;
            // public uint LocId;
            public string SerialNumber;
            // public string Description;
            // public IntPtr ftHandle;

            public FT_DEVICE_INFO_NODE()
            {
                this.SerialNumber = null;
            }

            public FT_DEVICE_INFO_NODE(string SerialNumber)
            {
                this.SerialNumber = SerialNumber;
            }
        }

        public enum FT_DEVICE
        {
            FT_DEVICE_BM,
            FT_DEVICE_AM,
            FT_DEVICE_100AX,
            FT_DEVICE_UNKNOWN,
            FT_DEVICE_2232,
            FT_DEVICE_232R,
            FT_DEVICE_2232H,
            FT_DEVICE_4232H
        }

        private TextWriter writer = TextWriter.Null;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        // [DllImport("kernel32.dll", SetLastError = true)]
        // private static extern bool FreeConsole();

        private string[] portNames;
        private SerialPort port = null;

        public FTDI()
        {
            using (StreamWriter fs = new StreamWriter("C:\\Users\\user\\Desktop\\dll_output.txt", false))
            {
                fs.Write("This is some text in the file.");
            }
            AllocConsole();
            writer = Console.Out;
            writer.WriteLine("Initializing FTDI");
        }

        public FT_STATUS GetNumberOfDevices(ref uint devcount)
        {
            devcount = 0;
            portNames = SerialPort.GetPortNames();
            devcount = (uint)portNames.Length;
            writer.WriteLine($"GetNumberOfDevices {devcount}");
            return FT_STATUS.FT_OK;
        }

        public FT_STATUS GetDeviceList(FT_DEVICE_INFO_NODE[] devicelist)
        {
            for (int i = 0; i < portNames.Length; i++)
            {
                devicelist[i] = new FT_DEVICE_INFO_NODE(portNames[i]);
            }
            for (int i = portNames.Length; i < devicelist.Length; i++)
            {
                devicelist[i] = null;
            }
            writer.WriteLine($"GetDeviceList {portNames}");
            return FT_STATUS.FT_OK;
        }

        public FT_STATUS OpenBySerialNumber(string serialnumber)
        {
            var retVal = FT_STATUS.FT_OK;
            var newPort = new SerialPort();
            newPort.PortName = serialnumber;
            newPort.BaudRate = 9600;
            newPort.Parity = Parity.None;
            newPort.DataBits = 8;
            newPort.Handshake = Handshake.None;
            newPort.StopBits = StopBits.One;
            try
            {
                newPort.Open();
                port = newPort;
            }
            catch
            {
                return FT_STATUS.FT_OTHER_ERROR;
            }
            writer.WriteLine($"OpenBySerialNumber {retVal}");
            return retVal;
        }

        public FT_STATUS SetBaudRate(uint BaudRate)
        {
            port.BaudRate = (int)BaudRate;
            writer.WriteLine($"SetBaudRate {BaudRate}");
            return FT_STATUS.FT_OK;
        }

        public FT_STATUS SetDataCharacteristics(byte DataBits, byte StopBits, byte Parity)
        {
            writer.WriteLine($"SetDataCharacteristics {DataBits} {StopBits} {Parity}");
            StopBits stopBits;
            Parity parity;
            switch (StopBits)
            {
                case 0: // wtf
                    stopBits = System.IO.Ports.StopBits.One;
                    break;
                case 2:
                    stopBits = System.IO.Ports.StopBits.Two;
                    break;
                default:
                    return FT_STATUS.FT_INVALID_ARGS;
            }
            switch (Parity)
            {
                case 0:
                    parity = System.IO.Ports.Parity.None;
                    break;
                case 1:
                    parity = System.IO.Ports.Parity.Odd;
                    break;
                case 2:
                    parity = System.IO.Ports.Parity.Even;
                    break;
                case 3:
                    parity = System.IO.Ports.Parity.Mark;
                    break;
                case 4:
                    parity = System.IO.Ports.Parity.Space;
                    break;
                default:
                    return FT_STATUS.FT_INVALID_ARGS;
            }
            port.DataBits = DataBits;
            port.StopBits = stopBits;
            port.Parity = parity;
            return FT_STATUS.FT_OK;
        }

        public FT_STATUS SetFlowControl(ushort FlowControl, byte Xon, byte Xoff)
        {
            Handshake handshake;
            switch (FlowControl)
            {
                case 0x0000:
                    handshake = Handshake.None;
                    break;
                // case 0x0100:
                //     handshake = Handshake.RequestToSend;
                //     break;
                // case 0x0200:
                //     handshake = ???;
                //     break;
                // case 0x0400:
                //     handshake = Handshake.XOnXOff;
                //     break;
                default:
                    return FT_STATUS.FT_INVALID_ARGS;
            }
            writer.WriteLine("SetFlowControl");
            port.Handshake = handshake;
            return FT_STATUS.FT_OK;
        }

        public FT_STATUS Purge(uint purgemask)
        {
            writer.WriteLine("Purge");
            if ((purgemask & 1) != 0) // FT_PURGE_RX
            {
                port.DiscardInBuffer();
            }
            if ((purgemask & 2) != 0) // FT_PURGE_TX
            {
                port.DiscardOutBuffer();
            }
            return FT_STATUS.FT_OK;
        }

        public FT_STATUS GetRxBytesAvailable(ref uint RxQueue)
        {
            writer.WriteLine("GetRxBytesAvailable");
            RxQueue = (uint)port.BytesToRead;
            return FT_STATUS.FT_OK;
        }

        public FT_STATUS GetTxBytesWaiting(ref uint TxQueue)
        {
            writer.WriteLine("GetTxBytesWaiting");
            TxQueue = (uint)port.BytesToWrite;
            return FT_STATUS.FT_OK;
        }

        public FT_STATUS SetLatency(byte Latency)
        {
            writer.WriteLine("SetLatency - ignoring");
            return FT_STATUS.FT_OK;
        }

        public FT_STATUS SetTimeouts(uint ReadTimeout, uint WriteTimeout)
        {
            writer.WriteLine("SetTimeouts");
            port.ReadTimeout = (int)ReadTimeout;
            port.WriteTimeout = (int)WriteTimeout;
            return FT_STATUS.FT_OK;
        }

        public FT_STATUS InTransferSize(uint InTransferSize)
        {
            writer.WriteLine("InTransferSize");
            // port.WriteBufferSize = (int)InTransferSize;
            // port.ReceivedBytesThreshold = (int)InTransferSize;
            return FT_STATUS.FT_OK;
        }
    
        public FT_STATUS Close()
        {
            writer.WriteLine("Close");
            port.Close();
            port = null;
            return FT_STATUS.FT_OK;
        }

        static byte[] cableName = Encoding.ASCII.GetBytes("K056");

        public FT_STATUS EEUserAreaSize(ref uint UASize)
        {
            writer.WriteLine("EEUserAreaSize");
            UASize = (uint)cableName.Length;
            return FT_STATUS.FT_OK;
        }

        public FT_STATUS EEReadUserArea(byte[] UserAreaDataBuffer, ref uint numBytesRead)
        {
            writer.WriteLine("EEReadUserArea");
            Array.Copy(cableName, UserAreaDataBuffer, cableName.Length);
            numBytesRead = (uint)cableName.Length;
            return FT_STATUS.FT_OK;
        }

        public FT_STATUS Read(byte[] dataBuffer, uint numBytesToRead, ref uint numBytesRead)
        {
            writer.WriteLine("Read");
            numBytesRead = (uint)port.Read(dataBuffer, 0, (int)numBytesToRead);
            return FT_STATUS.FT_OK;
        }

        public FT_STATUS Write(byte[] dataBuffer, int numBytesToWrite, ref uint numBytesWritten)
        {
            writer.WriteLine("Write");
            port.Write(dataBuffer, 0, numBytesToWrite);
            numBytesWritten = (uint)numBytesToWrite;
            return FT_STATUS.FT_OK;
        }
    }
}