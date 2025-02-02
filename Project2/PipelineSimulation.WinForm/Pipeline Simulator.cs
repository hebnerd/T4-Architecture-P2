﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using PipelineSimulation.Core;
using System.Diagnostics;
using PipelineSimulation.Core.Instructions;
using System.Threading;
using PipelineSimulation.Core.Caches;
using System.Collections.Concurrent;

namespace PipelineSimulation.WinForm
{
    public partial class PipelineSimulator : Form
    {
        //these attributes could be used in a method/class to create all needed
        //objects & threads to start the simulation
        CPU CPU1;
        CPU CPU2;
        CPU CPU3;
        CPU CPU4;

        Thread Thread1;
        Thread Thread2;
        Thread Thread3;
        Thread Thread4;

        // https://www.codeproject.com/Articles/5273783/Implementing-a-Thread-Safe-Message-Queue-in-Csharp
        ConcurrentQueue<Message> _messagesC1 = new ConcurrentQueue<Message>();
        SemaphoreSlim _messagesAvailableC1 = new SemaphoreSlim(0);

        ConcurrentQueue<Message> _messagesC2 = new ConcurrentQueue<Message>();
        SemaphoreSlim _messagesAvailableC2 = new SemaphoreSlim(0);

        ConcurrentQueue<Message> _messagesC3 = new ConcurrentQueue<Message>();
        SemaphoreSlim _messagesAvailableC3 = new SemaphoreSlim(0);

        ConcurrentQueue<Message> _messagesC4 = new ConcurrentQueue<Message>();
        SemaphoreSlim _messagesAvailableC4 = new SemaphoreSlim(0);

        Memory MainMem;

        L3 L31;
        L3 L32;

        string core1FilePath = null;
        string core2FilePath = null;
        string core3FilePath = null;
        string core4FilePath = null;

        int coreNumber = 1;

        public PipelineSimulator()
        {
            InitializeComponent();
            nextClockBtn.Enabled = false;   //cannot go to next clock without loading program
            initialCoreSetup();

        }

        // This is the work that each thread will accomplish
        public void DoWork(Object obj) {
            //Waiting for next clock?
            CPU cpu = (CPU)obj;

            //handle all the things
            while (!cpu.endReached) {

                if(cpu == CPU1) {
                    //Wait for button click
                    // wait until a message becomes available
                    _messagesAvailableC1.Wait();
                    Message msg;
                    // process messages
                    // we use Try here because we're multithreaded
                    // so it's possible that between the Wait() call
                    // and the dequeue call the queue may be cleared
                    if (_messagesC1.TryDequeue(out msg)) {
                        cpu.NextClockCycle();
                    }
                }

                if (cpu == CPU2) {
                    //Wait for button click
                    // wait until a message becomes available
                    _messagesAvailableC2.Wait();
                    Message msg;
                    // process messages
                    // we use Try here because we're multithreaded
                    // so it's possible that between the Wait() call
                    // and the dequeue call the queue may be cleared
                    if (_messagesC2.TryDequeue(out msg)) {
                        cpu.NextClockCycle();
                    }
                }

                if (cpu == CPU3) {
                    //Wait for button click
                    // wait until a message becomes available
                    _messagesAvailableC3.Wait();
                    Message msg;
                    // process messages
                    // we use Try here because we're multithreaded
                    // so it's possible that between the Wait() call
                    // and the dequeue call the queue may be cleared
                    if (_messagesC3.TryDequeue(out msg)) {
                        cpu.NextClockCycle();
                    }
                }

                if (cpu == CPU4) {
                    //Wait for button click
                    // wait until a message becomes available
                    _messagesAvailableC4.Wait();
                    Message msg;
                    // process messages
                    // we use Try here because we're multithreaded
                    // so it's possible that between the Wait() call
                    // and the dequeue call the queue may be cleared
                    if (_messagesC4.TryDequeue(out msg)) {
                        cpu.NextClockCycle();
                    }
                }

            }
        }

