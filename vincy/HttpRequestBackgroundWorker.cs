using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace stundataonline {
    class HttpRequestBackgroundWorker : System.ComponentModel.BackgroundWorker {

        public String Method, URL, PostBody = null, BasicAuth = null;
        public int responseCode;
        public string requestFailed = null;

        public HttpRequestBackgroundWorker() : base() {
            this.WorkerReportsProgress = true;
        }

        public void Start() {

        }

        protected override void OnDoWork(System.ComponentModel.DoWorkEventArgs e) {
            try {
                requestFailed = null;
                base.OnDoWork(e);
                this.ReportProgress(10);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.URL);
                request.Method = this.Method;
                if (this.BasicAuth != null)
                    request.Headers["Authorization"] = "Basic " + this.BasicAuth;
                if (this.PostBody != null) {
                    request.ContentType = "application/x-www-form-urlencoded";

                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII)) {
                        writer.Write(this.PostBody);
                    }
                }
                this.ReportProgress(20);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                this.ReportProgress(30);
                this.responseCode = (int)response.StatusCode;

                using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
                    String result = reader.ReadToEnd();
                    e.Result = result;
                }
            } catch (WebException ex) {
                if (ex.Response == null) {
                    requestFailed = ex.Message;
                } else {
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    this.responseCode = (int)response.StatusCode;

                    using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
                        String result = reader.ReadToEnd();
                        e.Result = result;
                    }
                }
            } catch (Exception ex) {
                requestFailed = ex.Message;
            }
            this.ReportProgress(40);
        }
        

    }
}
