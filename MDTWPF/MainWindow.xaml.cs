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
using System.IO;
using MDTGenerators;
using System.Collections.ObjectModel;
using System.Collections;
using MDTWPF.ViewModel;
using System.Threading;
using System.Collections.Specialized;
using System.Reflection;
using System.ComponentModel;

namespace MDTWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GeneratorMain main = null;
        GeneratorListViewModel generatorListViewModel = new GeneratorListViewModel();

        public MainWindow() {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            DataContext = generatorListViewModel;
            AddRefreshActionToChangeOfDataSetGridItemsChanged();
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e) {
        }

        private void AddRefreshActionToChangeOfDataSetGridItemsChanged(){
            CollectionView myCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(DataSetGrid.Items);
            ((INotifyCollectionChanged)myCollectionView).CollectionChanged += new NotifyCollectionChangedEventHandler(RefreshGriddWhenPropertyIsChanged);
        }
        private void RefreshGriddWhenPropertyIsChanged(object sender, EventArgs e) {
            ForceDataGridRefresh();
        }

        private void ForceDataGridRefresh() {
            //need a way to force refresh of the datagrid.  this seems to work.
            DataSetGrid.AutoGenerateColumns = false;
            DataSetGrid.AutoGenerateColumns = true;
        }
    }
}
