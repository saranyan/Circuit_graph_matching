
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Forms;

using vflibcs;


namespace Turuga_Match_v1._0
{
    /// <summary>
    /// COM Interface - enables to run c# code from c++
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [Guid("DA78E20D-78A0-408f-B78B-89DE3C723879")]
    
    public interface IManagedInterface
    {
        double matchPercent(String file1, String file2);
        void iN(int foo, String g);
        void gSummary(Graph g1, string s);
        void cN(Graph g, String n);
        Graph getGraphFromPath(string path, Graph gOrig);
        Graph readNetlist(string FileName);
        void readAllTinyCadNetlistFiles(string directory);
        void sendFromTinycad(string currentFile, string directory);
        
    }

    // [ProgId("Prisoner.PrisonerControl")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("05CD1850-3D36-4cae-937D-90E00E602D40")]
    public class TMatch : IManagedInterface
    {
        String logtext;
        List<string> namesFiles = new List<string>();
        List<string> netlists = new List<string>();
        List<double> percentages = new List<double>();
        List<string> names = new List<string>();
        

        [ComRegisterFunction()]
        public static void RegisterClass(string key)
        {
            // Strip off HKEY_CLASSES_ROOT\ from the passed key as I don't need it

            StringBuilder sb = new StringBuilder(key);
            sb.Replace(@"HKEY_CLASSES_ROOT\", "");

            // Open the CLSID\{guid} key for write access

            RegistryKey k = Registry.ClassesRoot.OpenSubKey(sb.ToString(), true);

            // And create the 'Control' key - this allows it to show up in 

            // the ActiveX control container 

            RegistryKey ctrl = k.CreateSubKey("Control");
            ctrl.Close();

            // Next create the CodeBase entry - needed if not string named and GACced.

            RegistryKey inprocServer32 = k.OpenSubKey("InprocServer32", true);
            inprocServer32.SetValue("CodeBase", Assembly.GetExecutingAssembly().CodeBase);
            inprocServer32.Close();

            // Finally close the main key

            k.Close();
        }

        [ComUnregisterFunction()]
        public static void UnregisterClass(string i_Key)
        {
            // strip off HKEY_CLASSES_ROOT\ from the passed key as I don't need it
            StringBuilder sb = new StringBuilder(i_Key);
            sb.Replace(@"HKEY_CLASSES_ROOT\", "");

            // open HKCR\CLSID\{guid} for write access
            RegistryKey registerKey =
                Registry.ClassesRoot.OpenSubKey(sb.ToString(), true);

            // delete the 'Control' key, 
            // but don't throw an exception if it does not exist
            registerKey.DeleteSubKey("Control", false);

            // next open up InprocServer32
            RegistryKey inprocServer32 =
                registerKey.OpenSubKey("InprocServer32", true);

            // and delete the CodeBase key, again not throwing if missing
            inprocServer32.DeleteSubKey("CodeBase", false);

            // finally close the main key
            registerKey.Close();
        }

        public void readAllTinyCadNetlistFiles(string directory)
        {
            String[] filePaths = Directory.GetFiles(directory, "*.net");
            String line;
            String nline = null;
            for (int i = 0; i < filePaths.Length; i++)
            {
                System.IO.StreamReader file = new System.IO.StreamReader(filePaths[i]);
                while ((line = file.ReadLine()) != null)
                {
                    if (!line.Contains('*'))
                    {
                        line = line.Replace("VDD", "1");
                        line = line.Replace("GND", "0");
                        line = line.Replace("VSS", "_N_VSS_");
                        nline = String.Concat(nline, line, "\n");
                    }
                }
                namesFiles.Add(filePaths[i]);
                netlists.Add(nline);
               // MessageBox.Show(nline);
                nline = null;
                
            }
            
        }

        public void sendFromTinycad(string currentFile, string directory)
        {
            String tempList = null;
            double matchP = 0.0;
            ///gets from tinycad, the current schematic file and directory
            ///
            //readAllTinyCadNetlistFiles(directory);
            if (currentFile.Length < 2)
            {
                //MessageBox.Show("Null");
                return;
            }
            //MessageBox.Show(String.Concat("File is ", currentFile, "\tDirectory = ", System.IO.Path.GetFullPath(directory)));
            //String currentFileName = currentFile.Split(new char [] {'\\'});
            String currentFileName = currentFile.Replace(".dsn",".net");
            String currentDirName = System.IO.Path.GetFullPath(directory);
            readAllTinyCadNetlistFiles(currentDirName);
            //MessageBox.Show("returned from readAllTiny..");
            for (int i = 0; i < namesFiles.Count; i++)
            {
                if (currentFileName != namesFiles[i])
                {
                    matchP = matchPercent(namesFiles[i], currentFileName);
                    //MessageBox.Show("returned from matchPercent");
                    percentages.Add(matchP);
                    tempList = String.Concat(tempList, namesFiles[i], " -> match = ", matchP.ToString(), "\n");
                    names.Add(namesFiles[i]);
                }

                
            }
            MessageLogWindow mlw = new MessageLogWindow(tempList,names,percentages);
            mlw.Show();
            

        }

        public void iN(int foo, String g)
        {
            Console.WriteLine(String.Concat("Node inserted at ", foo.ToString(), "for ", g));
        }

        public void gSummary(Graph g1, string s)
        {
            Console.WriteLine(String.Concat("Printing Graph Summary - "), s);

            for (int i = 0; i < g1.NodeCount; i++)
            {
                Console.WriteLine(String.Concat("Node ", g1.PosFromId(i).ToString(),
                    " ", g1.GetNodeAttr(i), " connected to - ", (g1.OutEdgeCount(i) + g1.InEdgeCount(i)).ToString(), " components "));
                logtext = String.Concat(logtext, String.Concat("Node ", g1.PosFromId(i).ToString(),
                    " ", g1.GetNodeAttr(i), " connected to - ", (g1.OutEdgeCount(i) + g1.InEdgeCount(i)).ToString(), " components "));

            }
        }

        public void cN(Graph g, String n)
        {
            g.InsertNode(new NodeType(n));
        }

        public Graph getGraphFromPath(string path, Graph gOrig)
        {
            char[] delims = { ' ', '\t' };
            string[] foo = path.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            Graph gtemp = new Graph();

            for (int i = 0; i < foo.Count() - 1; i++)
            {
                string type = gOrig.returnNodeTypeFromAttribute(int.Parse(foo[i]));
                cN(gtemp, type);

            }
            for (int i = 0; i < gtemp.NodeCount - 1; i++)
            {
                if (gOrig.doesOutEdgeExist(int.Parse(foo[i]), int.Parse(foo[i + 1])))
                    gtemp.InsertEdge(i, i + 1);
                else gtemp.InsertEdge(i + 1, i);
                ///preserve edges for nodes that already connect to each other
                for (int j = 0; j <= i; j++)
                {
                    if (gOrig.doesEdgeExist(int.Parse(foo[i]), int.Parse(foo[j])) && !gtemp.doesEdgeExist(i, j))
                    {
                        if (gOrig.doesOutEdgeExist(int.Parse(foo[i]), int.Parse(foo[j])))
                            gtemp.InsertEdge(i, j);
                        else gtemp.InsertEdge(j, i);
                    }
                }

                if (i == gtemp.NodeCount - 2)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        if (gOrig.doesEdgeExist(int.Parse(foo[i + 1]), int.Parse(foo[j])) && !gtemp.doesEdgeExist(i + 1, j))
                        {
                            if (gOrig.doesOutEdgeExist(int.Parse(foo[i + 1]), int.Parse(foo[j])))
                                gtemp.InsertEdge(i + 1, j);
                            else gtemp.InsertEdge(j, i + 1);
                        }
                    }
                }
                //gtemp.InsertEdge(i, i + 1);

            }

            ///loopback edge for last
            ///
            //if (gOrig.doesOutEdgeExist(int.Parse(foo[foo.Count() - 2]), int.Parse(foo[0])))
            //{
            //    gtemp.InsertEdge(gtemp.NodeCount - 1, 0);
            //}
            //else gtemp.InsertEdge(0, gtemp.NodeCount - 1);

            //gtemp.DeleteNode(gtemp.NodeCount - 1);
            //gtemp.DeleteNode(gtemp.NodeCount - 1); 
            //gtemp.DeleteNode(gtemp.NodeCount - 1);
            //gtemp.DeleteNode(gtemp.NodeCount - 1);

            //gtemp.InsertEdge(gtemp.NodeCount - 1, 0);
            //Console.WriteLine(gtemp.GetInfo());
            return gtemp;
        }

