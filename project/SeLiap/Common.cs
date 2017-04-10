using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MyLauncher
{

    class Common
    {
        // TexBoxに簡易なファイルのドラッグ＆ドロップができるようにする
        static public void SettingTextBoxDragEvent(TextBox textbox, bool is_true)
        {
            // とりあえず、trueのみ対応
            if (is_true)
            {
                textbox.AllowDrop = true;
                textbox.DragDrop += _TextBox_DragDrop;
                textbox.DragEnter += _TextBox_DragEnter;
            }
        }

        static private void _TextBox_DragEnter(object sender, DragEventArgs e)
        {
            //ファイルがドラッグされている場合、カーソルを変更する
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        static private void _TextBox_DragDrop(object sender, DragEventArgs e)
        {
            //ドロップされたファイルの一覧を取得
            string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (fileName.Length <= 0)
            {
                return;
            }

            // ドロップ先がTextBoxであるかチェック
            TextBox txtTarget = sender as TextBox;
            if (txtTarget == null)
            {
                return;
            }

            //TextBoxの内容をファイル名に変更
            txtTarget.Text = fileName[0];
        }

        // 随時へんこうされ、保存しておきたいコンフィグやオプションなどの情報の
        // 読み込み、保存、TextBoxなどのUIとの関連付け を行う
        public struct ConfigConectUI // コンフィグとUIの関連付け
        {
            public enum ControlType
            {
                None,
                TextBox,
                TextBox_FileDrop, // ファイルのドラッグ＆ドロップ可能なテキストボックス
                RitchTextBox,
                CheckBox,
            }

            public ConfigConectUI(string _key, System.Windows.Forms.Control _control, ControlType _control_type = ControlType.None, EventHandler _change_event = null)
            {
                key = _key;
                control = _control;
                control_type = _control_type;
                change_event = _change_event;

                switch (control_type)
                {
                    case ControlType.TextBox:
                        ((TextBox)control).TextChanged += _change_event;
                        break;
                    case ControlType.TextBox_FileDrop:
                        Common.SettingTextBoxDragEvent((TextBox)control, true);
                        ((TextBox)control).TextChanged += _change_event;
                        break;
                }
            }
            public string key;
            public System.Windows.Forms.Control control;

            public ControlType control_type;
            public EventHandler change_event; // 更新時のイベント
        }

        static public void LoadConfig(string file_path, List<ConfigConectUI> config_conect_ui)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(file_path);
            List<KeyValuePair<string, string>> key_list = LoadAssist(file);
            file.Close();

            foreach (KeyValuePair<string, string> i in key_list)
            {
                foreach (ConfigConectUI c in config_conect_ui)
                {
                    if (i.Key == c.key)
                    {
                        if (c.control is ComboBox)
                        {
                            ((ComboBox)c.control).Text = i.Value;
                        }
                        else if (c.control is TextBox)
                        {
                            ((TextBox)c.control).Text = i.Value;
                        }
                        else if (c.control is CheckBox)
                        {
                            ((CheckBox)c.control).Checked = i.Value == "True";
                        }
                        else if (c.control is RichTextBox)
                        {
                            ((CheckBox)c.control).Text = CommonStringBase64.ToUTF8(i.Value);
                        }
                    }
                }
            }
        }

        static public void SaveConfig(string file_path, List<ConfigConectUI> config_conect_ui)
        {
            //ファイルを上書きし、UTF-8で書き込む
            System.IO.StreamWriter sw = new System.IO.StreamWriter(
                file_path,
                false,
                System.Text.Encoding.GetEncoding("UTF-8"));

            foreach (ConfigConectUI c in config_conect_ui)
            {
                if (c.control is ComboBox)
                {
                    sw.WriteLine(c.key + " " + ((ComboBox)c.control).Text);
                }
                else if (c.control is TextBox)
                {
                    sw.WriteLine(c.key + " " + ((TextBox)c.control).Text);
                }
                else if (c.control is CheckBox)
                {
                    sw.WriteLine(c.key + " " + ((CheckBox)c.control).Checked);
                }
                else if (c.control is RichTextBox)
                {
                    sw.WriteLine(c.key + " " + CommonStringBase64.ToBase64(((RichTextBox)c.control).Text));
                }
            }


            //閉じる
            sw.Close();
        }

        static public List<KeyValuePair<string, string>> LoadAssist(System.IO.StreamReader file)
        {
            List<KeyValuePair<string, string>> key_list = new List<KeyValuePair<string, string>>();
            string line;
            while ((line = file.ReadLine()) != null)
            {
                string[] s = line.Split(' ');
                //
                // 後半をまとめ直す
                string aft = "";
                for (int i = 1; i < s.Length; i++)
                {
                    if (i != 1) aft += " ";
                    aft += s[i];
                }
                KeyValuePair<string, string> kv = new KeyValuePair<string, string>(s[0], aft);
                key_list.Add(kv);
                // 読み込み完了
            }
            return key_list;
        }
    }

    // 文字列をBase64に変換と戻す
    // 保存時に改行や空白があると問題があるときなどで利用
    public class CommonStringBase64
    {
        const string enc_str = "UTF-8";

        public static string ToBase64(string str)
        {
            var enc = Encoding.GetEncoding(enc_str);
            var res = Convert.ToBase64String(enc.GetBytes(str));
            return res;
        }

        public static string ToUTF8(string str)
        {
            var enc = Encoding.GetEncoding(enc_str);
            var res = enc.GetString(Convert.FromBase64String(str));
            return res;
        }
    }

    // 外部アプリケーションの起動
    public class EasyProcess
    {
        [DllImport("user32")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        // 実行する
        static public System.Diagnostics.Process StartExe(string path, bool is_window_show = true)
        {
            var psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = path;
            psi.WorkingDirectory = CommonFiles.GetDirectory(path);

            var p = System.Diagnostics.Process.Start(psi);
            //p.WaitForInputIdle(); // 起動してアイドル状態になるまで待つ...と、処理を専有するのでReleaseでビルドすると動かなくなる...

            if (is_window_show)
            {
                while (!IsWindowVisible(p.MainWindowHandle)) // ウィンドウが表示されるまで待つ
                {
                    WaitSleep.Do(10);
                }
            }
            return p;
        }
    }

    // Taskを使ったスリープ
    // 他のスレッドに処理を意図して移したい場合に使う
    // 処理速度と時間の精度は当然落ちる（他の処理へ譲るので）
    public class WaitSleep
    {
        static public void Do(int time)
        {
            Task taskA = Task.Factory.StartNew(() => _Sleep_Task(time));
            taskA.Wait();
        }

        static private void _Sleep_Task(int time)
        {
            Thread.Sleep(time);
        }
    }

    public class CommonFiles
    {
        // ファイル名を取得
        static public string GetFileName(string str)
        {
            var name = str.Substring(str.LastIndexOf('\\') + 1);
            name = name.Substring(0, name.LastIndexOf('.'));
            return name;
        }

        // ファイルパスからディレクトリを取得
        static public string GetDirectory(string path)
        {
            var pos = path.LastIndexOf("\\");
            return path.Substring(0, pos);
        }
    }

}
