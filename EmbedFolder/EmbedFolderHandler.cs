using MonoDevelop.Components.Commands;
using MonoDevelop.Core.Collections;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Projects;

namespace EmbedFolder
{
    public class EmbedFolderHandler : CommandHandler
    {
        protected override async void Run()
        {
            var nodes = getNodes();
            if (nodes == null)
            {
                MessageService.ShowError("Can't embed folder, nothing is selected");
                return;
            }
            int filesEmbedded = 0;
            int allFiles = 0;
            Set<SolutionItem> projects = new Set<SolutionItem>();
            foreach (var node in nodes)
            {
                embedFolder(node, projects, ref allFiles, ref filesEmbedded);
            }
            await IdeApp.ProjectOperations.SaveAsync(projects);
            MessageService.ShowMessage($"New files embedded: {filesEmbedded} (out of {allFiles}).");
        }

        protected override void Update(CommandInfo info)
        {
            var nodes = getNodes();
            if (nodes == null || nodes.Length == 0)
            {
                info.Visible = false;
                return;
            }
            foreach (var node in nodes)
            {
                if (!(node.DataItem is ProjectFolder))
                {
                    info.Visible = false;
                    return;
                }
            }
            info.Text = nodes.Length == 1 ? "Embed Folder" : "Embed Folders";
        }

        private ITreeNavigator[] getNodes()
        {
            if (!IdeApp.Workbench.Pads.SolutionPad.Visible) return null;
            SolutionPad pad = IdeApp.Workbench.Pads.SolutionPad.Content as SolutionPad;
            if (pad == null) return null;
            return pad.TreeView.GetSelectedNodes();
        }

        private void embedFolder(ITreeNavigator node, Set<SolutionItem> projects, ref int allFiles, ref int filesEmbedded)
        {
            node.MoveToFirstChild();
            do 
            {
                if (node.DataItem is ProjectFolder)
                {
                    embedFolder(node.Clone(), projects, ref allFiles, ref filesEmbedded);
                    continue;
                }
                ProjectFile file = node.DataItem as ProjectFile;
                if (file == null) continue;
                allFiles++;
                const string EMBEDDED_RESOURCE = "EmbeddedResource";
                if (file.BuildAction != EMBEDDED_RESOURCE)
                {
                    file.BuildAction = EMBEDDED_RESOURCE;
                    filesEmbedded++;
                    projects.Add(file.Project);
                }
            }
            while (node.MoveNext());
        }
    }
}
