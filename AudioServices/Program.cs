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
        private static DirectoryInfo MusicDir = new DirectoryInfo("F:\\music");
        private static DirectoryInfo AdDir = new DirectoryInfo("F:\\ad");
        private static string ProcName = "AIMP3";
        private static string RemoteMusicDir = "//192.168.1.11//music//";
        private static string RemoteAdDir = "//192.168.1.11//ad//";
        private static bool RemoveDuplicates = false;

        private static void GetDir()
        {
            string LMusicDir = Config.GetParametr("MusicDir");
            string LAdDir = Config.GetParametr("AdDir");
            string LRemoteMusicDir = Config.GetParametr("RemoteMusicDir");
            string LRemoteAdDir = Config.GetParametr("RemoteAdDir");


            if (LAdDir.Length > 0)
                AdDir = new DirectoryInfo(LAdDir);
            else
            {
                Color.WriteLineColor("          В конфигурационном файле не заданы параметры папки с рекламой.  ", ConsoleColor.Yellow);
                Color.WriteLineColor("          Использую папку по умолчанию 'F:\\ad' ", ConsoleColor.Yellow);
                AdDir = new DirectoryInfo("F:\\ad");
            }


            if (LMusicDir.Length > 0)
                MusicDir = new DirectoryInfo(LMusicDir);
            else
            {
                Color.WriteLineColor("          В конфигурационном файле не заданы параметры папки с музыкой.  ", ConsoleColor.Yellow);
                Color.WriteLineColor("          Использую папку по умолчанию 'F:\\music' ", ConsoleColor.Yellow);
                MusicDir = new DirectoryInfo("F:\\music");
            }

            if (LRemoteAdDir.Length > 0)
                RemoteAdDir = LRemoteAdDir;
            else
            {
                Color.WriteLineColor("          В конфигурационном файле не заданы параметры удаленной папки с рекламой.  ", ConsoleColor.Yellow);
                Color.WriteLineColor("          Использую папку по умолчанию '//192.168.1.11//ad//' ", ConsoleColor.Yellow);
                RemoteAdDir = "//192.168.1.11//ad//";
            }

            if (LRemoteMusicDir.Length > 0)
                RemoteMusicDir = LRemoteMusicDir;
            else
            {
                Color.WriteLineColor("          В конфигурационном файле не заданы параметры удаленной папки с музыкой.  ", ConsoleColor.Yellow);
                Color.WriteLineColor("          Использую папку по умолчанию '//192.168.1.11//music//' ", ConsoleColor.Yellow);
                RemoteMusicDir = "//192.168.1.11//music//";
            }


        }
        private static void GetProcName()
        {
            string LprocName = Config.GetParametr("ProcName");

            if (LprocName.Length > 0)
                ProcName = LprocName;
            else
            {
                Color.WriteLineColor("          В конфигурационном файле не заданы параметры плеера.", ConsoleColor.Yellow);
                Color.WriteLineColor("          Использую процесс по умолчанию AIMP3", ConsoleColor.Yellow);
                ProcName = "AIMP3";
            }
        }

        private static void GetRemoveDuplicatesStatus()
        {
            string LremoveDuplicates = Config.GetParametr("ProcName");

            if (LremoveDuplicates.Length > 0)
                bool.TryParse(LremoveDuplicates, out RemoveDuplicates);

            Color.WriteLineColor("          Параметр удаленния дубликатов равен : " + RemoveDuplicates.ToString(), ConsoleColor.Yellow);
        }

        static void Main(string[] args)
        {
            Color.WriteLineColor("          Запуск программы...", ConsoleColor.Green);
            
            GetDir();

            GetProcName();

            GetRemoveDuplicatesStatus();

            prg();

            while (true)
                Console.ReadKey();
        }

        private static void prg()
        {
            Color.WriteLineColor("          Запуск проверки...", ConsoleColor.Green);

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

        private static bool IsPlayerStarted()
        {
            Process[] AimpProcess;

            AimpProcess = Process.GetProcessesByName("AIMP3");

            if (AimpProcess.Length > 0)
            {
                Color.WriteLineColor("          Плеер работает...", ConsoleColor.Yellow);
                return true;
            }
            else
            {
                Color.WriteLineColor("          Плеер не работает...", ConsoleColor.Red);
                return false;
            }
        }

        private static void StartPlayer()
        {
            CleanUp();

            DownloadMusic();

            DownloadAd();

            GeneratePlayList();

            Process.Start(MusicDir + "\\default.m3u");
        }

        private static bool IsWorkTime()
        {
            if (!DateTime.Now.TimeOfDay.IsBetween(new TimeSpan(23, 0, 0), new TimeSpan(7, 0, 0)))
            {
                Color.WriteLineColor("          Рабочее время магазина.", ConsoleColor.Yellow);
                return true;
            }
            else
            {
                Color.WriteLineColor("          Нерабочее время магазина.", ConsoleColor.Cyan);
                return false;
            }    
        }

        private static void KillProc()
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
                    Color.WriteLineColor("          Всего завершенных процессов : " + i.ToString(),ConsoleColor.Red);
                }
            }
            catch (Win32Exception)
            {
                Color.WriteLineColor("          Ок...", ConsoleColor.Yellow);
            }

            catch (InvalidOperationException)
            {
                Console.WriteLine("The process has already exited.");
            }

            catch (Exception e)  // some other exception
            {
                Console.WriteLine("{0} Exception caught.", e);
            }

            //wait to process kill order to check
            Thread.Sleep(300);
        }

        private static void GeneratePlayList()
        {
            FileInfo[] MusicMass = MusicDir.GetFiles("*.mp3");
            FileInfo[] AdMass = AdDir.GetFiles("*.mp3");

            try
            {
                Color.WriteLineColor("          Генерирую Плейлист...", ConsoleColor.Yellow);

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

                        //AD
                        Console.WriteLine(AdMass[new Random().Next(0, AdMass.Length)].Name);
                        sw.WriteLine(MusicDir + "\\" + AdMass[new Random().Next(0, AdMass.Length)].Name);
                        sw.WriteLine("#EXTINF:3618," + AdMass[new Random().Next(0, AdMass.Length)].Name.Replace(".mp3", ""));
                    }
                }

                sw.Close();

                Color.WriteLineColor("          Плейлист сгенерирован...", ConsoleColor.Green);

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[GeneratePlayList]" + ex.Message);
                Log.ExcWrite("[GeneratePlayList]" + ex.Message);
            }
        }

        private static void CheckTime(bool day)
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

                Color.WriteLineColor("          Выполняю действие " + TimeEnd + "-" + TimeNow, ConsoleColor.DarkBlue); 

                diff = TimeEnd - TimeNow;
            }

            Thread thd = new Thread(delegate()
            {
                Color.WriteLineColor("          Запускаю поток контролера",ConsoleColor.Green);
                WaitTime(diff,day);
            });
            thd.Name = "Поток контроллер";
            thd.Start();
        }

        private static void WaitTime(TimeSpan diff, bool day)
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
                Color.WriteLineColor("          Завершаю процесс плеера!",ConsoleColor.Red);
                KillProc();
            }
            else
            {
                Color.WriteLineColor("          Запускаю процесс плеера!",ConsoleColor.Green);
                StartPlayer();
            }

            prg();
        }

        private static void DownloadMusic()
        {
            try
            {
                DirectoryInfo RMusicDir = new DirectoryInfo(RemoteMusicDir);

                FileInfo[] MusicMass = RMusicDir.GetFiles("*.mp3");

                var webClient = new WebClient();

                foreach (FileInfo file in MusicMass)
                {
                    if (file.Length > 1200976)
                    {
                        Console.WriteLine(file.Name);

                        webClient.DownloadFile(new Uri(RMusicDir + file.Name), MusicDir + "\\" + file.Name);
                    }

                }
            }
            catch (System.Exception ex)
            {
                Color.WriteLineColor(ex.Message, ConsoleColor.Red);
                Log.ExcWrite(ex.Message);
            }
        }

        private static void DownloadAd()
        {
            try
            {
                DirectoryInfo remADdir = new DirectoryInfo(RemoteAdDir);

                FileInfo[] AdMass = remADdir.GetFiles("*.mp3");

                var webClient = new WebClient();

                foreach (FileInfo file in AdMass)
                {
                    Console.WriteLine(file.Name);

                    webClient.DownloadFile(new Uri(remADdir + file.Name), AdDir + "\\" + file.Name);
                }
            }
            catch (System.Exception ex)
            {
                Color.WriteLineColor(ex.Message, ConsoleColor.Red);
                Log.ExcWrite("[DownloadAd]" + ex.Message);
            }
        }

        private static void CleanUp()
        {
            try
            {
                DirectoryInfo RMusicDir = new DirectoryInfo(RemoteMusicDir);

                FileInfo[] MusicMass = RMusicDir.GetFiles("*.mp3");

                foreach (FileInfo file in MusicMass)
                {

                    Console.WriteLine("Произвожу поиск и удаление рекламы...");

                    if (file.Length < 1200976)
                    {
                        //Console.WriteLine(RMusicDir + file.Name);
                        //File.Delete(RMusicDir + file.Name);
                        Log.Write(RMusicDir + file.Name, "[DEL_AD]", "del_ad");
                    }

                    Console.WriteLine("Произвожу поиск и удаление дубликатов алгоритм №1...");

                    if (file.Name.Contains(" (2).mp3") ||
                        file.Name.Contains(" (3).mp3") ||
                        file.Name.Contains(" (4).mp3") ||
                        file.Name.Contains(" (5).mp3") ||
                        file.Name.Contains(" (6).mp3") ||
                        file.Name.Contains(" (7).mp3") ||
                        file.Name.Contains(" (8).mp3") ||
                        file.Name.Contains(" (9).mp3") ||
                        file.Name.Contains(" (10).mp3")) {

                        //File.Delete(RMusicDir + file.Name);
                        Log.Write(RMusicDir + file.Name, "[DEL_ALG1]", "del1");
                    }

                    Console.WriteLine("Произвожу поиск и удаление дубликатов алгоритм №2,3...");

                    foreach (FileInfo files in MusicMass)
                    {
                        if (files.Length == file.Length && files.Name != file.Name)
                        {
                            Console.WriteLine(RMusicDir + files.Name);
                            //File.Delete(RMusicDir + files.Name);
                            Log.Write(RMusicDir + files.Name, "[DEL_ALG2]", "del2");
                        }

                        if (file.Name.Length != files.Name.Length && file.Name.Substring(0, file.Name.Length) == file.Name.Substring(0, file.Name.Length - 3))
                        {
                            Console.WriteLine(RMusicDir + files.Name);
                            //File.Delete(RMusicDir + files.Name);
                            Log.Write(RMusicDir + files.Name, "[DEL_ALG3]", "del3");
                        }

                    }

                    Console.WriteLine("Очистка завершена...");

                }


            }
            catch (System.Exception ex)
            {
                Color.WriteLineColor("[CleanUp]" + ex.Message, ConsoleColor.Red);
                Log.ExcWrite("[CleanUp]" + ex.Message);
            }
        }
            
    }
}
