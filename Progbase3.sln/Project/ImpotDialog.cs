using Terminal.Gui;
using System.Collections.Generic;
using AccessDataLib;
public class ImportWindow: Dialog
{
    public bool canceled;
    private TextField filename;
    private PostRepository postRepository;
    private CommentRepository commentRepository;
    private UserRepository userRepository;
    public ImportWindow(PostRepository postRepository, CommentRepository commentRepository, UserRepository userRepository)
    {
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
        this.userRepository = userRepository;

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
        if(!filepath.EndsWith(".xml"))
        {
            MessageBox.ErrorQuery("Import","File not xml","ok");
            filename.Text = "";
            return;
        }
        // if(!File.Exists(filepath))
        // {
        //     MessageBox.ErrorQuery("Import","File doesn't exist","ok");
        //     filename.Text = "";
        //     return;
        // }
        List<Post> posts = Export_Import.Import(filepath);
        if(posts != null)
        {
            foreach (Post item in posts)
            {
                postRepository.Insert(item, item.user);
                foreach (Comment comment in item.comments)
                {
                    commentRepository.Insert(comment, item, this.userRepository.GetByID(comment.userId));
                }
            }
            Application.RequestStop();
            return;
        }
    }
    private void OnCreateDialogCanceled()
    {
        this.canceled = true;
        Application.RequestStop();
    }
}