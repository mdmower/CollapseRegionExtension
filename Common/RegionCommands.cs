using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.TextManager.Interop;
using Task = System.Threading.Tasks.Task;

namespace ToggleRegionsExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RegionCommands
    {
        /// <summary>
        /// Expand Command ID.
        /// </summary>
        public const int ExpandCommandId = 0x0100;

        /// <summary>
        /// Collapse Command ID.
        /// </summary>
        public const int CollapseCommandId = 0x0101;

        /// <summary>
        /// Toggle Command ID.
        /// </summary>
        public const int ToggleCommandId = 0x0102;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("e7b8c17f-d4e1-419b-a144-4cfec19ad1a4");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionCommands"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private RegionCommands(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var expandCommandId = new CommandID(CommandSet, ExpandCommandId);
            var expandMenuItem = new MenuCommand(Expand, expandCommandId);
            commandService.AddCommand(expandMenuItem);

            var collapseCommandId = new CommandID(CommandSet, CollapseCommandId);
            var collapseMenuItem = new MenuCommand(Collapse, collapseCommandId);
            commandService.AddCommand(collapseMenuItem);

            var toggleCommandId = new CommandID(CommandSet, ToggleCommandId);
            var toggleMenuItem = new MenuCommand(Toggle, toggleCommandId);
            commandService.AddCommand(toggleMenuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RegionCommands Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider => _package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in RegionCommands's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new RegionCommands(package, commandService);
        }

        private void Expand(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _ = _package.JoinableTaskFactory.RunAsync(async () =>
            {
                if (_package.DisposalToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    var (manager, regions) = await GetCurrentDocInfoAsync();
                    foreach (var region in regions)
                    {
                        if (region is ICollapsed collapsed)
                        {
                            manager.Expand(collapsed);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ActivityLog.LogError(ex.Source, ex.Message);
                }
            });
        }

        private void Collapse(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _ = _package.JoinableTaskFactory.RunAsync(async () =>
            {
                if (_package.DisposalToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    var (manager, regions) = await GetCurrentDocInfoAsync();
                    foreach (var region in regions)
                    {
                        if (!region.IsCollapsed)
                        {
                            manager.TryCollapse(region);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ActivityLog.LogError(ex.Source, ex.Message);
                }
            });
        }

        private void Toggle(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _ = _package.JoinableTaskFactory.RunAsync(async () =>
            {
                if (_package.DisposalToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    var (manager, regions) = await GetCurrentDocInfoAsync();
                    foreach (var region in regions)
                    {
                        if (!region.IsCollapsed)
                        {
                            manager.TryCollapse(region);
                        }
                        else if (region is ICollapsed collapsed)
                        {
                            manager.Expand(collapsed);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ActivityLog.LogError(ex.Source, ex.Message);
                }
            });
        }

        private async Task<(IOutliningManager manager, IEnumerable<ICollapsible> regions)> GetCurrentDocInfoAsync()
        {
            var emptyResult = (default(IOutliningManager), new ICollapsible[] { });

            var textManager = await ServiceProvider.GetServiceAsync(typeof(SVsTextManager)) as IVsTextManager
                ?? throw new ApplicationException($"{nameof(SVsTextManager)} service is not available.");
            var componentModel = await ServiceProvider.GetServiceAsync(typeof(SComponentModel)) as IComponentModel
                ?? throw new ApplicationException($"{nameof(SComponentModel)} service is not available.");
            var outlining = componentModel.GetService<IOutliningManagerService>()
                ?? throw new ApplicationException($"{nameof(IOutliningManagerService)} service is not available.");
            var editor = componentModel.GetService<IVsEditorAdaptersFactoryService>()
                ?? throw new ApplicationException($"{nameof(IVsEditorAdaptersFactoryService)} service is not available.");

            textManager.GetActiveView(1, null, out IVsTextView textViewCurrent);
            if (textViewCurrent == null)
            {
                return emptyResult;
            }

            var wpfView = editor.GetWpfTextView(textViewCurrent);
            if (wpfView == null)
            {
                return emptyResult;
            }

            var outliningManager = outlining.GetOutliningManager(wpfView);
            if (outliningManager == null)
            {
                return emptyResult;
            }

            List<ICollapsible> regions = new List<ICollapsible>();
            var snapshot = new SnapshotSpan(wpfView.TextSnapshot, 0, wpfView.TextSnapshot.Length);
            foreach (var region in outliningManager.GetAllRegions(snapshot))
            {
                var regionSnapshot = region.Extent.TextBuffer.CurrentSnapshot;
                var text = region.Extent.GetText(regionSnapshot);
                if (text.StartsWith("#region", StringComparison.CurrentCultureIgnoreCase) ||
                    text.StartsWith("#pragma region", StringComparison.CurrentCultureIgnoreCase))
                {
                    regions.Add(region);
                }
                if (text.StartsWith("<!--"))
                {
                    var textLow = text.ToLower();
                    if (textLow.Contains("region"))
                    {
                        regions.Add(region);
                    }
                }
            }

            return (outliningManager, regions);
        }
    }
}
