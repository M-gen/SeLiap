using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

using System.Net;
using System.IO;
using System.Xml;


using CSV;

namespace SeLiap
{
    // 宣伝者(広告主)についてのモジュール
    
    public class PublicityData
    {
        public string name;
        public string comment;
        public int counter;
        public PublicityData(string name, string comment)
        {
            this.name = name;
            this.comment = comment;
            counter = 1;
        }

        public PublicityData Copy()
        {
            var pd = new PublicityData( name, comment );
            return pd;
        }
    }

    public class PublicityLog
    {
        RichTextBox log_output_target;

        public PublicityLog( RichTextBox log_output_target)
        {
            this.log_output_target = log_output_target;
        }

        public void WriteLine( string log )
        {
            log_output_target.Focus();
            if (log_output_target.Text!="")
            {
                log_output_target.AppendText(System.Environment.NewLine);
            }
            log_output_target.AppendText(log);

            //log_output_target.Text += log + "\n";
        }
    }

    public class PublicityAnalyze
    {
        public List<PublicityData> none_effect_publicitys = new List<PublicityData>();  // 加工してない宣伝者リスト
        public List<PublicityData> publicitys = new List<PublicityData>();              // 加工済み宣伝者リスト

        PublicityLog log = null;

        public PublicityAnalyze(PublicityLog log )
        {
            this.log = log;
        }

        public void Analyze( string url_source)
        {
            var vid = AnalyzeVID(url_source);
            if (!IsURLCheck(vid))
            {
                log.WriteLine("URLが不適切です > " + vid);
                return;
            }
            log.WriteLine("URLチェッククリア > " + vid);


            none_effect_publicitys.Clear();
            publicitys.Clear();

            GetStatus_2017_1212(vid);
            GetStatus_2017_1212_Befor(vid);
        }

        // 2017.12/12以前の情報を受け取る
        protected void GetStatus_2017_1212_Befor(string vid)
        {
            // まずは、動画の基本情報を取得する
            // 日付の確認をする
            var url = $"http://ext.nicovideo.jp/api/getthumbinfo/{vid}";
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            var res = (HttpWebResponse)req.GetResponse();
            var s = res.GetResponseStream();
            var sr = new StreamReader(s);
            var content = sr.ReadToEnd();

            //Console.WriteLine(content);
            // XML解析
            var doc = new XmlDocument();
            doc.Load(new StringReader(content));
            var root = doc.DocumentElement;
            // 日付
            var date_string = root.SelectSingleNode("thumb/first_retrieve").InnerText;
            var year = int.Parse(date_string.Substring(0, 4));
            var month = int.Parse(date_string.Substring(5, 2));
            var day = int.Parse(date_string.Substring(8, 2));
            var time1 = new DateTime(year, month, day);
            var time2 = new DateTime(2017, 12, 20);

            var is_ok = false;
            if (time1 <= time2)
            {
                is_ok = true;
            }
            if (!is_ok)
            {
                return;
            }

            try
            {
                // 日付が確定なら
                // CSVのダウンロード
                url = $"https://secure-dcdn.cdn.nimg.jp/nicoad/res/old-video-comments/{vid}.csv";
                req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                res = (HttpWebResponse)req.GetResponse();
                s = res.GetResponseStream();
                sr = new StreamReader(s);
                content = sr.ReadToEnd();
                Console.WriteLine(content);

                // CSVの解析
                var csv_analyze = new CSVAnalyze(content);

                // 解析結果を適応する
                foreach (var tmp in csv_analyze.lines)
                {
                    if (tmp.items[0] != "")
                    {
                        var item = new PublicityData(tmp.items[0], tmp.items[1]);
                        none_effect_publicitys.Add(item);
                    }
                }
                log.WriteLine("2017年12月20日以前の広告者データをCSVとして取得しました");
            }
            catch
            {
                log.WriteLine("2017年12月20日以前の広告者データ(CSV)はありません");
            }
        }

