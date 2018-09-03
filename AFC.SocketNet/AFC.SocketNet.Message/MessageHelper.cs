/*******************************************************************
 * * 文件名： MessageHelper.cs
 * * 文件作用： 消息处理工具类
 * *
 * *-------------------------------------------------------------------
 * *修改历史记录：
 * *修改时间      修改人    修改内容概要
 * *2017-04-22    xwj       新增
 * *******************************************************************/

using System;
using System.Text;

namespace AFC.SocketNet.Message
{
    public class MessageHelper
    {
        /// <summary>
        /// 校验MD5是否正确
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <param name="verifyMd5Bytes"></param>
        /// <returns></returns>
        public static bool VerifyMd5Bytes(byte[] dataBytes, byte[] verifyMd5Bytes)
        {
            byte[] computeMd5Bytes = GetMd5Bytes(dataBytes);
            return IsBytesEqual(verifyMd5Bytes, computeMd5Bytes);
        }

        public static string GetMd5HexString(string str)
        {
            byte[] pwdBytes = Encoding.ASCII.GetBytes(str);
            var md5Bytes = GetMd5Bytes(pwdBytes);
            return BitConverter.ToString(md5Bytes).Replace("-", "");
        }

        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] GetMd5Bytes(byte[] bytes)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider provider = new System.Security.Cryptography.MD5CryptoServiceProvider();
            return provider.ComputeHash(bytes);
        }

        private static bool IsBytesEqual(byte[] b1, byte[] b2)
        {
            if (b1 == null || b2 == null || b1.Length != b2.Length)
            {
                return false;
            }

            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}