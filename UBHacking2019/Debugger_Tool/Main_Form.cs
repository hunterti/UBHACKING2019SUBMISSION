using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Debugger_Tool
{
    public partial class Main_Form : Form
    {
        private ArrayList files_loaded;
        private Image unallocatedImg = Image.FromFile("sprite_red.png");
        private Image allocatedImg = Image.FromFile("sprite_green.png");
        Label ThreadIn = new Label();
        Label ThreadOut = new Label();
        Label ThreadInMapping = null;
        Label ThreadOutMapping = null;
        bool canAdd = true;
        int curBlue = 66;
        int curRed = 66;
        private TextBox inputValue;
        private Button submit;
        private Button run;
        private RichTextBox debugger;
        private Dictionary<string, Dictionary<string,ArrayList>> AvailableMethods;
        private Dictionary<string, Assembly> assemblies;
        private ComboBox assembly_drop = new ComboBox();
        private ComboBox class_drop = new ComboBox();
        private Dictionary<Label, Label_Extension> Selection_Parents;
        private FlowLayoutPanel options;
        private bool aLabelHasBeenSelected;
        private Color selectionBackReturnColor;
        private Color selectionForeReturnColor;
        private Label selectedLabel;
        private int width, height;
        private int label_width = 500;
        private string current_assembly;
        public Main_Form()
        {
            InitializeComponent();
            // NEVER FORGET 
            //this.TopMost = true;
            current_assembly = null;
            files_loaded = new ArrayList();
            width = SystemInformation.PrimaryMonitorSize.Width;
            height = SystemInformation.PrimaryMonitorSize.Height;
            this.WindowState = FormWindowState.Maximized;
            AvailableMethods = new Dictionary<string, Dictionary<string,ArrayList>>();
            assemblies = new Dictionary<string, Assembly>();
            Selection_Parents = new Dictionary<Label, Label_Extension>();
            AddDebugger();
            AddSelectFileButton();
            AddDropdowns();
            AddThreadinThreadOut();
            AddInputValue();
            AddFlowLayout();
            AddRunButton();
        }

        private void AddRunButton()
        {
            run = new Button();
            run.Text = "RUN";
            run.BackColor = Color.Black;
            run.ForeColor = Color.White;
            run.Location = new Point(3 * width / 4, height - 300);
            run.Width = width / 5;
            run.Height = 220;
            run.Click += ExecuteThread;
            this.Controls.Add(run);
        }

        private void AddDebugger()
        {
            debugger = new RichTextBox();
            debugger.Location = new Point(width / 4, height - 300);
            debugger.Width = width / 2;
            debugger.Height = 220;
            debugger.BackColor = Color.Black;
            debugger.ForeColor = Color.Green;
            debugger.BorderStyle = BorderStyle.FixedSingle;
            debugger.ReadOnly = true;
            this.Controls.Add(debugger);
        }

        private void AddInputValue()
        {
            inputValue = new TextBox();
            inputValue.Location = new Point(width / 2 - 100, 10);
            inputValue.Width = 450;
            inputValue.Height = 200;
            inputValue.Font = new Font(FontFamily.GenericSansSerif, 12);
            this.Controls.Add(inputValue);
            inputValue.Visible = false;
            submit = new Button();
            submit.Text = "Submit";
            submit.Visible = false;
            submit.Location = new Point(width / 2 + 350, 10);
            submit.MouseClick += SubmitButtonHandler;
            submit.Width = 100;
            submit.Height = 35;
            this.Controls.Add(submit);
        }

        private void SubmitButtonHandler(object sender, MouseEventArgs e)
        {
            try
            {
                Type t = Type.GetType(selectedLabel.Text.ToString());
                object obj;
                if (t.IsEnum)
                    obj = Enum.Parse(t, inputValue.Text.ToString());
                else
                    obj = Convert.ChangeType(inputValue.Text.ToString(), t);
                Selection_Parents[selectedLabel].value_mappings.Add(selectedLabel, obj);
                debugger.AppendText($"\r\n\r\nYou inputted {inputValue.Text.ToString()} and it was properly setup as type {t.Name}!");
                selectedLabel.BackColor = Color.MediumPurple;
            } catch (Exception exception) 
            {
                debugger.AppendText($"\r\n\r\n\r\nFailed to cast {inputValue.Text.ToString()} to type! {exception.ToString()}");
                selectedLabel.BackColor = selectionBackReturnColor;
                selectedLabel.ForeColor = selectionForeReturnColor;
            }
            selectedLabel = null;
            submit.Visible = false;
            aLabelHasBeenSelected = false;
            inputValue.Visible = false;
            inputValue.Clear();
        }

        private void AddThreadinThreadOut()
        {
            Image img = Image.FromFile("sprite_red.png");
            ThreadIn.Image = unallocatedImg;
            ThreadOut.Image = unallocatedImg;
            ThreadIn.Text = "Output Thread";
            ThreadOut.Text = "Input Thread";
            ThreadIn.Width = 100;
            ThreadOut.Width = 100;
            ThreadIn.Height = 100;
            ThreadOut.Height = 100;
            ThreadIn.Location = new Point(label_width + 75, height / 2 - 50);
            ThreadOut.Location = new Point(width - 125, height / 2 - 50);
            ThreadIn.Click += ThreadInOut_Click;
            ThreadOut.Click += ThreadInOut_Click;
            this.Controls.Add(ThreadIn);
            this.Controls.Add(ThreadOut);
        }

        private void ThreadInOut_Click(object sender, EventArgs e)
        {
            Label lab = (Label)sender;
            if (selectedLabel == null)
                lab.Text = lab.Text;
            else if (selectedLabel.Text.ToString() == "Output Thread" && ThreadInMapping != null)
                lab.Text = lab.Text;
            else if (selectedLabel.Text.ToString() == "Input Thread" && ThreadOutMapping != null)
                lab.Text = lab.Text;
            else if (selectedLabel.Text.ToString() == lab.Text.ToString())
            {
                selectedLabel.BackColor = selectionBackReturnColor;
                selectedLabel.ForeColor = selectionForeReturnColor;
                selectedLabel = null;
                aLabelHasBeenSelected = false;
                inputValue.Visible = false;
                inputValue.Clear();
            }
            else if (selectedLabel.Text.ToString() == "Output Thread" && lab.Text.ToString() == "Input Thread" || selectedLabel.Text.ToString() == "Input Thread" && lab.Text.ToString() == "Output Thread")
            {
                lab.Image = allocatedImg;
                
                selectedLabel.BackColor = Color.DarkGreen;
                selectedLabel.ForeColor = Color.White;
                Selection_Parents[selectedLabel].current_mappings.Add(selectedLabel, lab);
                if (selectedLabel.Text.ToString() == "Output Thread")
                    ThreadInMapping = selectedLabel;
                else
                    ThreadOutMapping = selectedLabel;
                selectedLabel = null;
                aLabelHasBeenSelected = false;
                inputValue.Visible = false;
                inputValue.Clear();
                submit.Visible = false;
            }
           
        }

        private void AddDropdowns()
        {
            assembly_drop = generateBox(assemblies.Keys.ToList(), 
                                        new Point(10, 65), 
                                        label_width, 
                                        25, 
                                        ComboBoxStyle.DropDownList, 
                                        Assembly_drop_SelectedIndexChanged);
            this.Controls.Add(assembly_drop);
            class_drop = generateBox(assemblies.Keys.ToList(), 
                                    new Point(10, 105), 
                                    label_width, 
                                    25, 
                                    ComboBoxStyle.DropDownList, 
                                    Class_drop_SelectedIndexChanged);
            this.Controls.Add(class_drop);
        }

        private ComboBox generateBox(List<string> source, Point location, int width, int height, ComboBoxStyle style, EventHandler method) 
        {
            ComboBox comboBox = new ComboBox();
            comboBox.DataSource = source;
            comboBox.Location = location;
            comboBox.Width = width;
            comboBox.Height = height;
            comboBox.DropDownStyle = style;
            comboBox.SelectedIndexChanged += method;
            return comboBox;
        }

        private void Assembly_drop_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayAssemblyTypes(((ComboBox)sender).SelectedItem.ToString());
        }

        private void DisplayAssemblyTypes(string assem)
        {
            current_assembly = assem;
            class_drop.DataSource = AvailableMethods[current_assembly].Keys.ToList();
        }

        private void Class_drop_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayTypeMethods(((ComboBox)sender).SelectedItem.ToString());
        }

        private void AddFlowLayout()
        {
            options = new FlowLayoutPanel();
            options.Location = new Point(10, 150);
            options.Width = label_width + 50;
            options.Height = height - 225;
            options.AutoScroll = true;
            this.Controls.Add(options);
        }

        public void DisplayTypeMethods(string type) 
        {
            options.Controls.Clear();
            foreach(MethodInfo info in AvailableMethods[current_assembly][type]) 
            {
                Label temp = new Label();
                temp.Width = label_width;
                temp.Height = 50;
                temp.BorderStyle = BorderStyle.FixedSingle;
                temp.Text = info.Name;
                temp.Font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
                temp.TextAlign = ContentAlignment.MiddleCenter;
                temp.MouseDoubleClick += Temp_MouseDoubleClick;
                options.Controls.Add(temp);
            }
        }

        private void Temp_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (canAdd) {
                ArrayList curType = AvailableMethods[current_assembly][class_drop.SelectedItem.ToString()];
                MethodInfo info = null;
                List<string> ins = new List<string>();
                List<string> outs = new List<string>();
                foreach (MethodInfo inf in curType)
                {
                    if (inf.Name == ((Label)sender).Text.ToString())
                        info = inf;
                }
                foreach (ParameterInfo inf in info.GetParameters())
                    ins.Add(inf.ParameterType.ToString());
                if (info.ReturnType.ToString() != "System.Void")
                    outs.Add(info.ReturnType.ToString());

                int num_inputs = info.GetParameters().Length;

                Label_Extension temp = new Label_Extension(((Label)sender).Text.ToString(), ins, outs, 300, 30 * num_inputs + 30, label_width + 150, 10, Color.White, Color.Black, info);
                this.Controls.Add(temp);
                temp.Click += GetInformation;
                foreach (Label child in temp.childLabels)
                {
                    Selection_Parents.Add(child, temp);
                    child.MouseClick += Child_MouseClick;
                    Controls.Add(child);
                }
            }
        }

        private void GetInformation(object sender, EventArgs e)
        {
            Label_Extension label = (Label_Extension)sender;
            debugger.AppendText("\r\n\r\n");
            debugger.AppendText($"Parameter Inputs: {string.Join(',', label.inputs)}");
            if (label.outputs.Count > 0)
                debugger.AppendText($"\r\nOutput Type: {label.outputs[0]}");
            List<Label> allocated = label.current_mappings.Keys.ToList<Label>();
            List<string> temp = allocated.Select(lab => lab.Name).ToList();
            debugger.AppendText($"\r\nCurrently mapped inputs and outputs: {string.Join(',', temp)}");
            string toMap = string.Join(',', label.childLabels.Where(lab => !(label.current_mappings.ContainsKey(lab) || label.value_mappings.ContainsKey(lab))).Select(lab => lab.Text.ToString()).ToList());
            debugger.AppendText($"\r\nPLEASE MAP THESE VALUES BY CLICKING ON THEIR PARAMETER AND AN ASSOCIATION OF THEIR TYPE {toMap}");
        }

        private void Child_MouseClick(object sender, MouseEventArgs e)
        {
            Label lab = (Label)sender;
            string name = lab.Text.ToString();
            if (!aLabelHasBeenSelected)
            {
                if (Selection_Parents[lab].value_mappings.ContainsKey(lab)) { }
                else if (Selection_Parents[lab].current_mappings.ContainsKey(lab)) { }
                else if (name == "Output Thread" || name == "Input Thread")
                {
                    selectedLabel = lab;
                    aLabelHasBeenSelected = true;
                    selectionBackReturnColor = lab.BackColor;
                    selectionForeReturnColor = lab.ForeColor;
                    lab.BackColor = Color.Blue;
                    lab.ForeColor = Color.White;
                }
                else
                {
                    selectedLabel = lab;
                    aLabelHasBeenSelected = true;
                    selectionBackReturnColor = lab.BackColor;
                    selectionForeReturnColor = lab.ForeColor;
                    lab.BackColor = Color.Blue;
                    lab.ForeColor = Color.White;
                    inputValue.Visible = true;
                    submit.Visible = true;
                }
            }
            else 
            {
                string selected_name = selectedLabel.Text.ToString();
                if (selectedLabel == lab)
                {
                    selectedLabel.BackColor = selectionBackReturnColor;
                    selectedLabel.ForeColor = selectionForeReturnColor;
                }
                else if ((name == "Output Thread" || name == "Input Thread") && (selected_name == "Output Thread" || selected_name == "Input Thread"))
                {
                    if (name != selected_name)
                    {
                        selectedLabel.BackColor = Color.Green;
                        lab.BackColor = Color.Green;
                        selectedLabel.ForeColor = Color.White;
                        lab.ForeColor = Color.White;
                        Selection_Parents[lab].current_mappings.Add(lab, selectedLabel);
                        Selection_Parents[selectedLabel].current_mappings.Add(selectedLabel, lab);
                    }
                    else
                    {
                        selectedLabel.BackColor = selectionBackReturnColor;
                        selectedLabel.ForeColor = selectionForeReturnColor;
                    }
                }
                else if (name == selected_name) 
                {
                    selectedLabel.BackColor = Color.Green;
                    lab.BackColor = Color.Green;
                    selectedLabel.ForeColor = Color.White;
                    lab.ForeColor = Color.White;
                }
                selectedLabel = null;
                aLabelHasBeenSelected = false;
                submit.Visible = false;
                inputValue.Clear();
                inputValue.Visible = false;
            }
        }
        public void ExecuteThread(object sender, EventArgs e)
        {
            if (!ValidateStartToEnd())
                debugger.AppendText("\r\n\r\nCurrently Missing Paths Or Parameters!");
            else
            {
                debugger.AppendText("\r\nAll Paths and Parameters Setup Correctly!");
                Dictionary<Label, ArrayList> completed_results = new Dictionary<Label, ArrayList>();
                Label cur_label = ThreadOutMapping;
                while (cur_label != ThreadOut)
                {
                    Label_Extension parent = Selection_Parents[cur_label];
                    MethodInfo info = parent.methodInfo;
                    Type type = info.DeclaringType;
                    
                    var temp = Activator.CreateInstance(type);
                    object[] args = new object[parent.methodInfo.GetParameters().Length];
                    for (int i = 1; i < args.Length + 1; i++) 
                    {
                        Label label = parent.childLabels[i];
                        if (parent.current_mappings.ContainsKey(label))
                            args[i-1] = completed_results[label];
                        else
                            args[i-1] = parent.value_mappings[label];
                    }
                    type.InvokeMember(parent.Text.ToString(), BindingFlags.InvokeMethod, null, temp, args);
                    debugger.AppendText($"\r\nSuccess on execution of {parent.Text.ToString()}");
                    cur_label = parent.current_mappings.Values.ToList().FirstOrDefault(label => label.Text.ToString() == "Input Thread");
                }
            }
        }
        public bool ValidateStartToEnd() 
        {
            Label first_to_execute = ThreadInMapping;
            Label last_to_execute = ThreadOutMapping;
            if (first_to_execute == null || last_to_execute == null)
                return false;
            Label cur_label = first_to_execute;
            while (cur_label != ThreadOut) 
            {
                Label_Extension parent = Selection_Parents[cur_label];
                int mapped_inputs = 0;
                for (int i = 1; i < parent.inputs.Count + 1; i++) 
                {
                    if (parent.current_mappings.ContainsKey(parent.childLabels[i]))
                        mapped_inputs++;
                    else if (parent.value_mappings.ContainsKey(parent.childLabels[i]))
                        mapped_inputs++;
                }

                if (parent.methodInfo.GetParameters().Length != mapped_inputs)
                {
                    debugger.AppendText("\r\n MISMATCH OF ALLOCATED PARAMETERS. PLEASE CHECK ALL PARAMETERS ARE NOT THEIR ORIGINAL COLOR FOR INVOKED METHODS");
                    debugger.AppendText($"\r\n ERROR ON CLASS {parent.Name}, METHOD {cur_label.Text.ToString()}");
                    return false;
                }
                if (!parent.current_mappings.ContainsKey(cur_label))
                    return false;
                cur_label = parent.current_mappings[cur_label];
            }
            return true;
        }
        public void AddSelectFileButton() 
        {
            Button button = new Button();
            button.Location = new Point(10, 10);
            button.Width = 250;
            button.Height = 35;
            button.Font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Italic);
            button.Text = "Import a DLL";
            button.Click += Select_File_Button_Click;
            this.Controls.Add(button);
        }
        public void Select_File_Button_Click(object sender, EventArgs e)
        {
            string filename = PromptUserSelectFile();
            if (filename != null)
                LoadDataFromFile(filename);
            else
                NotifyUserError(new string[] { "User Must Select a File to Add" });
        }

        public void LoadDataFromFile(string filename)
        {
            files_loaded.Add(filename);
            if (current_assembly == null)
                current_assembly = filename;
            if (!AvailableMethods.ContainsKey(filename))
            {
                AvailableMethods.Add(filename, new Dictionary<string, ArrayList>());
                Assembly assembly = Assembly.LoadFrom(filename);
                assemblies.Add(filename, assembly);
                
                foreach (Type type in assembly.GetTypes())
                {
                    AvailableMethods[filename].Add(type.FullName, new ArrayList());
                    AvailableMethods[filename][type.FullName].AddRange(type.GetMethods());
                    if (type.Name == "Main_Form")
                    {
                        //var temp = Activator.CreateInstance(type);
                        //type.InvokeMember("PromptUserSelectFile", BindingFlags.InvokeMethod, null, temp, null);
                        current_assembly = filename;
                        DisplayTypeMethods(type.FullName);
                    }
                }
                assembly_drop.DataSource = assemblies.Keys.ToList();
                class_drop.DataSource = AvailableMethods[current_assembly].Keys.ToList();
            }
        }

        public string PromptUserSelectFile()
        {
            OpenFileDialog prompt = new OpenFileDialog();
            prompt.Filter = "*.dll|*.dll";
            prompt.Multiselect = false;
            prompt.Title = "Please Select DLL";
            prompt.CheckFileExists = true;
            prompt.CheckPathExists = true;
            if (prompt.ShowDialog() == DialogResult.OK)
            {
                return prompt.FileName;
            }
            else 
            {
                return null;
            }
        }
        private void NotifyUserError(string[] err) 
        {
            MessageBox.Show(string.Join("\n", err), "Error Occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
