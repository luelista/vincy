using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace vincy
{
    public partial class Form1 : Form
    {
        public string confdir;
        List<Host> hostlist = new List<Host>();
        Dictionary<int, int> sshvncassoc = new Dictionary<int, int>();
        public string rechner;

        public Form1()
        {
            InitializeComponent();
            rechner = Environment.MachineName;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            initConfigDir();
            refreshView();
            Text = "vincy @ " + rechner;
        }

        private void refreshView()
        {
            try
            {
                loadHostlist();
                displayHostlist();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load host list\n\n" + ex.Message);
            }
        }

        private void loadHostlist()
        {
            hostlist.Clear();
            string[] inputstr = File.ReadAllLines(confdir + "\\hostlist.txt");
            foreach(String line in inputstr) {
                if (String.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                string[] d = line.Split('\t');
                if (d.Length < 5) continue;
                hostlist.Add(new Host(d));
            }
        }

        private void displayHostlist()
        {
            treeView1.Nodes.Clear();
            Dictionary<string, TreeNode> parents = new Dictionary<string, TreeNode>();
            foreach (Host host in hostlist)
            {
                TreeNode parent = null;
                if (parents.ContainsKey(host.tunnel))
                {
                    parent = parents[host.tunnel];
                }
                else
                {
                    parent = treeView1.Nodes.Add(host.tunnel);
                    parent.ImageIndex = 1; parent.SelectedImageIndex = 1;
                    parents[host.tunnel] = parent;
                }
                parent.Nodes.Add(host.id, host.id + " (" + host.hostname + ")");
            }
            treeView1.ExpandAll();
        }

        private void initConfigDir()
        {
            confdir = Path.Combine(Application.StartupPath, "rs\\vincy");
            if (Directory.Exists(confdir)) return;
            confdir = Environment.GetEnvironmentVariable("USERPROFILE") + "\\.config\\rs\\vincy";
            Directory.CreateDirectory(confdir);
        }

        private void doConnect(Host host)
        {
            var sshconf = Path.Combine(confdir, "ssh_client_conf.txt");
            var known_hosts = Path.Combine(confdir, "known_hosts.txt");
            Random rand = new Random();
            var locport = 53000 + rand.Next(1, 10000);
            var idfile = Path.Combine(confdir, "id_rsa");
            var sshexe = Path.Combine(Application.StartupPath, "helpers\\ssh.exe");
            var sshprocinfo = new System.Diagnostics.ProcessStartInfo(sshexe);
            sshprocinfo.Arguments = String.Format("-v -F \"{0}\" -i \"{1}\" -L {2}:{3}:{4} -o \"UserKnownHostsFile={7}\" \"{5}\" \"{6}\"",
                sshconf, idfile, locport, host.hostname, host.vncport, host.tunnel, "echo Tunnel connected && sleep 10000",
                known_hosts);
            
            var sshproc = System.Diagnostics.Process.Start(sshprocinfo);

            Application.DoEvents();
            Thread.Sleep(500);
            Application.DoEvents();
            this.Focus();
            this.Activate();
            Application.DoEvents();
            Thread.Sleep(200);
            Application.DoEvents();

            var rr = MessageBox.Show(sshprocinfo.Arguments, "Click OK to run vncviewer...", MessageBoxButtons.OKCancel);
            if (rr == DialogResult.Cancel)
            {
                try { sshproc.Kill(); }
                catch (Exception ex) { }
                return;
            }
            //Thread.Sleep(1500);

            var vncexe = Path.Combine(Application.StartupPath, "helpers\\tvnviewer.exe");
            var vncprocinfo = new System.Diagnostics.ProcessStartInfo(vncexe);
            vncprocinfo.Arguments = String.Format("-host=\"{0}\" -port={1} -password=\"{2}\"",
                "127.0.0.1", locport, host.vncpassword);
            var vncproc = System.Diagnostics.Process.Start(vncprocinfo);
            vncproc.EnableRaisingEvents = true;

            sshvncassoc[vncproc.Id] = sshproc.Id;
            Debug.Print("Running vnc="+vncproc.Id+" ssh="+sshproc.Id);
            vncproc.Exited += new EventHandler(vncproc_Exited);

        }

        void vncproc_Exited(object sender, EventArgs e)
        {
            Process p = (Process)sender;
            Debug.Print("Exited");
            Debug.Print(p.Id.ToString());
            if (sshvncassoc.ContainsKey(p.Id))
            {
                Debug.Print("Found");
                try
                {
                    var p2 = Process.GetProcessById(sshvncassoc[p.Id]);
                    if (p2 != null)
                    {
                        p2.Kill();
                    }
                }
                catch (Exception ex) { }
                sshvncassoc.Remove(p.Id);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (!File.Exists(confdir + "\\updateurl.txt"))
            {
                System.Diagnostics.Process.Start("notepad", confdir + "\\updateurl.txt");
                return;
            }
            treeView1.Nodes.Clear();
            toolStripButton1.Enabled = false;
            Application.DoEvents();
            try
            {
                string updateURL = File.ReadAllText(confdir + "\\updateurl.txt").Trim();
                Downloader.URLDownloadToFile2(null, updateURL + "/hostlist.txt?" + DateTime.Now.Ticks.ToString(), confdir + "\\hostlist.txt", 0, IntPtr.Zero);
                Downloader.URLDownloadToFile2(null, updateURL + "/ssh_client_conf.txt?" + DateTime.Now.Ticks.ToString(), confdir + "\\ssh_client_conf.txt", 0, IntPtr.Zero);
                MessageBox.Show("Update complete", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                refreshView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Update failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                toolStripButton1.Enabled = true;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var key = e.Node.Name;
            Host host = null;
            foreach (Host check in hostlist) if (check.id == key) host = check;
            if (host != null)
                doConnect(host);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
              case Keys.Escape:
                this.Close();
                break;
              case Keys.F5:
                toolStripButton1_Click(null, null);
                break;
            }
        }

        private void schlüsselpaarErzeugenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var idfile = Path.Combine(confdir, "keypair.txt");
            var sshexe = Path.Combine(Application.StartupPath, "helpers\\ssh-keygen.exe");
            var sshprocinfo = new System.Diagnostics.ProcessStartInfo(sshexe);
            sshprocinfo.Arguments = String.Format("-f \"{0}\" -N \"\" -b 4096 -C \"{1}\"",
                idfile, this.Text);

            var sshproc = System.Diagnostics.Process.Start(sshprocinfo);
            
            
            sshproc.WaitForExit();
            if (sshproc.ExitCode == 0)
            {
                copyAuthString();
                MessageBox.Show("Keypair created successfully.\n\nPublic key was sent to clipboard.");
            }
            else
            {
                MessageBox.Show("Error creating keypair.");
            }

        }

        String getAuth()
        {
            var idfile = Path.Combine(confdir, "keypair.txt");
            var privkey = File.ReadAllText(idfile);
            var hash = Helper.GetSHA1Hash(privkey);
            var auth = rechner + ":" + hash;
            return auth;
        }

        void copyAuthString()
        {
            var idfile = Path.Combine(confdir, "keypair.txt");
            var auth = getAuth();
            var pubkey = File.ReadAllText(idfile + ".pub");
            Clipboard.Clear();
            Clipboard.SetText("A:" + auth + "\r\n" + "B:" + pubkey);
        }

        private void copyAuthStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            copyAuthString();
        }

    }
}
