using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.ComponentModel;
using MusicServices;
using System.Net;

namespace Test
{
    class Program
    {
        public static DirectoryInfo MusicDir = new DirectoryInfo("F:\\music");
        public static string ProcName = "AIMP3";
        public static string RemoteMusicDir = "\\192.168.1.11\\music\\";

        static void GetDir()
        {
            if (Config.GetParametr("MusicDir") == "")
            {
                Color.WriteLineColor("          Использую папку по умолчанию 'F:\\music' ","Yellow");
                MusicDir = new DirectoryInfo("F:\\music");
            }
            else
                MusicDir = new DirectoryInfo(Config.GetParametr("MusicDir"));
        }
        static void GetProcName()
        {
            if (Config.GetParametr("ProcName") == "")
            {
                Color.WriteLineColor("          использую процесс по умолчанию AIMP3","Yellow");
                ProcName = "AIMP3";
            }
            else
                ProcName = Config.GetParametr("ProcName");
        }

        static void Main(string[] args)
        {
            Color.WriteLineColor("          Запуск программы...", "Green");
            
            GetDir();

            GetProcName();

            prg();

            while (true)
                Console.ReadKey();
        }

        static void prg()
        {
            Color.WriteLineColor("          Запуск проверки...", "Green");

            if (IsWorkTime())
            {
                if (!IsPlayerStarted())
                    StartPlayer();

                CheckTime(true);
            }
            else
            {
                if (IsPlayerStarted())
                    KillProc();

                CheckTime(false);
            }
        }

        static bool IsPlayerStarted()
        {
            Process[] AimpProcess;

            AimpProcess = Process.GetProcessesByName("AIMP3");

            if (AimpProcess.Length > 0)
            {
                Color.WriteLineColor("          Плеер запущен...", "Yellow");
                return true;
            }
            else
            {
                Color.WriteLineColor("          Плеер не запущен...", "Red");
                return false;
            }
        }

        static void StartPlayer()
        {
            DownloadMusic();

            GeneratePlayList();

            Process.Start(MusicDir + "\\default.m3u");
        }

        static bool IsWorkTime()
        {
            if (!DateTime.Now.TimeOfDay.IsBetween(new TimeSpan(23, 0, 0), new TimeSpan(7, 0, 0)))
            {
                Color.WriteLineColor("          Рабочее время магазина.", "Yellow");
                return true;
            }
            else
            {
                Color.WriteLineColor("          Нерабочее время магазина.", "Cyan");
                return false;
            }    
        }

        static void KillProc()
        {
            Process[] AimpProcess;

            AimpProcess = Process.GetProcessesByName("AIMP3");

            int i = 0;

            try
            {
                while (i != AimpProcess.Length)
                {
                    AimpProcess[i].Kill();
                    i++;
                    Color.WriteLineColor("          Всего завершенных процессов : " + i.ToString(),"Red");
                }
            }
            catch (Win32Exception)
            {
                Color.WriteLineColor("          Ок...", "Yellow");
            }

            catch (InvalidOperationException)
            {
                Console.WriteLine("The process has already exited.");
            }

            catch (Exception e)  // some other exception
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
        }

        static void GeneratePlayList()
        {
            FileInfo[] MusicMass = MusicDir.GetFiles("*.mp3");

            try
            {
                Color.WriteLineColor("          Генерирую Плейлист...", "Yellow");

                StreamWriter sw = new StreamWriter(MusicDir + "\\default.m3u", false, System.Text.Encoding.UTF8);
                sw.WriteLine("#EXTM3U");
                sw.WriteLine("#EXTINF:3618,supermarket");

                foreach (FileInfo file in MusicMass)
                {
                    if (file.Length > 1200976)
                    {
                        Console.WriteLine(MusicDir + "\\" + file.Name);
                        sw.WriteLine(MusicDir + "\\" + file.Name);
                        sw.WriteLine("#EXTINF:3618," + file.Name.Replace(".mp3", ""));
                    }
                }

                sw.Close();

                Color.WriteLineColor("          Плейлист сгенерирован...", "Green");

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void CheckTime(bool day)
        {
            TimeSpan diff = new TimeSpan();

            if (day)
            {
                DateTime TimeNow = DateTime.Now;
                DateTime TimeEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 00, 01);

                diff = TimeEnd - TimeNow;
            }
            else
            {
                DateTime TimeNow = DateTime.Now;
                DateTime TimeEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 07, 00, 01);

                if (TimeNow.Hour == 23)
                {
                    TimeEnd = TimeEnd.AddDays(1);
                }

                Color.WriteLineColor("          Выполняю действие " + TimeEnd + "-" + TimeNow, "DarkBlue"); 

                diff = TimeEnd - TimeNow;
            }

            Thread thd = new Thread(delegate()
            {
                Color.WriteLineColor("          Запускаю поток контролера","Green");
                WaitTime(diff,day);
            });
            thd.Name = "Поток контроллер";
            thd.Start();
        }

        static void WaitTime(TimeSpan diff, bool day)
        {
            int i = Convert.ToInt32(diff.TotalSeconds);
            string text;
            ConsoleColor cl = new ConsoleColor();

            if (day)
            {
                text = "            Завершение работы плеера через ";
                cl = ConsoleColor.Yellow;
            }
            else
            {
                text = "            Запуск плеера через ";
                cl = ConsoleColor.Green;
            }

            Console.WriteLine("\n");

            while (i != 0)
            {
                int hour = i / 3600;
                int minute = (i / 60) - (hour * 60);
                int second = i - (minute * 60) - (hour * 3600);
                string n = "";
                string m = "";


                if (second < 10)
                    n = "0";

                if (minute < 10)
                    m = "0";

                Code.RenderConsoleProgress(0, '\u2592', cl ,text + hour +":" + m +  minute +":"+ n + second );
                Thread.Sleep(1000);
                i--;

                n = "";
                m = "";
            }

            if (day)
            {
                Color.WriteLineColor("          Завершаю процесс плеера!","Red");
                KillProc();
            }
            else
            {
                Color.WriteLineColor("          Запускаю процесс плеера!","Green");
                StartPlayer();
            }

            prg();
        }

        static void DownloadMusic()
        {
            DirectoryInfo RMusicDir = new DirectoryInfo("//192.168.1.11//music//");

            FileInfo[] MusicMass = RMusicDir.GetFiles("*.mp3");

            var webClient = new WebClient();

            foreach (FileInfo file in MusicMass)
            {
                if (file.Length > 1200976)
                {
                    Console.WriteLine(file.Name);

                    try { webClient.DownloadFile(new Uri(RMusicDir + file.Name), MusicDir + "\\" + file.Name); }
                    catch { }
                }

            }
        }
            
    }
}