        public Graph readNetlist(string FileName)
        {
            Graph g1 = new Graph();
            Hashtable ht = new Hashtable();
            string[] st = null;

            st = System.IO.File.ReadAllLines(FileName);


            string[] foo = null;
            char[] delims = { ' ', '\n', '\t' };
            char firstChar;
            ///Loop adds nodes
            for (int i = 0; i < st.Count(); i++)
            {
                //Console.WriteLine(st[i]);
                if (!st[i].Contains("*"))
                {
                    foo = st[i].Split(delims, StringSplitOptions.RemoveEmptyEntries);
                    ///look at array length to account for empty lines.
                    for (int j = 0; j < foo.Count(); j++)
                    {
                        firstChar = (foo[j])[0];
                        switch (firstChar)
                        {
                            case 'M':
                            case 'T':
                                {
                                    if (!ht.ContainsKey(foo[j]) && !foo[j].Contains('='))
                                    {
                                        ht.Add(foo[j], g1.NodeCount);
                                        cN(g1, "Transistor");
                                    }
                                    break;
                                }
                            case '_':
                            case 'N':
                                {
                                    if (!ht.ContainsKey(foo[j]) && !foo[j].Contains('='))
                                    {
                                        ht.Add(foo[j], g1.NodeCount);
                                        cN(g1, "Node");
                                    }
                                    break;
                                }
                            case 'R':
                                {
                                    if (!ht.ContainsKey(foo[j]) && !foo[j].Contains('='))
                                    {
                                        ht.Add(foo[j], g1.NodeCount);
                                        cN(g1, "Resistor");
                                    }
                                    break;
                                }
                            case 'D':
                                {

                                    if (!ht.ContainsKey(foo[j]) && !foo[j].Contains("DIODE") && !foo[j].Contains('='))
                                    {
                                        ht.Add(foo[j], g1.NodeCount);
                                        cN(g1, "Diode");
                                    }
                                    break;
                                }
                            case 'V':
                                {
                                    if (!ht.ContainsKey(foo[j]) && !foo[j].Contains('='))
                                    {
                                        ht.Add(foo[j], g1.NodeCount);
                                        cN(g1, "Voltage");
                                    }
                                    break;
                                }
                            case 'I':
                                {
                                    if (!ht.ContainsKey(foo[j]) && !foo[j].Contains('='))
                                    {
                                        ht.Add(foo[j], g1.NodeCount);
                                        cN(g1, "Current");
                                    }
                                    break;
                                }
                            case 'C':
                                {
                                    if (!ht.ContainsKey(foo[j]) && !foo[j].Contains('='))
                                    {
                                        ht.Add(foo[j], g1.NodeCount);
                                        cN(g1, "Capacitor");
                                    }
                                    break;
                                }

                            case '0':
                                {
                                    if (!ht.ContainsKey(foo[j]) && !foo[j].Contains('='))
                                    {
                                        ht.Add(foo[j], g1.NodeCount);
                                        cN(g1, "Ground");
                                    }
                                    break;
                                }
                            case '1':
                                {
                                    if (!ht.ContainsKey(foo[j]) && !foo[j].Contains('='))
                                    {
                                        ht.Add(foo[j], g1.NodeCount);
                                        cN(g1, "Supply");
                                    }
                                    break;
                                }
                            default:
                                {
                                    break;

                                }

                        }
                    }
                }

            }
            ///Loop adds edges
            for (int i = 0; i < st.Count(); i++)
            {
                if (!st[i].Contains("*"))
                {
                    foo = st[i].Split(delims, StringSplitOptions.RemoveEmptyEntries);
                    if (foo.Length > 1)
                    {
                        int nodeFrom = int.Parse(ht[foo[0]].ToString()); ///first element is node from
                        int nodeTo = 0;
                        for (int j = 1; j < foo.Count() - 1; j++)
                        {
                            if (!foo[j].Contains('=') & ht.ContainsKey(foo[j]))
                            {
                                nodeTo = int.Parse(ht[foo[j]].ToString());
                                if (!g1.doesEdgeExist(nodeFrom, nodeTo))
                                    g1.InsertEdge(nodeFrom, nodeTo);
                            }
                        }
                    }
                }
            }
            Console.WriteLine(g1.GetInfo());
            return g1;
        }
        private class NodeType : IContextCheck
        {
            string _nodeType;
            public NodeType(string nodeType)
            {
                _nodeType = nodeType;

            }
            public bool FCompatible(IContextCheck icc)
            {
                return ((NodeType)icc)._nodeType == _nodeType;
            }
            public string iName()
            {
                return _nodeType;
            }
        }

