using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Collections.ObjectModel;

namespace FileByeBye
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // WPF/XAML : DataGrid にボタンの列を追加する
        // http://increment.hatenablog.com/entry/2015/08/21/054800

        // WPF4.5入門 その23 「DataGridコントロール その1」
        // http://blog.okazuki.jp/entry/20130218/1358172834

        private ObservableCollection<DirData> DirDataCollection;
        private string targetPath = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            //ダイアログ起こす
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            //パラメタ設定
            dlg.IsFolderPicker = true;
            // 読み取り専用フォルダ/コントロールパネルは開かない
            dlg.EnsureReadOnly = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = "C:\\Users\\" + System.Environment.UserName + "\\Desktop";
            //ダイアログ表示
            var Path = dlg.ShowDialog();
            if (Path == CommonFileDialogResult.Ok)
            {
                targetPath = dlg.FileName;

                openDir();
            }


        }

        private void openDir()
        {
            
            pathTB.Text = targetPath;
            string[] files = Directory.GetFileSystemEntries(targetPath);
            int n = files.Length;
            DirDataCollection = new ObservableCollection<DirData>(
                Enumerable.Range(0, n).Select(i => new DirData
                {
                    Path = files[i],
                    Name = "",
                    Size = 0,
                    isDir = Directory.Exists(files[i]),
                    Remove = false
                }));

            for (int i = 0; i < n; i++)
            {
                string path = DirDataCollection[i].Path;
                if (DirDataCollection[i].isDir)
                {
                    DirDataCollection[i].Name = path.Substring(System.IO.Path.GetDirectoryName(path).Length+1);
                    DirDataCollection[i].LastWriteTime = Directory.GetLastWriteTime(path);

                    if (System.IO.Directory.EnumerateFileSystemEntries(path).Any())
                    {
                        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
                        DirDataCollection[i].Size = (Int64)GetDirectorySize(di);
                        DirDataCollection[i].Count = di.GetFiles().Length + di.GetDirectories().Length;
                    }
                    else
                    {
                        DirDataCollection[i].Remove = true;
                    }
                }
                else
                {
                    DirDataCollection[i].Name = System.IO.Path.GetFileName(path);
                    System.IO.FileInfo fi = new System.IO.FileInfo(path);
                    DirDataCollection[i].Size = (Int64)fi.Length;
                    DirDataCollection[i].LastWriteTime = fi.LastWriteTime;

                }
            }


            for (int i = 0; i < n; i++)
            {
                if (!DirDataCollection[i].isDir)
                {
                    string path = DirDataCollection[i].Path;
                    string Extension = System.IO.Path.GetExtension(path);
                    if (Extension == ".jpg")
                    {
                        if (DirDataCollection[i].Name.IndexOf("-1.") > -1)
                        {
                            string FileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(path);
                            string newName = FileNameWithoutExtension.Substring(0, FileNameWithoutExtension.Length - 2) + Extension;

                            for (int j = 0; j < n; j++)
                            {
                                if (DirDataCollection[j].Name == newName && Math.Abs(DirDataCollection[j].Size - DirDataCollection[i].Size) < 30000)
                                {
                                    if (getMetadata(DirDataCollection[i].Path) == getMetadata(DirDataCollection[j].Path))
                                    {
                                        if (ExifManager.getExif(DirDataCollection[i].Path) == ExifManager.getExif(DirDataCollection[j].Path))
                                        {
                                            DirDataCollection[i].Remove = true;
                                            break;

                                        }
                                    }

                                }
                            }
                        }

                    }

                }
            }

            // DataGridに設定する
            mainDG.ItemsSource = DirDataCollection;
            RemoveCheckedCount();
        }

        private string getMetadata(string path)
        {
            Uri uri = new Uri(path, UriKind.Relative);
            BitmapFrame frame = BitmapFrame.Create(uri);
            BitmapMetadata metadata = frame.Metadata as BitmapMetadata;

            string result = "";
            result += "ApplicationName:" + metadata.ApplicationName;
            result += ", Author:" + metadata.Author;
            result += ", CameraManufacturer:" + metadata.CameraManufacturer;
            result += ", CameraModel:" + metadata.CameraModel;
            result += ", Comment:" + metadata.Comment;
            result += ", Copyright:" + metadata.Copyright;
            result += ", DateTaken:" + metadata.DateTaken;
            result += ", Format:" + metadata.Format;
            result += ", Keywords:" + metadata.Keywords;
            result += ", Location:" + metadata.Location;
            result += ", Rating:" + metadata.Rating;
            result += ", Subject:" + metadata.Subject;
            result += ", Title:" + metadata.Title;

            return result;
        }

        
        private void removeBtn_Click(object sender, RoutedEventArgs e)
        {
            int n = DirDataCollection.Count;
            for (int i = 0; i < n; i++)
            {
                if (DirDataCollection[i].Remove)
                {
                    string path = DirDataCollection[i].Path;
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    else if(File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                
            }
            openDir();
        }
        
        public static long GetDirectorySize(DirectoryInfo dirInfo)
        {
            long size = 0;

            //フォルダ内の全ファイルの合計サイズを計算する
            foreach (FileInfo fi in dirInfo.GetFiles())
                size += fi.Length;

            //サブフォルダのサイズを合計していく
            foreach (DirectoryInfo di in dirInfo.GetDirectories())
                size += GetDirectorySize(di);

            //結果を返す
            return size;
        }

        private void pathTB_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string path = pathTB.Text;
            openDir(path);
        }

        private void openDir(string path)
        {
            if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start(path);
            }

        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            DirData dirData = (sender as CheckBox).Tag as DirData;
            dirData.Remove = (bool)(sender as CheckBox).IsChecked;

            RemoveCheckedCount();
        }

        private void RemoveCheckedCount()
        {
            int count = 0;
            int n = DirDataCollection.Count;
            for (int i = 0; i < n; i++)
            {
                if (DirDataCollection[i].Remove)
                {
                    count++;
                }

            }
            RemoveCountLabel.Content = "RemoveCount: " + count;
            removeBtn.IsEnabled = count > 0;
        }

        private void Dir_Click(object sender, RoutedEventArgs e)
        {
            // Dir欄がクリックされた場合、エクスプローラー上でフォーカスされる
            DirData dirData = (sender as Label).Tag as DirData;
            if (dirData.isDir)
            {
                string dir = @"/select,";
                dir += dirData.Path;
                System.Diagnostics.Process.Start("EXPLORER.EXE", dir);
            }
            else
            {
                string dir = @"/select,";
                dir += dirData.Path;
                System.Diagnostics.Process.Start("EXPLORER.EXE", dir);
            }
        }

        private void Name_Click(object sender, RoutedEventArgs e)
        {
            // Name欄がクリックされた場合、直接開く
            DirData dirData = (sender as Label).Tag as DirData;
            if (dirData.isDir)
            {
                System.Diagnostics.Process.Start(dirData.Path);
            }
            else
            {
                System.Diagnostics.Process.Start(dirData.Path);
            }
        }
    }
}

class DirData
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool Remove { get; set; }
    private bool _isDir;
    public Int64 Size { get; set; }
    public string Dir { get; set; }
    private int _count;
    public DateTime LastWriteTime { get; set; }

    public int Count
    {
        get { return _count; }
        set
        {
            _count = value;
            setDir();
        }
    }

    public string SizeStr
    {
        get { return Size.ToString("n0"); }
    }

    public bool isDir
    {
        get { return _isDir; }
        set { _isDir = value; setDir(); }
    }

    private void setDir()
    {
        if (isDir)
        {
            this.Dir = "Dir(" + _count + ")";
        }
        else
        {
            this.Dir = "File";
        }
    }

}

