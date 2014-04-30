using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Timers;
using System.Threading;
using System.Net;
using System.Drawing;

namespace WeAreOneTrackInfo
{
    class WeAreOne : IDisposable
    {
        private WebClient wc;

        public void Dispose()
        {
            wc.Dispose();
            GC.SuppressFinalize(this);
        }

        public delegate void ResponseReceiveEventHandler(ResponseReceiveEventArgs e);
        public event ResponseReceiveEventHandler OnResponseReceived;
        public delegate void ImageReceiveEventHandler(ImageReceiveEventArgs e);
        public event ImageReceiveEventHandler OnImageReceived;
        public WeAreOne()
        {
            wc = new WebClient();
            wc.Proxy = null;
            wc.DownloadDataCompleted += wc_DownloadDataCompleted;
            wc.OpenReadCompleted += wc_OpenReadCompleted;
        }

        void wc_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            Response response;
            XmlSerializer serializer = new XmlSerializer(typeof(Response));
            response = (Response)serializer.Deserialize(e.Result);
            if (OnResponseReceived != null)
            {
                OnResponseReceived(new ResponseReceiveEventArgs()
                {
                    response = response
                });
            }
        }

        public void RequestResponse()
        {
            if (wc.IsBusy)
            {
                //RequestResponse();
                return;
            }
            wc.OpenReadAsync(new Uri("http://tray.technobase.fm/radio.xml"));
        }
        public void RequestPicture(Radio rad)
        {
            if (wc.IsBusy)
            {
                //RequestPicture(rad);
                return;
            }
            wc.DownloadDataAsync(new Uri(rad.picture), "PICTURE");
        }
        void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
                if (OnImageReceived != null)
                {
                    ImageReceiveEventArgs eargs = new ImageReceiveEventArgs();
                    using(MemoryStream ms = new MemoryStream(e.Result))
                    {
                        eargs.response = new Bitmap(ms);
                    }
                    OnImageReceived(eargs);
                }
                return;
        }
    }
    public class ResponseReceiveEventArgs
    {
        public Response response { get; set; }
    }
    public class ImageReceiveEventArgs
    {
        public Bitmap response { get; set; }
    }
}
