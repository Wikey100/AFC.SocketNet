/*******************************************************************
 * * 文件名： BaseMessage.cs
 * * 文件作用： 消息处理基类
 * *
 * *-------------------------------------------------------------------
 * *修改历史记录：
 * *修改时间      修改人    修改内容概要
 * *2017-04-22    xwj       新增
 * *******************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AFC.SocketNet.Message
{
    public class BaseMessage
    {
        #region 字段/属性

        /// <summary>
        /// 字节序位
        /// </summary>
        private int decodeIndex;

        /// <summary>
        /// 消息数组
        /// </summary>
        private byte[] data;

        /// <summary>
        /// 编码标识
        /// </summary>
        private bool isEncoded;

        /// <summary>
        /// 编码列表
        /// </summary>
        protected List<byte> encodeBuf = new List<byte>();

        public int DecodeIndex
        {
            get { return decodeIndex; }
            set { decodeIndex = value; }
        }

        public byte[] Data
        {
            get { return data; }
        }

        #endregion 字段/属性

        public BaseMessage()
        {
        }

        public BaseMessage(byte[] buf, int index = 0)
        {
            try
            {
                decodeIndex = index;
                data = buf;
                if (IsMd5Corrent())
                {
                    Decode();
                }
                else
                {
                    throw new Md5NotCorretException("MD5验证失败!");
                }
            }
            catch (Md5NotCorretException md5Ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new MsgParseFailException(decodeIndex, ex.Message);
            }
        }

        /// <summary>
        /// 判断是否MD5
        /// </summary>
        /// <returns></returns>
        public bool IsMd5Corrent()
        {
            if (data.Length < 16)
            {
                return false;
            }

            byte[] dataBytes = new byte[data.Length - 16];
            Array.Copy(data, dataBytes, dataBytes.Length);
            byte[] verfyMd5Bytes = new byte[16];
            Array.Copy(data, data.Length - 16, verfyMd5Bytes, 0, 16);
            return MessageHelper.VerifyMd5Bytes(dataBytes, verfyMd5Bytes);
        }

        /// <summary>
        /// 解包
        /// </summary>
        public virtual void Decode()
        {
            decodeIndex = 0;
        }

        /// <summary>
        /// 生成数据包
        /// </summary>
        public virtual void Encode()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="isBigEndian"></param>
        /// <returns></returns>
        private byte[] GetOneUnitBytes(int index, bool isBigEndian = true)
        {
            var bytes = new byte[4];
            if (isBigEndian)
            {
                bytes[0] = data[index + 3];
                bytes[1] = data[index + 2];
                bytes[2] = data[index + 1];
                bytes[3] = data[index];
            }
            else
            {
                bytes[0] = data[index];
                bytes[1] = data[index + 1];
                bytes[2] = data[index + 2];
                bytes[3] = data[index + 3];
            }
            return bytes;
        }

        /// <summary>
        /// 获取字节数组
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetBytes()
        {
            if (!isEncoded)
            {
                Encode();
                isEncoded = true;
                data = encodeBuf.ToArray();
            }
            return data;
        }

        /// <summary>
        /// 将字节数组转成编码
        /// </summary>
        /// <param name="len"></param>
        /// <param name="isBigEnd"></param>
        /// <param name="bytes"></param>
        private void AddBytesToEncodeBuf(int len, bool isBigEnd, byte[] bytes)
        {
            var data = new byte[len];
            if (isBigEnd)
            {
                bytes.CopyTo(data, len - bytes.Length);
            }
            else
            {
                bytes.CopyTo(data, 0);
            }
            encodeBuf.AddRange(data);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="byteNum"></param>
        /// <returns></returns>
        private byte[] GetBCDBytesFromInt(int value, int byteNum)
        {
            byte[] buf = new byte[byteNum];
            for (int i = buf.Length - 1; i >= 0; i--)
            {
                buf[i] |= (byte)(value % 10);
                value /= 10;
                buf[i] |= (byte)((value % 4) << 4);
                value /= 10;
            }
            return buf;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private byte[] GetBCDBytesFromIntString(string str)
        {
            if (str == null || str == string.Empty)
            {
                throw new ArgumentException("strin is null");
            }

            if (str.Length % 2 == 1)
            {
                str = "0" + str;
            }
            byte[] buf = new byte[str.Length / 2];

            for (int i = 0; i < buf.Length; i++)
            {
                int num = int.Parse(str.Substring(i * 2, 2));
                buf[i] = GetBCDBytesFromInt(num, 1)[0];
            }
            return buf;
        }

        #region ReverseBytes

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ushort ReverseBytes(ushort value)
        {
            return (ushort)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public uint ReverseBytes(uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public short ReverseBytes(short value)
        {
            return System.Net.IPAddress.HostToNetworkOrder(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int ReverseBytes(int value)
        {
            return System.Net.IPAddress.HostToNetworkOrder(value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        public void ReverseBytes(byte[] bytes)
        {
            byte tmp;
            int len = bytes.Length;

            for (int i = 0; i < len / 2; i++)
            {
                tmp = bytes[len - 1 - i];
                bytes[len - 1 - i] = bytes[i];
                bytes[i] = tmp;
            }
        }
        #endregion ReverseBytes

        #region Encode

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="len"></param>
        public void AddByte(byte value, int len = 1)
        {
            var bytes = new byte[len];
            bytes[len - 1] = value;
            encodeBuf.AddRange(bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="len"></param>
        public void AddSByte(sbyte value, int len = 1)
        {
            var bytes = new byte[len];
            bytes[len - 1] = value < 0 ? (byte)(value + 256) : (byte)value;
            encodeBuf.AddRange(bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="len"></param>
        /// <param name="isBigEnd"></param>
        public void AddUShort(ushort value, int len = 2, bool isBigEnd = false)
        {
            if (isBigEnd)
            {
                value = ReverseBytes(value);
            }
            var bytes = BitConverter.GetBytes(value);
            AddBytesToEncodeBuff(len, isBigEnd, bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="len"></param>
        /// <param name="isBigEnd"></param>
        public void AddShort(short value, int len = 2, bool isBigEnd = false)
        {
            if (isBigEnd)
            {
                value = ReverseBytes(value);
            }
            var bytes = BitConverter.GetBytes(value);
            AddBytesToEncodeBuff(len, isBigEnd, bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="len"></param>
        /// <param name="isBigEnd"></param>
        public void AddUInt(uint value, int len = 4, bool isBigEnd = false)
        {
            if (isBigEnd)
            {
                value = ReverseBytes(value);
            }
            var bytes = BitConverter.GetBytes(value);
            AddBytesToEncodeBuff(len, isBigEnd, bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="len"></param>
        /// <param name="isBigEnd"></param>
        public void AddInt(int value, int len = 4, bool isBigEnd = false)
        {
            if (isBigEnd)
            {
                value = ReverseBytes(value);
            }
            var bytes = BitConverter.GetBytes(value);
            AddBytesToEncodeBuff(len, isBigEnd, bytes);
        }

        /// <summary>
        /// 添加不定长ASCII码字符串
        /// </summary>
        /// <param name="value"></param>
        public void AddString(string value)
        {
            if (value == string.Empty)
            {
                AddInt(0, 4, true);
            }
            else
            {
                var bytes = Encoding.ASCII.GetBytes(value);
                int len = bytes.Length;

                AddInt(len, 4, true);
                FillBytesWithUnits(bytes, false);
            }
        }

        /// <summary>
        /// 添加固定长度ASCII码字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="len"></param>
        /// <param name="paddingChar"></param>
        /// <param name="isPaddingLeft"></param>
        public void AddString(string str, int len, char paddingChar = ' ', bool isPaddingLeft = true)
        {
            byte[] buf = new byte[len];
            if (str != null)
            {
                if (str.Length < len)
                {
                    if (isPaddingLeft)
                    {
                        str = str.PadLeft(len, paddingChar);
                    }
                    else
                    {
                        str = str.PadRight(len, paddingChar);
                    }
                }
                byte[] tempBuf = Encoding.ASCII.GetBytes(str);
                Array.Copy(tempBuf, buf, len);
            }
            encodeBuf.AddRange(buf);
        }

        /// <summary>
        /// 添加不定长unicode字符串，UTF8编码
        /// </summary>
        /// <param name="value"></param>
        internal void AddUnicodeString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            int len = bytes.Length;

            AddInt(len);
            FillBytesWithUnits(bytes, false);
        }

        /// <summary>
        /// 添加固定GB2312编码字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="len"></param>
        /// <param name="paddingChar"></param>
        /// <param name="isPaddingLeft"></param>
        public void AddUnicodeString(string str, int len, char paddingChar = ' ', bool isPaddingLeft = true)
        {
            byte[] buf = new byte[len];
            if (str != null)
            {
                if (str.Length < len)
                {
                    if (isPaddingLeft)
                    {
                        str = str.PadLeft(len, paddingChar);
                    }
                    else
                    {
                        str = str.PadRight(len, paddingChar);
                    }
                }
                byte[] tempBuf = Encoding.GetEncoding("GB2312").GetBytes(str);
                Array.Copy(tempBuf, buf, len);
            }
            encodeBuf.AddRange(buf);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bcdStr"></param>
        /// <param name="len"></param>
        public void AddBCDString(string bcdStr, int len)
        {
            byte[] buf = new byte[len];
            if (!string.IsNullOrEmpty(bcdStr) && !string.IsNullOrEmpty(bcdStr.Trim()))
            {
                byte[] tempBuf = GetBCDBytesFromIntString(bcdStr);
                Array.Copy(tempBuf, buf, tempBuf.Length < len ? tempBuf.Length : len);
            }
            encodeBuf.AddRange(buf);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public void AddUIntFromDateTime(DateTime dt)
        {
            var value = (uint)((dt.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds);
            AddUInt(value, 4, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public void AddUShortFromDateTime(DateTime dt)
        {
            var value = (ushort)((dt - new DateTime(2000, 1, 1)).Days);
            AddUShort(value, 4, true);
        }

        /// <summary>
        /// yyyyMMddHHmmss形式BCD日期
        /// </summary>
        /// <param name="time"></param>
        /// <param name="len"></param>
        public void AddBCDBytesFromDateTime(DateTime time, int len = 7)
        {
            byte[] buf = new byte[len];
            //转换日期
            int timeValue = time.Year;
            timeValue *= 100;
            timeValue += time.Month;
            timeValue *= 100;
            timeValue += time.Day;
            GetBCDBytesFromInt(timeValue, 4).CopyTo(buf, 0);

            if (len == 7)
            {
                //转换时间
                timeValue = time.Hour;
                timeValue *= 100;
                timeValue += time.Minute;
                timeValue *= 100;
                timeValue += time.Second;
                GetBCDBytesFromInt(timeValue, 3).CopyTo(buf, 4);
            }
            encodeBuf.AddRange(buf);
        }

        #endregion Encode

        #region Decode

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <param name="isBigEnd"></param>
        /// <returns></returns>
        public byte GetNextByte(int len = 1, bool isBigEnd = false)
        {
            var bytes = new byte[len];
            Buffer.BlockCopy(data, decodeIndex, bytes, 0, len);
            if (isBigEnd)
            {
                Array.Reverse(bytes);
            }
            decodeIndex += len;
            return bytes[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <param name="isBigEnd"></param>
        /// <returns></returns>
        public byte[] GetByteArryas(int len, bool isBigEnd = false)
        {
            var bytes = new byte[len];
            Buffer.BlockCopy(data, decodeIndex, bytes, 0, len);
            if (isBigEnd)
            {
                Array.Reverse(bytes);
            }
            decodeIndex += len;
            return bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <param name="isBigEnd"></param>
        /// <returns></returns>
        public sbyte GetNextSByte(int len = 1, bool isBigEnd = false)
        {
            var bytes = new byte[len];
            Buffer.BlockCopy(data, decodeIndex, bytes, 0, len);
            if (isBigEnd)
            {
                Array.Reverse(bytes);
            }
            decodeIndex += len;
            byte b = bytes[0];

            if (b > 127)
                return (sbyte)(b - 256);
            else
                return (sbyte)b;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <param name="isBigEnd"></param>
        /// <returns></returns>
        public ushort GetNextUShort(int len = 2, bool isBigEnd = false)
        {
            var bytes = new byte[len];
            Buffer.BlockCopy(data, decodeIndex, bytes, 0, len);
            if (isBigEnd)
            {
                Array.Reverse(bytes);
            }
            decodeIndex += len;
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <param name="isBigEnd"></param>
        /// <returns></returns>
        public short GetNextShort(int len = 2, bool isBigEnd = false)
        {
            var bytes = new byte[len];
            Buffer.BlockCopy(data, decodeIndex, bytes, 0, len);
            if (isBigEnd)
            {
                Array.Reverse(bytes);
            }
            decodeIndex += len;
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <param name="isBigEnd"></param>
        /// <returns></returns>
        public uint GetNextUInt(int len = 4, bool isBigEnd = false)
        {
            var bytes = new byte[len];
            Buffer.BlockCopy(data, decodeIndex, bytes, 0, len);
            if (isBigEnd)
            {
                Array.Reverse(bytes);
            }
            decodeIndex += len;
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <param name="isBigEnd"></param>
        /// <returns></returns>
        public int GetNextInt(int len = 4, bool isBigEnd = false)
        {
            var bytes = new byte[len];
            Buffer.BlockCopy(data, decodeIndex, bytes, 0, len);
            if (isBigEnd)
            {
                Array.Reverse(bytes);
            }
            decodeIndex += len;
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// 获取不定长ASCII字符串
        /// </summary>
        /// <returns></returns>
        public string GetNextString()
        {
            uint len = GetNextUInt(4, true);
            string str = Encoding.ASCII.GetString(data, decodeIndex, (int)len);
            if (len % 4 == 0)
            {
                decodeIndex += (int)len;
            }
            else
            {
                decodeIndex += ((int)len / 4 + 1) * 4;
            }
            return str;
        }

        /// <summary>
        /// 获取定长ASCII码字符串
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public string GetNextString(int len)
        {
            string str = Encoding.ASCII.GetString(data, decodeIndex, len);
            decodeIndex += len;
            return str;
        }

        /// <summary>
        /// 获取不定长UTF8编码字符串
        /// </summary>
        /// <returns></returns>
        internal string GetNextUnicodeString()
        {
            uint len = GetNextUInt(4, true);
            string str = Encoding.UTF8.GetString(data, decodeIndex, (int)len);

            if (len % 4 == 0)
            {
                decodeIndex += (int)len;
            }
            else
            {
                decodeIndex += ((int)len / 4 + 1) * 4;
            }

            return str;
        }

        /// <summary>
        /// 获取定长GB2312编码字符串
        /// </summary>
        /// <returns></returns>
        public string GetNextUnicodeString(int len)
        {
            string str = Encoding.GetEncoding("GB2312").GetString(data, decodeIndex, len);
            decodeIndex += len;
            return str;
        }

        /// <summary>
        /// 秒数转换为时间
        /// </summary>
        /// <returns></returns>
        public DateTime GetDateTimeFromUInt()
        {
            var seconds = GetNextUInt(4, true);
            return new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(seconds);
        }

        /// <summary>
        /// 天数转换成时间
        /// </summary>
        /// <returns></returns>
        public DateTime GetDateTimeFromUShort()
        {
            var days = GetNextUShort(4, true);
            return new DateTime(2000, 1, 1).AddDays(days);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public DateTime GetNextDateTimeFromBCD(int len)
        {
            string datetime = GetNextBCDString(len);
            return GetDateTime(datetime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public string GetNextBCDString(int len)
        {
            string res = GetBCDString(decodeIndex, len);
            decodeIndex += len;
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        private string GetBCDString(int index, int len)
        {
            string s = string.Empty;
            for (int i = 0; i < len; i++)
            {
                s += data[index + i].ToString("X2");
            }
            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private DateTime GetDateTime(string str)
        {
            var minTime = new DateTime(1900, 1, 1, 0, 0, 0);
            var maxTime = new DateTime(2079, 12, 31, 0, 0, 0);
            var returnTime = DateTime.Now;

            //为空时返回最小值
            if (str.Trim('0') == string.Empty) return minTime;

            try
            {
                if (Regex.IsMatch(str, "^[0-9]{14}$"))
                {
                    returnTime = DateTime.ParseExact(str, "yyyyMMddHHmmss", null);
                }
                else if (Regex.IsMatch(str, "^[0-9]{8}$"))
                {
                    returnTime = DateTime.ParseExact(str, "yyyyMMdd", null);
                }
                else
                {
                    returnTime = new DateTime(1900, 1, 1, 0, 0, 0);
                }
            }
            catch (Exception)
            {
                returnTime = new DateTime(1900, 1, 1, 0, 0, 0);
            }
            finally
            {
                if (returnTime < minTime || returnTime > maxTime)
                {
                    returnTime = new DateTime(1900, 1, 1, 0, 0, 0);
                }
            }
            return returnTime;
        }

        #endregion Decode

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="isPaddingLeft"></param>
        internal void FillBytesWithUnits(byte[] bytes, bool isPaddingLeft = true)
        {
            int paddingLen = 4 - bytes.Length % 4;
            if (paddingLen != 4)
            {
                if (isPaddingLeft)
                {
                    encodeBuf.AddRange(new byte[paddingLen]);
                    encodeBuf.AddRange(bytes);
                }
                else
                {
                    encodeBuf.AddRange(bytes);
                    encodeBuf.AddRange(new byte[paddingLen]);
                }
            }
            else
            {
                encodeBuf.AddRange(bytes);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <param name="isBigEnd"></param>
        /// <param name="bytes"></param>
        private void AddBytesToEncodeBuff(int len, bool isBigEnd, byte[] bytes)
        {
            var data = new byte[len];

            if (isBigEnd)
            {
                bytes.CopyTo(data, len - bytes.Length);
            }
            else
            {
                bytes.CopyTo(data, 0);
            }
            encodeBuf.AddRange(data);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Md5NotCorretException : ApplicationException
    {
        public Md5NotCorretException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MsgParseFailException : ApplicationException
    {
        public MsgParseFailException(int index, string message)
            : base(message)
        {
            failIndex = index;
        }

        public MsgParseFailException(string message)
            : base(message)
        {
        }

        public MsgParseFailException(string message, ApplicationException innerException)
            : base(message, innerException)
        {
        }

        private int failIndex;

        /// <summary>
        /// 解析失败的位置
        /// </summary>
        public int FailIndex
        {
            get { return failIndex; }
        }
    }
}