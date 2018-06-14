using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFC.SocketNet.Message
{
    public class BaseMessage
    {
        #region  字段/属性
        private byte[] data;


        #endregion


        public BaseMessage()
        {

        }

        public BaseMessage(byte[] buf,int index=0)
        {

        }

        /// <summary>
        /// 解包
        /// </summary>
        public virtual void Decode()
        {

        }

        /// <summary>
        /// 生成数据包
        /// </summary>
        public virtual void Encode()
        {

        }
    }
}
