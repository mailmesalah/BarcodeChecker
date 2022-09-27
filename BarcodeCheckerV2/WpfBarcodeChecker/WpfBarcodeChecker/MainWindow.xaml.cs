using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfBarcodeChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        

        private const int HOTKEY_ID = 9000;

        //Modifiers:
        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS
        //CAPS LOCK:
        private const uint VK_CAPITAL = 0x14;

        List<string> barcodes = new List<string>();
        private static bool bStopExecution = false;
        private static bool doubleBarcode = false;


        private IntPtr _windowHandle;
        private HwndSource _source;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_CONTROL, (uint)System.Windows.Forms.Keys.F1.GetHashCode() ); //CTRL + F1
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_CONTROL, (uint)System.Windows.Forms.Keys.F2.GetHashCode()); //CTRL + F2
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);

                            if (vkey == System.Windows.Forms.Keys.F1.GetHashCode())
                            {
                                bStopExecution = false;
                                System.Windows.Forms.SendKeys.SendWait("\n");
                                foreach (String barcode in barcodes)
                                {
                                    try
                                    {
                                        if (bStopExecution)
                                        {
                                            break;
                                        }
                                        System.Windows.Forms.SendKeys.SendWait(barcode + "\n");
                                        Thread.Sleep(Int32.Parse(textBoxSeconds.Text.Trim()) * 1000);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Data);
                                    }
                                }
                                if (bStopExecution)
                                {
                                    MessageBox.Show("Paste Canceled!");
                                }
                                else
                                {
                                    MessageBox.Show("Paste Completed!");
                                }

                            }
                            else if (vkey == System.Windows.Forms.Keys.F2.GetHashCode())
                            {
                                bStopExecution = true;
                            }
                                handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }

        public MainWindow()
        {
            InitializeComponent();
            //DateTime d = DateTime.Now;
            //if (new DateTime(2017, 1, 30).CompareTo(d) < 0)
            //{
            //    this.Close();
            //}

            listBoxBarcodes.ItemsSource = barcodes;
            labelCount.Content = barcodes.Count;
        }

        private void textBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Add to the Grid
                if (!doubleBarcode)
                {
                    if (textBoxInput.Text.Trim().Length < 7)
                    {
                        doubleBarcode = true;
                    }
                    else
                    {
                        if (!textBoxInput.Text.Trim().Equals(""))
                        {
                            textBoxInput.Text = textBoxInput.Text.Replace(".", "");

                            if (!barcodes.Contains(textBoxInput.Text.Trim()))
                            {
                                barcodes.Insert(0, textBoxInput.Text.Trim());                                
                                labelCount.Content = barcodes.Count;
                                listBoxBarcodes.Items.Refresh();
                            }
                        }

                        textBoxInput.Text = "";
                    }
                }
                else
                {
                    if (!textBoxInput.Text.Trim().Equals(""))
                    {
                        textBoxInput.Text = textBoxInput.Text.Replace(".", "");

                        if (!barcodes.Contains(textBoxInput.Text.Trim()))
                        {
                            barcodes.Insert(0, textBoxInput.Text.Trim());                           
                            labelCount.Content = barcodes.Count;
                            listBoxBarcodes.Items.Refresh();
                        }
                    }

                    textBoxInput.Text = "";

                    doubleBarcode = false;
                }
                
            }
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxBarcodes.SelectedIndex > -1)
            {
                barcodes.Remove((String)listBoxBarcodes.SelectedItem);
                listBoxBarcodes.Items.Refresh();
                labelCount.Content = barcodes.Count;
            }
        }

        private void buttonCopyAll_Click(object sender, RoutedEventArgs e)
        {
            string data = "";
            foreach (string barcode in barcodes)
            {
                data = data + barcode + Environment.NewLine;
            }

            Clipboard.SetText(data);

            MessageBox.Show("Copied To Clipboard!");
        }

        private void buttonRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            barcodes.Clear();
            listBoxBarcodes.Items.Refresh();
            labelCount.Content = barcodes.Count;
        }
   

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Do you want to Close", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
                
        }

        private void textBoxHost_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                int host = 0, drive = 0;
                Int32.TryParse(textBoxDrive.Text.ToString(), out host);
                Int32.TryParse(textBoxHost.Text.ToString(), out drive);
                Console.WriteLine(host + " " + drive);
                labelProduct.Content = (host * drive) + "";
            }
            catch
            {

            }            
            
        }

        private void textBoxDrive_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                int host = 0, drive = 0;
                Int32.TryParse(textBoxDrive.Text.ToString(), out host);
                Int32.TryParse(textBoxHost.Text.ToString(), out drive);
                Console.WriteLine(host + " " + drive);
                labelProduct.Content = (host * drive) + "";
            }
            catch
            {

            }
            
        }

        private void textBoxInput_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
