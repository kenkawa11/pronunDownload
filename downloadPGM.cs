using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace testscraping
{
    class Downloader
    {
        static void Main(string[] args)
        {

            //var url_base = "https://www.oxfordlearnersdictionaries.com/definition/english/";
            var url_base = "https://ejje.weblio.jp/content/";

            var target_word = "be able to";
            target_word = target_word.Trim().Replace(" ", "+");
            var url = url_base+ target_word;
            var fn = "TKR-003";
            //var ptn = "https://www.oxfordlearnersdictionaries.com/media/english/us_pron/.+?mp3";

            var ptn = "https://weblio.hs.llnwd.net/.+?mp3\"";


            if (Downloadmp3(url_base, target_word, ptn, fn))
            {

            }

        }



        public static Boolean Downloadmp3(string url_dic,string w,string ptn, string fn)
        {
            var url = url_dic + w;
            var req = (HttpWebRequest)WebRequest.Create(url);
            string html = "";
            using (var res = (HttpWebResponse)req.GetResponse())
            using (var resSt = res.GetResponseStream())
            // 取得した文字列をUTF8でエンコードします。
            using (var sr = new StreamReader(resSt, Encoding.UTF8))
            {
                // HTMLを取得する。
                html = sr.ReadToEnd();
            }

           

            var reg = new Regex(ptn);
            var m = reg.Match(html);
            if (m.Success)
            {
                var url_mp3 = m.Groups[0].Value;
                if(url_dic== "https://ejje.weblio.jp/content/")
                {
                    url_mp3 = url_mp3.Substring(0, url_mp3.Length - 1);
                }
                WebClient webClient = new WebClient();
                var basepath = @"C:\Users\naobaby\Desktop\test\";
                webClient.DownloadFile(url_mp3, basepath + fn + ".mp3");
                return true;
            }
            return false;
          

        }
    }
    


}
