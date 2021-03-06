﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ec2Bootstrapperlib;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;

namespace Ec2BootstrapperGUI
{
    /// <summary>
    /// Interaction logic for ListInstances.xaml
    /// </summary>
    public partial class InstanceList
    {
        private List<Border> lstBorders = new List<Border>();
        Dashboard _dashboard;
        CBeginInvokeOC<CEc2Instance> _instances;
        bool fectchingInstance = false;

        public InstanceList()
        {
            this.InitializeComponent();
            _instances = new CBeginInvokeOC<CEc2Instance>(this.Dispatcher);
        }

        public ObservableCollection<CEc2Instance> instances
        {
            get
            {
                lock (_instances)
                {
                    if (fectchingInstance == false)
                    {
                        fectchingInstance = true;

                        if (_instances.Count == 0)
                        {
                            _dashboard.showStatus(ConstantString.ContactAmazon);
                            Thread oThread = new Thread(new ThreadStart(getInstances));
                            instancesLV.BorderThickness = new Thickness(0);
                            oThread.Start();
                            _dashboard.enableProgressBar();
                        }
                    }
                }
                return _instances;
            }
        }

        void setBorderThickNess()
        {
            if (_instances.Count != 0)
            {
                instancesLV.BorderThickness = new Thickness(1);
                _dashboard.showStatus(ConstantString.Done);
            }
            else
            {
                _dashboard.showStatus(ConstantString.NoInstance);
            }
        }

        public delegate void SetBorderThickNessCallback();

        public delegate void StopProgressbarCallback();

