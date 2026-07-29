//using System;
//using System.Collections.Generic;
//using System.Diagnostics;

//class Program
//{
//    static void Main()
//    {
//        List<int> scores = new List<int> { 80, 91, 70, 60, 100 };
//        double average = GetAverage(scores);
//        Console.WriteLine($"Average score:{average}");
//    }
//    static double GetAverage(List<int> scores)
//    {
//        int total = 0;
//        for (int i = 0; i < scores.Count; i++)
//        {
//            total += scores[i];
//        }
//        return (double)total / scores.Count;
//    }
//}

//Create a class named EMPLOYEE as super class and ENGINEER and DOCTOR as sub class. Make your own assumptions as properties and methods.   
//class EMPLOYEE
//    {
//        public int id;
//        public string Name;
//    }
//    class Engineer : EMPLOYEE
//    {

//        public string Profession = "Engineer";
//        public void Display()
//        {
//            Console.WriteLine($"ID: {id}, Name: {Name}");
//        }
//    }
//    class Doctor : EMPLOYEE
//    {

//        public string Profession = "Doctor";
//        public void Display()
//        {
//            Console.WriteLine($"ID: {id}, Name: {Name}");
//        }

//    }
//class Program
//{

//    static void Main(string[] args)
//    {
//        Engineer e = new Engineer();
//        e.id = 1;
//        e.Name = "Karan";
//        Doctor d = new Doctor();
//        d.id = 2;
//        d.Name = "Ram";
//        e.Display();
//            d.Display();

//    }
//}
//Write a program to create your own exception when the user gives subject name than “C#
//public class IVE : Exception
//{ 
//   public IVE(string message):base(message)
//    { }

//}

//class Program
//{
//    static void Main(string[] args)
//    {
//        Console.Write("Enter your subject name:");
//        String subject = Console.ReadLine();
//        try {
//            string message = Message(subject);
//            Console.WriteLine(message);
//        }
//        catch(IVE ex)
//        {
//            Console.WriteLine(ex.Message);
//        }




//    }
//    static string Message(string message)
//    {
//        if (message != "C#")
//            throw new IVE("exception occured");
//        return message;
//    }

//}
