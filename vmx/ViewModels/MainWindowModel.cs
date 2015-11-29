using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using vmx.Helper;
using vmx.Model;

namespace vmx.ViewModels
{
    public class MainWindowModel : BaseViewModel
    {

        #region MergeCommand

        public ICommand MergeCommand { get { return new RelayCommand(DoMerge); } }

        private void DoMerge(object e)
        {
            if (DoCheck())
            {
                IsMerging = true;
                StatusName = Properties.Resources.Merging;

                string O = OfileName;
                string A = AfileName;
                string B = BfileName;
                string C = CfileName;

                Task.Factory
                    .StartNew(() => VersionMergeExample.DoMerge(O, A, B, C))
                    .ContinueWith(tx => TaskCompleted(), TaskScheduler.FromCurrentSynchronizationContext())
                    .ContinueWith(tx => Thread.Sleep(3000))
                    .ContinueWith(tx => { StatusName = Properties.Resources.RunMerge; }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void TaskCompleted()
        {
            IsMerging = false;
            StatusName = Properties.Resources.TaskComplete;
        }

        private bool DoCheck()
        {
            bool fileOK = !string.IsNullOrEmpty(OfileName) && File.Exists(OfileName);
            if (!fileOK)
            {
                ShowErrrow(Properties.Resources.errOfileNotFound);
                return false;
            }

            fileOK = !string.IsNullOrEmpty(AfileName) && File.Exists(AfileName);
            if (!fileOK)
            {
                ShowErrrow(Properties.Resources.errAfileNotFound);
                return false;
            }

            fileOK = !string.IsNullOrEmpty(BfileName) && File.Exists(BfileName);
            if (!fileOK)
            {
                ShowErrrow(Properties.Resources.errBfileNotFound);
                return false;
            }

            fileOK = !string.IsNullOrEmpty(CfileName) && (!File.Exists(CfileName) || RewriteResult);
            if (!fileOK)
            {
                ShowErrrow(Properties.Resources.errCfileNotFound);
                return false;
            }

            int q =
                (from s in new string[] { OfileName, AfileName, BfileName, CfileName }
                 select s).Distinct().Count();
            if (q < 4)
            {
                ShowErrrow(Properties.Resources.errNamesIsEquals);
                return false;
            }

            return true;
        }

        private void ShowErrrow(string v)
        {
            MessageBox.Show(v, Properties.Resources.errMessageBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion


        #region BrowseCommand

        public ICommand BrowseCommand { get { return new RelayCommand(DoBrowse); } }

        private void DoBrowse(object e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All Files|*.*";
            if (ofd.ShowDialog() == true)
            {
                switch ((string)e)
                {
                    case "O":
                        OfileName = ofd.FileName;
                        break;
                    case "A":
                        AfileName = ofd.FileName;
                        break;
                    case "B":
                        BfileName = ofd.FileName;
                        break;
                    case "C":
                        CfileName = ofd.FileName;
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion
        
        
        // Исходная версия файла
        public string OfileName
        {
            get { return (string)GetValue(OfileNameProperty); }
            set { SetValue(OfileNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OfileName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OfileNameProperty =
            DependencyProperty.Register("OfileName", typeof(string), typeof(MainWindowModel), new PropertyMetadata(string.Empty));


        // Первая измененная версия
        public string AfileName
        {
            get { return (string)GetValue(AfileNameProperty); }
            set { SetValue(AfileNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AfileName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AfileNameProperty =
            DependencyProperty.Register("AfileName", typeof(string), typeof(MainWindowModel), new PropertyMetadata(string.Empty));


        // Вторая ихмененная весия
        public string BfileName
        {
            get { return (string)GetValue(BfileNameProperty); }
            set { SetValue(BfileNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BfileName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BfileNameProperty =
            DependencyProperty.Register("BfileName", typeof(string), typeof(MainWindowModel), new PropertyMetadata(string.Empty));


        // Результат слияния
        public string CfileName
        {
            get { return (string)GetValue(CfileNameProperty); }
            set { SetValue(CfileNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CfileName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CfileNameProperty =
            DependencyProperty.Register("CfileName", typeof(string), typeof(MainWindowModel), new PropertyMetadata(string.Empty));


        // Перезаписывать файл результата слияния.
        public bool RewriteResult
        {
            get { return (bool)GetValue(RewriteResultProperty); }
            set { SetValue(RewriteResultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RewriteResult.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RewriteResultProperty =
            DependencyProperty.Register("RewriteResult", typeof(bool), typeof(MainWindowModel), new PropertyMetadata(false));


        // Флаг выполнения слияния
        public bool IsMerging
        {
            get { return (bool)GetValue(IsMergingProperty); }
            set { SetValue(IsMergingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMerging.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMergingProperty =
            DependencyProperty.Register("IsMerging", typeof(bool), typeof(MainWindowModel), new PropertyMetadata(false));


        // Наименование текущего состояния
        public string StatusName
        {
            get { return (string)GetValue(StatusNameProperty); }
            set { SetValue(StatusNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusNameProperty =
            DependencyProperty.Register("StatusName", typeof(string), typeof(MainWindowModel), new PropertyMetadata(Properties.Resources.RunMerge));


    }
}