        protected void GetStatus_2017_1212(string vid)
        {
            var offset = 0;
            var page_limit = 100;
            //var count_max = -1;
            while (true)
            {
                var content = GetJSONPDataByWebAPI_ver2017_1212(vid, offset, page_limit);
                var ja = new JSONPAnalyze();
                var jsonp = ja.AnalyzeByJSON(content);

                try
                {
                    dynamic jsonp_meta = jsonp.value["meta"].value;

                    if (jsonp_meta["status"].value == 200) 
                    {
                        log.WriteLine("データ取得成功");

                    }
                }
                catch
                {
                    log.WriteLine("データ取得失敗 B");
                    return;
                }

                try
                {
                    //count_max = jsonp.value["data"].value["count"].value;
                    dynamic jsonp_data = jsonp.value["data"].value["histories"].value;
                    log.WriteLine("データ数 " + jsonp_data.Count);
                    if (jsonp_data.Count == 1)
                    {
                        if (jsonp_data[0].value == null)
                        { // 中身がないのでキャンセル
                            return;
                        }
                    }

                    foreach (var j in jsonp_data)
                    {
                        var name = j.value["advertiserName"].value;
                        var comment = "";// j.value["message"].value;
                        if (j.value.ContainsKey("message")) comment = j.value["message"].value;

                        var item = new PublicityData(name, comment);
                        none_effect_publicitys.Add(item);
                    }
                }
                catch
                {
                    return;
                }
                
                offset += page_limit;
                //if ((count_max > 0) && (count_max > offset)) break;
                MyLauncher.WaitSleep.Do(10);
            }
        }

        //protected string GetJSONPDataByWebAPI_old( string vid, int offset, int page_limit)
        //{
        //    var url = string.Format(@"http://uad-api.nicovideo.jp/UadsCampaignService/getAdHistoryJsonp?vid={0}&offset={1}&limit={2}", vid, offset, page_limit);
        //    log.WriteLine(url);

        //    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
        //    req.Method = "GET";
        //    //req.Method = "PUSH";

        //    HttpWebResponse res = (HttpWebResponse)req.GetResponse();

        //    Stream s = res.GetResponseStream();
        //    StreamReader sr = new StreamReader(s);
        //    string content = sr.ReadToEnd();

        //    return content;
        //}

        // 取得方法が異なるので
        protected string GetJSONPDataByWebAPI_ver2017_1212(string vid, int offset, int page_limit)
        {
            var url = string.Format(@"https://api.nicoad.nicovideo.jp/v1/contents/video/{0}/histories?offset={1}&limit={2}", vid, offset, page_limit);
            log.WriteLine(url);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            //req.Method = "PUSH";

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            Stream s = res.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            string content = sr.ReadToEnd();

            return content;
        }

        // URLを解析しvidの引数として適切な値になるように加工
        protected string AnalyzeVID( string url_source )
        {
            var res = url_source;
            // /の最後でカットする
            {
                var i = res.LastIndexOf(@"/");
                if ( i >= 0 )
                {
                    res = res.Substring(i+1);
                } 
            }

            // 後ろにゴミが入っている場合の削除（?が境界)
            {
                var i = res.LastIndexOf(@"?");
                if (i >= 0)
                {
                    res = res.Substring(0,i);
                }

            }

            return res;
        }

        // URLが適切かどうかチェックする
        protected bool IsURLCheck(string url)
        {
            var rem = url; // 調査の残り

            // 正規表現で言えば /^(sm|so|nm|)([0-9]+)$/ 
            // 先頭が sm| so| nm | 数字である
            if (rem.IndexOf("sm") == 0)
            {
                rem =  rem.Substring(2);
            }
            else if (rem.IndexOf("so") == 0)
            {
                rem = rem.Substring(2);
            }
            else if (rem.IndexOf("nm") == 0)
            {
                rem = rem.Substring(2);
            }

            // 残りが最初から最後まで半角の数字である
            var d = "0123456789";

            foreach( var s1 in rem )
            {
                var is_ok = false;
                foreach( var s2 in d )
                {
                    if ( s1==s2 )
                    {
                        is_ok = true;
                        break;
                    }
                }
                if (!is_ok)
                {
                    // d以外の文字が入っている → false
                    return false;
                }
            }

            return true;
        }

    }
}
