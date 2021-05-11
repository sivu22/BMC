using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BMC
{
    class Model: BindableProperty
    {
        public struct Item
        {
            public string FullPath { get; set; }
            public string Name { get; set; }
            public MediaConverter.IDriveType Type { get; set; }
        }

        public struct ConvertedItem
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Status { get; set; }
        }

        public Item[] Items { get; private set; }
        public ObservableCollection<ConvertedItem> ConvertedItems { get; private set; }


        class Defaults
        {
            public const string SourcePath = "";
            public const bool Subfolders = false;
            public static readonly string OutputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            public const bool NextToSource = false;
            public const int HUIndex = (int)MediaConverter.HeadUnit.NBT;
        }

        private readonly string[] convertDep = new string[] { nameof(CanConvert) };

        private string sourcePath = Defaults.SourcePath;
        public string SourcePath { get => sourcePath; set => ChangeProperty(ref sourcePath, value, convertDep); }

        private bool subfolders = Defaults.Subfolders;
        public bool Subfolders { get => subfolders; set => ChangeProperty(ref subfolders, value); }

        private string outputPath = Defaults.OutputPath;
        public string OutputPath { get => outputPath; set => ChangeProperty(ref outputPath, value, convertDep); }

        private bool nextToSource = Defaults.NextToSource;
        public bool NextToSource { get => nextToSource; set => ChangeProperty(ref nextToSource, value, new string[] { nameof(OutputEnabled) }); }

        private int huIndex = Defaults.HUIndex;
        public int HUIndex { get => huIndex; set => ChangeProperty(ref huIndex, value); }


        public bool CanConvert { get => !string.IsNullOrEmpty(SourcePath) && !string.IsNullOrEmpty(OutputPath); }


        private string status = "";
        public string Status { get => status; set => ChangeProperty(ref status, value); }

        private bool converting = false;
        public bool Converting { get => converting; set => ChangeProperty(ref converting, value, new string[] { nameof(NotConverting), nameof(RunText), nameof(OutputEnabled) }); }
        public bool NotConverting { get => !Converting; }

        public bool OutputEnabled
        {
            get
            {
                return NotConverting && !NextToSource;
            }
        }

        public string RunText { get => Converting ? "Cancel" : "Convert"; }

        public Model()
        {
            ConvertedItems = new ObservableCollection<ConvertedItem>();
        }

        public void FindMediaFiles(CancellationToken? token = null)
        {
            var items = new List<Item>();

            List<string> files;
            if (Subfolders)
                files = Directory.EnumerateFiles(SourcePath, "*.*", SearchOption.AllDirectories).Where(fileName => MediaConverter.SearchFilter(fileName)).ToList();
            else
                files = Directory.EnumerateFiles(SourcePath).Where(fileName => MediaConverter.SearchFilter(fileName)).ToList();
            token?.ThrowIfCancellationRequested();

            Item item = new Item();
            int namePos = 0;
            foreach (string file in files)
            {
                namePos = file.LastIndexOf("\\");
                item.FullPath = file;
                (item.Name, item.Type) = MediaConverter.GetFileNameAndIDriveType(file.Substring(namePos + 1));

                if (item.Type != MediaConverter.IDriveType.Unknown) items.Add(item);
                token?.ThrowIfCancellationRequested();
            }

            Items = items.ToArray();
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                ConvertedItems.Clear();
            });
        }

        public void CreateOutputFolder()
        {
            if (NextToSource || Directory.Exists(OutputPath)) return;

            Directory.CreateDirectory(OutputPath);
        }

        public async Task ConvertItemAsync(Item item)
        {
            if (item.Type == MediaConverter.IDriveType.Unknown) return;

            var fileName = String.Format("{0}.{1}", item.Name, MediaConverter.GetMediaType(item.Type).ToString().ToLower());

            string newFile = "";
            if (NextToSource)
            {
                int namePos = item.FullPath.LastIndexOf("\\");
                var path = item.FullPath.Substring(0, namePos + 1);
                newFile = String.Format("{0}{1}", path, fileName);
            }
            else newFile = String.Format("{0}\\{1}", OutputPath, fileName);

            var itemConv = new ConvertedItem
            {
                Name = fileName,
                Status = "OK",
                Type = item.Type.ToString()
            };

            try
            {
                var bytes = File.ReadAllBytes(item.FullPath);
                var bytesConv = await Task.Run(() => MediaConverter.Convert(bytes, item.Type, (MediaConverter.HeadUnit)huIndex));
                File.WriteAllBytes(newFile, bytesConv);

                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    ConvertedItems.Add(itemConv);
                });
            }
            catch (Exception e)
            {
                itemConv.Status = $"Error: {e.Message}";
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    ConvertedItems.Add(itemConv);
                });

                throw;
            }
        }
    }
}
