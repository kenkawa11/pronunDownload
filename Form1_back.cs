using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace pronunDownload
{
    public partial class Form1 : Form
    {
        Boolean isCancel;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            isCancel = false;

            button2.Enabled = false;
            /*
            Task task = Task.Run(() => {
                fntreat();
            });
            */
            fntreat();
            button2.Enabled = true;
        }


        public async void fntreat()
        {
            label1.Text = "Processing test";
            var rfn = textBox1.Text;
            List<List<string>> LoadFileData = new List<List<string>>();
            string line;
            String[] readLine;

            using (StreamReader sr = new StreamReader(rfn))
            {

                while (!sr.EndOfStream) //csvファイルをリストに読み込む
                {
                    // CSVファイルの一行を読み込む
                    List<string> stringList = new List<string>();
                    line = sr.ReadLine();
                    // 読み込んだ一行をカンマ毎に分けて配列に格納する
                    readLine = line.Split(',');
                    stringList.AddRange(readLine);
                    LoadFileData.Add(stringList);
                    readLine = null;
                }
            }
            var num_treat = 0;
            var num_gross = LoadFileData.Count;
            foreach (var values in LoadFileData)
            {
                var target_word = values[2];
                target_word = target_word.Trim().Replace(" ", "+");
                string url;
                string fn = values[0];

                if (values[3] == "n")
                {
                    values[3] = "A";
                    url = "https://www.oxfordlearnersdictionaries.com/definition/english/";
                    if (Downloadmp3(url, target_word, fn) == "0")
                    {
                        url = "https://www.ldoceonline.com/jp/dictionary/";
                        if (Downloadmp3(url, target_word, fn)== "0")
                        {
                            url = "https://ejje.weblio.jp/content/";
                            if (Downloadmp3(url, target_word, fn)=="0")
                            {
                                values[3] = "n";
                            }
                        }
                    }
                }

                if (values[4] == "n")
                {
                    url = "https://eow.alc.co.jp/";

                    target_word = "search?q=" + target_word;
                    var temp = Downloadmp3(url, target_word, fn);
                    if (temp == "0")
                    {
                        values[4] = "n";

                    }
                    else
                    {
                        values[4] = temp;
                    }

                }


                num_treat = num_treat + 1;
                label2.Text = $"{num_treat} in {num_gross}";

                if (isCancel)
                {
                    break;
                }

                if (num_treat<num_gross)
                {
                    await Task.Delay(1000);
                }
            }



            label1.Text = "Writing data in file";

            line = null;
            using (StreamWriter file = new StreamWriter(rfn, false, Encoding.UTF8))
            {
                foreach (var v in LoadFileData)
                {
                    foreach (var u in v)
                    {
                        line += u;
                        line += ",";
                    }
                    line = line.Substring(0, line.Length - 1);
                    file.WriteLine(line);
                    line = null;
                }
            }
            label1.Text = "Complete";
        }

        public string get_body(string url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "Mozilla/5.0";
            string html = "";
            try
            {
                using (var res = (HttpWebResponse)req.GetResponse())
                using (var resSt = res.GetResponseStream())
                // 取得した文字列をUTF8でエンコードします。
                using (var sr = new StreamReader(resSt, Encoding.UTF8))
                {
                    // HTMLを取得する。
                    html = sr.ReadToEnd();
                }
                return html;
            }
            catch (System.Net.WebException)
            {
                return "0";
            }
        }

        public void get_mp3(string url_mp3, string fn)
        {
            WebClient webClient = new WebClient();
            var basepath = @"C:\Users\naobaby\Desktop\test\";
            webClient.DownloadFile(url_mp3, basepath + fn + ".mp3");
        }
        
        public string Downloadmp3(string url_dic,string w,string fn)
        {
            string ptn="";

            if(url_dic == "https://www.oxfordlearnersdictionaries.com/definition/english/")
            {
                ptn = "https://www.oxfordlearnersdictionaries.com/media/english/us_pron/.+?mp3";
            }

            if (url_dic == "https://www.ldoceonline.com/jp/dictionary/")
            {
                ptn = "https://.+/ameProns/.+?mp3";
            }

            if (url_dic == "https://ejje.weblio.jp/content/")
            {
                ptn = "https://weblio.hs.llnwd.net/.+?mp3\""; 
            }

            if (url_dic == "https://eow.alc.co.jp/")
            {
                ptn = "【発音】.+?【カナ】|【発音.+?】.+?【カナ】";
            }



            var url = url_dic + w;

            var html = get_body(url);

            if (html=="0")
            {
                return "0";
            }

            var reg = new Regex(ptn);
            var m = reg.Match(html);
            if (m.Success)
            {
                var url_mp3 = m.Groups[0].Value;
                if (url_dic == "https://eow.alc.co.jp/")
                {
                    var pos = url_mp3.IndexOf("】");

                    return  url_mp3.Substring(pos+1,url_mp3.Length-pos-5);

                }
                else
                {
                    if (url_dic == "https://ejje.weblio.jp/content/")
                    {
                        url_mp3 = url_mp3.Substring(0, url_mp3.Length - 1);
                    }
                    get_mp3(url_mp3, fn);
                }
                return "1";
            }
            return "0";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            isCancel=true;
        }

    }
}
