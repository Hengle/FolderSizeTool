using SGF;
using SGF.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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


namespace FolderSizeTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DirItem m_current;

        public MainWindow()
        {
            InitializeComponent();
            InitConfig();
            InitDebuger();
            
        }

        public void InitDebuger()
        {
            Debuger.Init(AppDomain.CurrentDomain.BaseDirectory + "/Log/", new SystemColorConsole());
            Debuger.EnableLog = AppPrefs.GetInt("EnableLog", 1) > 0;
#if DEBUG
            Debuger.EnableLog = true;
#endif
            Debuger.EnableDate = true;
            Debuger.EnableLogVerbose = false;
            Debuger.EnableSave = true;
            Debuger.EnableErrorStack = false;
            Debuger.LogWarning("AppDomain:{0}", AppDomain.CurrentDomain.FriendlyName);
            Debuger.LogWarning("WorkDir:{0}", Directory.GetCurrentDirectory());
        }

        public void InitConfig()
        {
            AppPrefs.Init("./AppConfig.json");
            cbBaseDir.Text = AppPrefs.GetString("cbBaseDir","");

            List<SortType> listTypes = new List<SortType>();
            listTypes.Add(SortType.从大到小);
            listTypes.Add(SortType.从小到大);

            cbSortType.ItemsSource = listTypes;

            cbSortType.SelectedIndex = AppPrefs.GetInt("cbSortType", 0);
        }

        private void btnCalc_Click(object sender, RoutedEventArgs e)
        {
            var dirPath = cbBaseDir.Text;
            if (!Directory.Exists(dirPath))
            {
                Debuger.LogError("目录不存在:{0}", dirPath);
                return;
            }
            
            AppPrefs.SetString("cbBaseDir", cbBaseDir.Text);
            AppPrefs.Save();

            m_current = new DirItem(new DirectoryInfo(dirPath));
            m_current.CalcSize();
            m_current.Sort((SortType)cbSortType.SelectedValue);

            List<DirItem> list = new List<DirItem>();
            list.Add(m_current);
            tvDirTree.ItemsSource = list;
        }

        private void TvDirTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = tvDirTree.SelectedItem as FileSystemItem;
            lbDirChildren.ItemsSource = item.Children;
        }

        private void cbSortType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppPrefs.SetInt("cbSortType", cbSortType.SelectedIndex);
            if (m_current != null)
            {
                m_current.Sort((SortType)cbSortType.SelectedValue);
                List<DirItem> list = new List<DirItem>();
                list.Add(m_current);
                tvDirTree.ItemsSource = list;
            }
        }
    }
}
