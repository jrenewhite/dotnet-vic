using dotnet_vic.core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_vic.core.IO
{
    public interface IDatasetSerializer
    {
        /// <summary>
        /// Obtains all instances available in the file
        /// </summary>
        /// <returns>An enumeration of all <see cref="Instance"/> elements</returns>
        IEnumerable<Instance> DeserializeInstances();

        /// <summary>
        /// Obtains all instances available in the file asynchronously
        /// </summary>
        /// <returns>An enumeration of all <see cref="Instance"/> elements</returns>
        Task<IEnumerable<Instance>> DeserializeInstancesAsync();

        /// <summary>
        /// Obtains a definition for a <see cref="Dataset"/>
        /// </summary>
        /// <returns>A <see cref="Dataset"/> definition (may include all <see cref="Instance"/> elements)</returns>
        Dataset DeserializeDataset();

        /// <summary>
        /// Obtains a definition for a <see cref="Dataset"/> asynchronously
        /// </summary>
        /// <returns>A <see cref="Dataset"/> definition (may include all <see cref="Instance"/> elements)</returns>
        Task<Dataset> DeserializeDatasetAsync();

        /// <summary>
        /// Writes all <see cref="Instance"/> elements into the file
        /// </summary>
        /// <param name="instances">An enumeration of <see cref="Instance"/> elements</param>
        void SerializeInstances(IEnumerable<Instance> instances);

        /// <summary>
        /// Writes all <see cref="Instance"/> elements into the file asynchronously
        /// </summary>
        /// <param name="instances">An enumeration of <see cref="Instance"/> elements</param>
        /// <returns>A <see cref="Task"/> operation result</returns>
        Task SerializeInstancesAsync(IEnumerable<Instance> instances);

        /// <summary>
        /// Writes a <see cref="Dataset"/> into a file
        /// </summary>
        /// <param name="dataset">A <see cref="Dataset"/> definition that my contain any number of <see cref="Instance"/> elements</param>
        void SerializeDataset(Dataset dataset);

        /// <summary>
        /// Writes a <see cref="Dataset"/> into a file asynchronously
        /// </summary>
        /// <param name="dataset">A <see cref="Dataset"/> definition that my contain any number of <see cref="Instance"/> elements</param>
        /// <returns>A <see cref="Task"/> operation result</returns>
        Task SerializeDatasetAsync(Dataset dataset);

    }
    public abstract class DatasetSerializer : IDatasetSerializer
    {
        /// <summary>
        /// The name of the file
        /// </summary>
        public string FileName { get; protected set; }

        /// <summary>
        /// The size of the file
        /// </summary>
        public int Size { get; protected set; }

        /// <summary>
        /// Path to the file to read/write
        /// </summary>
        public string FilePath { get; protected set; }

        /// <summary>
        /// Content of the file.
        /// </summary>
        /// <remarks>By default is empty</remarks>
        protected byte[] Content;

        /// <summary>
        /// Base contructor for a dataset serializer
        /// </summary>
        protected DatasetSerializer()
        {
            Content = new byte[0];
            FilePath = string.Empty;
            FileName = string.Empty;
            Size = 0;
        }
        
        /// <summary>
        /// Constructor for <see cref="DatasetSerializer"/> that requires the name of the file to serialize/deserialize
        /// </summary>
        /// <param name="path">Path of the file to serialize/deserialize</param>
        public DatasetSerializer(string path) : this()
        {
            FilePath = path ?? string.Empty;
        }

        /// <summary>
        /// Attempts to read the file and obtain a <see cref="byte"/> array
        /// </summary>
        /// <returns>A <see cref="bool"/> indicating if file reading was succesful</returns>
        protected bool ReadFromPath()
        {
            if (File.Exists(FilePath))
            {
                FileName = Path.GetFileNameWithoutExtension(FilePath);
                Content = File.ReadAllBytes(FilePath);
                Size = Content.Length;
            }

            return Content.Length != 0;
        }

        /// <summary>
        /// Attempts to read the file and obtain a <see cref="byte"/> array asynchronously
        /// </summary>
        /// <returns>A <see cref="bool"/> indicating if file reading was succesful</returns>
        protected async Task<bool> ReadFromPathAsync()
        {
            if (File.Exists(FilePath))
            {
                FileName = Path.GetFileNameWithoutExtension(FilePath);
                Content = await File.ReadAllBytesAsync(FilePath);
                Size = Content.Length;
            }

            return Content.Length != 0;
        }

        /// <summary>
        /// Obtains a definition for a <see cref="Dataset"/>
        /// </summary>
        /// <returns>A <see cref="Dataset"/> definition (may include all <see cref="Instance"/> elements)</returns>
        public abstract Dataset DeserializeDataset();

        /// <summary>
        /// Obtains a definition for a <see cref="Dataset"/> asynchronously
        /// </summary>
        /// <returns>A <see cref="Dataset"/> definition (may include all <see cref="Instance"/> elements)</returns>
        public abstract Task<Dataset> DeserializeDatasetAsync();

        /// <summary>
        /// Obtains all instances available in the file
        /// </summary>
        /// <returns>An enumeration of all <see cref="Instance"/> elements</returns>
        public abstract IEnumerable<Instance> DeserializeInstances();

        /// <summary>
        /// Obtains all instances available in the file asynchronously
        /// </summary>
        /// <returns>An enumeration of all <see cref="Instance"/> elements</returns>
        public abstract Task<IEnumerable<Instance>> DeserializeInstancesAsync();

        /// <summary>
        /// Writes a <see cref="Dataset"/> into a file
        /// </summary>
        /// <param name="dataset">A <see cref="Dataset"/> definition that my contain any number of <see cref="Instance"/> elements</param>
        public abstract void SerializeDataset(Dataset dataset);

        /// <summary>
        /// Writes a <see cref="Dataset"/> into a file asynchronously
        /// </summary>
        /// <param name="dataset">A <see cref="Dataset"/> definition that my contain any number of <see cref="Instance"/> elements</param>
        /// <returns>A <see cref="Task"/> operation result</returns>
        public abstract Task SerializeDatasetAsync(Dataset dataset);

        /// <summary>
        /// Writes all <see cref="Instance"/> elements into the file
        /// </summary>
        /// <param name="instances">An enumeration of <see cref="Instance"/> elements</param>
        public abstract void SerializeInstances(IEnumerable<Instance> instances);

        /// <summary>
        /// Writes all <see cref="Instance"/> elements into the file asynchronously
        /// </summary>
        /// <param name="instances">An enumeration of <see cref="Instance"/> elements</param>
        /// <returns>A <see cref="Task"/> operation result</returns>
        public abstract Task SerializeInstancesAsync(IEnumerable<Instance> instances);
    }
}
