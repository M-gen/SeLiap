using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

using System.Net;
using System.IO;

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

            var offset = 0;
            var page_limit = 100;
            //var page_limit = 10;

            none_effect_publicitys.Clear();
            publicitys.Clear();

            while (true)
            {
                var content = GetJSONPDataByWebAPI(vid, offset, page_limit);
                var ja = new JSONPAnalyze();
                var jsonp = ja.Analyze(content);

                try
                {
                    dynamic jsonp_meta = jsonp.value["meta"].value;

                    if ((jsonp_meta["status"].value == 200) && (jsonp_meta["message"].value == "succeed"))
                    {
                        log.WriteLine("データ取得成功");

                    }
                    else
                    {
                        log.WriteLine("データ取得失敗 A");
                        return;
                    }


                }
                catch
                {
                    log.WriteLine("データ取得失敗 B");
                    return;
                }

                try
                {
                    dynamic jsonp_data = jsonp.value["data"].value;
                    log.WriteLine("データ数 " + jsonp_data.Count);
                    if (jsonp_data.Count == 1)
                    {
                        if (jsonp_data[0].type == JSONPAnalyze.JSONP.Type.None)
                        { // 中身がないのでキャンセル
                            return; 
                        }
                    }

                    foreach (var j in jsonp_data)
                    {
                        var name = j.value["name"].value;
                        var comment = j.value["campaignname"].value;
                        var item = new PublicityData(name, comment);
                        none_effect_publicitys.Add(item);
                    }
                }
                catch
                {
                    return;
                }

                offset += page_limit;
                MyLauncher.WaitSleep.Do(10);
            }
        }

        protected string GetJSONPDataByWebAPI( string vid, int offset, int page_limit)
        {
            var url = string.Format(@"http://uad-api.nicovideo.jp/UadsCampaignService/getAdHistoryJsonp?vid={0}&offset={1}&limit={2}", vid, offset, page_limit);
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
