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
            TextBox1.Text = Properties.Settings.Default.savefolder;
            CheckBox1.IsChecked = Properties.Settings.Default.chapterinspection;
            CheckBox3.IsChecked = Properties.Settings.Default.round;
            CheckBox4.IsChecked = Properties.Settings.Default.sound;
            Console.WriteLine("fpsIndex:" + Properties.Settings.Default.fps);
            cb_fps.SelectedIndex = Properties.Settings.Default.fps;


            DataGrid1.ItemsSource = Chapters;

            //テスト用データ挿入
            //Chapter c1 = new Chapter("100");
            //c1.Name = "Opening";
            //Chapters.Add(c1);

            //Chapter c2 = new Chapter("200");
            //c2.Name = "aaaa";
            //Chapters.Add(c2);

        }

        // TMPGEnc MPEG Editor 3.0の.keyframeファイルをchapter.txtに変換
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

                        Console.WriteLine(hh + ":" + mm + ":" + ss + "." + fff);

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
                            Console.WriteLine(String.Format("{0:D2}", hh) + ":" + String.Format("{0:D2}", mm) + ":" + String.Format("{0:D2}", ss) + "." + String.Format("{0:D3}", fff));
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
            if (CheckBox1.IsChecked == true) {
                // チャプター数からチャプター名を推測してセットする
                inspect_chapter_name();
            }

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
                TextBox1.Text = dlg.FileName;
            }

        }

        // 保存ボタン（ファイル出力を実行）
        private void Button2_Click(object sender, RoutedEventArgs e) {
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
        public void inspect_chapter_name() {
            if (CheckBox2.IsChecked == true) {
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
            } else
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

        // チャプター推測のチェックボックス設定を保存
        private void CheckBox1_Click(object sender, RoutedEventArgs e) {
            Properties.Settings.Default.chapterinspection = (bool)CheckBox1.IsChecked;
            Properties.Settings.Default.Save();
            if (CheckBox1.IsChecked == true) {
                inspect_chapter_name();
                CheckBox2.IsEnabled = true;
            } else {
                CheckBox2.IsEnabled = false;
            }
        }

        // 「アバンタイトル有り」チェックが変更された時
        private void CheckBox2_Click(object sender, RoutedEventArgs e) {
            Properties.Settings.Default.avant = (bool)CheckBox2.IsChecked;
            Properties.Settings.Default.Save();
            inspect_chapter_name();
        }

        // 「秒単位で丸める」チェックが変更された時
        private void CheckBox3_Click(object sender, RoutedEventArgs e) {
            Properties.Settings.Default.round = (bool)CheckBox3.IsChecked;
            Properties.Settings.Default.Save();

            //表示更新
            foreach (Chapter c in Chapters) {
                c.UpdateTC();
            }

        }

        // 「確認音再生」チェックが変更された時
        private void CheckBox4_Click(object sender, RoutedEventArgs e) {
            Properties.Settings.Default.sound = (bool)CheckBox4.IsChecked;
            Properties.Settings.Default.Save();
        }

        // FPS設定が変更された時
        private void cb_fps_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Console.WriteLine(((ComboBox)sender).SelectedIndex + ":" + ((ComboBox)sender).SelectedItem);
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

    }
}