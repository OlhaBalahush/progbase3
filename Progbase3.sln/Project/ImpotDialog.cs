using System;
using Terminal.Gui;
using System.Collections.Generic;
public class ImportWindow: Dialog
{
    public bool canceled;
    // private TextField searchText;
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
        List<Post> posts = Export_Import.Import(filename.Text.ToString());
        foreach (Post item in posts)
        {
            postReposytory.Insert(item, item.user);
            foreach (Comment comment in item.comments)
            {
                commentReposytory.Insert(comment, item, this.userReposytory.GetByID(comment.userId));
            }
            //MessageBox.ErrorQuery("",item.id.ToString(),"ok");
        }
        Application.RequestStop();
    }
    private void OnCreateDialogCanceled()
    {
        this.canceled = true;
        Application.RequestStop();
    }
}