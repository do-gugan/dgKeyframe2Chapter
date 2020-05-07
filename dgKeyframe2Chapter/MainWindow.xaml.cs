using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Collections.ObjectModel;
using MSAPI = Microsoft.WindowsAPICodePack; //フォルダ選択ダイアログに使用

namespace dgKeyframe2Chapter {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public ObservableCollection<Chapter> Chapters { set; get; } = new ObservableCollection<Chapter>();


        public MainWindow() {
            InitializeComponent();
            //AppModelに参照セット
            AppModel.MW = this;

            //設定の復元
            switch (Properties.Settings.Default.chapterNameMode) {
                case 0:
                    RadioPattern.IsChecked = true;
                    break;
                case 1:
                    RadioInspect.IsChecked = true;
                    break;
                case 2:
                    RadioNone.IsChecked = true;
                    break;
            }


            DataGrid1.ItemsSource = Chapters;

        }

        // TMPGEnc MPEG Editorの.keyframeファイルをchapter.txtに変換
        private void keyframe2chapter(string filename, string extention) {
            // ファイルから１行ずつ読み込んで処理
            int hh, mm, ss, fff, hh0, mm0, ss0, fff0;
            bool isfirst = true;
            int line,line0 = 0;

            if (System.IO.File.Exists(filename) == false) {
                MessageBox.Show("指定されたkeyframeファイルが存在しません");
                return;
            } else {
                Chapters.Clear();
                foreach (string strLine in File.ReadLines(filename, Encoding.GetEncoding("Shift_JIS"))) {
                    // keyframe形式の場合
                    if (extention == ".keyframe") {
                        AppModel.CurrentMode = AppModel.Mode.Keyframe;
                        //数値以外はスルー
                        line = 0;
                        if (!int.TryParse(strLine, out line)) break;

                        // 先頭行を0とした相対値に書き換える
                        if (isfirst == true) {
                            // 先頭行を基準値として保存
                            line0 = line;
                            isfirst = false;
                        }
                        line = line - line0;
                        Chapter cp = new Chapter(line);
                        Chapters.Add(cp);

                    } else if (extention == ".txt") {
                        AppModel.CurrentMode = AppModel.Mode.ChaptersTxt;

                        // 「00:00:00:000 (chaptername)」形式を再読み込みする
                        try {
                            hh = int.Parse(strLine.Substring(0, 2));
                            mm = int.Parse(strLine.Substring(3, 2));
                            ss = int.Parse(strLine.Substring(6, 2));
                            fff = int.Parse(strLine.Substring(9, 3));
                            string chapterName = strLine.Substring(13);

                        // 先頭行を00:00:00.000とした相対値に書き換える
                        if (isfirst == true) {
                            // 先頭行を基準値として保存
                            hh0 = hh;
                            mm0 = mm;
                            ss0 = ss;
                            fff0 = fff;
                            isfirst = false;
                        } else {
                            hh0 = 0;
                            mm0 = 0;
                            ss0 = 0;
                            fff0 = 0;
                        }

                        //Console.WriteLine(hh + ":" + mm + ":" + ss + "." + fff);

                            // 基準値を引く
                            // 時
                            hh = hh - hh0;

                            // 分
                            if (mm >= mm0)
                                mm = mm - mm0;
                            else {
                                hh = hh - 1;
                                mm = 60 + mm - mm0;
                            }

                            // 秒
                            if (ss >= ss0)
                                ss = ss - ss0;
                            else {
                                ss = 60 + ss - ss0;
                                mm = mm - 1;
                                if (mm < 0) {
                                    mm = 59;
                                    hh = hh - 1;
                                }
                            }
                            // フレーム
                            if (fff >= fff0)
                                fff = fff - fff0;
                            else {
                                fff = 1000 + fff - fff0;
                                ss = ss - 1;
                                if (ss < 0) {
                                    ss = 59;
                                    mm = mm - 1;
                                    if (mm < 0) {
                                        mm = 59;
                                        hh = hh - 1;
                                    }
                                }
                            }

                            int sec = (int)(((hh * 3600 + mm * 60 + ss) + (double)(fff / 1000)) * AppModel.FPS);
                            //Console.WriteLine(String.Format("{0:D2}", hh) + ":" + String.Format("{0:D2}", mm) + ":" + String.Format("{0:D2}", ss) + "." + String.Format("{0:D3}", fff));
                            Chapter c = new Chapter(String.Format("{0:D2}", hh) + ":" + String.Format("{0:D2}", mm) + ":" + String.Format("{0:D2}", ss) + "." + String.Format("{0:D3}", fff));
                            if (!String.IsNullOrEmpty(chapterName)) {
                                c.Name = chapterName;
                            }
                            Chapters.Add(c);

                        } catch {
                            TextBox2.Text = "";
                            MessageBox.Show("不正な形式の行がありました。処理を中止します。");
                            Chapters.Clear();
                        }

                    }
                }
            }
            //チャプター名自動処理
            inspect_chapter_name(null, null);
        
        }


