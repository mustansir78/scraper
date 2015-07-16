using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HSJiDataRipperTool.ServiceReference1;


namespace HSJiDataRipperTool.Models
{
    public class RequestCallback : IService1Callback
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
            }
        }

        public event EventHandler MessageReceived;

        protected virtual void OnMessageReceived(EventArgs e)
        {
            EventHandler handler = MessageReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void SendResult(Result result)
        {
            Message = result.Message;
            OnMessageReceived(EventArgs.Empty);
        }
    }
}