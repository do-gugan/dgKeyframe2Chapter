using System;
using System.ComponentModel;
using System.Windows;

namespace dgKeyframe2Chapter {

    public class Chapter: INotifyPropertyChanged {
        private int secs;
        private String timeCode ="";
        private String name ="";

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public String TimeCode {
            get {
                if (Properties.Settings.Default.round == true) {
                    //丸める
                    return timeCode.Substring(0,9) + "000";
                } else {
                    //丸めない
                    return timeCode;
                }
            }
                set { timeCode = value; }
            }


        public String Name {
            set { name = value;
                UpdateName();
            }
            get { return name; }
        }


        //コンストラクタ（Keyframeモード。秒数で初期化）
        public Chapter(int sec) {
            secs = sec;
            UpdateTC();
        }

        //コンストラクタ（Chapters.txtモード。00:00:00.000形式文字列で初期化）
        public Chapter(string tc) {
            timeCode = tc;
        }

        public void UpdateTC() {
            if (AppModel.CurrentMode == AppModel.Mode.Keyframe) {
                string tc;
                double frame;
                int hh, mm, ss;
                double fff;
                Window mainWindow = Application.Current.MainWindow;
                frame = secs / AppModel.FPS; // 29.97/23.975/24
                hh = (int)Math.Floor(frame / 3600);
                mm = (int)Math.Floor((frame - (hh * 3600)) / 60);
                ss = (int)Math.Floor(frame - (hh * 3600) - (mm * 60));
                fff = (double)(frame - (hh * 3600) - (mm * 60) - ss) * 1000;
                //Console.WriteLine("fps:" + AppModel.FPS);
                //Console.WriteLine("frame:" + frame);
                //Console.WriteLine("hh:" + hh);
                //Console.WriteLine("mm:" + mm);
                //Console.WriteLine("ss:" + ss);
                //Console.WriteLine("fff:" + fff);

                // 設定で分岐
                if (Properties.Settings.Default.round == true) {
                    // 一般に少しチャプター位置が後ろにズレがちなので、fffは丸めて000にしておく
                    tc = String.Format("{0:D2}", hh) + ":" + String.Format("{0:D2}", mm) + ":" + String.Format("{0:D2}", ss) + ".000";
                } else {
                    // ドンピシャの値を使用
                    tc = String.Format("{0:D2}", hh) + ":" + String.Format("{0:D2}", mm) + ":" + String.Format("{0:D2}", ss) + "." + String.Format("{0:D3}", (int)fff);
                }
                //Console.WriteLine(tc);
                timeCode = tc;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeCode))); //更新イベント発生
        }

        public void UpdateName() {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); //更新イベント発生
        }

    }
}
