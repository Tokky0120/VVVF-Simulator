using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VVVF_Simulator.GUI.TaskViewer
{
    /// <summary>
    /// TaskViewer_Main.xaml の相互作用ロジック
    /// </summary>
    public partial class TaskViewer_Main : Window
    {
        public TaskViewer_Main()
        {
            InitializeComponent();

            DataContext = MainWindow.taskProgresses;

            runUpdateTask();
        }

        public bool updateGridTask = true;
        public void runUpdateTask()
        {
            Task task = Task.Run(() =>
            {
                while (updateGridTask)
                {
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            TaskView.Items.Refresh();
                        });
                    }
                    catch
                    {
                        break;
                    }

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

            List<MainWindow.TaskProgressData> taskProgresses = MainWindow.taskProgresses;
            for (int i = 0; i < taskProgresses.Count; i++)
            {
                MainWindow.TaskProgressData data = taskProgresses[i];
                if (data.Task.Id.ToString().Equals(tag.ToString()))
                {
                    data.progressData.Cancel = true;
                    break;
                }
            }
        }
    }
}
