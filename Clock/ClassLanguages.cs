using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;





// Namespace
namespace Clock
{





    // Klasse der Sprachen
    class ClassLanguages
    {





        // Variablen
        public string name { get; set; }
        public string code { get; set; }
        public string translator { get; set; }


        // Der Liste hinzufügen
        public ClassLanguages(string name, string code, string translator)
        {
            this.name = name;
            this.code = code;
            this.translator = translator;
        }


        // Für Collections
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
