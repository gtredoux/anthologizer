using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AnthologizerClient.AnthologizerService;
using com.renoster.Anthologizer.Media;

namespace AnthologizerClient
{
    public partial class MainForm : Form
    {
        private Anthologizer anthologizer;
 
        public MainForm()
        {
            InitializeComponent();

            myDataGrid.MouseWheel += myDataGrid_MouseWheel;
            myDataGrid.DoubleClick += myDataGrid_DoubleClick;

            anthologizer = new Anthologizer();
 
            anthologizer.EventNewItems += anthologizer_EventNewItems;
            anthologizer.EventMediaStarted += anthologizer_EventMediaStarted;
            anthologizer.EventMediaStopped += anthologizer_EventMediaStopped;
            anthologizer.EventError += anthologizer_EventError;

            this.Closed += MainForm_Closed;
        }

        void MainForm_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            Cleanup();
        }

        private void Cleanup()
        {
            anthologizer.Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            anthologizer.Start();

            if (myAnthologyName.Text.Trim() == String.Empty)
                myAnthologyName.Text = "Anthology-" + DateTime.Now.ToString();

            myAnthologyCount.Text = "";
            anthologizer.Anthology.Name = myAnthologyName.Text;

            if (myAnthologySection.Items.Count > 0)
                myAnthologySection.SelectedIndex = 0;

            anthologizer.Anthology.OnUpdate += anthology_OnUpdate;

            anthologizer.Connect(URL);
        }

        void anthologizer_EventError(Anthologizer a, string error, Exception ex)
        {
            if (InvokeRequired)
            {
                Invoke(new Anthologizer.ErrorEvent(anthologizer_EventError), a, error, ex);
                return;
            }
            Message(error + " " + ((ex == null) ? "" : ex.Message + " " + ex.StackTrace));
        }

        void anthologizer_EventMediaStopped(Anthologizer a, int index, Item item)
        {
            Message(item.Name  + " " + "stopped");

            //myDataGrid.DataSource = null;
            //myDataGrid.Rows.Clear();         
        }

        void anthologizer_EventMediaStarted(Anthologizer a, int index, Item item)
        {
            if (InvokeRequired)
            {
                Invoke(new Anthologizer.MediaStartedEvent(this.anthologizer_EventMediaStarted), a, index, item);
                return;
            }

            if (index >= 0)
                myDataGrid.Rows[index].Selected = false;

            myDataGrid.Rows[index].Selected = true;
            if (myDataGrid.Rows[index].Cells.Count > 0)
                myDataGrid.CurrentCell = myDataGrid.Rows[index].Cells[0];
            Message("Playing" + " " + item.Name);
        }

        void anthologizer_EventNewItems(Anthologizer a, Record newItems)
        {
            if (InvokeRequired)
            {
                Invoke(new Anthologizer.NewItemsEvent(this.anthologizer_EventNewItems), a, newItems);
                return;
            }

            List(newItems);
        }

        private void List(Record r)
        {
            DisplayPath(r.Path);

            myDataGrid.DataSource = r.Contents;

            if (myDataGrid.Columns.Count > 0)
                myDataGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            if (myDataGrid.Columns.Count > 1)
                myDataGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            if (myDataGrid.Columns.Count > 2)
            {
                myDataGrid.Columns[2].ReadOnly = false;
                myDataGrid.CellContentClick += myDataGrid_CellContentClick; // essential even if weird
            }

            myDataGrid.Focus();
        }

        void anthology_OnUpdate(Anthology thisAnthology)
        {
            if (InvokeRequired)
            {
                Invoke(new Anthology.UpdatedEvent(this.anthology_OnUpdate), thisAnthology);
                return;
            }
            this.myAnthologyCount.Text = thisAnthology.Count.ToString();
        }
      
        private void myDataGrid_MouseWheel(object sender, MouseEventArgs e)
        {
            int currentIndex = this.myDataGrid.FirstDisplayedScrollingRowIndex;
            int scrollLines = SystemInformation.MouseWheelScrollLines;

            if (e.Delta > 0)
            {
                this.myDataGrid.FirstDisplayedScrollingRowIndex = Math.Max(0, currentIndex - scrollLines);
            }
            else if (e.Delta < 0)
            {
                this.myDataGrid.FirstDisplayedScrollingRowIndex = currentIndex + scrollLines;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void Exit()
        {
            Close();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            anthologizer.Connect(URL);

            myDataGrid.Focus();
        }

        void DisplayPath(string path)
        {
            this.Text = path.Replace('\\','/');
        }

        void myDataGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // do this to make the click turn into a value change
            // pick that value change up of the object itself and not the cell
            // which fires multiple times for some reason
            myDataGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private string URL
        {
            get { return myURL.Text; }
        }

        private void Message(string msg)
        {
            if (msg.Length < 160)
                myStatus.Text = msg;
            else
            {
                MessageBox.Show(msg);
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void Clear()
        {
            Message("");
            myDataGrid.DataSource = null;
            myDataGrid.Rows.Clear();
        }

        private void myBackBtn_Click(object sender, EventArgs e)
        {
            anthologizer.Back();
        }

        private void myStopBtn_Click(object sender, EventArgs e)
        {
            anthologizer.StopPlaying();
        }

        private void myNextBtn_Click(object sender, EventArgs e)
        {
            anthologizer.PlayNext();
        }

        private void myAnthologyName_TextChanged(object sender, EventArgs e)
        {
            anthologizer.Anthology.Name = myAnthologyName.Text;
        }

        private void myAnthologySection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (anthologizer.Anthology != null)
                anthologizer.Anthology.Section = myAnthologySection.Text;
        }

        private void myDataGrid_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (myDataGrid.SelectedRows == null || myDataGrid.SelectedRows.Count <= 0)
                    return;

                ItemSelector itemUI = (ItemSelector)myDataGrid.SelectedRows[0].DataBoundItem;
                Item item = itemUI.GetItem();

                anthologizer.PlayMedia(myDataGrid.SelectedRows[0].Index, item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void myURL_TextChanged(object sender, EventArgs e)
        {

        }

        private void powerBtn_Click(object sender, EventArgs e)
        {
            Exit();
        }

 
    }
}
