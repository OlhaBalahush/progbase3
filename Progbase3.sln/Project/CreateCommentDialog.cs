// using System;
// using Terminal.Gui;
// public class CreateCommentDialog: Dialog
// {
//     public bool canceled;
//     protected TextField usernameInput;
//     protected TextField passwordInput;
//     public CreateCommentDialog()
//     {
//         this.Title = "Create comment";
//         Button okBtn = new Button("Ok");
//         okBtn.Clicked += OnCreateDialogSubmit;
//         this.AddButton(okBtn);

//         Button cancelButton = new Button("Cancel");
//         cancelButton.Clicked += OnCreateDialogCanceled;
//         this.AddButton(cancelButton);

//         int rightColumnX = 20;

//         Label usernameLbl = new Label(2,2,"Created by|username");
//         usernameInput = new TextField("")
//         {
//             X = rightColumnX,
//             Y = Pos.Top(usernameLbl),
//             Width = 40,
//         };
//         this.Add(usernameLbl, usernameInput);

//         Label usernameLbl = new Label(2,2,"Created for|post");
//         usernameInput = new TextField("")
//         {
//             X = rightColumnX,
//             Y = Pos.Top(usernameLbl),
//             Width = 40,
//         };
//         this.Add(usernameLbl, usernameInput);

//         Label passwordLbl = new Label(2,6,"Password");
//         passwordInput = new TextField ("")
//         {
//             X = rightColumnX,
//             Y = Pos.Top(passwordLbl),
//             Width = 40,
//         };
//         this.Add(passwordLbl, passwordInput);
//     }
//     private void OnCreateDialogCanceled()
//     {
//         this.canceled = true;
//         Application.RequestStop();
//     }
//     private void OnCreateDialogSubmit()
//     {
//         this.canceled = false;
//         Application.RequestStop();
//     }
//     public User GetComment()
//     {
//         string username = usernameInput.Text.ToString();
//         int moderator = 0;
//         if(moderatorCheck.Checked == true)
//         {
//             moderator = 1;
//         }
//         string password = passwordInput.Text.ToString();
//         return new User(username, moderator, password, DateTime.Now.ToString());
//     }
//     public bool CheckUser()
//     {
//         return false;
//     }
//     public bool CheckPost()
//     {
//         return false;
//     }
// }