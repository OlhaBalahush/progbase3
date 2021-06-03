using System;
using Terminal.Gui;
using System.Collections.Generic;
using AccessDataLib;
public class ExportWindow: Dialog
{
    public bool canceled;
    private TextField searchText;
    private TextField filename;
    private PostReposytory postReposytory;
    private CommentReposytory commentReposytory;
    public ExportWindow(PostReposytory postReposytory, CommentReposytory commentReposytory)
    {
        this.postReposytory = postReposytory;
        this.commentReposytory = commentReposytory;
        int rightColumnX = 20;

        Label fileNameLbl = new Label(2,4,"File name:");
        filename = new TextField("")
        {
            X = rightColumnX,
            Y = Pos.Top(fileNameLbl),
            Width = 40,
        };
        this.Add(fileNameLbl, filename);

        Label textSearchLbl = new Label(2,2,"Text:");
        searchText = new TextField("")
        {
            X = rightColumnX,
            Y = Pos.Top(textSearchLbl),
            Width = 40,
        };
        this.Add(textSearchLbl, searchText);

        Button exportBtn = new Button("Export");
        exportBtn.Clicked += OnExportDialog;
        this.AddButton(exportBtn);

        Button cancelButton = new Button("Cancel");
        cancelButton.Clicked += OnCreateDialogCanceled;
        this.AddButton(cancelButton);
    }
    private void OnExportDialog()
    {
        string filepath = filename.Text.ToString();
        if(!filepath.EndsWith(".xml"))
        {
            filepath += ".xml";
        }
        string searchValue = this.searchText.Text.ToString();
        List<Post> posts = postReposytory.GetSearchValue(null, searchValue, -1);
        foreach (Post item in posts)
        {
            item.user = this.postReposytory.User(item.id);
            item.comments = this.postReposytory.CommentsOfPost(item.id, this.commentReposytory);
        }
        Export_Import.Export(posts, filepath);
        Application.RequestStop();
    }
    private void OnCreateDialogCanceled()
    {
        this.canceled = true;
        Application.RequestStop();
    }
}