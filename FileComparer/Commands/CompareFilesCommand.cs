//------------------------------------------------------------------------------
// <copyright file="CompareFilesCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using EnvDTE80;
using System.Linq;
using EnvDTE;
using System.Windows.Forms;
using System.IO;

namespace FileComparer
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class CompareFilesCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 0x0100;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("1753e55d-81b2-418e-b4f7-7add6a5c2634");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly Package package;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompareFilesCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private CompareFilesCommand(Package package)
    {
      if (package == null)
      {
        throw new ArgumentNullException("package");
      }

      this.package = package;

      OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      if (commandService != null)
      {
        var menuCommandID = new CommandID(CommandSet, CommandId);
        var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
        commandService.AddCommand(menuItem);
      }
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static CompareFilesCommand Instance
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the service provider from the owner package.
    /// </summary>
    private IServiceProvider ServiceProvider
    {
      get
      {
        return this.package;
      }
    }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static void Initialize(Package package)
    {
      Instance = new CompareFilesCommand(package);
    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void MenuItemCallback(object sender, EventArgs e)
    {
      var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
      string file1, file2;
      if (IsFileComparable(dte, out file1, out file2))
      {
        dte.ExecuteCommand("Tools.DiffFiles", $"\"{ file1}\" \"{ file2}\"");
      }
    }

    /// <summary>
    /// Fetches the files for comparison
    /// </summary>
    /// <param name="dte"></param>
    /// <returns></returns>
    public static IEnumerable<string> FetchSelectedFiles(DTE2 dte)
    {
      var files = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;
      return from file in files.Cast<UIHierarchyItem>()
             let p = file.Object as ProjectItem
             select p.FileNames[1];
    }

    /// <summary>
    /// Checks the no. of files and if the files are comparable or not.
    /// </summary>
    /// <param name="dte"></param>
    /// <param name="file1"></param>
    /// <param name="file2"></param>
    /// <returns></returns>
    private static bool IsFileComparable(DTE2 dte, out string file1, out string file2)
    {
      var files = FetchSelectedFiles(dte);
      file1 = files.ElementAtOrDefault(0);
      file2 = files.ElementAtOrDefault(1);
      if (files.Count() == 1)
      {
        var fileDialog = new OpenFileDialog();
        fileDialog.InitialDirectory = Path.GetDirectoryName(file1);
        fileDialog.ShowDialog();
        file2 = fileDialog.FileName;
      }
      return !String.IsNullOrEmpty(file1) && !String.IsNullOrEmpty(file2);

    }
  }
}
