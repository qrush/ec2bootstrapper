using System;
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

        public InstanceList()
        {
            this.InitializeComponent();
            _instances = new CBeginInvokeOC<CEc2Instance>(this.Dispatcher);
        }

        public ObservableCollection<CEc2Instance> instances
        {
            get
            {
                if (_instances.Count == 0)
                {
                    _dashboard.showStatus(ConstantString.ContactAmazon);
                    Thread oThread = new Thread(new ThreadStart(getInstances));
                    instancesLV.BorderThickness = new Thickness(0);
                    oThread.Start();
                    _dashboard.enableProgressBar();
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
                    CEc2Service serv = new CEc2Service(_dashboard.awsConfig);
                    List<CEc2Instance> list = serv.describeInstances();
                    foreach (CEc2Instance inst in list)
                        _instances.Add(inst);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            _dashboard.Dispatcher.Invoke(new StopProgressbarCallback(_dashboard.disableProgressBar));
            this.Dispatcher.Invoke(new SetBorderThickNessCallback(setBorderThickNess));
        }

        void remoteConnect_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = (ContextMenu)ContextMenu.ItemsControlFromItemContainer((MenuItem)e.OriginalSource);
            string header = ((Expander)cm.PlacementTarget).Header.ToString();

            string publicDns = CEc2Instance.getPublicDnsFromHeader(header);

            System.Diagnostics.ProcessStartInfo procStartInfo =
                new System.Diagnostics.ProcessStartInfo("mstsc.exe ", "/v:" + publicDns);

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
            string header = ((Expander)cm.PlacementTarget).Header.ToString();

            CEc2Instance ins = getInstance(CEc2Instance.getInsanceIdFromHeader(header));
            PasswordPrompt pw = new PasswordPrompt();
            pw.instance = ins;
            pw.Show();
        }

        void password_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContextMenu cm = (ContextMenu)ContextMenu.ItemsControlFromItemContainer((MenuItem)e.OriginalSource);
                string header = ((Expander)cm.PlacementTarget).Header.ToString();

                CEc2Instance ins = getInstance(CEc2Instance.getInsanceIdFromHeader(header));
                if (ins != null)
                {
                    string pw = ins.getAdministratorPassord();
                    if (string.IsNullOrEmpty(pw) == true)
                        pw = "not available.";
                    MessageBox.Show("Password is " + pw);
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
                string header = ((Expander)cm.PlacementTarget).Header.ToString();

                string instanceId = CEc2Instance.getInsanceIdFromHeader(header);
                if (!string.IsNullOrEmpty(instanceId))
                {
                    CEc2Instance ins = getInstance(instanceId);
                    if (ins != null)
                    {
                        ins.reboot();
                        removeInstance(instanceId);
                    }
                    else
                    {
                        MessageBox.Show("No instance available with id " + instanceId);
                    }
                }
                else
                {
                    MessageBox.Show("No instance id available");
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
                string header = ((Expander)cm.PlacementTarget).Header.ToString();

                MessageBoxResult result = MessageBox.Show(
                    "You are about to terminate the selected instance. Are you sure you want to continue?",
                    "Terminate Instance",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);
                if (result == MessageBoxResult.No)
                    return;

                string instanceId = CEc2Instance.getInsanceIdFromHeader(header);
                if (!string.IsNullOrEmpty(instanceId))
                {
                    CEc2Service serv = new CEc2Service(_dashboard.awsConfig);
                    serv.terminate(instanceId);
                }
                else
                {
                    MessageBox.Show("No instance id available");
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
                string header = ((Expander)cm.PlacementTarget).Header.ToString();

                CEc2Instance inst = getInstance(CEc2Instance.getInsanceIdFromHeader(header));
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
                if (!CEc2Instance.isWindowsPlatform(header))
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

        private void instancesLV_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (Border b in lstBorders)
            {
                b.Width = instancesLV.ActualWidth - 15;
            }
        }

        private void border_Loaded(object sender, RoutedEventArgs e)
        {
            Border border = (Border)sender;
            border.Width = instancesLV.ActualWidth - 15;
            lstBorders.Add(border);
        }

        private void instancesLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //enable/disable deploy menu based on the image id
            CEc2Instance inst = instancesLV.SelectedItem as CEc2Instance;
            if (inst != null)
            {
                if (string.Compare(inst.imageId, CEc2Instance.deployableAmiImageId, true) == 0)
                {
                    _dashboard.DeployMenu.IsEnabled = true;
                }
                else
                {
                    _dashboard.DeployMenu.IsEnabled = false;
                }
            }
        }
    }
}
