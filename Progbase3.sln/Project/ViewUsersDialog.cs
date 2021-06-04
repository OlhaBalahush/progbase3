using Terminal.Gui;
using System.Collections;
using System.Collections.Generic;
using AccessDataLib;

public class ViewUsersDialog: Dialog
{
    private UserRepository userRepository;
    private PostRepository postRepository;
    private CommentRepository commentRepository;
    protected User currentUser;
    private ListView allUserListView;
    private Button prevPageBtn;
    private Button nextPageBtn;
    private Label pageLbl;
    private Label totalPagesLbl;
    private FrameView frameView;
    private Label noUserLbl;
    private int pageLength = 5;
    private int currentpage = 1;
    private string searchValue = "";
    private TextField searchInput;
    public ViewUsersDialog(User current, UserRepository userRepository, PostRepository postRepository, CommentRepository commentRepository)
    {
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
        this.currentUser = current;

        this.Title = "All Users";
        Button backBtn = new Button("Back");
        backBtn.Clicked += OnCreateDialogSubmit;
        this.AddButton(backBtn);

        noUserLbl = new Label("No user")
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        allUserListView = new ListView((IList)null)
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        allUserListView.OpenSelectedItem += OnOpenUser;
        
        prevPageBtn = new Button(2,10,"Prev");
        prevPageBtn.Clicked += OnPreviousPage;
        pageLbl = new Label("?")
        {
            X = Pos.Right(prevPageBtn) + 2,
            Y = Pos.Top(prevPageBtn),
            Width = 5,
        };
        totalPagesLbl = new Label("?")
        {
            X = Pos.Right(pageLbl) + 2,
            Y = Pos.Top(prevPageBtn),
            Width = 5,
        };
        nextPageBtn = new Button("Next")
        {
            X = Pos.Right(totalPagesLbl) + 2,
            Y = Pos.Top(prevPageBtn),
        };
        nextPageBtn.Clicked += OnNextPage;
        this.Add(prevPageBtn, pageLbl, totalPagesLbl, nextPageBtn);

        frameView = new FrameView("Users:")
        {
            X = 2,
            Y = 12,
            Width = Dim.Fill() - 4,
            Height = pageLength + 3,
        };
        if(userRepository.GetSearchPage(searchValue, currentpage, pageLength).Count == 0)
        {
            frameView.Add(noUserLbl);
        }
        else
        {
            frameView.Add(allUserListView);
        }
        this.Add(frameView);

        searchInput = new TextField(2,4,20,"");
        searchInput.KeyPress += OnSearchEnter;
        this.Add(searchInput);

        UpdateCurrentPage();
    }
    private void OnSearchEnter(KeyEventEventArgs args)
    {
        if(args.KeyEvent.Key == Key.Enter)
        {
            this.searchValue = this.searchInput.Text.ToString();
            UpdateCurrentPage();
        }
    }
    private void UpdateCurrentPage()
    {
        int totalPages = userRepository.NumberOfPages(this.searchValue, this.pageLength);
        if(totalPages == 0)
        {
            totalPages = 1;
        }
        if(currentpage > totalPages && currentpage > 1)
        {
            currentpage = totalPages;
        }
        this.pageLbl.Text = currentpage.ToString();
        this.totalPagesLbl.Text = totalPages.ToString();
        List<User> users = userRepository.GetSearchPage(searchValue,  currentpage, this.pageLength);
        if(users.Count != 0)
        {
            allUserListView.SetSource(users);  
            frameView.Add(allUserListView); 
        }
        else
        {
            frameView.Add(noUserLbl);
        }
        
        prevPageBtn.Visible = (currentpage != 1);
        nextPageBtn.Visible = (currentpage != totalPages);
    }
    private void OnPreviousPage()
    {
        if(currentpage == 1)
        {
            return;
        }
        this.currentpage--;
        UpdateCurrentPage();
    }
    private void OnNextPage()
    {
        int totalPages = userRepository.NumberOfPages(this.searchValue, this.pageLength);
        if(currentpage >= totalPages)
        {
            return;
        }
        this.currentpage++;
        UpdateCurrentPage();
    }
    private void OnOpenUser(ListViewItemEventArgs args)
    {
        User user = (User)args.Value;
        OpenProfileDialog dialog = new OpenProfileDialog(this.currentUser ,user, this.userRepository, this.postRepository, this.commentRepository);

        Application.Run(dialog);

        if(dialog.deleted)
        {
            bool result = userRepository.Delete(user.id);
            if(result)
            {
                int pages = userRepository.NumberOfPages(searchValue, pageLength);
                if(currentpage > pages && pageLength > 1)
                {
                    pages--;
                    this.UpdateCurrentPage();
                }
                this.UpdateCurrentPage();
                //this.users = 
                allUserListView.SetSource(userRepository.UserssOnPage(currentpage));
            }
            else
            {
                MessageBox.ErrorQuery("Delete post", "Can't delete post", "Ok");
            }
        }
        if(dialog.updated)
        {
            if(dialog.GetUser() != null)
            {
                bool result = userRepository.Update(user.id, dialog.GetUser());
                if(result)
                {
                    allUserListView.SetSource(userRepository.UserssOnPage(currentpage));
                    this.UpdateCurrentPage();
                }
                else
                {
                    MessageBox.ErrorQuery("Update concert", "Can't update concert", "Ok");
                }
            }
        }
    }
    private void OnCreateDialogSubmit()
    {
        Application.RequestStop();
    }
}