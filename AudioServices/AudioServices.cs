using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioServices
{
    public partial class AudioServices : ServiceBase
    {
        public AudioServices()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Console.WriteLine("Запуск программы");

            GetDir();

            GetProcName();

            prg();

            while (true)
                Console.ReadKey();
        }

        protected override void OnStop()
        {
            Environment.Exit(0);
        }

        public static DirectoryInfo MusicDir = new DirectoryInfo("F:\\music");
        public static string ProcName = "AIMP3";

        static void GetDir()
        {
            MusicDir = new DirectoryInfo(Config.GetParametr("ProcName"));

            if (MusicDir.FullName == "")
                MusicDir = new DirectoryInfo("F:\\music");
        }
        static void GetProcName()
        {
            ProcName = Config.GetParametr("ProcName");

            if (ProcName == "")
                ProcName = "AIMP3";
        }

        static void prg()
        {
            Console.WriteLine("Запуск начальной проверки!");

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
                Console.WriteLine("Плеер запущен!");
                return true;
            }
            else
            {
                Console.WriteLine("Плеер не запущен!");
                return false;
            }
        }

        static void StartPlayer()
        {
            GeneratePlayList();

            Process.Start(MusicDir + "\\default.m3u");
        }

        static bool IsWorkTime()
        {
            if (!DateTime.Now.TimeOfDay.IsBetween(new TimeSpan(23, 0, 0), new TimeSpan(7, 0, 0)))
            {
                Console.WriteLine("Рабочее время магазина.");
                return true;
            }
            else
            {
                Console.WriteLine("Нерабочее время магазина.");
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
                    Console.WriteLine("Всего завершенных процессов : " + i.ToString());
                }
            }
            catch (Win32Exception)
            {
                Console.WriteLine("The process is terminating or could not be terminated.");
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
                Console.WriteLine("Генерирую Плейлист!");

                StreamWriter sw = new StreamWriter(MusicDir + "\\default.m3u", false, System.Text.Encoding.UTF8);
                sw.WriteLine("#EXTM3U");
                sw.WriteLine("#EXTINF:3618,supermarket");

                foreach (FileInfo file in MusicMass)
                {
                    Console.WriteLine(MusicDir + "\\" + file.Name);
                    sw.WriteLine(MusicDir + "\\" + file.Name);
                    sw.WriteLine("#EXTINF:3618," + file.Name.Replace(".mp3", ""));
                }

                sw.Close();

                Console.WriteLine("Плейлист сгенерирован!");
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

                Console.WriteLine("Выполняю действие " + TimeEnd + "-" + TimeNow);

                diff = TimeEnd - TimeNow;
            }

            Thread thd = new Thread(delegate()
            {
                Console.WriteLine("Запускаю поток контролера");
                WaitTime(diff, day);
            });
            thd.Name = "Поток контроллер";
            thd.Start();
        }

        static void WaitTime(TimeSpan diff, bool day)
        {
            Console.WriteLine("Засыпаю на " + diff);
            Thread.Sleep(diff);

            if (day)
            {
                Console.WriteLine("Завершаю процесс плеера!");
                KillProc();
            }
            else
            {
                Console.WriteLine("Запускаю процесс плеера!");
                StartPlayer();
            }

            Console.WriteLine("Перезапуск цикла!");
            prg();
        }
    }
}