        public double matchPercent(string file1, string file2)
        {
            //MessageBox.Show(String.Concat(file1, "\t", file2));
            Graph graph = new Graph();
            Console.WriteLine("Graph 1 detail ------\n\n");
            graph = readNetlist(file1);

            Console.WriteLine("Graph 2 detail ------\n\n");
            Graph graph2 = new Graph();
            graph2 = readNetlist(file2);

            bool f = graph2.doesEdgeExist(0, 4);
            Console.WriteLine(String.Concat("Does edge exist = ", f.ToString()));
            List<int> neighbors = graph2.GetNeighbors(5);
            for (int i = 0; i < neighbors.Count; i++)
            {
                Console.Write(String.Concat(neighbors[i].ToString(), "\t"));

            }
            Console.WriteLine("");
            neighbors.Clear();
            neighbors = graph2.GetOutNeighbors(7);
            for (int i = 0; i < neighbors.Count; i++)
            {
                Console.Write(String.Concat(neighbors[i].ToString(), "\t"));

            }

            List<string> cliques = graph2.GetCliques();
            int count = 0;
            for (int i = 0; i < cliques.Count; i++)
            {
                char[] delims = { ' ', '\t' };
                //Graph gtemp = new Graph();
                Graph gtemp = getGraphFromPath(cliques[i], graph2);

                //Console.WriteLine(gtemp.GetInfo());
                VfState vfs = new VfState(graph, gtemp, false, true);
                bool fIsomorphic = vfs.FMatch();
                if (fIsomorphic) count++;
                Console.WriteLine(String.Concat("clique (", i.ToString(), ") value = ",
                    cliques[i], " Match = ", fIsomorphic.ToString()));

            }

            double percent = ((double)count / cliques.Count) * 100.0;
            //MessageLogWindow mlw = new MessageLogWindow(logtext);
            //mlw.Show();
            //MessageBox.Show(String.Concat("Percent match = ", percent.ToString()));
            return percent;
            //return 21.3;
        }

        
    }
}