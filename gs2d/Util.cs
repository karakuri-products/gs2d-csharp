using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace gs2d
{
    /// <summary>
    /// コールバック戻り値用クラス
    /// </summary>
    internal class CallbackResult
    {
        private int intValue;
        private double doubleValue;

        public bool doubleFlag = false;

        public CallbackResult(int intValue, double doubleValue)
        {
            this.intValue = intValue;
            this.doubleValue = doubleValue;
            this.doubleFlag = true;
        }

        public CallbackResult(int intValue)
        {
            this.intValue = intValue;
            this.doubleValue = (double)intValue;
            this.doubleFlag = false;
        }

        public static implicit operator int(CallbackResult value)
        {
            if (value.doubleFlag) return (int)value.doubleValue;
            return value.intValue;
        }

        public static implicit operator double(CallbackResult value)
        {
            if (value.doubleFlag) return value.doubleValue;
            return (double)value.intValue;
        }
    }
}