        public void getInstances()
        {
            try
            {
                if (_instances.Count == 0)
                {
                    CEc2Service serv = new CEc2Service();
                    List<CEc2Instance> list = serv.describeInstances();
                    foreach (CEc2Instance inst in list)
                    {
                        _instances.Add(inst);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            _dashboard.Dispatcher.Invoke(new StopProgressbarCallback(_dashboard.disableProgressBar));
            this.Dispatcher.Invoke(new SetBorderThickNessCallback(setBorderThickNess));
            fectchingInstance = false;
        }

        void remoteConnect_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = (ContextMenu)ContextMenu.ItemsControlFromItemContainer((MenuItem)e.OriginalSource);
            CEc2Instance inst = (CEc2Instance)((FrameworkElement)(((Panel)(cm.PlacementTarget)).Children[0])).DataContext;

            System.Diagnostics.ProcessStartInfo procStartInfo =
                new System.Diagnostics.ProcessStartInfo("mstsc.exe ", "/v:" + inst.publicDns);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = procStartInfo;
            process.Start();
        }

        public Dashboard dashboard
        {
            set { _dashboard = value; }
        }

        void deploy_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = (ContextMenu)ContextMenu.ItemsControlFromItemContainer((MenuItem)e.OriginalSource);
            CEc2Instance ins = (CEc2Instance)((FrameworkElement)(((Panel)(cm.PlacementTarget)).Children[0])).DataContext;
            if (ins != null)
            {
                AppDeployment pw = new AppDeployment(_dashboard);
                pw.instance = ins;
                pw.Show();
            }
        }

        void password_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContextMenu cm = (ContextMenu)ContextMenu.ItemsControlFromItemContainer((MenuItem)e.OriginalSource);
                CEc2Instance ins = (CEc2Instance)((FrameworkElement)(((Panel)(cm.PlacementTarget)).Children[0])).DataContext;
                if (ins != null)
                {
                    //check key file 
                    string keyFile = CAwsConfig.Instance.getKeyFilePath(ins.keyPairName);
                    if (string.IsNullOrEmpty(keyFile) == true ||
                        File.Exists(keyFile) == false)
                    {
                        KeyFileInputDlg kfInput = new KeyFileInputDlg(ins.keyPairName);
                        kfInput.ShowDialog();
                        keyFile = CAwsConfig.Instance.getKeyFilePath(ins.keyPairName);
                        if (string.IsNullOrEmpty(keyFile) == true ||
                            File.Exists(keyFile) == false)
                        {
                            MessageBox.Show("Cannot find the key file.", "Get Password", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    FetchPassword pw = new FetchPassword(ins);
                    pw.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Cannot fetch the password for this instance.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void reboot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContextMenu cm = (ContextMenu)ContextMenu.ItemsControlFromItemContainer((MenuItem)e.OriginalSource);
                CEc2Instance ins = (CEc2Instance)((FrameworkElement)(((Panel)(cm.PlacementTarget)).Children[0])).DataContext;
                if (ins != null)
                {
                    ins.reboot();
                    removeInstance(ins.instanceId);
                }
                else
                {
                    MessageBox.Show("No instance available");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void terminate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContextMenu cm = (ContextMenu)ContextMenu.ItemsControlFromItemContainer((MenuItem)e.OriginalSource);
                CEc2Instance ins = (CEc2Instance)((FrameworkElement)(((Panel)(cm.PlacementTarget)).Children[0])).DataContext;

                MessageBoxResult result = MessageBox.Show(
                    "You are about to terminate the selected instance. Are you sure you want to continue?",
                    "Terminate Instance",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);
                if (result == MessageBoxResult.No)
                    return;

                if (!string.IsNullOrEmpty(ins.instanceId))
                {
                    CEc2Service serv = new CEc2Service();
                    serv.terminate(ins.instanceId);
                }
                else
                {
                    MessageBox.Show("No instance ID available");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        CEc2Instance getInstance(string instanceId)
        {
            foreach (CEc2Instance ins in _instances)
            {
                if (ins.instanceId == instanceId)
                    return ins;
            }
            return null;
        }

        void removeInstance(string instanceId)
        {
            foreach (CEc2Instance ins in _instances)
            {
                if (ins.instanceId == instanceId)
                {
                    _instances.Remove(ins);
                    break;
                }
            }
        }

        private void contextMenu_Opened(object sender, RoutedEventArgs e)
        {
            try
            {
                ContextMenu cm = (ContextMenu)e.OriginalSource;
                CEc2Instance inst = (CEc2Instance)((FrameworkElement)(((Panel)(cm.PlacementTarget)).Children[0])).DataContext;
                if (string.Compare(inst.status, "running") != 0)
                {
                    for (int i = 0; i < cm.Items.Count; ++i)
                    {
                        MenuItem item = cm.Items[i] as MenuItem;
                        if (item != null)
                        {
                            item.IsEnabled = false;
                        }
                    }
                    return;
                }

                //remote connect is not available for non windows system
                if (string.Compare(inst.platform, "windows", true) == 0)
                {
                    for (int i = 0; i < cm.Items.Count; ++i)
                    {
                        MenuItem item = cm.Items[i] as MenuItem;
                        if (item != null)
                        {
                            if (string.Compare(item.Header.ToString(), "Remote Connect") == 0)
                            {
                                item.IsEnabled = false;
                                break;
                            }
                        }
                    }
                }

                //deployment is not available for any other image id
                if (string.Compare(inst.imageId, CEc2Instance.deployableAmiImageId, true) != 0)
                {
                    for (int i = 0; i < cm.Items.Count; ++i)
                    {
                        MenuItem item = cm.Items[i] as MenuItem;
                        if (item != null)
                        {
                            if (string.Compare(item.Header.ToString(), "Deploy") == 0)
                            {
                                item.IsEnabled = false;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //private void instancesLV_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    foreach (Border b in lstBorders)
        //    {
        //        b.Width = instancesLV.ActualWidth - 15;
        //    }
        //}

        //private void border_Loaded(object sender, RoutedEventArgs e)
        //{
        //    Border border = (Border)sender;
        //    border.Width = instancesLV.ActualWidth - 15;
        //    lstBorders.Add(border);
        //}

        private void instancesLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //enable/disable deploy menu based on the image id
            CEc2Instance inst = instancesLV.SelectedItem as CEc2Instance;
            if (inst != null)
            {
                if (string.Compare(inst.imageId, CEc2Instance.deployableAmiImageId, true) == 0)
                {
                    if (string.Compare(inst.status, "running") == 0)
                    {
                        _dashboard.DeployMenu.IsEnabled = true;
                        return;
                    }
                }
                _dashboard.DeployMenu.IsEnabled = false;
            }
        }
    }
}