        private void Begin_Click(object sender, EventArgs e)
        {
            if (ConfigIsValid() == true)
            {
                //begin simulation, spawn the correct number of CPU classes/threads & pass into them the appropriate file paths

                // Before creating CPUs, create an instance of Memory.
                MainMem = Memory.GetInstance();

                // Also, create the appropriate number of L3 caches
                L31 = new L3(4); //4-way set assoc
                if (coreNumber == 4) L32 = new L3(4);

                // Create each CPU instance that is needed, then pass file path to Reader and open it for each cpu

                if (coreNumber >= 1) { //Set up core 1
                    CPU1 = new CPU(L31);
                    String s = String.Format("{0,-15} {1,-15} {2,-15} {3,-15} {4,-15} {5,-15}\n\n", "Fetch", "Decode", "MemRead", "Execute", "MemWrite", "RegWrite");
                    core1Box.Text = s;
                    CPU1.Rd.fileStr = core1FilePath;
					CPU1.Rd.OpenFile();
                    List<string> dispProgCode = new List<string>();
                    foreach (ushort x in CPU1.Rd.proMem)
                    {
                        string output = "";

                        //get opcode
                        ushort op = (ushort)(x >> 11);
                        Instruction tempInst = CPU1.CreateInstructionInstance(op);
                        output += tempInst.ToString() + " ";

                        //get rest
                        ushort remainder = (ushort)(0b_0000_0111_1111_1111 & x); //bit mask for removing opcode
                        output += tempInst.ToText(remainder);

                        core1ProgBox.Text += output + "\n";
                        //dispProgCode.Add(output);
                    }

                    //Do work
                    Thread1 = new Thread(DoWork);
                    Thread1.Start(CPU1);

				}
                if (coreNumber >= 2) { //Set up core 2
                    CPU2 = new CPU(L31);

                    CPU2.Rd.fileStr = core2FilePath;
                    CPU2.Rd.OpenFile();

                    //Do work in Thread2
                    Thread2 = new Thread(DoWork);
                    Thread2.Start(CPU2);

                }
                if (coreNumber >= 3) { //Set up core 3
                    if (coreNumber == 4) CPU3 = new CPU(L32);
                    else CPU3 = new CPU(L31);

                    CPU3.Rd.fileStr = core3FilePath;
                    CPU3.Rd.OpenFile();

                    //Do work in Thread3
                    Thread3 = new Thread(DoWork);
                    Thread3.Start(CPU3);

                }
                if (coreNumber == 4) { //Set up core 4
                    CPU4 = new CPU(L32);

                    CPU4.Rd.fileStr = core4FilePath;
                    CPU4.Rd.OpenFile();

                    //Do work in Thread4
                    Thread4 = new Thread(DoWork);
                    Thread4.Start(CPU4);

                }

                nextClockBtn.Enabled = true;
            }
            else
                MessageBox.Show("Please open files for all enabled cores", "Pipeline Simulator");
        }

		private void initialCoreSetup()
        {
            this.coreNumber = 1;
            Core1ItemsVisible();
            Core2ItemsHide();
            Core3ItemsHide();
            Core4ItemsHide();
        }

		#region core vis

		private void Core1ItemsVisible()
        {
            core1Box.Visible = true;
            core1Label.Visible = true;
            core1ProgBox.Visible = true;
            core1ProgLabel.Visible = true;
        }

        private void Core1ItemsHide()
        {
            core1Box.Visible = false;
            core1Label.Visible = false;
            core1ProgBox.Visible = false;
            core1ProgLabel.Visible = false;
        }

        private void Core2ItemsVisible()
        {
            core2Box.Visible = true;
            core2Label.Visible = true;
            core2ProgBox.Visible = true;
            core2ProgLabel.Visible = true;
        }

        private void Core2ItemsHide()
        {
            core2Box.Visible = false;
            core2Label.Visible = false;
            core2ProgBox.Visible = false;
            core2ProgLabel.Visible = false;
        }

        private void Core3ItemsVisible()
        {
            core3Box.Visible = true;
            core3Label.Visible = true;
            core3ProgBox.Visible = true;
            core3ProgLabel.Visible = true;
        }

