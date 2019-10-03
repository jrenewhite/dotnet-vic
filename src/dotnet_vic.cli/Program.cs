using dotnet_vic.core.IO;
using dotnet_vic.core.Model;
using System;
using System.IO;
using Yaap;
using static Yaap.YaapConsole;
using static System.Linq.Enumerable;
using dotnet_vic.core;
using dotnet_vic.core.Classifiers;
using System.Collections.Generic;
using System.Reflection;

namespace dotnet_vic.cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string uciDir = "C:\\Users\\jrenewhite\\OneDrive\\Maestría\\UCI data\\Datasets";
            var files = Directory.GetFiles(uciDir).Yaap(settings: new YaapSettings
            {
                Description = "Reading files",
                Width = 100,
                ColorScheme = YaapColorScheme.Dark,
                SmoothingFactor = 0.5
            });
            var type = typeof(ISupervisedClassifier);

            var classifiersTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type mytype in assembly.GetTypes()
                    .Where(mytype => mytype.GetInterfaces().Contains(type)))
                {
                    //do stuff
                    //Console.WriteLine($"{mytype.Name} is implementable? {mytype.(type)}");
                    classifiersTypes.Add(mytype);
                }
            }

            List<ISupervisedClassifier> classifiers = new List<ISupervisedClassifier>();
            foreach (var classifierType in classifiersTypes)
            {
                try
                {
                    // test creating an instance of the class.
                    ISupervisedClassifier classifier = (ISupervisedClassifier)Activator.CreateInstance(classifierType);
                    classifiers.Add(classifier);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not create {0} class.", classifierType);
                }
            }

            foreach (var item in files)
            {
                try
                {
                    var serializer = new ARFFSerializer(item);
                    Dataset dataset = serializer.DeserializeDataset();
                    VIC vic = new VIC(dataset, classifiers, new AccordKMeans());
                    double index = vic.GetValidityIndex();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
