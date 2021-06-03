using Terminal.Gui;
using System.Collections.Generic;
using AccessDataLib;
using System;
public class ImportWindow: Dialog
{
    public bool canceled;
    private TextField filename;
    private PostReposytory postReposytory;
    private CommentReposytory commentReposytory;
    private UserReposytory userReposytory;
    public ImportWindow(PostReposytory postReposytory, CommentReposytory commentReposytory, UserReposytory userReposytory)
    {
        this.postReposytory = postReposytory;
        this.commentReposytory = commentReposytory;
        this.userReposytory = userReposytory;

        int rightColumnX = 20;
        this.Title = "Import";

        Label fileNameLbl = new Label(2,4,"File name:");
        filename = new TextField("")
        {
            X = rightColumnX,
            Y = Pos.Top(fileNameLbl),
            Width = 40,
        };
        this.Add(fileNameLbl, filename);

        Button exportBtn = new Button("Import");
        exportBtn.Clicked += OnImportDialog;
        this.AddButton(exportBtn);

        Button cancelButton = new Button("Cancel");
        cancelButton.Clicked += OnCreateDialogCanceled;
        this.AddButton(cancelButton);
    }
    private void OnImportDialog()
    {
        string filepath = filename.Text.ToString();
        if(filepath.EndsWith(".xml"))
        {
            MessageBox.ErrorQuery("Import","File not xml","ok");
            filename.Text = "";
            return;
        }
        List<Post> posts = Export_Import.Import(filepath);
        if(posts != null)
        {
            foreach (Post item in posts)
            {
                postReposytory.Insert(item, item.user);
                foreach (Comment comment in item.comments)
                {
                    commentReposytory.Insert(comment, item, this.userReposytory.GetByID(comment.userId));
                }
            }
            Application.RequestStop();
        }
        MessageBox.ErrorQuery("Import","File doesn't exist","ok");
        filename.Text = "";
        return;
    }
    private void OnCreateDialogCanceled()
    {
        this.canceled = true;
        Application.RequestStop();
    }
}