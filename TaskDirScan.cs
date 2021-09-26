using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace SearchErrors
{
    class TaskDirScanStart
    {
        private string dirStr = null;

        public TaskDirScanStart(string _dirStr)
        {
            dirStr = _dirStr;
        }


        public void Run()
        {
            TaskDirScan.Semaphore = new Semaphore(100,100);
            Task t = new Task(new TaskDirScan(dirStr).Run);
            t.Start();
            Console.WriteLine("Task Started");
            Task.WaitAll(t);

            //если нужна логика по завершению можно добавить сюда
            Console.WriteLine("Done");


           
        }
        

    }

    class TaskDirScan
    {
        //управление потоком и задачами
        public static bool isPause = false;
        public static bool isCancel = false;
        public static Semaphore Semaphore;

        private List<Task> tasks = new List<Task>();
        private List<Thread> threads = new List<Thread>();
        public  List<Error> errors = new List<Error>();

        //конструктор и начальные параметры
        private string dirStr = null;
        private string[] files = null;

        public TaskDirScan(string _dirStr)
        {
            dirStr = _dirStr; 
            files = Directory.GetFiles(dirStr); // забираем все файлы из текущей директории
        }

        public void Run()
        {
            if (Semaphore==null)  throw new Exception("нет семафора");
            if (isCancel) return;
            while (isPause)
            {
                Thread.Sleep(1000);
                if (isCancel) return;

            }

            try
            {
                Semaphore.WaitOne();
                DoDir();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally // в любом случае - отпускаем семафор и ждем завержения созданных задач
            {
         

                Semaphore.Release();
                Task.WaitAll(tasks.ToArray());
            }

           

        }

        private void DoDir()
        {
            string[] dirs = Directory.GetDirectories(dirStr); //забираем все сабдиректории из текущей 
            foreach (var file in files)
            {
                threads.Add(new Thread(new TaskDirScan(dirStr).SerchErrors));

                if (file.ToLower().EndsWith(".docx")) 
                    threads.Last().Priority = ThreadPriority.Highest;
                
                threads.Last().Start();
            }
            foreach (var dir in dirs)  //создаем задачу для каждой из директорий +  понадобится логика 
            {
                tasks.Add(new Task(new TaskDirScan(dir).Run));
                tasks.Last().Start();
            }
        }

        private void SerchErrors( )
        {
            foreach (var file in files)
            {
                using (TextFieldParser tfp = new TextFieldParser(file))
                {
                    tfp.TextFieldType = FieldType.Delimited;
                    tfp.SetDelimiters(","," ,",", "," , ");

                    while (!tfp.EndOfData)
                    {
                        string[] fields = tfp.ReadFields();

                        //логика по которой мы отбираем нужные строки 
                        if (fields.Length == 14 && fields[2].ToLower() == "new bug")
                        {
                            
                            Error e = new Error();
                            e.name = fields[4];
                            e.bugTypes = new List<string>();
                         
                            if (errors.Count <= 0)
                            {
                                e.bugTypes.Add(fields[9]);
                                errors.Add(e);
                                continue;
                            }

                            bool isFound = false;
                            foreach (var error in errors)
                                if (error.name == e.name)
                                {
                                    isFound = true;
                                    error.bugTypes.Add(fields[9]);
                                }

                            if (isFound == false)
                            {
                                 e.bugTypes.Add(fields[9]);
                                errors.Add(e);
                            }
                            


                        }
                    }
                }

            }
           
        }

    }
}
