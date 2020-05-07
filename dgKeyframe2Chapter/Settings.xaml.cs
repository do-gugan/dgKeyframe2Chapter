using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace dgKeyframe2Chapter {
    /// <summary>
    /// Settings.xaml の相互作用ロジック
    /// </summary>
    public partial class Settings : Window, INotifyPropertyChanged {
        private string preview1 = "";
        private string preview10 = "";
        public string Preview1 {
            get { return preview1; }
            set {
                preview1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Preview1)));
            }
        }

        public string Preview10 { get { return preview10; } }
        public int Digits { get { return Properties.Settings.Default.digits; } }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Settings() {
            InitializeComponent();
            this.DataContext = this;
        }

        //閉じる
        private void Button_Click_1(object sender, RoutedEventArgs e) {
            Close();
        }

        //▲
        private void Button_Click_2(object sender, RoutedEventArgs e) {
            int digits = Properties.Settings.Default.digits;
            if (digits < 5) {
                digits++;
                Properties.Settings.Default.digits = digits;
                Properties.Settings.Default.Save();
                BtnDown.IsEnabled = true;
            }

            if  (digits >=5) {
                BtnUp.IsEnabled = false;
            } else {
                BtnUp.IsEnabled = true;
            }
            UpdatePreview(null, null);
        }

        //▼
        private void Button_Click_3(object sender, RoutedEventArgs e) {
            int digits = Properties.Settings.Default.digits;
            if (digits > 0) {
                digits--;
                Properties.Settings.Default.digits = digits;
                Properties.Settings.Default.Save();
                BtnUp.IsEnabled = true;
            }

            if (digits <= 0 ) {
                BtnDown.IsEnabled = false;
            } else {
                BtnDown.IsEnabled = true;
            }
            UpdatePreview(null, null);
        }

        //プレビューを更新する
        private void UpdatePreview(object sender, TextChangedEventArgs e) {
            if (Postfix == null) return; //Prefix欄のセットでPostfix欄が未初期化で呼ばれることがあるので


            int digits = Properties.Settings.Default.digits;
            Properties.Settings.Default.Save();

            if (digits==0) {
                preview1 = Prefix.Text + Postfix.Text;
                preview10 = Prefix.Text + Postfix.Text;
            } else {
                string fmt = "{0:D" + digits + "}";
                preview1 = Prefix.Text + String.Format(fmt, 1) + Postfix.Text;
                preview10 = Prefix.Text + String.Format(fmt, 10) + Postfix.Text;
            }
            Console.WriteLine(preview1);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Preview1)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Preview10)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Digits)));

        }

        private void Window_Closed(object sender, EventArgs e) {
            AppModel.MW.inspect_chapter_name(null,null);
        }


    }
}
