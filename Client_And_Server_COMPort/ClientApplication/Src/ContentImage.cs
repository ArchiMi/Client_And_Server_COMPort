using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClientAppNameSpace.Src
{
    public class Command : ICommand
    {
        public Command(Action action)
        {
            this.action = action;
        }

        Action action;

        EventHandler canExecuteChanged;
        event EventHandler ICommand.CanExecuteChanged
        {
            add { canExecuteChanged += value; }
            remove { canExecuteChanged -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action();
        }
    }

    // В классе Content реализуем интерфейс INotifyPropertyChanged
    // Чтобы послать нотификацию о том, что свойство Image поменялось
    public class Content : INotifyPropertyChanged
    {
        public Content()
        {
            // Инициализация команды
            openFileDialogCommand = new Command(ExecuteOpenFileDialog);
            // Инициализация OpenFileDialog
            openFileDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "Image files (*.BMP, *.JPG, *.GIF, *.TIF, *.PNG, *.ICO, *.EMF, *.WMF)|*.bmp;*.jpg;*.gif; *.tif; *.png; *.ico; *.emf; *.wmf"
            };
        }

        readonly OpenFileDialog openFileDialog;

        // Наша картинка
        public ImageSource Image { get; private set; }

        readonly ICommand openFileDialogCommand;
        public ICommand OpenFileDialogCommand { get { return openFileDialogCommand; } }

        // Действие при нажатии на кнопку "Open File Dialog"
        void ExecuteOpenFileDialog()
        {
            if (openFileDialog.ShowDialog() == true)
            {
                using (var stream = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    Image = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    RaisePropertyChanged("Image");
                }
            }
        }

        // Реализация интерфейса INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
