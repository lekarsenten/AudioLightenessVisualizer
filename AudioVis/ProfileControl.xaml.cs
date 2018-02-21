using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace AudioVis
{
    /// <summary>
    /// Interaction logic for ProfileControl.xaml
    /// </summary>
    public enum ProfileAction
    {
        Create,
        Delete,
        Apply
    }
    public partial class ProfileControl : UserControl, INotifyPropertyChanged
    {

        public event EventHandler<Tuple<ProfileAction, byte>> CheckBoxClicked;
        public event PropertyChangedEventHandler PropertyChanged;

        public byte ProfileNumber
        {
            get => _profileNumber; set
            {
                _profileNumber = value;
                if (File.Exists(String.Format("{0}.xml", value)))
                {
                    Profile_cb.Checked -= CheckBox_Checked;
                    Profile_cb.IsChecked = true;
                    Profile_cb.Checked += CheckBox_Checked;
                }
                OnPropertyChanged("ProfileNumber");
            }
        }

        private byte _profileNumber;

        public ProfileControl()
        {
            InitializeComponent();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBoxClicked?.Invoke(this, new Tuple<ProfileAction, byte>(ProfileAction.Create, ProfileNumber));
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBoxClicked?.Invoke(this, new Tuple<ProfileAction, byte>(ProfileAction.Delete, ProfileNumber));
        }

        private void CheckBox_Click(object sender, MouseButtonEventArgs e)
        {
            CheckBoxClicked?.Invoke(this, new Tuple<ProfileAction, byte>(ProfileAction.Apply, ProfileNumber));
        }
    }
}
