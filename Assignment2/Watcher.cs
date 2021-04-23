/*   Created/Modified By Chaz Hurd 
 *   This program is made to log the activity of a directory.(ie modifying the directory by deleting, creating etc files)
 *   Most of the code is from: http://msdn.microsoft.com/en-us/library/system.io.filesystemwatcher.aspx
 *   Some code borrowed from MSDN and Professors "in-class notes"
 */ 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;
using System.Globalization;
using System.Data.SQLite;
using System.Net.Mime;

public class Watcher
{
    static private long MAXLINES = 100000;
    static private long curLine = 0;
    static string[] lines;
    static string curFile;
    static bool contd = true;
    public static void Main()
    {

        try
        {
            SQLiteConnection sqlite_conn;
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;

            // create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=filewatcher.db;Version=3;New=True;Compress=True;");

            sqlite_conn.Open();

            sqlite_cmd = sqlite_conn.CreateCommand();

            sqlite_cmd.CommandText = "CREATE TABLE if not exists FileInfo (id integer, text varchar(100));";

            sqlite_cmd.ExecuteNonQuery();


            string dir = "";
            string myDir = "";
            //curFile = @"C:\Users\vanil\source\repos\Assignment2\Assignment2\Log.txt";
            lines = new string[MAXLINES];
            /*if (!File.Exists(curFile))
            {
                using (StreamWriter sw = File.CreateText(curFile))
                {
                    sw.WriteLine("Welcome to the log program:");
                }
            }*/


            int v = 0;
            
            while (contd == true)
            {

                System.Console.WriteLine("Enter The Directory Path to start logging, Or e to Exit");
                dir = System.Console.ReadLine();
                if(dir.Equals("e"))
                {
                    contd = false;
                }

                if (dir.Length > 2 && contd == true)
                {
                    v++;
                    if (Directory.Exists(dir))
                    {
                        if (v < 2)
                        {
                            myDir = dir;
                        }
                        else
                        {
                            
                            myDir +=  " & " + dir;
                            
                        }

                        Run(dir);
                        
                    }
                    else
                    {
                        System.Console.WriteLine("Error: Directory Not Found!");
                    }
                }
            }
            // using (StreamWriter sw = File.AppendText(curFile))
            //{
            string modline;
            int y = 1;
            string title = "'" + "------------------START OF Logging activity for " + myDir + "------------------'";
            //   sw.WriteLine(title);
            sqlite_cmd.CommandText = "INSERT INTO FileInfo (id, text) VALUES (" + y + "," + title + ");";
            sqlite_cmd.ExecuteNonQuery();
            y++;
            foreach (string line in lines)
            {
                if (line != null)
                {
                    //          sw.WriteLine(line);
                    modline = "'" + line + "'";
                    sqlite_cmd.CommandText = "INSERT INTO FileInfo (id, text) VALUES (" + y + "," + modline + ");";
                    sqlite_cmd.ExecuteNonQuery();
                    y++;
                }
            }
            string end = "'------------------END OF LOG FOR " + DateTime.Now + "------------------'";
            //  sw.WriteLine(end);
            sqlite_cmd.CommandText = "INSERT INTO FileInfo (id, text) VALUES (" + y + ", " + end + ");";
            sqlite_cmd.ExecuteNonQuery();
            //}

            System.Console.WriteLine("Read from database y/n?");
            string read = System.Console.ReadLine();
            if (read.Equals("y"))
            {
                // First lets build a SQL-Query again:
                sqlite_cmd.CommandText = "SELECT * FROM FileInfo";
                // Now the SQLiteCommand object can give us a DataReader-Object:
                sqlite_datareader = sqlite_cmd.ExecuteReader();
                // The SQLiteDataReader allows us to run through the result lines:
                while (sqlite_datareader.Read()) // Read() returns true if there is still a result line to read
                {
                    // Print out the content of the text field:
                    if (sqlite_datareader["text"] != null)
                        System.Console.WriteLine(sqlite_datareader["text"]);
                }
            }
            else
            {

                System.Console.WriteLine("GoodBye");
            }

            //sqlite_cmd = sqlite_conn.CreateCommand();
            //sqlite_cmd.CommandText = "DROP TABLE FileInfo";
            //sqlite_cmd.ExecuteNonQuery();
            sqlite_conn.Close();
        }
        catch
        {
            System.Console.WriteLine("Error Creating Database");
        }
        
    }

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    private static void Run(String dir)
    {
        string[] args = Environment.GetCommandLineArgs();

        using (FileSystemWatcher watcher = new FileSystemWatcher())
        {
            watcher.Path = dir;

            // Watch for changes in LastAccess and LastWrite times, and
            // the renaming of files or directories.
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;


            // Add event handlers.
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;

            // Begin watching.
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;

            // Wait for the user to quit the program.
            Console.WriteLine("Press 'q' to quit logging info.");
            while (Console.ReadLine() != "q") ;
        }
    }

    // Define the event handlers.
    private static void OnChanged(object source, FileSystemEventArgs e)
    {
 
        if (e.FullPath.Contains("FileInfo"))
        {
            return;
        }
        else
        {
            //Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
            lines[curLine] ="Name: " + e.Name+ ", Path: "+  e.FullPath + ", Type:" + e.ChangeType + ", Date:" + DateTime.Now.ToString();
            curLine++;

        }
    }

    private static void OnRenamed(object source, RenamedEventArgs e)
    {
        if (e.FullPath.Contains("FileInfo"))
        {
            return;
        }
        else
        {
            //Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}");
            lines[curLine] = "Name: " + e.Name + ", Path: " + e.FullPath + ", Type:" + e.ChangeType + ", Date:" + DateTime.Now.ToString();
            curLine++;
        }
    }
}
