using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string APP_KEY = "";
        private string APP_SECRET = "";
        private string APP_ID = "";
        private string APP_HOST = "";

        private string ucloud_put_auth(string method, string query, string mime)
        {
            string options = method + "\n"
                + "" + "\n"
                + mime + "\n"
                + "" + "\n"
                + "/" + APP_ID + "/" + query;
            return APP_KEY + ":" + Convert.ToBase64String(new HMACSHA1(Encoding.UTF8.GetBytes(APP_SECRET)).ComputeHash(Encoding.UTF8.GetBytes(options)));
        }
        private string ucloud_list_auth(string method, string query)
        {
            string options = method + "\n"
                + "" + "\n"
                + "" + "\n"
                + "" + "\n"
                + "/" + APP_ID + "/";
            return APP_KEY + ":" + Convert.ToBase64String(new HMACSHA1(Encoding.UTF8.GetBytes(APP_SECRET)).ComputeHash(Encoding.UTF8.GetBytes(options)));
        }
        private string ucloud_delete_auth(string method, string query)
        {
            string options = method + "\n"
                + "" + "\n"
                + "" + "\n"
                + "" + "\n"
                + "/" + APP_ID + "/" + query;
            return APP_KEY + ":" + Convert.ToBase64String(new HMACSHA1(Encoding.UTF8.GetBytes(APP_SECRET)).ComputeHash(Encoding.UTF8.GetBytes(options)));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog FileDialog = new OpenFileDialog();
            FileDialog.Multiselect = false;
            FileDialog.Title = "请选择文件";
            FileDialog.Filter = "所有文件(*.*)|*.*";
            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                upload_file(FileDialog.FileName);
            }
        }
        private void upload_file(string FilePath)
        {
            FileStream fs = File.OpenRead(@FilePath);
            FileInfo fi = new FileInfo(@FilePath);
            string FileName = fi.Name;
            string FileMime = mime_content_type(fi.Extension);

            HttpContent content = new StreamContent(fs);
            content.Headers.ContentType = new MediaTypeHeaderValue(FileMime);

            HttpClient client = new HttpClient();
            string test = "";
            try
            {
                client.DefaultRequestHeaders.Add("Authorization", "UCloud " + ucloud_put_auth("PUT", FileName, FileMime));
                Task<HttpResponseMessage> task = client.PutAsync(APP_HOST + FileName, content);

                test = task.Result.ReasonPhrase.ToString();

                richTextBox1.Text = test;

            }
            catch (Exception yc)
            {
                test = yc.Message;
            }
        }
        private string mime_content_type(string str)
        {
            switch (str)
            {
                case ".323": return "text/h323";
                case ".html": return "text/html";
                case ".txt": return "text/plain";
                case ".bmp": return "image/bmp";
                case ".gif": return "image/gif";
                case ".jpeg": return "image/jpeg";
                case ".jpg": return "image/jpeg";
                case ".png": return "image/png";
                case ".ico": return "image/x-ico";
                default: return "application/octet-stream";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            HttpClient client = new HttpClient();
            string test = "";
            try
            {
                client.DefaultRequestHeaders.Add("Authorization", "UCloud " + ucloud_list_auth("GET", "?list"));
                Task<string> task = client.GetStringAsync(APP_HOST + "?list");

                test = task.Result;
                UcloudList test2 = JsonSerializer.Deserialize<UcloudList>(test, new JsonSerializerOptions
                {
                    AllowTrailingCommas = true
                });
                foreach (DataSet item in test2.DataSet)
                {
                    ListViewItem lv_item = new ListViewItem(item.FileName);
                    lv_item.SubItems.Add(item.Size.ToString());
                    listView1.Items.Add(lv_item);
                }
                richTextBox1.Text = task.Result.ToString();

            }
            catch (Exception yc)
            {
                test = yc.Message;
            }
        }
        class UcloudList
        {
            public string BucketName { get; set; }
            public string BucketId { get; set; }
            public string NextMarker { get; set; }
            public List<DataSet> DataSet { get; set; }
        }
        public class DataSet

        {
            public string BucketName { get; set; }
            public string FileName { get; set; }
            public string Hash { get; set; }
            public string MimeType { get; set; }
            public string first_object { get; set; }
            public int Size { get; set; }
            public int CreateTime { get; set; }
            public int ModifyTime { get; set; }
            public string StorageClass { get; set; }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            foreach (dynamic item in listView1.SelectedItems)
            {
                string FileName = item.SubItems[0].Text;
                delete_file(FileName);
                listView1.Items.Remove(item);
            }
        }
        private void delete_file(string FileName)
        {

            HttpClient client = new HttpClient();
            string test = "";
            try
            {
                client.DefaultRequestHeaders.Add("Authorization", "UCloud " + ucloud_delete_auth("DELETE", FileName));
                Task<HttpResponseMessage> task = client.DeleteAsync(APP_HOST + FileName);

                test = task.Result.ReasonPhrase.ToString();

                richTextBox1.Text = test;

            }
            catch (Exception yc)
            {
                test = yc.Message;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string cfg_path = Directory.GetCurrentDirectory() + "\\ucloudFM.cfg";
            if (File.Exists(cfg_path))
            {
                string[] FileText = File.ReadAllLines(cfg_path);
                APP_KEY = FileText[0].Replace("APP_KEY=", "");
                APP_SECRET = FileText[1].Replace("APP_SECRET=", "");
                APP_ID = FileText[2].Replace("APP_ID=", "");
                APP_HOST = FileText[3].Replace("APP_HOST=", "");
            }
            else
            {
                richTextBox1.Text = "需要配置文件";
            }
        }
    }
}