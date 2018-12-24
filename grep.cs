using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using System.Security;
using CommandLine.Utility;

namespace grep
{

    class grep
    {

        private bool m_bRecursive;
        private bool m_bIgnoreCase;
        private bool m_bJustFiles;
        private bool m_bLineNumbers;
        private bool m_bCountLines;
        private string m_strRegEx;
        private string m_strFiles;
        private string m_startdir;
        private bool m_replace;
        private string m_replacestring;

        private ArrayList m_arrFiles = new ArrayList();


        public bool Recursive
        {
            get { return m_bRecursive; }
            set { m_bRecursive = value; }
        }

        public bool IgnoreCase
        {
            get { return m_bIgnoreCase; }
            set { m_bIgnoreCase = value; }
        }

        public bool JustFiles
        {
            get { return m_bJustFiles; }
            set { m_bJustFiles = value; }
        }

        public bool LineNumbers
        {
            get { return m_bLineNumbers; }
            set { m_bLineNumbers = value; }
        }

        public bool CountLines
        {
            get { return m_bCountLines; }
            set { m_bCountLines = value; }
        }

        public string RegEx
        {
            get { return m_strRegEx; }
            set { m_strRegEx = value; }
        }

        public string Files
        {
            get { return m_strFiles; }
            set { m_strFiles = value; }
        }

        public string StartDir
        {
            get { return m_startdir; }
            set { m_startdir = value; }
        }

        public bool Replace
        {
            get { return m_replace; }
            set { m_replace = value; }
        }

        public string Replacestring
        {
            get { return m_replacestring; }
            set { m_replacestring = value; }
        }


        private void GetFiles(String strDir, String strExt, bool bRecursive)
        {
            try
            {

                string[] fileList = Directory.GetFiles(strDir, strExt);
                for (int i = 0; i < fileList.Length; i++)
                {
                    if (File.Exists(fileList[i]))
                        m_arrFiles.Add(fileList[i]);
                }
                if (bRecursive == true)
                {

                    string[] dirList = Directory.GetDirectories(strDir);
                    for (int i = 0; i < dirList.Length; i++)
                    {
                        GetFiles(dirList[i], strExt, true);
                    }
                }
            } catch(Exception) { }
        }


        public void Search()
        {

            String strDir = StartDir; 
                                      
            m_arrFiles.Clear();

            String[] astrFiles = m_strFiles.Split(new Char[] { ',' });
            for (int i = 0; i < astrFiles.Length; i++)
            {

                astrFiles[i] = astrFiles[i].Trim();
                GetFiles(strDir, astrFiles[i], m_bRecursive);
            }

            String strResults = "";
            String strLine;
            int iLine, iCount;
            bool bEmpty = true;
            IEnumerator enm = m_arrFiles.GetEnumerator();
            while (enm.MoveNext())
            {
                try
                {
                    StreamReader sr = File.OpenText((string)enm.Current);
                    iLine = 0;
                    iCount = 0;

                    System.Collections.Generic.List<string> termsList = new System.Collections.Generic.List<string>();

                    bool bFirst = true;
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        iLine++;
                        termsList.Add(strLine);

                        Match mtch;
                        if (m_bIgnoreCase == true)
                            mtch = Regex.Match(strLine, m_strRegEx, RegexOptions.IgnoreCase);
                        else
                            mtch = Regex.Match(strLine, m_strRegEx);
                        if (mtch.Success == true)
                        {
                            bEmpty = false;
                            iCount++;
                            if (bFirst == true)
                            {
                                if (m_bJustFiles == true)
                                {
                                    strResults += "[***] Datei: " + (string)enm.Current + "\n";
                                    break;
                                }
                                else
                                {
                                    strResults += "[***] Datei: " + (string)enm.Current + "\n";
                                }

                                bFirst = false;
                            }

                            if (m_bLineNumbers == true)
                                strResults += "[***] Zeile " + iLine + ": " + strLine + "\n";
                            else
                                strResults += "[***] " + strLine + "\n";
                        }
                    }
                    sr.Close();

                    /* REPLACE MODE */
                    if (Replace) {

                        using (System.IO.StreamWriter file = new System.IO.StreamWriter((string)enm.Current)) {
                            foreach (string line in termsList) {

                                Console.WriteLine(Replacestring);
                                
                                Match mtch2;
                                if (m_bIgnoreCase == true) mtch2 = Regex.Match(line, Replacestring, RegexOptions.IgnoreCase);
                                else mtch2 = Regex.Match(line, m_strRegEx);

                                if (mtch2.Success == true) {                                    
                                    string repl = Regex.Replace(line, m_strRegEx, Replacestring);                                   
                                    file.WriteLine(repl);
                                } else {
                                    file.WriteLine(line);
                                }
                                

                            }
                        }


                        for (int i = 0; i < termsList.Count; i++)
                        {
                            Console.WriteLine(i + ": " + termsList[i]);
                        }

                    }

                    if (bFirst == false)
                    {
                        if (m_bCountLines == true)
                            strResults += "[***] Ergebnis: " + iCount + " Zeilen\n";
                        strResults += "\n";
                    }
                }
                catch (SecurityException)
                {
                    strResults += "\r\n" + (string)enm.Current + ": Ausnahme\r\n\r\n";
                }
                catch (FileNotFoundException)
                {
                    strResults += "\r\n" + (string)enm.Current + ": File Not Found Exception\r\n";
                }
                catch (UnauthorizedAccessException)
                {
                    strResults += "\r\n" + (string)enm.Current + ": Are you admin?\r\n";
                }
                catch (Exception) { }
        }
            if (bEmpty == true)
                Console.WriteLine("No matches found!");
            else
                Console.WriteLine(strResults);
        
		}

        private static void PrintHelp()
		{
			Console.WriteLine("[***] Usage: grep [-c] [-i] [-l] [-n] [-r] -D:StartDirectory -E:Expression(RegEx) -F:Files(RegEx) -p(=replace mode!!!)");
		}

		[STAThread]
		static void Main(string[] args)
		{

            Console.WriteLine("[***] WinGrep 1.0\n");

			Arguments CommandLine = new Arguments(args);
			if(CommandLine["h"] != null || CommandLine["H"] != null)
			{
				PrintHelp();
				return;
			}

			grep grep = new grep();

			if(CommandLine["E"] != null)
				grep.RegEx = (string)CommandLine["E"];
			else
			{
				Console.WriteLine("Error: No Regular Expression specified!");
				Console.WriteLine();
				PrintHelp();
				return;
			}
			if(CommandLine["F"] != null)
				grep.Files = (string)CommandLine["F"];
			else
			{
				Console.WriteLine("Error: No Search Files specified!");
				Console.WriteLine();
				PrintHelp();
				return;
			}
            if(CommandLine["D"] != null)
				grep.StartDir = (string)CommandLine["D"];
			else
			{
				grep.StartDir = Environment.CurrentDirectory;
			}
            if (CommandLine["P"] != null)
            {
                grep.Replace = true;
                grep.Replacestring = (string)CommandLine["P"];
            }
			
            grep.Recursive = (CommandLine["r"] != null);
			grep.IgnoreCase = (CommandLine["i"] != null);
			grep.JustFiles = (CommandLine["l"] != null);
            if (grep.JustFiles == true)
				grep.LineNumbers = false;
			else
				grep.LineNumbers = (CommandLine["n"] != null);
			if(grep.JustFiles == true)
				grep.CountLines = false;
			else
				grep.CountLines = (CommandLine["c"] != null);

			grep.Search();
		}
	}
}
