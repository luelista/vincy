using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vincy
{
    class Host
    {
        public string id, hostname, tunnel, vncport, vncpassword;
        public Host() { }
        public Host(String[] info)
        {
            id = info[0];
            hostname = info[1];
            tunnel = info[2];
            vncport = info[3];
            vncpassword = info[4];
        }
    }
}
