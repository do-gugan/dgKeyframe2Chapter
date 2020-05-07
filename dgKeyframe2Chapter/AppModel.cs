namespace dgKeyframe2Chapter {
    static class AppModel {

        public static MainWindow MW;

        static public double FPS {
            get {
                //Console.WriteLine("SavedFPSIndex:" + Properties.Settings.Default.fps);
                double fps = 0;
                switch (Properties.Settings.Default.fps) {
                    case 0:
                        fps = 29.97;
                        break;
                    case 1:
                        fps = 23.976;
                        break;
                    case 2:
                        fps = 24;
                        break;
                }
                return fps;
            }
        }

        public enum Mode {
            Keyframe,
            ChaptersTxt
        }

        static private Mode currentMode;
        static public Mode CurrentMode {
            set {
                currentMode = value;
                if (currentMode == Mode.Keyframe) {
                    MW.cb_fps.IsEnabled = true;
                } else {
                    MW.cb_fps.IsEnabled = false;
                    MW.RadioNone.IsChecked = true; //ファイルにあるチャプター名を上書きしないように推測をオフにする
                }
            }
            get { return currentMode; }

        }
    }
}