        private void Core3ItemsHide()
        {
            core3Box.Visible = false;
            core3Label.Visible = false;
            core3ProgBox.Visible = false;
            core3ProgLabel.Visible = false;
        }

        private void Core4ItemsVisible()
        {
            core4Box.Visible = true;
            core4Label.Visible = true;
            core4ProgBox.Visible = true;
            core4ProgLabel.Visible = true;
        }

        private void Core4ItemsHide()
        {
            core4Box.Visible = false;
            core4Label.Visible = false;
            core4ProgBox.Visible = false;
            core4ProgLabel.Visible = false;
        }

		#endregion

		#region core change

		//Change cores to 1
		private void changeCores_1(object sender, EventArgs e)
        {
            this.coreNumber = 1;
            Core1ItemsVisible();
            Core2ItemsHide();
            Core3ItemsHide();
            Core4ItemsHide();
            this.Size = new Size(984, 544);
        }

        //Change cores to 2
        private void changeCores_2(object sender, EventArgs e)
        {
            this.coreNumber = 2;
            Core1ItemsVisible();
            Core2ItemsVisible();
            Core3ItemsHide();
            Core4ItemsHide();
            this.Size = new Size(1910, 544);
        }

        //Change cores to 3
        private void changeCores_3(object sender, EventArgs e)
        {
            this.coreNumber = 3;
            Core1ItemsVisible();
            Core2ItemsVisible();
            Core3ItemsVisible();
            Core4ItemsHide();
            this.Size = new Size(1910, 912);
        }

        //Change cores to 4
        private void changeCores_4(object sender, EventArgs e)
        {
            this.coreNumber = 4;
            Core1ItemsVisible();
            Core2ItemsVisible();
            Core3ItemsVisible();
            Core4ItemsVisible();
            this.Size = new Size(1910, 912);
        }

		#endregion