        //ファイルをドロップ
        private void DataGrid1_Drop(object sender, DragEventArgs e) {
            //ドラッグ中のファイルやディレクトリの取得
            string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);
            Array.Sort(drags);
            foreach (string d in drags) {
                if (!System.IO.File.Exists(d))
                    // ファイル以外であればイベント・ハンドラを抜ける
                    return;
                else {
                    //System.IO.DirectoryInfo filedata = GetDirectoryInfo(d);
                    FileInfo fi = new FileInfo(d);

                    if (fi.Extension == ".keyframe" | fi.Extension == ".txt") {
                        TextBox2.Text = fi.Name;
                        keyframe2chapter(fi.FullName, fi.Extension);
                    }
                }
            }
            //複数ドロップしても最後の1つで上書きされる

        }

        //ファイルをドラッグ
        private void DataGrid1_PreviewDragEnter(object sender, DragEventArgs e) {
            e.Effects = (e.Data.GetDataPresent(DataFormats.FileDrop)
                         ? DragDropEffects.Copy
                         : DragDropEffects.None);
            e.Handled = true;
        }


        // 参照ボタン（chapterファイルの保存フォルダを選択する）
        private void Button1_Click(object sender, RoutedEventArgs e) {

            //参考: https://qiita.com/Kosen-amai/items/9de7a77a1e6b7851a0b3
            var dlg = new MSAPI::Dialogs.CommonOpenFileDialog();

            // フォルダ選択ダイアログ（falseにするとファイル選択ダイアログ）
            dlg.IsFolderPicker = true;
            // タイトル
            dlg.Title = "chapterファイルの保存先を指定して下さい。";

            if (dlg.ShowDialog() == MSAPI::Dialogs.CommonFileDialogResult.Ok) {
                Properties.Settings.Default.savefolder = dlg.FileName;
                Properties.Settings.Default.Save();
                //TextBox1.Text = dlg.FileName;
            }

        }

        // 保存ボタン（ファイル出力を実行）
        private void Button2_Click(object sender, RoutedEventArgs e) {
            //保存先の存在確認
            if (!Directory.Exists(Properties.Settings.Default.savefolder)) {
                MessageBox.Show("保存フォルダが存在しません。");
                return;
            }


            string body = "";
            foreach (Chapter c in Chapters) {
                body = body + c.TimeCode + " " + c.Name + "\r\n";
            }

            // 書き出し実行
            System.IO.StreamWriter sw = new System.IO.StreamWriter(Properties.Settings.Default.savefolder + @"\" + TextBox2.Text.Replace(".txt", ".chapters.txt").Replace(".keyframe", ".chapters.txt"), false, System.Text.Encoding.UTF8);
            // TextBox1.Textの内容をすべて書き込む
            sw.Write(body);
            // 閉じる
            sw.Close();

            // 設定に応じてサウンドを鳴らす
            if (Properties.Settings.Default.sound == true)
                play_click_sound();

            //データを初期化
            Chapters.Clear();
        }

        public void play_click_sound() {
            // オーディオリソースを取り出す
            System.IO.Stream strm = Properties.Resources.button_30;
            // 同期再生する
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(strm);
            player.Play();
            // 後始末
            player.Dispose();
        }


        // 終了時に設定を保存する
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
        }


        //順番でチャプター名を推測する
        public void inspect_chapter_name(object sender, RoutedEventArgs e) {
            if (RadioInspect.IsChecked == true) {
                //チャプター名推測
                Properties.Settings.Default.chapterNameMode = 1;
                if (Properties.Settings.Default.avant == true) {
                    // アバンタイトル有り
                    switch (Chapters.Count) {
                        case 5: {
                            Chapters[0].Name = "Avant-Title";
                            Chapters[1].Name = "Opening";
                            Chapters[2].Name = "A Part";
                            Chapters[3].Name = "B Part";
                            Chapters[4].Name = "Ending";
                            break;
                        }
                        case 6: {
                            Chapters[0].Name = "Avant-Title";
                            Chapters[1].Name = "Opening";
                            Chapters[2].Name = "A Part";
                            Chapters[3].Name = "B Part";
                            Chapters[4].Name = "Ending";
                            Chapters[5].Name = "Preview";
                            break;
                        }
                        case 7: {
                            Chapters[0].Name = "Avant-Title";
                            Chapters[1].Name = "Opening";
                            Chapters[2].Name = "A Part";
                            Chapters[3].Name = "B Part";
                            Chapters[4].Name = "Ending";
                            Chapters[5].Name = "C Part";
                            Chapters[6].Name = "Preview";
                            break;
                        }
                    }
                } else {
                    // アバンタイトル無し
                    switch (Chapters.Count) {
                        case 4: {
                            Chapters[0].Name = "Opening";
                            Chapters[1].Name = "A Part";
                            Chapters[2].Name = "B Part";
                            Chapters[3].Name = "Ending";
                            break;
                        }
                        case 5: {
                            Chapters[0].Name = "Opening";
                            Chapters[1].Name = "A Part";
                            Chapters[2].Name = "B Part";
                            Chapters[3].Name = "Ending";
                            Chapters[4].Name = "Preview";
                            break;
                        }
                        case 6: {
                            Chapters[0].Name = "Opening";
                            Chapters[1].Name = "A Part";
                            Chapters[2].Name = "B Part";
                            Chapters[3].Name = "Ending";
                            Chapters[4].Name = "C Part";
                            Chapters[5].Name = "Preview";
                            break;
                        }
                    }
                }
            } else if (RadioPattern.IsChecked == true) {
                //パターン入力
                Properties.Settings.Default.chapterNameMode = 0;
                string fmt = "{0:D" + Properties.Settings.Default.digits + "}";
                //Console.WriteLine(fmt);
                foreach (Chapter c in Chapters) {
                    c.Name = Properties.Settings.Default.prefix + String.Format(fmt, Chapters.IndexOf(c)+1) + Properties.Settings.Default.postfix;
                }

            } else {
                //「なし」
                Properties.Settings.Default.chapterNameMode = 2;
            }
            Properties.Settings.Default.Save();
        }


        // 「アバンタイトル有り」チェックが変更された時
        private void CheckBox2_Click(object sender, RoutedEventArgs e) {
            inspect_chapter_name(null, null);
        }

        // FPS設定が変更された時
        private void cb_fps_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Properties.Settings.Default.fps = ((ComboBox)sender).SelectedIndex;
            Properties.Settings.Default.Save();

            //表示更新
            foreach(Chapter c in Chapters) {
                c.UpdateTC();
            }
        }

        //終了メニュー
        private void MenuItem_Checked(object sender, RoutedEventArgs e) {
            this.Close();
        }

        //設定メニュー
        private void MenuItem_Checked_1(object sender, RoutedEventArgs e) {
            Settings sw = new Settings();
            sw.ShowDialog();
        }

        //パスの手動書き換え
        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e) {
            Properties.Settings.Default.savefolder = TextBox1.Text;
            Properties.Settings.Default.Save();
        }

    }
}