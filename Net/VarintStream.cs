using System;
using System.Collections;

namespace NetCore
{
    public class VarintStream : ByteStream
    {
        public byte readVarint8()
        {
            uint val = 0;
            return (byte)readVarint32Byte(ref val, sizeof(byte));
        }

        public void writeVarint8(byte value)
        {
            writeVarint32Byte(value);
        }

        public Int16 readVarint16()
        {
            uint val = 0;
            return (Int16)readVarint32Byte(ref val, sizeof(Int16));
        }

        public void writeVarint16(Int16 value)
        {
            writeVarint32Byte((UInt32)value);
        }

        public Int32 readVarint32()
        {
            uint val = 0;
            return (Int32)readVarint32Byte(ref val, sizeof(Int32));
        }

        public void writeVarint32(Int32 value)
        {
            writeVarint32Byte((UInt32)value);
        }

        public Int64 readVarint64()
        {
            ulong val = 0;
            return (Int64)readVarint64Byte(ref val, sizeof(Int64));
        }
        public void writeVarint64(Int64 value)
        {
            writeVarint64Byte((UInt64)value);
        }

        protected void writeVarint32Byte(UInt32 value)
        {
            if (value < 128)
            {
                writeInt8((byte)value);
                return;
            }

            while (value > 127)
            {
                writeInt8((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }
            writeInt8((byte)value);
        }

        protected uint readVarint32Byte(ref uint value, int len)
        {
            int tmp = m_pBuffer.readInt8();
            if (tmp < 128)
            {
                return (uint)tmp;
            }
            int result = tmp & 0x7f;
            if ((tmp = m_pBuffer.readInt8()) < 128)
            {
                result |= tmp << 7;
            }
            else
            {
                result |= (tmp & 0x7f) << 7;
                if ((tmp = m_pBuffer.readInt8()) < 128)
                {
                    result |= tmp << 14;
                }
                else
                {
                    result |= (tmp & 0x7f) << 14;
                    if ((tmp = m_pBuffer.readInt8()) < 128)
                    {
                        result |= tmp << 21;
                    }
                    else
                    {
                        result |= (tmp & 0x7f) << 21;
                        result |= (tmp = m_pBuffer.readInt8()) << 28;
                        if (tmp >= 128)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                if (m_pBuffer.readInt8() < 128)
                                {
                                    return (uint)result;
                                }
                            }
                            throw new InvalidCastException();
                        }
                    }
                }
            }
            return (uint)result;
        }

        protected void writeVarint64Byte(UInt64 value)
        {
            while (value > 127)
            {
                writeInt8((byte) ((value & 0x7F) | 0x80));
                value >>= 7;
            }
            while (value > 127)
            {
                writeInt8((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }
            writeInt8((byte)value);
        }

        protected ulong readVarint64Byte(ref ulong value, int len)
        {
            int shift = 0;
            ulong result = 0;
            while (shift < 64)
            {
                byte b = m_pBuffer.readInt8();
                result |= (ulong)(b & 0x7F) << shift;
                if ((b & 0x80) == 0)
                {
                    return result;
                }
                shift += 7;
            }
            throw new InvalidCastException();
        }

        public string readVarintString()
        {
            Int16 nStrLen = 0;

            nStrLen = readVarint16();

            byte[] b = m_pBuffer.readBytes(nStrLen);

            string s = System.Text.Encoding.Default.GetString(b);

            return s;
        }

        public void writeVarintString(string s)
        {
            short len = (short)s.Length;
            writeVarint16(len);
            byte[] val = System.Text.Encoding.Default.GetBytes(s);
            m_pBuffer.writeBytes(val);
        }
    }
}