		//Closes program
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            while(MessageBox.Show("Exit simulator?", "Pipeline Simulator", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            Application.Exit();
        }


        //maybe using a sleep function would be more useful in a multithreaded environment. Maybe this could be a functionality to pause/play
        private void nxtClockBtn_Click(object sender, EventArgs e)
        {
            string output;
            // Signal threads to do next work

            // https://www.codeproject.com/Articles/5273783/Implementing-a-Thread-Safe-Message-Queue-in-Csharp

            // Core 1

            // Print to screen
            //Console.Write($"{ins} ");
            string fetchoutput = "";
            foreach(var ins in CPU1.Buffers[0].FetchedInstructions) {
                fetchoutput += $"{ins}";
			}

            string decodeOutput = "";
            foreach(var ins in CPU1.Buffers[1].FetchedInstructions) {
                decodeOutput += $"{ins}";
			}

            string memReadOutput = "";
            foreach(var ins in CPU1.Buffers[2].DecodedInstructions) {
                memReadOutput += $"{ins}";
			}

            string executeOutput = "";
            foreach(var ins in CPU1.Buffers[3].DecodedInstructions) {
                executeOutput += $"{ins}";
			}

            string memWrite = "";
            foreach(var ins in CPU1.Buffers[4].DecodedInstructions) {
                memWrite += $"{ins}";
			}

            string regWriteOutput = "";
            foreach(var ins in CPU1.Buffers[5].DecodedInstructions) {
                regWriteOutput += $"{ins}";
			}

            output = String.Format("{0,-15} {1,-15} {2,-15} {3,-15} {4,-15} {5,-15}\n", fetchoutput, decodeOutput, memReadOutput, executeOutput, memWrite, memReadOutput);
            core1Box.Text += output + "\n";

            // Core 2
            if(coreNumber >= 2) {
                // Print to screen
                //Console.Write($"{ins} ");
                fetchoutput = "";
                foreach (var ins in CPU2.Buffers[0].FetchedInstructions) {
                    fetchoutput += $"{ins}";
                }

                decodeOutput = "";
                foreach (var ins in CPU2.Buffers[1].FetchedInstructions) {
                    decodeOutput += $"{ins}";
                }

                memReadOutput = "";
                foreach (var ins in CPU2.Buffers[2].DecodedInstructions) {
                    memReadOutput += $"{ins}";
                }

                executeOutput = "";
                foreach (var ins in CPU2.Buffers[3].DecodedInstructions) {
                    executeOutput += $"{ins}";
                }

                memWrite = "";
                foreach (var ins in CPU2.Buffers[4].DecodedInstructions) {
                    memWrite += $"{ins}";
                }

                regWriteOutput = "";
                foreach (var ins in CPU2.Buffers[5].DecodedInstructions) {
                    regWriteOutput += $"{ins}";
                }

                output = String.Format("{0,-15} {1,-15} {2,-15} {3,-15} {4,-15} {5,-15}\n", fetchoutput, decodeOutput, memReadOutput, executeOutput, memWrite, memReadOutput);
                core2Box.Text += output + "\n";
            }

            // Core 3
            if (coreNumber >= 3) {
                // Print to screen
                //Console.Write($"{ins} ");
                fetchoutput = "";
                foreach (var ins in CPU3.Buffers[0].FetchedInstructions) {
                    fetchoutput += $"{ins}";
                }

                decodeOutput = "";
                foreach (var ins in CPU3.Buffers[1].FetchedInstructions) {
                    decodeOutput += $"{ins}";
                }

                memReadOutput = "";
                foreach (var ins in CPU3.Buffers[2].DecodedInstructions) {
                    memReadOutput += $"{ins}";
                }

                executeOutput = "";
                foreach (var ins in CPU3.Buffers[3].DecodedInstructions) {
                    executeOutput += $"{ins}";
                }

                memWrite = "";
                foreach (var ins in CPU3.Buffers[4].DecodedInstructions) {
                    memWrite += $"{ins}";
                }

                regWriteOutput = "";
                foreach (var ins in CPU3.Buffers[5].DecodedInstructions) {
                    regWriteOutput += $"{ins}";
                }

                output = String.Format("{0,-15} {1,-15} {2,-15} {3,-15} {4,-15} {5,-15}\n", fetchoutput, decodeOutput, memReadOutput, executeOutput, memWrite, memReadOutput);
                core3Box.Text += output + "\n";
            }

            // Core 4
            if (coreNumber >= 4) {
                // Print to screen
                //Console.Write($"{ins} ");
                fetchoutput = "";
                foreach (var ins in CPU4.Buffers[0].FetchedInstructions) {
                    fetchoutput += $"{ins}";
                }

                decodeOutput = "";
                foreach (var ins in CPU4.Buffers[1].FetchedInstructions) {
                    decodeOutput += $"{ins}";
                }

                memReadOutput = "";
                foreach (var ins in CPU4.Buffers[2].DecodedInstructions) {
                    memReadOutput += $"{ins}";
                }

                executeOutput = "";
                foreach (var ins in CPU4.Buffers[3].DecodedInstructions) {
                    executeOutput += $"{ins}";
                }

                memWrite = "";
                foreach (var ins in CPU4.Buffers[4].DecodedInstructions) {
                    memWrite += $"{ins}";
                }

                regWriteOutput = "";
                foreach (var ins in CPU4.Buffers[5].DecodedInstructions) {
                    regWriteOutput += $"{ins}";
                }

                output = String.Format("{0,-15} {1,-15} {2,-15} {3,-15} {4,-15} {5,-15}\n", fetchoutput, decodeOutput, memReadOutput, executeOutput, memWrite, memReadOutput);
                core4Box.Text += output + "\n";
            }

            //MessageBox.Show($"{CPU1.Buffers[0].FetchedInstructions.Count}");

            // pass the increment message
            _messagesC1.Enqueue(new Message());
            // signal messages available
            _messagesAvailableC1.Release(1);

            if (coreNumber >= 2) {
                // pass the increment message
                _messagesC2.Enqueue(new Message());
                // signal messages available
                _messagesAvailableC2.Release(1);
            }

            if (coreNumber >= 3) {
                // pass the increment message
                _messagesC3.Enqueue(new Message());
                // signal messages available
                _messagesAvailableC3.Release(1);
            }

            if (coreNumber >= 4) {
                // pass the increment message
                _messagesC4.Enqueue(new Message());
                // signal messages available
                _messagesAvailableC4.Release(1);
            }

        }

        private string openFile() {
            var filePath = string.Empty;
            
            using(OpenFileDialog ofd = new OpenFileDialog()) {
                if(ofd.ShowDialog() == DialogResult.OK) {
                    return ofd.FileName;
				}
			}

            return null;

		}


        private void core1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.core1FilePath = openFile();
        }

