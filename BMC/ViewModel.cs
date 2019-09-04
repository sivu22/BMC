using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BMC
{
    class ViewModel: BindableProperty
    {
        private CancellationTokenSource cts;
        private readonly Progress<int> progressHandler;

        public Model Model { get; set; }

        public ICommand ConvertCommand { get; set; }

        private int sortProgress;
        public int SortProgress { get => sortProgress; set => ChangeProperty(ref sortProgress, value); }

        public ViewModel()
        {
            Model = new Model();

            progressHandler = new Progress<int>(value =>
            {
                SortProgress = value;
            });

            ConvertCommand = new RelayCommand(async param => await Convert(progressHandler as IProgress<int>), param => Model.CanConvert);
        }

        public void UpdateSourcePath(string newPath)
        {
            Model.SourcePath = newPath;
        }

        public void UpdateOutputPath(string newPath)
        {
            Model.OutputPath = newPath;
        }

        public async Task Convert(IProgress<int> progress)
        {
            if (!Model.Converting)
            {
                Model.Converting = true;
                Model.Status = "Preparing data...";
                SortProgress = 0;

                cts = new CancellationTokenSource();
                var token = cts.Token;
                bool canceled = false, ignore = false;

                try
                {
                    await Task.Run(() => Model.FindMediaFiles(token));
                }
                catch (OperationCanceledException)
                {
                    canceled = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to search source path: " + e.Message, "BMC", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (Model.Items.Length > 0)
                {
                    try
                    {
                        Model.CreateOutputFolder();
                    }
                    catch (Exception e)
                    {
                        canceled = true;
                        MessageBox.Show($"Could not create output folder: {e.Message}", "BMC", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    if (!canceled)
                    {
                        var currentItem = 0;
                        foreach (var item in Model.Items)
                        {
                            try
                            {
                                await Task.Run(() => Model.ConvertItemAsync(item));

                                Model.Status = $"{++currentItem}/{Model.Items.Length}";
                                int progressPercent = 100 * currentItem / Model.Items.Length;

                                progress?.Report(progressPercent);
                                token.ThrowIfCancellationRequested();
                            }
                            catch (OperationCanceledException)
                            {
                                canceled = true;
                                break;
                            }
                            catch (Exception e)
                            {
                                if (!ignore)
                                {
                                    var selection = MessageBox.Show($"Failed to convert {item.Name}: {e.Message}\n\nIgnore all errors from now on?", "BMC", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
                                    if (selection == MessageBoxResult.Cancel)
                                    {
                                        canceled = true;
                                        break;
                                    }
                                    else if (selection == MessageBoxResult.Yes)
                                    {
                                        ignore = true;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No BMW media files found.", "BMC", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                Model.Converting = false;
                cts = null;
                if (canceled) Model.Status = "";
                else Model.Status = "Done";
            }
            else
            {
                Model.Converting = false;
                if (cts != null) cts.Cancel();
            }
        }
    }
}
