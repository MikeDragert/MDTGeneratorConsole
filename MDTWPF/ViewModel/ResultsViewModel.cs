using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MDTWPF.ViewModel
{   //Seperate viewmodel for the results window
    //  this is pretty simple, maybe didn't have to be it's own viewmodel
    public class ResultsViewModel : INotifyPropertyChanged
    {
        private string _results = "";

        public string Results {
            get {
                return _results;
            }
            set {
                _results = value;
                OnPropertyChanged("Results");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