        private void core2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.core2FilePath = openFile();
        }

        private void core3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.core3FilePath = openFile();
        }

        private void core4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.core4FilePath = openFile();
        }

        private bool ConfigIsValid()
        {
            switch (this.coreNumber)
            {
                case 1:
                    if(core1FilePath != null)
                    {
                        return true;
                    }
                    return false;
                case 2:
                    if(core2FilePath != null && core1FilePath != null)
                    {
                        return true;
                    }
                    return false;
                case 3:
                    if(core3FilePath != null && core2FilePath != null && core3FilePath != null)
                    {
                        return true;
                    }
                    return false;
                case 4:
                    if(core4FilePath != null && core3FilePath != null && core2FilePath != null && core1FilePath != null)
                    {
                        return true;
                    }
                    return false;
            }

            return false;
        }

        //TODO Remove this - it is only here to test the formatted string
        private void button3_Click(object sender, EventArgs e)
        {
            String s = String.Format("{0,-15} {1,-15} {2,-15} {3,-15} {4,-15} {5,-15}\n\n", "Fetch", "Decode", "MemRead", "Execute", "MemWrite", "RegWrite");
            s += String.Format      ("{0,-15} {1,-15} {2,-15} {3,-15} {4,-15} {5,-15}\n", "MOV R1,15", "FADD R4, R3", "ADD 15, 15", "ADD 0x01A45C", "test instruction", "test test");

            Debug.WriteLine(s);

            core1Box.Text = s;
            core2Box.Text = s;
            core3Box.Text = s;
            core4Box.Text = s;
        }

        //private string openFile(CPU cpu)
        //{
        //    var filePath = string.Empty;
        //    List<string> dispProgCode = new List<string>();

        //    using (OpenFileDialog openFileDialog = new OpenFileDialog())
        //    {
        //        if (openFileDialog.ShowDialog() == DialogResult.OK)
        //        {
        //            cpu.Rd.fileStr = openFileDialog.FileName;
        //            cpu.Rd.OpenFile();

        //            //clear the corresponding cores program textbox
        //            if (cpu == CPU1)
        //                core1ProgBox.Text = "";
        //            else if (cpu == CPU2)
        //                core2ProgBox.Text = "";
        //            else if (cpu == CPU3)
        //                core3ProgBox.Text = "";
        //            else if (cpu == CPU4)
        //                core4ProgBox.Text = "";

        //            //get the instruction in readable language
        //            foreach (ushort x in cpu.Rd.proMem)
        //            {
        //                string output = "";

        //                //get opcode
        //                ushort op = (ushort)(x >> 11);
        //                Instruction tempInst = cpu.CreateInstructionInstance(op);
        //                output += tempInst.ToString() + " ";

        //                //get rest
        //                ushort remainder = (ushort)(0b_0000_0111_1111_1111 & x); //bit mask for removing opcode
        //                output += tempInst.ToText(remainder);

        //                dispProgCode.Add(output);
        //            }

        //            //determine which core a program was just loaded for and display it
        //            foreach (string str in dispProgCode)
        //            {
        //                if (cpu == CPU1)
        //                {
        //                    core1ProgBox.Text += str + "\n";
        //                }
        //                else if (cpu == CPU2)
        //                {
        //                    core2ProgBox.Text += str + "\n";
        //                }
        //                else if (cpu == CPU3)
        //                {
        //                    core3ProgBox.Text += str + "\n";
        //                }
        //                else if (cpu == CPU4)
        //                {
        //                    core4ProgBox.Text += str + "\n";
        //                }
        //            }
        //        }
        //    }

        //    return null;
        //}
    }
}
