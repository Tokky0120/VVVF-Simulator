using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static VVVF_Simulator.MainWindow;

namespace VVVF_Simulator.GUI.TaskViewer
{
    /// <summary>
    /// TaskViewer_Main.xaml の相互作用ロジック
    /// </summary>
    public partial class TaskViewer_Main : Window
    {

        MainWindow mainWindow;
        public TaskViewer_Main(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;

            DataContext = mainWindow.taskProgresses;

            runUpdateTask();
        }

        public bool updateGridTask = true;
        public void runUpdateTask()
        {
            Task task = Task.Run(() =>
            {
                while (updateGridTask)
                {
                    Dispatcher.Invoke(() =>
                    {
                        TaskView.Items.Refresh();
                    });

                    Thread.Sleep(500);
                }
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            updateGridTask = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Object tag = btn.Tag;
            if (tag == null) return;

            List<TaskProgressData> taskProgresses = mainWindow.taskProgresses;
            for (int i = 0; i < taskProgresses.Count; i++)
            {
                TaskProgressData data = taskProgresses[i];
                if (data.Task.Id.ToString().Equals(tag.ToString()))
                {
                    data.progressData.Cancel = true;
                    break;
                }
            }
        }
    }
}
