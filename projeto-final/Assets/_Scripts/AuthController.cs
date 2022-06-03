using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;

public class AuthController : MonoBehaviour
{
    public Text emailInputReg, passwordInputReg, emailInputLog, passwordInputLog;

    public void Login(){
        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailInputLog.text,passwordInputLog.text).ContinueWith((task=>{
            if (task.IsCanceled) {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",newUser.DisplayName, newUser.UserId);
            }));

    }

    public void Registrar(){
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailInputReg.text, passwordInputReg.text).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",newUser.DisplayName, newUser.UserId);
            });
    }


}
